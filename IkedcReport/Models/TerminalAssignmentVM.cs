using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalletReport.Models
{
    public class TerminalAssignmentVM : TerminalAssignment
    {
        public String AgentName { get; set; }
        public String DealerName { get; set; }
    }
}
