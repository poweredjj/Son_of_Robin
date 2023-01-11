using System;

namespace SonOfRobin.DesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new SonOfRobinGame())
            {
                SonOfRobinGame.platform = Platform.Desktop;
                SonOfRobinGame.os = OS.DesktopGL;
                game.Run();
            }
        }
    }
}
