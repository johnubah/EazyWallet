using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.DBConnector
{
    public class DataHelper
    {
        
        public static T GetValue<T>(object value)
        {
            T result = default(T);
            try
            {
                if (value != null && value != DBNull.Value)
                {
                    var t = typeof(T);

                    if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        t = Nullable.GetUnderlyingType(t);


                    result = (T)Convert.ChangeType(value, t);
                }
            }
            catch (Exception ex)
            {
            }

            return result;

        }
        public static T GetValue<T>(String value)
        {
            T result = default(T);
            try
            {
                if (!String.IsNullOrEmpty(value))
                {
                    var t = typeof(T);

                    if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        t = Nullable.GetUnderlyingType(t);


                    result = (T)Convert.ChangeType(value, t);
                }
            }
            catch (Exception ex)
            {
            }

            return result;

        }
    }
}