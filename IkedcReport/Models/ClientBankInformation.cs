using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class ClientBankInformation
    {
        public int Id { get;set; }
        public String Name {  get; set; }
        public String ClientId { get; set; }
        public string SecretKey { get; set; }
    }
}