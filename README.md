## Features

* Creates a secure link between an in game player and a Discord user in your Discord server.
* This link can be accessed by any Discord Link compatible plugin.
* Add/Remove Oxide Group / Discord Role on link / unlink

**Note:** This plugin only performs discord link functionality and should not be loaded otherwise.

## Discord Link
This plugin supports Discord Link provided by the Discord Extension.
All plugins that support Discord Link can use this plugin as the discord linking plugin.

## Permissions

* `discordcore.use` - Allows players to use the `/dc` chat command

## Linking

**Note:**
* In order for the bot to work the user who is trying to link must be in the Discord server with the Discord bot.

### Starting In-Game

* Option 1: Type /dc join code in game and write the command in a private message to the bot
* Option 2: Type /dc join username#discrimiator and respond to the private message to the bot

### Starting in Discord

* Option 1: Type /dc join and paste the command in game chat.
* Option 2: If enabled click on the link button in the guild channel and paste the command in game chat.

## Player Commands

### Game Server

`/dc code` - to start the link process using a code  
`/dc user username` - to start the link process by discord username  
`/dc user userid` - to start the link process by discord userid  
`/dc leave` - to to to unlink yourself from discord  
`/dc` to see this message again

### Discord Server

`/dc code` - to start the link process using a code  
`/dc user` - to start the link process by steam name  
`/dc leave` - to to to unlink yourself from discord

## Admin Commands

### Discord Server

`/dca link` - admin link player game account and Discord user  
`/dca unlink` - admin unlink player game account and Discord user  
`/dca search player` - search by server player  
`/dca search user` - search by Discord user  
`/dca unban player` - unban a linked banned player  
`/dca unban user` - unban a linked banned Discord user


## Getting Your Bot Token
[Click Here to learn how to get an Discord Bot Token](https://umod.org/extensions/discord#getting-your-api-key)

## Configuration

```json
{
  "Discord Bot Token": "",
  "Discord Server ID (Optional if bot only in 1 guild)": "",
  "Discord Server Name Override": "",
  "Discord Server Invite Url": "",
  "Link Settings": {
    "Announcement Channel Id": "",
    "Link Code Generator Characters": "123456789",
    "Link Code Generator Length": 6,
    "Automatically Relink A Player If They Leave And Rejoin The Discord Server": true,
    "Inactive Settings": {
      "Automatically Unlink Inactive Players": false,
      "Player Considered Inactive After X (Days)": 90.0,
      "Automatically Relink Inactive Players On Game Server Join": true
    }
  },
  "Welcome Message Settings": {
    "Enable Welcome DM Message": true,
    "Send Welcome Message On Discord Server Join": false,
    "Send Welcome Message On Role ID Added": [
      "559471108240310292"
    ],
    "Add Link Accounts Button In Welcome Message": true
  },
  "Guild Link Message Settings": {
    "Enable Guild Link Message": true,
    "Message Channel ID": "824633181336371250"
  },
  "Link Permission Settings": {
    "On Link Server Permissions To Add": [],
    "On Unlink Server Permissions To Remove": [],
    "On Link Server Groups To Add": [
      "Linked"
    ],
    "On Unlink Server Groups To Remove": [
      "Linked"
    ],
    "On Link Discord Roles To Add": [
      "759424466119032832"
    ],
    "On Unlink Discord Roles To Remove": [
      "759424466119032832"
    ]
  },
  "Link Ban Settings": {
    "Enable Link Ban": true,
    "Ban Announcement Channel ID": "564966011292614667",
    "Ban Link After X Join Declines": 3,
    "Ban Duration (Hours)": 24
  },
  "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)": "Info"
}
```

### Guild Link Message
Guild link message will place a message in a guild discord channel with a button users can use to start the linking process.
The players will be sent a message in the same channel that only they can see and isn't displayed to any other users.

![](https://i.postimg.cc/fbky50pw/link-example.png)

## Localization
```json
{
  "Format": "[#CCCCCC][[#DE8732]Discord Core[/#]] {0}[/#]",
  "NoPermission": "You do not have permission to use this command",
  "Commands.DcCommand": "dc",
  "Commands.CodeCommand": "code",
  "Commands.UserCommand": "user",
  "Commands.LeaveCommand": "leave",
  "Commands.AcceptCommand": "accept",
  "Commands.DeclineCommand": "decline",
  "Commands.Code.LinkInfo": "To complete your activation please open Discord use the following command: [#DE8732]/{plugin.lang:Discord.DiscordCommand} {plugin.lang:Discord.LinkCommand} {discordcore.link.code}[/#].\n",
  "Commands.Code.LinkServer": "In order to use this command you must be in the [#DE8732]{guild.name}[/#] discord server. You can join @ [#DE8732]discord.gg/{discordcore.invite.code}[/#].\n",
  "Commands.Code.LinkInGuild": "This command can be used in the following guild channels {discordcore.command.channels}.\n",
  "Commands.Code.LinkInDm": "This command can be used in the following in a direct message to [#DE8732]{user.fullname}[/#] bot",
  "Commands.User.MatchFound": "We found a match by username. We have a sent a discord message to [#DE8732]{user.fullname}[/#] to complete the link.\nIf you haven't received a message make sure you allow DM's from [#DE8732]{bot.fullname}[/#].",
  "Commands.User.Errors.InvalidSyntax": "Invalid User Join Syntax.\n[#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.UserCommand} username[/#] to start the link process by your discord username\n[#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.UserCommand} userid[/#] to start the link process by your discord user ID",
  "Commands.User.Errors.UserIdNotFound": "Failed to find a discord user in the [#DE8732]{guild.name}[/#] Discord server with user ID [#F04747]{snowflake.id}[/#]",
  "Commands.User.Errors.UserNotFound": "Failed to find a any discord users in the [#DE8732]{guild.name}[/#] Discord server with the username [#F04747]{discordcore.notfound}[/#]",
  "Commands.User.Errors.MultipleUsersFound": "Multiple discord users found in the the [#DE8732]{guild.name}[/#] Discord server matching [#F04747]{discordcore.notfound}[/#]. Please include more of the username and/or the discriminator in your search.",
  "Commands.User.Errors.SearchError": "An error occured while trying to search by username. Please try a different username or try again later. If the issue persists please notify an admin.",
  "Commands.Leave.Errors.NotLinked": "We were unable to unlink your account as you do not appear to have been linked.",
  "Announcements.Link.Command": "[#DE8732]{player.name}[/#] has successfully linked their game account with their discord user [#DE8732]{user.fullname}[/#]. If you would would like to be linked type /{plugin.lang:Commands.DcCommand} to learn more.",
  "Announcements.Link.Admin": "[#DE8732]{player.name}[/#] has successfully been linked by an admin to discord user [#DE8732]{user.fullname}[/#].",
  "Announcements.Link.Api": "[#DE8732]{player.name}[/#] has successfully linked their game account with their discord user [#DE8732]{user.fullname}[/#]. If you would would like to be linked type /{plugin.lang:Commands.DcCommand} to learn more.",
  "Announcements.Link.GuildRejoin": "[#DE8732]{player.name}[/#] has been relinked with discord user [#DE8732]{user.fullname}[/#] for rejoining the [#DE8732]{guild.name}[/#] discord server",
  "Announcements.Link.InactiveRejoin": "[#DE8732]{player.name}[/#] has been relinked with discord user [#DE8732]{user.fullname}[/#] for rejoining the [#DE8732]{server.name}[/#] game server",
  "Announcements.Unlink.Command": "[#DE8732]{player.name}[/#] has successfully unlinked their game account from their discord user [#DE8732]{user.fullname}[/#].",
  "Announcements.Unlink.Admin": "[#DE8732]{player.name}[/#] has successfully been unlinked by an admin from discord user [#DE8732]{user.fullname}[/#].",
  "Announcements.Unlink.Api": "[#DE8732]{player.name}[/#] has successfully unlinked their game account from their discord user [#DE8732]{user.fullname}[/#].",
  "Announcements.Unlink.LeftGuild": "[#DE8732]{player.name}[/#] has been unlinked from discord user [#DE8732]{user.fullname}[/#] they left the [#DE8732]{guild.name}[/#] Discord server",
  "Announcements.Unlink.Inactive": "[#DE8732]{player.name}[/#] has been unlinked from discord user [#DE8732]{user.fullname}[/#] because they haven't been active on [#DE8732]{server.name}[/#] game server for [#DE8732]{timespan.total.days}[/#] days",
  "Link.Completed.Command": "You have successfully linked your player [#DE8732]{player.name}[/#] with discord user [#DE8732]{user.fullname}[/#]",
  "Link.Completed.Admin": "You have been successfully linked by an admin with player [#DE8732]{player.name}[/#] and discord user [#DE8732]{user.fullname}[/#]",
  "Link.Completed.Api": "You have successfully linked your player [#DE8732]{player.name}[/#] with discord user [#DE8732]{user.fullname}[/#]",
  "Link.Completed.GuildRejoin": "Your player [#DE8732]{player.name}[/#] has been relinked with discord user [#DE8732]{user.fullname}[/#] because rejoined the [#DE8732]{guild.name}[/#] Discord server",
  "Link.Completed.InactiveRejoin": "Your player [#DE8732]{player.name}[/#] has been relinked with discord user [#DE8732]{user.fullname}[/#] because rejoined [#DE8732]{server.name}[/#] server",
  "Unlink.Completed.Command": "You have successfully unlinked your player [#DE8732]{player.name}[/#] from discord user [#DE8732]{user.fullname}[/#]",
  "Unlink.Completed.Admin": "You have been successfully unlinked by an admin from discord user [#DE8732]{user.fullname}[/#]",
  "Unlink.Completed.Api": "You have successfully unlinked your player [#DE8732]{player.name}[/#] from discord user [#DE8732]{user.fullname}[/#]",
  "Unlink.Completed.LeftGuild": "Your player [#DE8732]{player.name}[/#] has been unlinked from discord user [#DE8732]{user.fullname}[/#] because you left the [#DE8732]{guild.name}[/#] Discord server",
  "Link.Declined.JoinWithPlayer": "We have declined the discord link between [#DE8732]{player.name}[/#] and [#DE8732]{user.fullname}[/#]",
  "Link.Declined.JoinWithUser": "[#DE8732]{user.fullname}[/#] has declined your link to [#DE8732]{player.name}[/#]",
  "Link.Errors.InvalidSyntax": "Invalid Link Syntax. Please type the command you were given in Discord. Command should be in the following format:[#DE8732]/{plugin.lang:Commands.DcCommand} {discordcore.server.link.arg} {code}[/#] where {code} is the code sent to you in Discord.",
  "Banned.IsUserBanned": "You have been banned from joining by Discord user due to multiple declined join attempts. Your ban will end in {timespan.days} days {timespan.hours} hours {timespan.minutes} minutes {timespan.seconds} Seconds.",
  "Join.ByPlayer": "[#DE8732]{user.fullname}[/#] is trying to link their Discord account with your game account. If you wish to [#43B581]accept[/#] this link please type [#43B581]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.AcceptCommand}[/#]. If you wish to [#F04747]decline[/#] this link please type [#F04747]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.DeclineCommand}[/#]",
  "Discord.DiscordCommand": "dc",
  "Discord.LinkCommand": "link",
  "Join.Errors.PlayerJoinActivationNotFound": "There are no pending joins in progress for this game account. Please start the link in Discord and try again.",
  "Errors.PlayerAlreadyLinked": "This player is already linked to Discord user [#DE8732]{user.fullname}[/#]. If you wish to link yourself to another account please type [#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.LeaveCommand}[/#]",
  "Errors.DiscordAlreadyLinked": "This Discord user is already linked to player [#DE8732]{player.name}[/#]. If you wish to link yourself to another account please type [#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.LeaveCommand}[/#]",
  "Errors.ActivationNotFound": "We failed to find any pending joins with code [#DE8732]/{plugin.lang:Commands.DcCommand}[/#]. Please verify the code is correct and try again.",
  "Errors.MustBeCompletedInDiscord": "You need to complete the steps provided in Discord since you started the link from the game server.",
  "Errors.ConsolePlayerNotSupported": "This command cannot be ran in the server console. ",
  "Commands.HelpMessage": "Allows players to link their player and discord accounts together. Players must first join the [#DE8732]{guild.name}[/#] Discord @ [#DE8732]discord.gg/{discordcore.invite.code}[/#]\n[#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.CodeCommand}[/#] to start the link process using a code\n[#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.UserCommand} username[/#] to start the link process by your discord username\n[#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.UserCommand} userid[/#] to start the link process by your discord user ID\n[#DE8732]/{plugin.lang:Commands.DcCommand} {plugin.lang:Commands.LeaveCommand}[/#] to to unlink yourself from discord\n[#DE8732]/{plugin.lang:Commands.DcCommand}[/#] to see this message again",
  "Commands.LinkCommand": "link"
}
```