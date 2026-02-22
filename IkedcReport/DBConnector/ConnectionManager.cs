using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace WalletReport.DBConnector
{
    public class ConnectionManager
    {
        public static IDbConnection GetConnection()
        {
            IDbConnection conn = (IDbConnection)new MySql.Data.MySqlClient.MySqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["WalletReportConnection"].ConnectionString);
            return conn;
        }

        internal static void Close(IDbConnection conn)
        {
            if (conn != null)
            {
                try
                {
                    conn.Close();

                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}