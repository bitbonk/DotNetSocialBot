using HtmlAgilityPack;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DotNetSocialBot.FunctionApp;

public class RepostOnMastodon
{
    private readonly MastodonClient client;

    public RepostOnMastodon()
    {
        var handler = new HttpClientHandler();
#if DEBUG // TODO: find out why certs are not accepted locally
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
        client = new MastodonClient(Config.Instance, Config.AccessToken, new HttpClient(handler));
    }


    [FunctionName("RepostOnMastodon")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
    {
        await BoostMentions(log);
        await BoostTags(log);
    }

    private async Task BoostTags(ILogger log)
    {
        // TODO: Get the tags from the followed tags, instead of hardcoding "dotnet"
        foreach (var status in (await client.GetHomeTimeline(new ArrayOptions { Limit = 100 })).Where(s =>
                     !s.IsReply()
                     && s.Reblogged != true
                     && s.Reblog is null
                     && s.Account.Bot != true
                     && !s.IsFromMe()
                     && s.Tags.Any(t =>
                         t.Name.Equals("dotnet", StringComparison.InvariantCultureIgnoreCase))))
        {
            await client.Reblog(status.Id);
            log.LogInformation("Boosted post with hashtag by @{Account} from {PostTime}", status.Account.AccountName,
                status.CreatedAt);

        }
    }

    private async Task BoostMentions(ILogger log)
    {
        var notifications = await client
            .GetNotifications(excludeTypes: NotificationType.Follow | NotificationType.Favourite |
                                            NotificationType.Reblog);
        foreach (var notification in notifications
                     .Where(n => n.Status.Account.Bot != true))
        {
            if (!notification.Status.IsReply())
            {
                await client.Reblog(notification.Status.Id);
                log.LogInformation("Boosted post that mentioned me by @{Account} from {PostTime}", notification.Account.AccountName,
                    notification.Status.CreatedAt);
            }
            else
            {
                var document = new HtmlDocument();
                document.LoadHtml(notification.Status.Content);
                var replyText = document.DocumentNode.InnerText;
                if (Config.ValidBoostRequestMessages.Any(m => replyText.EndsWith(m, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var statusToBoost = await client.GetStatus(notification.Status.InReplyToId);
                    if (statusToBoost.IsReply() || statusToBoost.Account.Bot == true || statusToBoost.IsFromMe() || statusToBoost.Reblogged == true)
                    {
                        await client.PublishStatus("That's nothing I can boost. 😔", replyStatusId: notification.Status.Id);
                        log.LogInformation("Denied boost request from @{Account} from {PostTime}", notification.Account.AccountName, notification.Status.CreatedAt);
                    }
                    else
                    {
                        await client.Reblog(statusToBoost.Id);
                        log.LogInformation
                            ("Boosted post from @{Account} from {PostTime}, requested by @{RequesterAccount} at {RequestTime}",
                            statusToBoost.Account.AccountName,
                            statusToBoost.CreatedAt,
                            notification.Account.AccountName,
                            notification.Status.CreatedAt);
                    }
                }
            }

        }

        await client.ClearNotifications();
        log.LogInformation("Cleared all notifications");
    }


}
