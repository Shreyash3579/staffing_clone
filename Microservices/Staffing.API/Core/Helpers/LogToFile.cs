//using System.IO;
//using System;

//namespace Staffing.API.Core.Helpers
//{
//    public static class LogToFile
//    {
//        public static string _file = "log.txt";
//        public static object _locked = new object();

//        public static void AppendToLog(string text)
//        {
//            lock (_locked)
//            {
//                string path = Path.Combine(Directory.GetCurrentDirectory(), _file);
//                File.AppendAllText(path, text + Environment.NewLine);
//            }
//        }

//        public static void ClearFileAndAddLog(string text)
//        {
//            lock (_locked)
//            {
//                string path = Path.Combine(Directory.GetCurrentDirectory(), _file);
//                File.WriteAllText(path, text + Environment.NewLine);
//            }
//        }
//    }
//}
