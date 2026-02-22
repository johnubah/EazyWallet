using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalletReport.Models
{
    public class TerminalAssignment
    {
        public String TerminalID { get; set; }
        public String AccountNumber { get; set; }
        public decimal DebitLimit { get; set; }
        public bool IgnoreLimit { get; set; }
        public bool IsActive { get; set; }
        public Int64 DealerID { get; set; }
        public Int64 AgentID { get; set; }

        public String OldTerminalID { get; set; }

    }
}
