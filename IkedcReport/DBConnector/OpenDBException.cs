using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.DBConnector
{
    public class OpenDBException : Exception 
    {
        public OpenDBException()
            : base()
        {
        }
        public OpenDBException(String message)
            : base(message)
        {
        }
        public OpenDBException(String message, Exception ex)
            : base(message, ex)
        {
        }
    }
}