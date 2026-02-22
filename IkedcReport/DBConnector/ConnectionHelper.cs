using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace WalletReport.DBConnector
{
    public static class ConnectionHelper
    {
        public static void OpenIfClosed(this IDbConnection conn)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
            }
            catch (System.Data.DataException ex)
            {
                throw new OpenDBException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new OpenDBException(ex.Message, ex);
            }
        }
    }
}