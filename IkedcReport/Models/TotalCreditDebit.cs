using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class TotalCreditDebit
    {
        public decimal CreditTotal { get; internal set; }
        public decimal DebitTotal { get; internal set; }
    }
}