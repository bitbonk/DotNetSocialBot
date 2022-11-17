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
        public static bool IsReply(this Status n)
        {
            return n.InReplyToId != null;
        }
    }
}
