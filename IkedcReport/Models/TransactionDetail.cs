using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class TransactionDetail
    {
        private string requestXml;
        public string RequestXml
        {
            get
            {
                if (String.IsNullOrWhiteSpace(requestXml))
                    return String.Empty;

                return requestXml;
            }
            set
            {
                requestXml = value;
            }

        }
        private string responseXml;
        public string ResponseXml {
            get
            {
                if (String.IsNullOrWhiteSpace(responseXml))
                    return String.Empty;
                return responseXml;

            }
            set
            {
                responseXml = value;
            }
        }
        public WalletTransaction Transaction { get;  set; }
    }
}