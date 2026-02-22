using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace WalletReport.Models
{
    public class SearchModel
    {
        public Int64 DealerID { get; set; }
        public Int64 BankID { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String TerminalID { get; set; }
        public Int64  AgentID { get; set; }
        public int CurrentPageIndex { get; set; }

        private String _CustomSearch;
        public String CustomSearch
        {
            get
            {
                if (String.IsNullOrEmpty(_CustomSearch))
                {
                    return "ALL";
                }
                return _CustomSearch;
            }
            set
            {
                _CustomSearch = value;
            }
        }
        private String _CustomSearchValue;

        public String CustomSearchValue
        {
            get
            {
                return _CustomSearchValue;
            }
            set
            {
                _CustomSearchValue = value;
            }
        }

        private int _PageSize;
        public int PageSize
        {
            get
            {
                if (_PageSize == 0)
                {
                    _PageSize = 15;
                }
                return _PageSize;
            }
            set
            {
                if (value < 15)
                {
                    _PageSize = 15;
                }
                else
                {
                    _PageSize = value;
                }
            }
        }




        public int ItemCount { get; set; }

        public decimal TotalAmount { get; set; }

        public int Skip { get; set; }

        public decimal TotalCredit { get; set; }

        public decimal TotalDebit { get; set; }
        private String transactionType = "ALL";
        public String TransactionType { 
            get
            {
                if (String.IsNullOrWhiteSpace(transactionType))
                {
                    transactionType = "ALL";
                
                }
                return transactionType;
            }
            set
            {
                transactionType = value;
            }
        }
    }
}