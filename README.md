# dotnet.social Mastodon bot

This is the code for the Mastodon bot [@bot@dotnet.social](https://dotnet.social/@bot).
Its goal ist to help grow and connect the .NET community on Mastodon by boosting posts that talk about .NET.

The code is generic and could be used for any kind of bot that should boost posts, not just about .NET.

The bot runs as an Azure function and boosts posts

- where the bot was @ mentioned,
- that contain at least one # hashtag that the bot follows,
- that has a boost request reply, i.e. a reply with the message `boost this!` ([or similar](https://github.com/bitbonk/DotNetSocialBot/blob/d889186a212a02ca7b41d0ee955bf077323897f9/DotNetSocialBot.FunctionApp/Config.cs#L5))

Posts are generally only boosted if the post to boost

- does not come from this bot account itself,
- does not come from any account that is marked as a bot,
- is not a direct message,
- is not a reply

## Contribution

I am aware that this little experiment is a rather naive implementation.
Any kind of contribution to make this bot better is welcome. 

## Thanks

- [Guillaume Lacasa](https://github.com/glacasa) ([@glacasa@mamot.fr](https://mamot.fr/@glacasa) on Mastodon) for building and maintaining [Mastonet](https://github.com/glacasa/mastonet)
