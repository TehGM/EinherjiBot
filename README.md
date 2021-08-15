# Einherji Bot
[![GitHub top language](https://img.shields.io/github/languages/top/TehGM/EinherjiBot)](https://github.com/TehGM/EinherjiBot) [![GitHub](https://img.shields.io/github/license/TehGM/EinherjiBot)](LICENSE) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/TehGM/EinherjiBot/.NET%20Core%20Build)](https://github.com/TehGM/EinherjiBot/actions) [![GitHub issues](https://img.shields.io/github/issues/TehGM/EinherjiBot)](https://github.com/TehGM/EinherjiBot/issues)

Einherji is my private Discord administration bot. It mainly contains features that are useful to me or my friends.  
Einherji is built using .NET Core Hosting approach, making use of its Dependency Injection container, Logging abstractions and configuration loading.

Uses [Discord.Net](https://github.com/discord-net/Discord.Net) for connection and MongoDB (using [C# MongoDB Driver](https://docs.mongodb.com/drivers/csharp)) for storage.
The bot contains a custom Regex Commands System built on top of [Discord.Net](https://discord.foxbot.me/stable/guides/commands/intro.html)'s default command system. Its implementation can be found in [CommandsProcessing](https://github.com/TehGM/EinherjiBot/tree/master/EinherjiBot.Core/CommandsProcessing) directory.

The bot is split into 2 projects:
- EinherjiBot.General - contains bot's main features.
- EinherjiBot.Core - contains extensions and core abstractions.

## Running locally
> Note: requires [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) since version 2.3.0.

1. Clone this repository to get all files.
2. Set up MongoDB database with following collections: `Miscellaneous`, `StellarisMods`, `PatchbotGames`, `EliteCommunityGoals` and `UsersData`.
3. Create `appsecrets.json` file. See [example file](appsecrets-example.json) for example structure.  
This file will hold secrets, so it should not be included in source control repository. `.gitignore` file included with this repo will ignore `appsecrets.json` and `appsecrets.*.json` files.
4. Add bot token from Discord Developer Portal to secrets file.
5. Add MongoDB connection string for your DB to secrets file.
6. *(optional)* If using DataDog for logs, create a following section in `appsecrets.json`, replacying `<api-key>` with your DataDog application API key:  
```json
"Serilog": {
  "DataDog": {
    "ApiKey": "<api-key>"
  }
}
```
7. *(optional)* If you're registered with Inara API, create a following section in `appsecrets.json`, replacing all tags as needed. Note: not prividing these values will disable Elite Dangerous Community Goals checker feature.
```json
"EliteCommunityGoals": {
  "InaraAppInDevelopment": true,
  "InaraAppName": "<app-name>",
  "InaraApiKey": "<api-key>"
}
```
8. Update [appsettings.json](EinherjiBot.General/appsettings.json). At very least, all channel, user and role IDs will need changing to match values in your Discord guild.
9. Build and run `EinherjiBot.General` project.

## Development
This bot is under continuous (if sometimes slow) development. Breaking changes might be introduced at any time.

If you spot a bug or want to suggest a feature or improvement, feel free to open a new [Issue](https://github.com/TehGM/EinherjiBot/issues).

## License
Copyright (c) 2020 TehGM

Licensed under [Apache License 2.0](LICENSE).
