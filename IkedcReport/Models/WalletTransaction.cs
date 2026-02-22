using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalletReport.Models
{
    public class WalletTransaction
    {
        public String TranID { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String TerminalID { get; set; }
        public Decimal Limit { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastTransacDate { get; set; }
        public String SettlementAccount { get; set; }
        public String TransactionDateBuild
        {
            get
            {
                if (LastTransacDate != null)
                {
                    try
                    {

                        StringBuilder builder = new StringBuilder();

                        builder.Append(LastTransacDate.Year.ToString()).Append("-");

                        if (LastTransacDate.Month.ToString().Length > 1)
                        {
                            builder.Append(LastTransacDate.Month.ToString()).Append("-");
                        }
                        else
                        {
                            builder.Append("0").Append(LastTransacDate.Month.ToString()).Append("-");
                        }

                        if (LastTransacDate.Day.ToString().Length > 1)
                        {
                            builder.Append(LastTransacDate.Day.ToString());
                        }
                        else
                        {
                            builder.Append("0").Append(LastTransacDate.Day.ToString());


                        }
                        builder.Append(" ");

                        if (LastTransacDate.Hour.ToString().Length > 1)
                        {
                            builder.Append(LastTransacDate.Hour.ToString()).Append(":");
                        }
                        else
                        {
                            builder.Append("0").Append(LastTransacDate.Hour.ToString()).Append(":");
                        }
                        if (LastTransacDate.Minute.ToString().Length > 1)
                        {
                            builder.Append(LastTransacDate.Minute.ToString()).Append(":");
                        }
                        else
                        {
                            builder.Append("0").Append(LastTransacDate.Minute.ToString()).Append(":");
                        }
                        if (LastTransacDate.Second.ToString().Length > 1)
                        {
                            builder.Append(LastTransacDate.Second.ToString());
                        }
                        else
                        {
                            builder.Append("0").Append(LastTransacDate.Second.ToString());
                        }


                        return builder.ToString();
                    }
                    catch (Exception ex)
                    {
                    }
                    return LastTransacDate.ToString();

                }
                return String.Empty;
            }
        }

        public decimal LastTransactionAmt { get; set; }

        public string DealerName { get; set; }

        public string TransactionType { get; set; }

        public string AgentName { get; set; }

        public string CustomerRef { get; set; }

        public string Reference { get; set; }

        public decimal CreditAmount { get; set; }

        public decimal DebitAmount { get; set; }
        public decimal Amount { get; internal set; }
        public long? AgentID { get; internal set; }
        public long? BankID { get; internal set; }
        public String AccountNumber { get; internal set; }
        public bool IsReversed { get; internal set; }
    }
}
