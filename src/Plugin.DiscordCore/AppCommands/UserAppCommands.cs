namespace DiscordCorePlugin.AppCommands
{
    public static class UserAppCommands
    {
        public const string Command = "dc";
        public const string Description = "Discord Core Commands";

        public static class Code
        {
            public const string Command = "code";
            public const string Description = "start the link between discord and the game server using a link code";
        }

        public static class User
        {
            public const string Command = "user";
            public const string Description = "start the link between discord and the game server by game server player name";

            public static class Args
            {
                public static class Player
                {
                    public const string Name = "player";
                    public const string Description = "Player name on the game server";
                }
            }
        }
        
        public static class Leave
        {
            public const string Command = "leave";
            public const string Description = "unlink your discord and game server accounts";
        }
        
        public static class Link
        {
            public const string Command = "link";
            public const string Description =  "complete the link using the given link code";

            public static class Args
            {

                public static class Code
                {
                    public const string Name = "code";
                    public const string Description = "code to complete the link";
                }
            }
        }
    }
}