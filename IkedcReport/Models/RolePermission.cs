using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class RolePermission : IPersist
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public int CreatedByUserID { get; set; }
        public Nullable<int> ModifiedByUserID { get; set; }

        void IPersist.Create()
        {
            throw new NotImplementedException();
        }

        void IPersist.Update()
        {
            throw new NotImplementedException();
        }
    }
}