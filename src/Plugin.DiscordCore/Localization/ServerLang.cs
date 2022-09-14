namespace DiscordCorePlugin.Localization
{
    public static class ServerLang
    {
        private const string Base = "Chat.";
        
        public const string Format = Base + nameof(Format);
        public const string NoPermission = Base + nameof(NoPermission);

        public static class Announcements
        {
            private const string Base = ServerLang.Base + nameof(Announcements) + ".";

            public static class Link
            {
                private const string Base = Announcements.Base + nameof(Link) + ".";
                public const string Command = Base + nameof(Command);
                public const string Admin = Base + nameof(Admin);
                public const string Api = Base + nameof(Api);
                public const string GuildRejoin = Base + nameof(GuildRejoin);
                public const string InactiveRejoin = Base + nameof(InactiveRejoin);
            }

            public static class Unlink
            {
                private const string Base = Announcements.Base + nameof(Unlink) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Admin = Base + nameof(Admin);
                public const string Api = Base + nameof(Api);
                public const string LeftGuild = Base + nameof(LeftGuild);
                public const string Inactive = Base + nameof(Inactive);
            }
        }
        
        public static class Commands
        {
            private const string Base = ServerLang.Base + nameof(Commands) + ".";
            
            public const string DcCommand = Base + nameof(DcCommand);
            public const string CodeCommand = Base + nameof(CodeCommand);
            public const string UserCommand = Base + nameof(UserCommand);
            public const string LeaveCommand = Base + nameof(LeaveCommand);
            public const string AcceptCommand = Base + nameof(AcceptCommand);
            public const string DeclineCommand = Base + nameof(DeclineCommand);
            public const string HelpMessage = Base + nameof(HelpMessage);

            public static class Code
            {
                private const string Base = Commands.Base + nameof(Code) + ".";
                
                public const string LinkInfo = Base + nameof(LinkInfo);
                public const string LinkServer = Base + nameof(LinkServer);
                public const string LinkInGuild = Base + nameof(LinkInGuild);
                public const string LinkInDm = Base + nameof(LinkInDm);
            }

            public static class User
            {
                private const string Base = Commands.Base + nameof(User) + ".";

                public static class Errors
                {
                    private const string Base = User.Base + nameof(Errors) + ".";
                    
                    public const string InvalidSyntax = Base + nameof(InvalidSyntax);
                    public const string UserIdNotFound = Base + nameof(UserIdNotFound);
                    public const string UserNotFound = Base + nameof(UserNotFound);
                    public const string MultipleUsersFound = Base + nameof(MultipleUsersFound);
                    public const string SearchError = Base + nameof(SearchError);
                }
            }

            public static class Leave
            {
                private const string Base = Commands.Base + nameof(Leave) + ".";

                public static class Errors
                {
                    private const string Base = Leave.Base + nameof(Errors) + ".";
                    
                    public const string NotLinked = Base + nameof(NotLinked);
                }
            }
        }
        
        public static class Link
        {
            private const string Base = ServerLang.Base + nameof(Link) + ".";
            public static class Completed
            {
                private const string Base = Link.Base + nameof(Completed) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string Admin = Base + nameof(Admin);
                public const string Api = Base + nameof(Api);
                public const string GuildRejoin = Base + nameof(GuildRejoin);
                public const string InactiveRejoin = Base + nameof(InactiveRejoin);
            }
            
            public static class Declined
            {
                private const string Base = Link.Base + nameof(Declined) + ".";
                
                public const string JoinWithPlayer = Base + nameof(JoinWithPlayer);
                public const string JoinWithUser = Base + nameof(JoinWithUser);
            }
            
            public static class Errors
            {
                private const string Base = Link.Base + nameof(Errors) + ".";
                    
                public const string InvalidSyntax = Base + nameof(InvalidSyntax);
            }
        }

        public static class Unlink
        {
            private const string Base = ServerLang.Base + nameof(Unlink) + ".";

            public static class Completed
            {
                private const string Base = Unlink.Base + nameof(Completed) + ".";
                
                public const string Command = Base + nameof(Command);
                public const string LeftGuild = Base + nameof(LeftGuild);
                public const string Admin = Base + nameof(Admin);
                public const string Api = Base + nameof(Api);
            }
        }

        public static class Banned
        {
            private const string Base = ServerLang.Base + nameof(Banned) + ".";

            public const string IsUserBanned = Base + nameof(IsUserBanned);
        }
        
        public static class Join
        {
            private const string Base = ServerLang.Base + nameof(Join) + ".";
            
            public const string ByPlayer = Base + nameof(ByPlayer);

            public static class Errors
            {
                private const string Base = Join.Base + nameof(Errors) + ".";

                public const string PlayerJoinActivationNotFound = Base + nameof(PlayerJoinActivationNotFound);
            }
        }

        public static class Errors
        {
            private const string Base = ServerLang.Base + nameof(Errors) + ".";

            public const string PlayerAlreadyLinked = Base + nameof(PlayerAlreadyLinked);
            public const string DiscordAlreadyLinked = Base + nameof(DiscordAlreadyLinked);
            public const string ActivationNotFound = Base + nameof(ActivationNotFound);
            public const string MustBeCompletedInDiscord = Base + nameof(MustBeCompletedInDiscord);
            public const string ConsolePlayerNotSupported = Base + nameof(ConsolePlayerNotSupported);
        }
    }
}