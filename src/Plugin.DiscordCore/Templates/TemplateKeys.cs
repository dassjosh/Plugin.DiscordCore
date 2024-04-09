using Oxide.Ext.Discord.Libraries;

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
                public static readonly TemplateKey Command = new(Base + nameof(Command));
                public static readonly TemplateKey Admin = new(Base + nameof(Admin));
                public static readonly TemplateKey Api = new(Base + nameof(Api));
                public static readonly TemplateKey GuildRejoin = new(Base + nameof(GuildRejoin));
                public static readonly TemplateKey InactiveRejoin = new(Base + nameof(InactiveRejoin));
            }

            public static class Unlink
            {
                private const string Base = Announcements.Base + nameof(Unlink) + ".";
                
                public static readonly TemplateKey Command = new(Base + nameof(Command));
                public static readonly TemplateKey Admin = new(Base + nameof(Admin));
                public static readonly TemplateKey Api = new(Base + nameof(Api));
                public static readonly TemplateKey LeftGuild = new(Base + nameof(LeftGuild));
                public static readonly TemplateKey Inactive = new(Base + nameof(Inactive));
            }

            public static class Ban
            {
                private const string Base = Announcements.Base + nameof(Ban) + ".";

                public static readonly TemplateKey PlayerBanned = new(Base + nameof(PlayerBanned));
                public static readonly TemplateKey UserBanned = new(Base + nameof(UserBanned));
            }
        }

        public static class WelcomeMessage
        {
            private const string Base = nameof(WelcomeMessage) + ".";
            
            public static readonly TemplateKey PmWelcomeMessage = new(Base + nameof(PmWelcomeMessage));
            public static readonly TemplateKey GuildWelcomeMessage = new(Base + nameof(GuildWelcomeMessage));

            public static class Error
            {
                private const string Base = WelcomeMessage.Base + nameof(Error) + ".";
                
                public static readonly TemplateKey AlreadyLinked = new(Base + nameof(AlreadyLinked));
            }
        }
        
        public static class Commands
        {
            private const string Base = nameof(Commands) + ".";

            public static class Code
            {
                private const string Base = Commands.Base + nameof(Code) + ".";
                
                public static readonly TemplateKey Success = new(Base + nameof(Success));
            }

            public static class User
            {
                private const string Base = Commands.Base + nameof(User) + ".";
                
                public static readonly TemplateKey Success = new(Base + nameof(Success));

                public static class Error
                {
                    private const string Base = User.Base + nameof(Error) + ".";
                    
                    public static readonly TemplateKey PlayerIsInvalid = new(Base + nameof(PlayerIsInvalid));
                    public static readonly TemplateKey PlayerNotConnected = new(Base + nameof(PlayerNotConnected));
                }
            }

            public static class Leave
            {
                private const string Base = Commands.Base + nameof(Leave) + ".";
                
                public static class Error
                {
                    private const string Base = Leave.Base + nameof(Error) + ".";
                    
                    public static readonly TemplateKey UserNotLinked = new(Base + nameof(UserNotLinked));
                }
            }
            
            public static class Admin
            {
                private const string Base = Commands.Base + nameof(Admin) + ".";

                public static class Link
                {
                    private const string Base = Admin.Base + nameof(Link) + ".";
                    
                    public static readonly TemplateKey Success = new(Base + nameof(Success));

                    public static class Error
                    {
                        private const string Base = Link.Base + nameof(Error) + ".";

                        public static readonly TemplateKey PlayerNotFound = new(Base + nameof(PlayerNotFound));
                        public static readonly TemplateKey PlayerAlreadyLinked = new(Base + nameof(PlayerAlreadyLinked));
                        public static readonly TemplateKey UserAlreadyLinked = new(Base + nameof(UserAlreadyLinked));
                    }
                }

                public static class Unlink
                {
                    private const string Base = Admin.Base + nameof(Unlink) + ".";

                    public static readonly TemplateKey Success = new(Base + nameof(Success));

                    public static class Error
                    {
                        private const string Base = Unlink.Base + nameof(Error) + ".";
                        
                        public static readonly TemplateKey MustSpecifyOne = new(Base + nameof(MustSpecifyOne));
                        public static readonly TemplateKey PlayerIsNotLinked = new(Base + nameof(PlayerIsNotLinked));
                        public static readonly TemplateKey UserIsNotLinked = new(Base + nameof(UserIsNotLinked));
                        public static readonly TemplateKey LinkNotSame = new(Base + nameof(LinkNotSame));
                    }
                }

                public static class Search
                {
                    private const string Base = Admin.Base + nameof(Search) + ".";

                    public static readonly TemplateKey Success = new(Base + nameof(Success));

                    public static class Error
                    {
                        private const string Base = Search.Base + nameof(Error) + ".";
                        
                        public static readonly TemplateKey PlayerNotFound = new(Base + nameof(PlayerNotFound));
                    }
                }
                
                
                public static class Unban
                {
                    private const string Base = nameof(Unban) + ".";
            
                    public static readonly TemplateKey Player = new(Base + nameof(Player)); 
                    public static readonly TemplateKey User = new(Base + nameof(User));

                    public static class Error
                    {
                        private const string Base = Unban.Base + nameof(Error) + ".";

                        public static readonly TemplateKey PlayerNotFound = new(Base + nameof(PlayerNotFound));
                        public static readonly TemplateKey PlayerNotBanned = new(Base + nameof(PlayerNotBanned));
                        public static readonly TemplateKey UserNotBanned = new(Base + nameof(UserNotBanned));
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
                
                public static readonly TemplateKey Command = new(Base + nameof(Command));
                public static readonly TemplateKey Admin = new(Base + nameof(Admin));
                public static readonly TemplateKey Api = new(Base + nameof(Api));
                public static readonly TemplateKey GuildRejoin = new(Base + nameof(GuildRejoin));
                public static readonly TemplateKey InactiveRejoin = new(Base + nameof(InactiveRejoin));
            }
            
            public static class Declined
            {
                private const string Base = Link.Base + nameof(Declined) + ".";

                public static readonly TemplateKey JoinWithUser = new(Base + nameof(JoinWithUser));
                public static readonly TemplateKey JoinWithPlayer = new(Base + nameof(JoinWithPlayer));
            }

            public static class WelcomeMessage
            {
                private const string Base = Link.Base + nameof(WelcomeMessage) + ".";

                public static readonly TemplateKey DmLinkAccounts = new(Base + nameof(DmLinkAccounts));
                public static readonly TemplateKey GuildLinkAccounts = new(Base + nameof(GuildLinkAccounts));
            }
        }

        public static class Unlink
        {
            private const string Base = nameof(Unlink) + ".";

            public static class Completed
            {
                private const string Base = Unlink.Base + nameof(Completed) + ".";
                public static readonly TemplateKey Command = new(Base + nameof(Command));
                public static readonly TemplateKey Admin = new(Base + nameof(Admin));
                public static readonly TemplateKey Api = new(Base + nameof(Api));
                public static readonly TemplateKey Inactive = new(Base + nameof(Inactive));
            }
        }
        
        public static class Banned
        {
            private const string Base = nameof(Banned) + ".";

            public static readonly TemplateKey PlayerBanned = new(Base + nameof(PlayerBanned));
        }

        public static class Join
        {
            private const string Base = nameof(Join) + ".";
            
            public static readonly TemplateKey CompleteLink = new(Base + nameof(CompleteLink));
        }

        public static class Errors
        {
            private const string Base = nameof(Errors) + ".";

            public static readonly TemplateKey UserAlreadyLinked = new(Base + nameof(UserAlreadyLinked));
            public static readonly TemplateKey PlayerAlreadyLinked = new(Base + nameof(PlayerAlreadyLinked));
            public static readonly TemplateKey CodeActivationNotFound = new(Base + nameof(CodeActivationNotFound));
            public static readonly TemplateKey LookupActivationNotFound = new(Base + nameof(LookupActivationNotFound));
            public static readonly TemplateKey MustBeCompletedInServer = new(Base + nameof(MustBeCompletedInServer));
        }
    }
}