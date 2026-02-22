using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net.Mail;
using WalletReport.DBConnector;

namespace WalletReport.Processor
{
    public class NotificationManager
    {
        public static void NotifyCustomer(StringBuilder builder, String emailAddress, String subject)
        {
            try
            {
                Logger.WriteToLog("Sending notification to: " + emailAddress);
                MailMessage message = new MailMessage();
                message.Body = builder.ToString();
                message.Subject = subject;
                message.To.Add(emailAddress);

                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;

                client.SendAsync(message, message);
                Logger.WriteToLog("Successfully sending notification to: " + emailAddress);
            }
            catch(Exception ex)
            {
                Logger.WriteToLog("email to " + emailAddress +" : " + ex.Message);

            }
        }
    }
}