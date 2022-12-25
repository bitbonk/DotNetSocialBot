namespace DotNetSocialBot.FunctionApp;

internal static class Config
{
    public static readonly string[] ValidBoostRequestMessages =
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

    public static string AccessToken =>
        Environment.GetEnvironmentVariable("access_token", EnvironmentVariableTarget.Process) ??
        throw new InvalidOperationException("access_token token not present in environment variables");

    public static string Instance =>
        Environment.GetEnvironmentVariable("instance", EnvironmentVariableTarget.Process) ??
        throw new InvalidOperationException("instance not present in environment variables");
}
