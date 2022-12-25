using Mastonet.Entities;

namespace DotNetSocialBot.FunctionApp;

public static class StatusExtensions
{
    public static bool IsReply(this Status? status)
    {
        return status?.InReplyToId != null;
    }
}
