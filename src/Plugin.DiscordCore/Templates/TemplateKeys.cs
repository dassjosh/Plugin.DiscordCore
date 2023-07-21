namespace DiscordCorePlugin.Templates
{
    public static class TemplateKeys
    {
        public static class Announcements
        {
            private const string Base = nameof(Announcements) + ".";

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

        public static class WelcomeMessage
        {
            private const string Base = nameof(WelcomeMessage) + ".";
            
            public const string PmWelcomeMessage = Base + nameof(PmWelcomeMessage);
            public const string GuildWelcomeMessage = Base + nameof(GuildWelcomeMessage);

            public static class Error
            {
                private const string Base = WelcomeMessage.Base + nameof(Error) + ".";
                
                public const string AlreadyLinked = Base + nameof(AlreadyLinked);
            }
        }
        
        public static class Commands
        {
            private const string Base = nameof(Commands) + ".";

            public static class Code
            {
                private const string Base = Commands.Base + nameof(Code) + ".";
                
                public const string Success = Base + nameof(Success);
            }

            public static class User
            {
                private const string Base = Commands.Base + nameof(User) + ".";
                
                public const string Success = Base + nameof(Success);

                public static class Error
                {
                    private const string Base = User.Base + nameof(Error) + ".";
                    
                    public const string PlayerIsInvalid = Base + nameof(PlayerIsInvalid);
                    public const string PlayerNotConnected = Base + nameof(PlayerNotConnected);
                }
            }

            public static class Leave
            {
                private const string Base = Commands.Base + nameof(Leave) + ".";
                
                public static class Error
                {
                    private const string Base = Leave.Base + nameof(Error) + ".";
                    
                    public const string UserNotLinked = Base + nameof(UserNotLinked);
                }
            }
            
            public static class Admin
            {
                private const string Base = Commands.Base + nameof(Admin) + ".";

                public static class Link
                {
                    private const string Base = Admin.Base + nameof(Link) + ".";
                    
                    public const string Success = Base + nameof(Success);

                    public static class Error
                    {
                        private const string Base = Link.Base + nameof(Error) + ".";

                        public const string PlayerNotFound = Base + nameof(PlayerNotFound);
                        public const string PlayerAlreadyLinked = Base + nameof(PlayerAlreadyLinked);
                        public const string UserAlreadyLinked = Base + nameof(UserAlreadyLinked);
                    }
                }

                public static class Unlink
                {
                    private const string Base = Admin.Base + nameof(Unlink) + ".";

                    public const string Success = Base + nameof(Success);

                    public static class Error
                    {
                        private const string Base = Unlink.Base + nameof(Error) + ".";
                        
                        public const string MustSpecifyOne = Base + nameof(MustSpecifyOne);
                        public const string PlayerIsNotLinked = Base + nameof(PlayerIsNotLinked);
                        public const string UserIsNotLinked = Base + nameof(UserIsNotLinked);
                        public const string LinkNotSame = Base + nameof(LinkNotSame);
                    }
                }

                public static class Search
                {
                    private const string Base = Admin.Base + nameof(Search) + ".";

                    public const string Success = Base + nameof(Success);

                    public static class Error
                    {
                        private const string Base = Search.Base + nameof(Error) + ".";
                        
                        public const string PlayerNotFound = Base + nameof(PlayerNotFound);
                    }
                }
                
                
                public static class Unban
                {
                    private const string Base = nameof(Unban) + ".";
            
                    public const string Player = Base + nameof(Player); 
                    public const string User = Base + nameof(User);

                    public static class Error
                    {
                        private const string Base = Unban.Base + nameof(Error) + ".";

                        public const string PlayerNotFound = Base + nameof(PlayerNotFound);
                        public const string PlayerNotBanned = Base + nameof(PlayerNotBanned);
                        public const string UserNotBanned = Base + nameof(UserNotBanned);
                    }
                }
            }
        }

        public static class Link
        {
            private const string Base = nameof(Link) + ".";

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

                public const string JoinWithUser = Base + nameof(JoinWithUser);
                public const string JoinWithPlayer = Base + nameof(JoinWithPlayer);
            }

            public static class WelcomeMessage
            {
                private const string Base = Link.Base + nameof(WelcomeMessage) + ".";

                public const string DmLinkAccounts = Base + nameof(DmLinkAccounts);
                public const string GuildLinkAccounts = Base + nameof(GuildLinkAccounts);
            }
        }

        public static class Unlink
        {
            private const string Base = nameof(Unlink) + ".";

            public static class Completed
            {
                private const string Base = Unlink.Base + nameof(Completed) + ".";
                public const string Command = Base + nameof(Command);
                public const string Admin = Base + nameof(Admin);
                public const string Api = Base + nameof(Api);
                public const string Inactive = Base + nameof(Inactive);
            }
        }
        
        public static class Banned
        {
            private const string Base = nameof(Banned) + ".";

            public const string PlayerBanned = Base + nameof(PlayerBanned);
        }

        public static class Join
        {
            private const string Base = nameof(Join) + ".";
            
            public const string CompleteLink = Base + nameof(CompleteLink);
        }

        public static class Errors
        {
            private const string Base = nameof(Errors) + ".";

            public const string UserAlreadyLinked = Base + nameof(UserAlreadyLinked);
            public const string PlayerAlreadyLinked = Base + nameof(PlayerAlreadyLinked);
            public const string CodActivationNotFound = Base + nameof(CodActivationNotFound);
            public const string LookupActivationNotFound = Base + nameof(LookupActivationNotFound);
        }
    }
}