using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class GroupTransaction
    {
        public string MeterType { get; set; }

        public decimal Amount { get; set; }

        public int TransCount { get; set; }
    }
}