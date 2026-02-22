using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.DBConnector
{
    public class UserLockOutException : Exception
    {
         
        public UserLockOutException()
            : base()
        {
        }
        public UserLockOutException(String message)
            : base(message)
        {
        }
        public UserLockOutException(String message, Exception ex)
            : base(message, ex)
        {
        }
    }
}