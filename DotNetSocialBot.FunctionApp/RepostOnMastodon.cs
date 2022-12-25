using HtmlAgilityPack;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DotNetSocialBot.FunctionApp;

public class RepostOnMastodon
{
    private readonly MastodonClient _client;
    private Account? _currentUser;

    public RepostOnMastodon()
    {
        var handler = new HttpClientHandler();
#if DEBUG // TODO: find out why certs are not accepted locally
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
        _client = new MastodonClient(Config.Instance, Config.AccessToken, new HttpClient(handler));
    }


    [FunctionName("RepostOnMastodon")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
    {
        _currentUser = await _client.GetCurrentUser();
        await HandleNotifications(log);
        await BoostTags(log);
    }

    private async Task HandleNotifications(ILogger log)
    {
        var notifications = await _client
            .GetNotifications(excludeTypes: NotificationType.Follow | NotificationType.Favourite |
                                            NotificationType.Reblog);

        try
        {
            foreach (var notification in notifications
                         .Where(n => n.Status?.Account?.Bot != true))
                if (!notification.Status.IsReply())
                    await BoostDirectMention(log, notification);
                else
                    await BoostBoostRequest(log, notification);
        }
        finally
        {
            await _client.ClearNotifications();
            log.LogInformation("Cleared all notifications");
        }
    }

    private async Task BoostBoostRequest(ILogger log, Notification notification)
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
                    log.LogInformation("Denied boost request from @{Account} from {PostTime}",
                        notification.Account.AccountName, notification.Status?.CreatedAt);
                }
                else
                {
                    await _client.Reblog(statusToBoost.Id);
                    log.LogInformation
                    ("Boosted post from @{Account} from {PostTime}, requested by @{RequesterAccount} at {RequestTime}",
                        statusToBoost.Account.AccountName,
                        statusToBoost.CreatedAt,
                        notification.Account.AccountName,
                        notification.Status?.CreatedAt);
                }
            }
        }
    }

    private async Task BoostDirectMention(ILogger log, Notification notification)
    {
        var statusId = notification.Status?.Id;
        if (statusId is null)
        {
            log.LogError(
                "Could not determine ID of status that mentioned me, ignoring post by @{Account} from {PostTime}",
                notification.Status?.Account.AccountName,
                notification.Status?.CreatedAt);
            return;
        }

        var statusVisibility = notification.Status?.Visibility;
        if (statusVisibility is null)
        {
            log.LogError(
                "Could not determine visibility of status that mentioned me, ignoring post by @{Account} from {PostTime}",
                notification.Status?.Account.AccountName,
                notification.Status?.CreatedAt);
            return;
        }

        if (statusVisibility == Visibility.Direct)
        {
            log.LogInformation(
                "Ignoring direct message post by @{Account} from {PostTime}",
                notification.Status?.Account.AccountName,
                notification.Status?.CreatedAt);
            return;
        }

        await _client.Reblog(statusId);
        log.LogInformation("Boosted post that mentioned me by @{Account} from {PostTime}",
            notification.Account.AccountName,
            notification.Status?.CreatedAt);
    }

    private async Task BoostTags(ILogger log)
    {
        var followedTags = await _client.ViewFollowedTags();
        if (!followedTags.Any()) return;

        foreach (var status in (await _client.GetHomeTimeline(new ArrayOptions {Limit = 100})).Where(s =>
                     !s.IsReply()
                     && s.Reblogged != true
                     && s.Reblog is null
                     && s.Account.Bot != true
                     && s.Account.Id != _currentUser!.Id))
        {
            var followedTagsInPost = status.Tags.Select(t => t.Name)
                .Intersect(followedTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();

            await _client.Reblog(status.Id);
            log.LogInformation(
                "Boosted hashtagged post by @{Account} from {PostTime} because of followed hashtags {FollowedTagsInPost}",
                status.Account.AccountName,
                status.CreatedAt,
                followedTagsInPost);
        }
    }
}
