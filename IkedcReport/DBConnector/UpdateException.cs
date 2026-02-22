using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.DBConnector
{
    public class UpdateException : Exception
    {
        
        public UpdateException()
            : base()
        {
        }
        public UpdateException(String message)
            : base(message)
        {
        }
        public UpdateException(String message, Exception ex)
            : base(message, ex)
        {
        }
    }
}