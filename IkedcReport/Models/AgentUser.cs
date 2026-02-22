using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class AgentUser
    {
        public Int64 Id { get; set; }
        public String AgenName { get; set; }
        public String AddressLine1 { get; set; }
        public String AddressLine2 { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public String Country { get; set; }
        public String ContactAddress { get; set; }
        public String ContactEmail { get; set; }
        public String ContactMobile { get; set; }
        public String ContactName { get; set; }
        public Int64 DealerId { get; set; }

        private String postingMode;
        public String PostingMode {
            get
            {
                if (String.IsNullOrWhiteSpace(postingMode))
                    return "N";
                return postingMode;
            }
            set
            {
                postingMode = value;
            }
        }

        public decimal Balance { get;  set; }

        public String AccountNo { get; set; }
        public String UBAAccountNo { get; set; }
    }
}