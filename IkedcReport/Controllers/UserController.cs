using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.Models;
using WalletReport.Processor;
using System.Net.Mail;

namespace WalletReport.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        //
        // GET: /User/
        [Authorize]
        [HttpGet]
        public ActionResult ResetUser(Int64 Id = 0)
        {
            User CurrentUser = Session["CurrentUser"] as User;


            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            User thisUser = UserProcessor.GetUsersByID(Id);

            //String NewPassword = "UserProcessor.GetRandomPassword()";
            String NewPassword = "P@ssw0rd";
            NewPassword = NewPassword.Trim();

            bool IsChange = UserProcessor.ChangePassword(thisUser, NewPassword,true);
            if (IsChange)
            {
                //MailMessage msg = new MailMessage();
                //msg.Subject = String.Format("WALLET Report Password Reset for {0}", thisUser.Firstname);
                //msg.IsBodyHtml = false;
                //msg.Priority = MailPriority.High;
                //msg.To.Add(thisUser.EmailAddress);
                //msg.Body = String.Format("Your WALLET REPORT password have been change for {0}.  Please use this password to logon the portal thanks", NewPassword);

                //try
                //{
                //    SmtpClient client = new SmtpClient();
                //    client.Send(msg);
                //    ViewBag.message = "The new email have been sent to the email address. User should logon to her email to retrieve her password";

                //}
                //catch (Exception ex)
                //{
                //    ViewBag.message = String.Format( "Please ensure your SMTP server is connected. Exception was throw while sending password<br/> {0}",ex.Message);
                //}

                ViewBag.Message = "Your password change was successful";
            }
            else
            {
                ViewBag.Message = "Change Password failed";
            }

            return View();


        }
        [Authorize]
        [HttpGet]
        public ActionResult Activete(Int64 Id = 0)
        {
            User CurrentUser = Session["CurrentUser"] as User;


            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            try
            {
                if (Id > 0)
                {
                    Processor.UserProcessor.ActivateUser(Id);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("UserList");
        }
        [Authorize]
        [HttpGet]
        public ActionResult Deactive(Int64 Id = 0)
        {
            User CurrentUser = Session["CurrentUser"] as User;


            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            try
            {
                if (Id > 0)
                {
                    Processor.UserProcessor.DeactivateUser(Id);
                }
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("UserList");
        }
        [Authorize]
        [HttpPost]
        public ActionResult Create(Models.User oUser)
        {

            User CurrentUser = Session["CurrentUser"] as User;


            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            //if (!ModelState.IsValid)
            //{
            //    System.Text.StringBuilder ErrorBuilder = new System.Text.StringBuilder();

            //    foreach (ModelState mStates in ModelState.Values)
            //    {
            //        foreach (ModelError error in mStates.Errors)
            //        {
            //            ErrorBuilder.Append(error.ErrorMessage).Append("<br/>");
            //        }
            //    }
            //    TempData["ErrorMessage"] = ErrorBuilder.ToString();
            //    return RedirectToAction("Error", "Home");


            //}
            oUser.EmailAddress = oUser.Username;
            if (oUser.UserTypeID == -1)
            {
                TempData["oUser"] = oUser;
                TempData["ErrorMessage"] = "Please Select the appropriate user type";
                return RedirectToAction("Create");

            }
            if (oUser.UserTypeID == 1)
            {
                oUser.isBank = true;
                oUser.IsAgent = false;
                oUser.IsDealer = false;
                oUser.DealerID = 0;
                oUser.AgentID = 0;

                if (oUser.BankID == 0)
                {
                    //return to redirect option

                    TempData["oUser"] = oUser;
                    TempData["ErrorMessage"] = "Bank information must be selected";
                    return RedirectToAction("Create");
                }
            }
            else if (oUser.UserTypeID == 2)
            {
                oUser.isBank = false;
                oUser.IsAgent = false;
                oUser.IsDealer = true;
                oUser.AgentID = 0;

                if (oUser.DealerID == 0)
                {
                    //return to redirect option

                    TempData["oUser"] = oUser;
                    TempData["ErrorMessage"] = "Dealer information must be selected";
                    return RedirectToAction("Create");
                }
               
            }
            else if (oUser.UserTypeID == 3)
            {
                oUser.isBank = false;
                oUser.IsAgent = true;
                oUser.IsDealer = false;

                if (oUser.AgentID < 0)
                {
                    //return to redirect option

                    TempData["oUser"] = oUser;
                    TempData["ErrorMessage"] = "Agent information must be selected";
                    return RedirectToAction("Error","Home");
                }
                oUser.AllowTerminalConfig = "N";

            }
            else
            {
                oUser.isBank = false;
                oUser.IsAgent = false;
                oUser.IsDealer = false;
                oUser.IsEtop = true;
            }
            String ErrorMessage = String.Empty;
            try
            {
                if (oUser.Id == 0)
                {
                    oUser.Create();
                }
                else
                {
                    oUser.Update();
                }
                return RedirectToAction("UserList");
            }
            catch (Exception ex)
            {
            }

            TempData["oUser"] = oUser;
            TempData["ErrorMessage"] = ErrorMessage;

            return RedirectToAction("Create");


        }
        [Authorize]
        [HttpPost]
        public ActionResult UserList(GeneralSearch search)
        {
            User CurrentUser = Session["CurrentUser"] as User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }


            TempData["GeneralSearch"] = search;

            return RedirectToAction("UserList");


            
        }
        [Authorize]
        public ActionResult UserList()
        {
            User oUser = Session["CurrentUser"] as User;
            if (oUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (oUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(oUser.IsSuperUserOrAdmin || oUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }


            List<Models.User> oUsers = null;

            GeneralSearch generalSearch = TempData["GeneralSearch"] as Models.GeneralSearch;
             try
             {
                 if (generalSearch == null)
                 {
                     generalSearch = new GeneralSearch();
                     generalSearch.SearchCriteria = "ALL";
                 }


                 generalSearch.Skip = (generalSearch.CurrentPageIndex - 1) * generalSearch.PageSize;

                 oUsers = Processor.UserProcessor.GetUsers(oUser, generalSearch);
                 ViewBag.Users = oUsers;
             }
             catch (Exception ex)
             {
             }

            List<String> SearchCriteria = new List<string>();

            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Firstname");
            SearchCriteria.Add("Lastname");
            SearchCriteria.Add("Email");
           
            SelectList SearchList = new SelectList(SearchCriteria);

            ViewBag.SearchList = SearchList;

           

            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;


            if (oUsers != null && generalSearch.ItemCount > oUsers.Count())
            {
                ViewBag.IsPagination = true;
            }
            else
            {
                ViewBag.IsPagination = false;
            }
            return View(generalSearch);
        }
        [Authorize]
        [HttpGet]
        public ActionResult Create(Int64 Id=0)
        {
            //IDictionary<int, String> UserType = new Dictionary<int, String>();
            //UserType.Add(-1, "Choose one");
            //UserType.Add(1, "Ik");

            User CurrentUser = Session["CurrentUser"] as User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }


            List<UserType> Usertypes =  UserType.GetUserType();
            ViewBag.UserType = new SelectList(Usertypes, "UserTypeID", "Description");


            User oUser = TempData["oUser"] as User;
            String ErrorMessage = TempData["ErrorMessage"] as String;

            if (!String.IsNullOrEmpty(ErrorMessage))
            {
                ViewBag.ErrorMessage = ErrorMessage;
            }


            if (Id > 0)
            {
                oUser = UserProcessor.GetUsersByID(Id);
            }

           List<Bank> Banks = BankProcessor.GetBankByUser(CurrentUser);
           if (Banks == null)
               Banks = new List<Bank>();

           if (CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop)
           {

               Banks.Insert(0,new Bank() { Id = 0, BankName = "SELECT A BANK" });
           }
           SelectList BankSelect = new SelectList(Banks, "Id", "BankName");
           ViewBag.Banks = BankSelect;


           List<Models.DealerVM> dealers = null;



            //if (oUser != null && oUser.BankID > 0 && oUser.UserTypeID == 2)
            //{
            //    dealers = DealersProcessor.GetFullDealerByBank(oUser.BankID);
            //    if (dealers == null)
            //        dealers = new List<Dealers>();
            //}
            //else
            //{
            //    if(oUser != null) oUser.DealerID = 0;
            //    dealers = DealersProcessor.GetDealersByUser(CurrentUser);

            //}
            //if (dealers == null)
            dealers = DealersProcessor.GetDealerByBank(0);

           if (CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank)
           {
               dealers.Insert(0,new Models.DealerVM() { Id = 0, DealerName = "SELECT A DEALER" });
           }
           SelectList selectList = new SelectList(dealers, "Id", "DealerName");
           ViewBag.Dealers = selectList;

           List<AgentUser> Agents = null;

           //usertypes.Add(new UserType() { UserTypeID = 1, Description = "Bank" });
           //usertypes.Add(new UserType() { UserTypeID = 2, Description = "Dealer" });
           //usertypes.Add(new UserType() { UserTypeID = 3, Description = "Agent" });
           //usertypes.Add(new UserType() { UserTypeID = 4, Description = "Etop" });
           if (oUser != null && oUser.UserTypeID == 3 && oUser.DealerID > 0)
           {
               Agents = AgenrtProcessor.GetFullAgentByDealer(oUser.DealerID);
           }
           else
           {
               if (oUser != null) oUser.AgentID = 0;
               Agents = AgenrtProcessor.GetAgentByUser(CurrentUser);
           }
           

           if (Agents == null)
               Agents = new List<AgentUser>();

           Agents.Insert(0, new AgentUser() { Id = 0, AgenName = "PLEASE SELECT AGENT" });

           SelectList AgentList = new SelectList(Agents, "Id", "AgenName");
           ViewBag.Agents = AgentList;

            String[] allowTerminalConfig = new string[] { "Y", "N" };
            SelectList allowTerminalViewModelBind = new SelectList(allowTerminalConfig);
            ViewBag.AllowTerminalViewModelBind = allowTerminalViewModelBind;


            if (oUser == null)
           {
               return View();
           }
           else
               return View(oUser);
        }

    }
}
