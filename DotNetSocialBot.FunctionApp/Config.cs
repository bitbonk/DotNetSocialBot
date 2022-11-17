using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSocialBot.FunctionApp
{
    internal static class Config
    {
        public static string AccessToken  => Environment.GetEnvironmentVariable("access_token", EnvironmentVariableTarget.Process);

        public static string Instance => Environment.GetEnvironmentVariable("instance", EnvironmentVariableTarget.Process);
    }
}
