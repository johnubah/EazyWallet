using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalletReport.Models
{
    public class WalletLoadRequestVM :WalletLoadRequest
    {
        public String DealerName { get; set; }
        public String BankName { get; set; }
        public Decimal WalletBalance { get; set; }



        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }
    }
}
