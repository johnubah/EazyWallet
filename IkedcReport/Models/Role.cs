using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class Role : IPersist
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<int> CreatedByUserID { get; set; }
        public Nullable<int> ModifiedByUserID { get; set; }
        public System.DateTime DateLastModified { get; set; }


        public void Create()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}