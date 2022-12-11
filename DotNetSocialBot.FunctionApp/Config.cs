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

        public static readonly string[] ValidBoostRequestMessages = new string[]
        {
            "bot boost this", 
            "bot boost that", 
            "bot boost it", 
            "bot boost",
            "bot boost this please",
            "bot boost that please",
            "bot boost it please",
            "bot boost please",
            "bot boost this, please",
            "bot boost that, please",
            "bot boost it, please",
            "bot boost, please",
            "bot boost this!",
            "bot boost that!",
            "bot boost it!",
            "bot boost!",
            "bot boost this please!",
            "bot boost that please!",
            "bot boost it please!",
            "bot boost please!",
            "bot boost this, please!",
            "bot boost that, please!",
            "bot boost it, please!",
            "bot boost, please!",
            "bot boost this ☝️",
            "bot boost that ☝️",
            "bot boost it ☝️",
            "bot boost ☝️",
            "bot boost this please ☝️",
            "bot boost that please ☝️",
            "bot boost it please ☝️",
            "bot boost please ☝️",
            "bot boost this, please ☝️",
            "bot boost that, please ☝️",
            "bot boost it, please ☝️",
            "bot boost, please ☝️",
            "bot boost this! ☝️",
            "bot boost that! ☝️",
            "bot boost it! ☝️",
            "bot boost! ☝️",
            "bot boost this please! ☝️",
            "bot boost that please! ☝️",
            "bot boost it please! ☝️",
            "bot boost please! ☝️",
            "bot boost this, please! ☝️",
            "bot boost that, please! ☝️",
            "bot boost it, please! ☝️",
            "bot boost, please! ☝️"
        };
    }
}
