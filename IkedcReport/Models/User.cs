using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace WalletReport.Models
{
    [Serializable]
    public class User : IPersist
    {
        
        public Int64 Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Othernames { get; set; }
       
        public string EmailAddress { get; set; }
        [Required]
        public string DisplayName { get; set; }
        public string UserNote { get; set; }
        public System.DateTime? DateCreated { get; set; }
        public System.DateTime DateModified { get; set; }
        public Nullable<int> ModifiedByUserID { get; set; }
        public System.DateTime ExpiryDate { get; set; }
        public Nullable<int> CreatedByUserID { get; set; }
        [Required]
        public String Mobile { get; set; }
        public String City { get; set; }
        public String Country { get; set; }

        public bool IsActive { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public String Password { get; set; }
        public List<String> _Roles;
        public List<String> Roles {
            get
            {
                if (_Roles == null) _Roles = new List<string>();
                return _Roles;
            }
            set 
            {
                _Roles = value;
            }
         }

        private bool _IsDistrictUser;
        public bool IsDistrictUser
        {
            get
            {
                return _IsDistrictUser;
            }
            set
            {
                _IsDistrictUser = value;
            }
        }
        public bool IsSuperUserOrAdmin
        {
            get
            {
                if (Roles == null)
                    return false;


                List<String> o = new List<string>();
                o.Add("admin");
                o.Add("administrator");
                o.Add("super");
                o.Add("posters");

                var count = (from c in Roles
                             where o.Contains(c.ToLower().Trim())
                             select c).Count();


                if(count > 0)
                    return true;
                else
                    return false;
            }

        }

        public void Create()
        {
            WalletReport.Processor.UserProcessor.Create(this);
        }

        public void Update()
        {
            WalletReport.Processor.UserProcessor.Update(this);
        }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public String ConfirmPassword { get; set; }
        public List<Int64> District { get; set; }
        public int DealerID { get; set; }

        public bool ActiveStatus { get; set; }

        private bool _Agent;
        public bool IsAgent
        {
            get
            {

                if (Roles == null)
                {
                    List<Role> roles = Processor.UserInRoleProcessor.GetRoleInUser(this);
                    Roles = (from c in roles
                             select c.RoleName).ToList();
                }


                List<String> o = new List<string>();
                o.Add("agent");

                var count = (from c in Roles
                             where o.Contains(c.ToLower().Trim())
                             select c).Count();


                if (count > 0 && this.UserTypeID == 3)
                    return true;
                else
                    return false;
            }
            set
            {
                _Agent = value;
            }
        }
        [Required]
        public Int64 UserTypeID { get; set; }

        private bool _IsDealer;
        public bool IsDealer
        {
            get
            {

                if (Roles == null)
                {
                    List<Role> roles = Processor.UserInRoleProcessor.GetRoleInUser(this);
                    Roles = (from c in roles
                             select c.RoleName).ToList();
                }


                List<String> o = new List<string>();
                o.Add("dealer");

                var count = (from c in Roles
                             where o.Contains(c.ToLower().Trim())
                             select c).Count();


                if (count > 0 && this.UserTypeID == 2)
                    return true;
                else
                    return false;
            }
            set
            {
                _IsDealer = value;
            }
        }

        public bool ISALLDistrict()
        {
            return Processor.DistrictProcessor.ISALLDistrict(this);
        }

        public bool ForceChangePassword { get; set; }

        public bool IsManager { get; set; }


        //usertypes.Add(new UserType() { UserTypeID = 1, Description = "Bank" });
        //        usertypes.Add(new UserType() { UserTypeID = 2, Description = "Dealer" });
        //        usertypes.Add(new UserType() { UserTypeID = 3, Description = "Agent" });
        //        usertypes.Add(new UserType() { UserTypeID = 4, Description = "Etop" });

        private bool _isBank;
        public bool isBank
        {
            get
            {
                if (this.Roles.Count == 0)
                {
                    List<Role> roles = Processor.UserInRoleProcessor.GetRoleInUser(this);
                    Roles = (from c in roles
                             select c.RoleName).ToList();
                }

                List<String> o = new List<string>();
                o.Add("bank");


                //if (Roles == null)
                //{
                //    List<Role> roles = Processor.UserInRoleProcessor.GetRoleInUser(this);
                //    Roles = (from c in roles
                //             select c.RoleName).ToList();
                //}

                var count = (from c in Roles
                             where o.Contains(c.ToLower().Trim())
                             select c).Count();


                if (count > 0 )
                    return true;
                else
                    return false;
            }
            set
            {
                _isBank = value;
            }
           
        }

        private bool _IsEtop;
        public bool IsEtop
        {
            get
            {
                List<String> o = new List<string>();
                o.Add("bank");

                if (Roles == null)
                {
                    List<Role> roles = Processor.UserInRoleProcessor.GetRoleInUser(this);
                    Roles = (from c in roles
                             select c.RoleName).ToList();
                }

                var count = (from c in this.Roles
                             where o.Contains(c.ToLower().Trim())
                             select c).Count();


                if (count > 0 && this.UserTypeID == 4)
                    return true;
                else
                    return false;
            }
            set
            {
                _IsEtop = value;
            }
        }

        public long BankID { get; set; }

        public long AgentID { get; set; }
        public String AllowTerminalConfig { get;  set; }
    }
}