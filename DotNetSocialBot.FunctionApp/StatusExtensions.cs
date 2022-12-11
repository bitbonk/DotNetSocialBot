using Mastonet.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSocialBot.FunctionApp
{
    public static class StatusExtensions
    {
        public static bool IsReply(this Status status)
        {
            return status.InReplyToId != null;
        }

        // TODO: Get "my" AccountName instead of hard coding it
        public static bool IsFromMe(this Status status)
        {
            return status.Account.AccountName.Equals("bot@dotnet.social", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
