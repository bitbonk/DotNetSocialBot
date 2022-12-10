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
        this.client = new MastodonClient(
            new AppRegistration { Instance = Config.Instance },
            new Auth { AccessToken = Config.AccessToken },
            new HttpClient(handler));
    }


    [FunctionName("RepostOnMastodon")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
    {
        var notifications = await client
            .GetNotifications(excludeTypes: NotificationType.Follow | NotificationType.Favourite | NotificationType.Reblog);
        foreach (var notification in notifications
            .Where(n => !n.Status.IsReply() && n.Status.Account.Bot != true))
        {
            await client.Reblog(notification.Status.Id);
            log.LogInformation("Boosted post by @{Account} from {PostTime}", notification.Account.AccountName, notification.Status.CreatedAt);

        }

        await client.ClearNotifications();
        log.LogInformation("Cleared all notifications");
    }
}
