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

        public void AddEntry(Object obj, Exception exception, bool showTextWindow)
        {
            Type type = obj.GetType();

            string stringToSave = $"\n\n\n{this.DateString} error occured ({type.Name}):\n\n{exception}";

            File.AppendAllText(this.logPath, stringToSave);

            string windowText = $"{type.Name} - an error occured:\n{exception}";
            if (windowText.Length > 1000) windowText = windowText.Substring(0, 1000);

            if (showTextWindow) new TextWindow(text: windowText, textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, priority: -1, inputType: Scene.InputTypes.Normal);
        }
    }
}