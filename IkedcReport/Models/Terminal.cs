using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WalletReport.Processor;

namespace WalletReport.Models
{
    public class Terminal : IPersist
    {
        public Int64 Id { get; set; }
        public String TerminalID { get; set; }
        public String TerminalCode { get; set; }
        public String TerminalDesc { get; set; }
        public String Location { get; set; }
        public decimal MinimumPsamValue { get; set; }
        public bool NotifyPsamExceeded { get; set; }
        public Int64 DealerID { get; set; }
        public Int64 AgentId { get; set; }

        public void Create()
        {
            //TerminalProcessor.Create(this);
        }

        public void Update()
        {
            //TerminalProcessor.Update(this);
        }
    }
}