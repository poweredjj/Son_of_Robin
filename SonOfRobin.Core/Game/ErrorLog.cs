using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace SonOfRobin
{
    public class ErrorLog
    {
        private readonly string logName;
        private readonly string logPath;

        public ErrorLog()
        {
            this.logName = DateTime.Now.ToString("error_yyyy-MM-dd_HH_mm_ss") + ".log";
            this.logPath = Path.Combine(SonOfRobinGame.errorsPath, this.logName);
        }

        private string DateString
        { get { return DateTime.Now.ToString("HH:mm:ss"); } }

        public void AddEntry(Type type, Exception exception, bool showTextWindow)
        {
            string stringToSave = $"{this.DateString} error occured ({type.Name}):\n\n{exception}";

            File.AppendAllText(this.logPath, stringToSave);

            if (showTextWindow) new TextWindow(text: $"{type.Name} - an error occured:\n{exception}", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, priority: -1, inputType: Scene.InputTypes.Normal);
        }

        public void AddEntry(string message)
        {
            string stringToSave = $"{this.DateString} error occured (no class info):\n\n{message}";

            File.AppendAllText(this.logPath, stringToSave);
        }
    }
}