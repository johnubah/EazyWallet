using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace WalletReport.Models
{
    public class TerminalOwner
    {
        public String TerminalId { get; set; }


        public string UserName { get; set; }

        public string Firstname { get; set; }

        public string LastName { get; set; }
    }
}