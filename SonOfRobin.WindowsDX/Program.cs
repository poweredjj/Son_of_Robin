using System;

namespace SonOfRobin.WindowsDX
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {

            using (var game = new SonOfRobinGame())
            {
                SonOfRobinGame.platform = Platform.Desktop;
                game.Run();
            }
        }
    }
}
