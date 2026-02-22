using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class DashBoardModel
    {

        public decimal DailyCash { get; set; }
        public decimal WeeklyCash { get; set; }
        public decimal MonthlyCash { get; set; }


        public List<WalletTransaction> WalletTransaction { get; set; }


        public Summary Today { get; set; }

        public Summary WeeklySummary { get; set; }

        public Summary Monthly { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }
        public User CurrentUser { get;  set; }
        public Dealers Dealer { get;  set; }

        public AgentUser AgentUser { get; set; }
        public Summary TwoDaysSummary { get;  set; }
        public Summary YesterdaySummary { get;  set; }
    }
}