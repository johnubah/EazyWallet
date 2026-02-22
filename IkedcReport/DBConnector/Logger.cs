using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WalletReport.DBConnector
{
    public class Logger
    {
        private static object o = new object();
        public static void WriteToLog(String message)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;
                String errorPath = HttpContext.Current.Server.MapPath("~/Log");
                StringBuilder FileNameBuilder = new StringBuilder();

                FileNameBuilder.Append("Error_").Append(DateTime.Now.Year.ToString());
                FileNameBuilder.Append("_").Append(DateTime.Now.Month.ToString());
                FileNameBuilder.Append("_").Append(DateTime.Now.Day.ToString()).Append(".txt");
                String path = System.IO.Path.Combine(errorPath, FileNameBuilder.ToString());

                lock (o)
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path, true))
                    {
                        writer.AutoFlush = true;
                        writer.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}