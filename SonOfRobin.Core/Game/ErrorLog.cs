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

        public void AddEntry(Type type, Exception exception)
        {
            string stringToSave = $"{this.DateString} error occured ({type.Name}):\n\n{exception}";

            File.AppendAllText(this.logPath, stringToSave);
        }

        public void AddEntry(string message)
        {
            string stringToSave = $"{this.DateString} error occured (no class info):\n\n{message}";

            File.AppendAllText(this.logPath, stringToSave);
        }
    }
}