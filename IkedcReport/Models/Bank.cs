using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalletReport.Models
{
    public class Bank
    {
        public Int64 Id { get; set; }
        public String BankName { get; set; }
        public String AddressLine1 { get; set; }
        public String AddressLine2 { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public String Country { get; set; }
        public String ContactAddress { get; set; }
        public String ContactEmail { get; set; }
        public String ContactMobile { get; set; }
        public String ContactName { get; set; }
        public String TerminalPrefix { get; set; }
    }
}
