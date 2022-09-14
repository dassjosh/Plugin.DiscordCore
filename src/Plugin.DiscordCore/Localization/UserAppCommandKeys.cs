namespace DiscordCorePlugin.Localization
{
    public static class UserAppCommandKeys
    {
        private const string Base = "AppCommand.User.";
            
        public const string Command = Base + nameof(Command);
        public const string Description = Base + nameof(Description);

        public static class Code
        {
            private const string Base = UserAppCommandKeys.Base + nameof(Code) + ".";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);
        }

        public static class User
        {
            private const string Base = UserAppCommandKeys.Base + nameof(User) + ".";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);

            public static class Args
            {
                private const string Base = User.Base + nameof(Args) + ".";

                public static class Player
                {
                    private const string Base = Args.Base + nameof(Player) + ".";

                    public const string Name = Base + nameof(Name);
                    public const string Description = Base + nameof(Description);
                }
            }
        }

        public static class Leave
        {
            private const string Base = UserAppCommandKeys.Base + nameof(Leave) + ".";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);
        }

        public static class Link
        {
            private const string Base = UserAppCommandKeys.Base + nameof(Link) + ".";
            
            public const string Command = Base + nameof(Command);
            public const string Description = Base + nameof(Description);

            public static class Args
            {
                private const string Base = Link.Base + nameof(Args) + ".";

                public static class Code
                {
                    private const string Base = Args.Base + nameof(Code) + ".";
                    
                    public const string Name = Base + nameof(Command);
                    public const string Description = Base + nameof(Description);
                }
            }
        }
    }
}