namespace DiscordCorePlugin.Localization
{
    public static class AdminAppCommandKeys
    {
        private const string Base = "AppCommand.Admin.";
            
        public const string Command = Base + nameof(Command);
        public const string Description = Base + nameof(Description);

        public static class Link
        {
            private const string Base = AdminAppCommandKeys.Base + nameof(Link) + ".";

            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);
            
            public static class Args
            {
                private const string Base = Link.Base + nameof(Args) + ".";
                
                public static class Player
                {
                    private const string Base = Args.Base + nameof(Player) + ".";
                    
                    public const string Name = Base + nameof(Name);
                    public const string Description = Base + nameof(Description);
                }

                public static class User
                {
                    private const string Base = Args.Base + nameof(User) + ".";
                    
                    public const string Name = Base + nameof(Name);
                    public const string Description = Base + nameof(Description);
                }
            }
        }

        public static class Unlink
        {
            private const string Base = AdminAppCommandKeys.Base + nameof(Unlink) + ".";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);
            
            public static class Args
            {
                private const string Base = Unlink.Base + nameof(Args) + ".";

                public static class Player
                {
                    private const string Base = Args.Base + nameof(Player) + ".";
                    
                    public const string Name = Base + nameof(Name);
                    public const string Description = Base + nameof(Description);
                }

                public static class User
                {
                    private const string Base = Args.Base + nameof(User) + ".";
                    
                    public const string Name = Base + nameof(Name);
                    public const string Description = Base + nameof(Description);
                }
            }
        }

        public static class Search
        {
            private const string Base = AdminAppCommandKeys.Base + nameof(Search) + ".";
            
            public const string Command = Base + nameof(Command);
            public const string CommandDescription = Base + nameof(CommandDescription);

            public static class SubCommand
            {
                private const string Base = Search.Base + nameof(SubCommand) + ".";

                public static class Player
                {
                    private const string Base = SubCommand.Base + nameof(Player) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string Description = Base + nameof(Description);

                    public static class Args
                    {
                        private const string Base = Player.Base + nameof(Args) + ".";

                        public static class Players
                        {
                            private const string Base = Args.Base + nameof(Players) + ".";

                            public const string Name = Base + nameof(Name);
                            public const string Description = Base + nameof(Description);
                        }
                    }
                }
                
                public static class User
                {
                    private const string Base = SubCommand.Base + nameof(User) + ".";
                    
                    public const string Command = Base + nameof(Command);
                    public const string Description = Base + nameof(Description);

                    public static class Args
                    {
                        private const string Base = User.Base + nameof(Args) + ".";

                        public static class Users
                        {
                            private const string Base = Args.Base + nameof(Users) + ".";

                            public const string Name = Base + nameof(Name);
                            public const string Description = Base + nameof(Description);
                        }
                    }
                }
            }
        }
    }
}