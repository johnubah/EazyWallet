using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace WalletReport.Processor
{
    public class IkedcHeper
    {
        public static DateTime? GetDate(String date)
        {
            DateTime? t = null;
            //2014-12-07 08:59:08
            String format = "dd/MM/yyyy hh:mm:ss";

            if (date.Contains(":"))
            {
                format = "dd/MM/yyyy hh:mm:ss";
            }
            else
            {
                format = "dd/MM/yyyy";
            }
            System.IFormatProvider info = CultureInfo.CreateSpecificCulture("en-US");

            try
            {
                t = DateTime.ParseExact(date, format, info);

            }
            catch (Exception ex)
            {
            }
            return t;
        }
    }
}