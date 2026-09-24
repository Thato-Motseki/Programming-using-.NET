using System;
using System.IO;

namespace EcoCashSimulation
{
    public static class Logger
    {
        public static void LogError(Exception ex)
        {
            string message =
                "----------------------------------------\n" +
                "Date: " + DateTime.Now + "\n" +
                "Error: " + ex.Message + "\n" +
                "Stack Trace: " + ex.StackTrace + "\n";

            File.AppendAllText("errors.log", message);
        }
    }
}