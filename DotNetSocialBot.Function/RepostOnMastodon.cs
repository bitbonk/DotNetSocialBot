using DotNetSocialBot.FunctionApp;
using HtmlAgilityPack;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DotNetSocialBot.Function;

public class RepostOnMastodon
{
    private readonly MastodonClient _client;
    private readonly ILogger _logger;
    private Account? _currentUser;

    public RepostOnMastodon(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RepostOnMastodon>();
        _logger.LogInformation("Initializing function {FunctionName}", nameof(RepostOnMastodon));
        var handler = new HttpClientHandler();
#if DEBUG // TODO: find out why certs are not accepted locally
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
        _client = new MastodonClient(Config.Instance, Config.AccessToken, new HttpClient(handler));
    }

    [Function(nameof(RepostOnMastodon))]
    public async Task RunAsync([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation(
            "Started function {FunctionName} at {StartTime}", nameof(RepostOnMastodon),
            DateTime.Now);
        _currentUser = await _client.GetCurrentUser();
        await HandleNotifications();
        await BoostTags();
        _logger.LogInformation(
            "Completed function {FunctionName} at {EndTime}", nameof(RepostOnMastodon),
            DateTime.Now);
    }

    private async Task HandleNotifications()
    {
        var notifications = await _client
            .GetNotifications(excludeTypes: NotificationType.Follow | NotificationType.Favourite |
                                            NotificationType.Reblog);

        try
        {
            foreach (var notification in notifications
                         .Where(n => n.Status?.Account?.Bot != true))
                if (!notification.Status.IsReply())
                    await BoostDirectMention(notification);
                else
                    await BoostBoostRequest(notification);
        }
        finally
        {
            await _client.ClearNotifications();
            _logger.LogInformation("Cleared all notifications");
        }
    }

    private async Task BoostBoostRequest(Notification notification)
    {
        var document = new HtmlDocument();
        document.LoadHtml(notification.Status?.Content);
        var replyText = document.DocumentNode.InnerText;
        if (Config.ValidBoostRequestMessages.Any(m =>
                replyText.EndsWith(m, StringComparison.InvariantCultureIgnoreCase)))
        {
            var statusIdToBoost = notification.Status?.InReplyToId;
            if (statusIdToBoost is not null)
            {
                var statusToBoost = await _client.GetStatus(statusIdToBoost);
                if (statusToBoost.IsReply()
                    || statusToBoost.Account.Bot == true
                    || statusToBoost.Account.Id == _currentUser!.Id
                    || statusToBoost.Reblogged == true)
                {
                    await _client.PublishStatus("That's nothing I can boost. 😔",
                        replyStatusId: notification.Status?.Id);
                    _logger.LogInformation("Denied boost request from @{Account} from {PostTime}",
                        notification.Account.AccountName, notification.Status?.CreatedAt);
                }
                else
                {
                    await _client.Reblog(statusToBoost.Id);
                    _logger.LogInformation
                    ("Boosted post from @{Account} from {PostTime}, requested by @{RequesterAccount} at {RequestTime}",
                        statusToBoost.Account.AccountName,
                        statusToBoost.CreatedAt,
                        notification.Account.AccountName,
                        notification.Status?.CreatedAt);
                }
            }
        }
    }

    private async Task BoostDirectMention(Notification notification)
    {
        var statusId = notification.Status?.Id;
        if (statusId is null)
        {
            _logger.LogError(
                "Could not determine ID of status that mentioned me, ignoring post by @{Account} from {PostTime}",
                notification.Status?.Account.AccountName,
                notification.Status?.CreatedAt);
            return;
        }

        var statusVisibility = notification.Status?.Visibility;
        if (statusVisibility is null)
        {
            _logger.LogError(
                "Could not determine visibility of status that mentioned me, ignoring post by @{Account} from {PostTime}",
                notification.Status?.Account.AccountName,
                notification.Status?.CreatedAt);
            return;
        }

        if (statusVisibility == Visibility.Direct)
        {
            _logger.LogInformation(
                "Ignoring direct message post by @{Account} from {PostTime}",
                notification.Status?.Account.AccountName,
                notification.Status?.CreatedAt);
            return;
        }

        await _client.Reblog(statusId);
        _logger.LogInformation("Boosted post that mentioned me by @{Account} from {PostTime}",
            notification.Account.AccountName,
            notification.Status?.CreatedAt);
    }

    private async Task BoostTags()
    {
        var followedTags = await _client.ViewFollowedTags();
        if (!followedTags.Any()) return;

        foreach (var status in (await _client.GetHomeTimeline(new ArrayOptions { Limit = 100 })).Where(s =>
                     !s.IsReply()
                     && s.Reblogged != true
                     && s.Reblog is null
                     && s.Account.Bot != true
                     && s.Account.Id != _currentUser!.Id))
        {
            var followedTagsInPost = status.Tags.Select(t => t.Name)
                .Intersect(followedTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();

            await _client.Reblog(status.Id);
            _logger.LogInformation(
                "Boosted hash-tagged post by @{Account} from {PostTime} because of followed hashtags {FollowedTagsInPost}",
                status.Account.AccountName,
                status.CreatedAt,
                followedTagsInPost);
        }
    }
}