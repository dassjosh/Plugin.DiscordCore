namespace DiscordCorePlugin.AppCommands
{
    public static class AdminAppCommands
    {
        public const string Command = "dca";
        public const string Description =  "Discord Core Admin Commands";

        public static class Link
        {
            public const string Command = "link";
            public const string Description = "admin link player game account and Discord user";

            public static class Args
            {
                public static class Player
                {
                    public const string Name = "player";
                    public const string Description = "player to link";
                }

                public static class User
                {
                    public const string Name = "user";
                    public const string Description = "user to link";
                }
            }
        }
        
        public static class Unlink
        {
            public const string Command = "unlink";
            public const string Description =  "admin unlink player game account and Discord user";

            public static class Args
            {
                public static class Player
                {
                    public const string Name = "player";
                    public const string Description = "player to unlink";
                }

                public static class User
                {
                    public const string Name = "user";
                    public const string Description = "user to unlink";
                }
            }
        }
        
        public static class Search
        {
            public const string Command = "search";
            public const string Description =  "search linked accounts by discord or player";

            public static class SubCommand
            {
                public static class Player
                {
                    public const string Command = "player";
                    public const string Description = "search by player";

                    public static class Args
                    {
                        public static class Players
                        {
                            public const string Name = "player";
                            public const string Description = "player to search";
                        }
                    }
                }
                
                public static class User
                {
                    public const string Command = "user";
                    public const string Description = "search by user";
                    
                    public static class Args
                    {
                        public static class Users
                        {
                            public const string Name = "user";
                            public const string Description = "user to search";
                        }
                    }
                }
            }
        }
    }
}