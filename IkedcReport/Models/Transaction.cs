using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WalletReport.Models
{
    public class Transaction
    {
        private String _Reference;
        public String Reference
        {
            get
            {
                if (String.IsNullOrEmpty(_Reference))
                    return String.Empty;

                return _Reference;
            }
            set
            {
                _Reference = value;
            }
        
        }
        public String BusinessUnit { get; set; }
        public String AccountNumber { get; set; }
        public String MeterNumber { get; set; }
        public decimal TransactionAmt {get;set;}
        public String MeterType { get; set; }
        private String _CustomerName;

        public String CustomerName
        {
            get
            {
                if (String.IsNullOrEmpty(_CustomerName))
                {
                    return String.Empty;
                }
                return _CustomerName.Replace(",","");
            }
            set
            {
                _CustomerName = value;
            }
        }


        public decimal TotalCharge { get; set; }

        public DateTime TransactionDate { get; set; }

        public string ReceiptNumber { get; set; }

        public string District { get; set; }

        public string AVRdownload { get; set; }

        public String TransactionDateBuild
        {
            get
            {
                if (TransactionDate != null)
                {
                    try
                    {

                        StringBuilder builder = new StringBuilder();

                        builder.Append(TransactionDate.Year.ToString()).Append("-");

                        if (TransactionDate.Month.ToString().Length > 1)
                        {
                            builder.Append(TransactionDate.Month.ToString()).Append("-");
                        }
                        else
                        {
                            builder.Append("0").Append(TransactionDate.Month.ToString()).Append("-");
                        }

                        if (TransactionDate.Day.ToString().Length > 1)
                        {
                            builder.Append(TransactionDate.Day.ToString());
                        }
                        else
                        {
                            builder.Append("0").Append(TransactionDate.Day.ToString());


                        }
                        builder.Append(" ");

                        if (TransactionDate.Hour.ToString().Length > 1)
                        {
                            builder.Append(TransactionDate.Hour.ToString()).Append(":");
                        }
                        else 
                        {
                            builder.Append("0").Append(TransactionDate.Hour.ToString()).Append(":");
                        }
                        if (TransactionDate.Minute.ToString().Length > 1)
                        {
                            builder.Append(TransactionDate.Minute.ToString()).Append(":");
                        }
                        else
                        {
                            builder.Append("0").Append(TransactionDate.Minute.ToString()).Append(":");
                        }
                        if (TransactionDate.Second.ToString().Length > 1)
                        {
                            builder.Append(TransactionDate.Second.ToString());
                        }
                        else
                        {
                            builder.Append("0").Append(TransactionDate.Second.ToString());
                        }
                      

                        return builder.ToString();
                    }
                    catch (Exception ex)
                    {
                    }
                    return TransactionDate.ToString();

                }
                return String.Empty;
            }
        }


        public string TerminalID { get; set; }

        private String _TerminalOwner;
        public string TerminalOwner
        {
            get
            {
                if (_TerminalOwner == null)
                    return String.Empty;

                return _TerminalOwner;
            }
            set
            {
                _TerminalOwner = value;
            }
        }

        public String _Token;
        public string Token
        {
            get
            {
                if (String.IsNullOrEmpty(_Token))
                {
                    return String.Empty;
                }
                return _Token;
            }
            set
            {
                _Token = value;
            }
        }
    }
}