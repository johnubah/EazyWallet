using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WalletReport.DBConnector;
using WalletReport.Models;
using WalletReport.Processor;

namespace WalletReport.Controllers
{
    public class WalletController : Controller
    {
        //
        // GET: /Wallet/

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult NotifyCustomerDealer(String reference)
        {

            Models.NotifyResponse response = new NotifyResponse();

            User oUser = Session["CurrentUser"] as Models.User;

            try
            {
                if (oUser == null)
                {
                    response.ResponseCode = "96";
                    response.Description = "Security Violation";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                if (!(oUser.IsDealer))
                {
                    response.ResponseCode = "16";
                    response.Description = "Security Violation";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                if (oUser.ForceChangePassword)
                {
                    response.ResponseCode = "16";
                    response.Description = "Security Violation";

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    String ConfirmationCode = Processor.WalletProcessor.GenerateReference();
                    String MobileNumber = oUser.Mobile;
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    builder.Append("Please enter");
                    builder.Append(String.Format(" your confirmation code\n {0} on the confirmation code window", ConfirmationCode));
                    builder.Append("\nThanks");

                    Session["ConfirmationCode"] = ConfirmationCode;

                    if (!String.IsNullOrWhiteSpace(oUser.EmailAddress))
                    {
                        NotificationManager.NotifyCustomer(builder, oUser.EmailAddress, "Wallet Credit Confirmation Code");
                        response.ResponseCode = "00";
                        response.Description = "Successful";
                    }
                    else
                    {
                        response.ResponseCode = "00";
                        response.Description = "Successful";

                    }
                    if (!String.IsNullOrWhiteSpace(MobileNumber))
                    {
                        MobileNumber = Processor.WalletProcessor.FormatMobileNumber(MobileNumber);

                        try
                        {
                            String requestParam = String.Format("http://www.smslive247.com/http/index.aspx?cmd=sendquickmsg&owneremail=ubahjohn@yahoo.co.uk&subacct=jubah@smslive&subacctpwd=p@ssw0rd1478&message={0}&sender=Confirm&sendto={1}&msgtype=0", builder.ToString(), MobileNumber);
                            WebRequest request = HttpWebRequest.Create(requestParam);
                            HttpWebRequest req = (HttpWebRequest)request;
                            req.Method = "GET";
                            req.Timeout = 30000;

                            using (System.IO.Stream notifyStream = req.GetResponse().GetResponseStream())
                            {
                                using (System.IO.StreamReader reader = new System.IO.StreamReader(notifyStream))
                                {
                                    String s = reader.ReadLine();
                                    if (s.Contains("OK"))
                                    {
                                        response.ResponseCode = "00";
                                        response.Description = "Successful";


                                    }
                                    else
                                    {
                                        if(!(!String.IsNullOrWhiteSpace(response.ResponseCode) && response.ResponseCode == "00"))
                                        {
                                            response.ResponseCode = "20";
                                            response.Description = "Message Failed";
                                        }
                                       
                                    }
                                }
                            }
                        }
                        catch (Exception notifyEx)
                        {
                            Logger.WriteToLog("sms:" + notifyEx.Message);
                        }

                    }
                  
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = "06";
                response.Description = "Error";

            }
            return Json(response, JsonRequestBehavior.AllowGet);





        }
        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult NotifyCustomer(String reference)
        {

            
            Models.NotifyResponse response = new NotifyResponse();

            User oUser = Session["CurrentUser"] as Models.User;

            try
            {
                Logger.WriteToLog("NotifyCustomer:" + reference);
                if (oUser == null)
                {
                    response.ResponseCode = "96";
                    response.Description = "Security Violation";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                if (!(oUser.IsSuperUserOrAdmin || oUser.isBank || oUser.IsEtop))
                {
                    response.ResponseCode = "16";
                    response.Description = "Security Violation";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                if (oUser.ForceChangePassword)
                {
                    response.ResponseCode = "16";
                    response.Description = "Security Violation";

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    String ConfirmationCode = Processor.WalletProcessor.GenerateReference();
                    String MobileNumber = oUser.Mobile;
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    builder.Append("Please enter");
                    builder.Append(String.Format(" your confirmation code\n {0} on the confirmation code window", ConfirmationCode));
                    builder.Append("\nThanks");

                    Session["ConfirmationCode"] = ConfirmationCode;
                    if (!String.IsNullOrWhiteSpace(oUser.EmailAddress))
                    {
                        NotificationManager.NotifyCustomer(builder, oUser.EmailAddress, "Wallet Credit Confirmation Code");
                        response.ResponseCode = "00";
                        response.Description = "Successful";
                    }
                    else
                    {
                        Logger.WriteToLog("Email Address is empty");
                    }
                    if (!String.IsNullOrWhiteSpace(MobileNumber))
                    {
                        MobileNumber = Processor.WalletProcessor.FormatMobileNumber(MobileNumber);

                        try
                        {
                            String requestParam = String.Format("http://www.smslive247.com/http/index.aspx?cmd=sendquickmsg&owneremail=ubahjohn@yahoo.co.uk&subacct=jubah@smslive&subacctpwd=p@ssw0rd1478&message={0}&sender=Confirm&sendto={1}&msgtype=0", builder.ToString(), MobileNumber);
                            WebRequest request = HttpWebRequest.Create(requestParam);
                            HttpWebRequest req = (HttpWebRequest)request;
                            req.Method = "GET";
                            req.Timeout = 30000;

                            using (System.IO.Stream notifyStream = req.GetResponse().GetResponseStream())
                            {
                                using (System.IO.StreamReader reader = new System.IO.StreamReader(notifyStream))
                                {
                                    String s = reader.ReadLine();
                                    if (s.Contains("OK"))
                                    {
                                        response.ResponseCode = "00";
                                        response.Description = "Successful";


                                    }
                                    else
                                    {
                                        response.ResponseCode = "20";
                                        response.Description = "Message Failed";
                                    }
                                }
                            }
                        }
                        catch (Exception notifyEx)
                        {

                        }

                    }

                    else
                    {
                        response.ResponseCode = "00";
                        response.Description = "Successful";

                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = "06";
                response.Description = "Error";
                Logger.WriteToLog("Error Notifying Customer:" +ex.Message);

            }
            return Json(response, JsonRequestBehavior.AllowGet);





        }
        [Authorize]
        [HttpPost]
        public ActionResult Index(GeneralSearch search)
        {
            User oUser = Session["CurrentUser"] as Models.User;

            if (oUser == null)
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            if (oUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(oUser.IsSuperUserOrAdmin || oUser.IsEtop || oUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            TempData["GeneralSearch"] = search;


            return RedirectToAction("Index");

        }
        [HttpGet]
        public ActionResult Index()
        {

            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            User CurrentUser = (User)Session["CurrentUser"];
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            GeneralSearch generalSearch = TempData["GeneralSearch"] as Models.GeneralSearch;
            if (generalSearch == null)
            {
                generalSearch = new GeneralSearch();
                generalSearch.SearchCriteria = "ALL";
            }

            generalSearch.Skip = (generalSearch.CurrentPageIndex - 1) * generalSearch.PageSize;


            List<Models.WalletLoadRequestVM> WalletRequests = null;
            WalletRequests = Processor.WalletProcessor.GetAllWallet(generalSearch);
            ViewBag.WalletRequests = WalletRequests;


            List<String> SearchCriteria = new List<string>();


            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Dealer Name");
            SearchCriteria.Add("Reference");
            SearchCriteria.Add("Description");

            SelectList SearchList = new SelectList(SearchCriteria);
            ViewBag.SearchList = SearchList;

            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            if (WalletRequests != null && generalSearch.ItemCount > WalletRequests.Count())
            {
                ViewBag.IsPagination = true;
            }
            else
            {
                ViewBag.IsPagination = false;
            }
            return View(generalSearch);
        }
        [HttpGet]
        public ActionResult Load(Int64 id = 0)
        {

            ViewBag.Disable = false;
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            User CurrentUser = (User)Session["CurrentUser"];
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            if (id == 0)
            {
                String Reference = String.Format("{0}{1}", Guid.NewGuid(), Processor.WalletProcessor.GenerateReference());

                if (!Processor.WalletProcessor.InsertHistoryLoadReference(Reference, CurrentUser.Id))
                {
                    TempData["ErrorMessage"] = "An Error Occurred while Processing your Request. Please Try again";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    ViewBag.IDReference = Reference;
                }
            }
            else
            {
                ViewBag.IDReference = String.Empty;
            }

            List<Dealers> DealersInfor = Processor.DealersProcessor.GetAllDealers(CurrentUser);
            if(DealersInfor.Count() == 0)
            {
                TempData["ErrorMessage"] = "No Dealer has been setup. Please setup dealers";
                return RedirectToAction("Error", "Home");
            }
            SelectList DealerSelect = new SelectList(DealersInfor, "Id", "DealerName");
            ViewBag.Dealers = DealerSelect;

            

            if (id > 0)
            {
                Models.WalletLoadRequest walletLoad = Processor.WalletProcessor.GetLoad(id);

                if (walletLoad == null)
                {
                    return RedirectToAction("Error", "Home");
                }

                ViewBag.Disable = true;
                return View(walletLoad);
                
            }
            else
            {
                ViewBag.Disable = false;
                return View();
            }
           
        }
        [HttpGet]
        public ActionResult LoadAgent(Int64 id = 0)
        {

            ViewBag.Disable = false;
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            User CurrentUser = (User)Session["CurrentUser"];
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            if (id == 0)
            {
                String Reference = String.Format("{0}{1}", Guid.NewGuid(), Processor.WalletProcessor.GenerateReference());

                if (!Processor.WalletProcessor.InsertHistoryLoadReference(Reference, CurrentUser.Id))
                {
                    TempData["ErrorMessage"] = "An Error Occurred while Processing your Request. Please Try again";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    ViewBag.IDReference = Reference;
                }
            }
            else
            {
                ViewBag.IDReference = String.Empty;
            }

            List<Dealers> DealersInfor = Processor.DealersProcessor.GetAllDealers(CurrentUser);
            if (DealersInfor.Count() == 0)
            {
                TempData["ErrorMessage"] = "No Dealer has been setup. Please setup dealers";
                return RedirectToAction("Error", "Home");
            }
            DealersInfor.Insert(0, new Dealers() { Id = 0, DealerName = "Please Select" });
            SelectList DealerSelect = new SelectList(DealersInfor, "Id", "DealerName");
            ViewBag.Dealers = DealerSelect;



            if (id > 0)
            {
                Models.WalletLoadRequest walletLoad = Processor.WalletProcessor.GetLoad(id);

                if (walletLoad == null)
                {
                    return RedirectToAction("Error", "Home");
                }

               
                return View(walletLoad);

            }
            else
            {
               
                return View();
            }

        }
        [HttpGet]
        public ActionResult LoadSuccess()
        {
            String Message = TempData["Message"] as String;

            if (!String.IsNullOrEmpty(Message))
            {
                ViewBag.Message = Message;
            }
            return View();
        }
        [HttpPost]
        public ActionResult Load(Models.WalletLoadRequest WalletLoad)
        {



            //we want to varify that the session Id was correct with what was sent
            User CurrentUser = Session["CurrentUser"] as Models.User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.isBank || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            // the user must be a bank

            String ConfirmationCode = Session["ConfirmationCode"] as String;
            if (String.IsNullOrEmpty(ConfirmationCode))
            {
                TempData["ErrorMessage"] = "Your Confirmation code has expired. Please try again";
                return RedirectToAction("Error", "Home");


            }
            else
            {


                if (Processor.WalletProcessor.ConfirmCode(WalletLoad, ConfirmationCode))
                {
                    WalletLoad.BankId = 1;
                    WalletLoad.CreateDate = DateTime.Now;
                    String ErrorMessage = String.Empty;


                    if (Processor.WalletProcessor.ISValidateLoadReference(WalletLoad.IDReference, CurrentUser))
                    {
                        //Create another table for duplicate check

                        if(!Processor.WalletProcessor.HasDuplicateTransaction(WalletLoad.IDReference))
                        {
                            //
                            if (Processor.WalletProcessor.CreditWallet(WalletLoad,CurrentUser, ref ErrorMessage))
                            {
                                Session["ConfirmationCode"] = null;
                                TempData["Message"] = "Your wallet load was successful";
                                return RedirectToAction("LoadSuccess");
                            }
                            else
                            {
                                Processor.WalletProcessor.DeleteDuplicateReference(WalletLoad.IDReference);
                                TempData["ErrorMessage"] = ErrorMessage;
                                return RedirectToAction("Error", "Home");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Please confirm that these transaction has not already been posted";
                            return RedirectToAction("Error", "Home");
                        }
                       
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid Wallet Reference  ";
                        return RedirectToAction("Error", "Home");
                    }


                }
                else
                {
                    TempData["ErrorMessage"] = "Wrong confirmation code entered. Please try again";
                    return RedirectToAction("Error", "Home");
                }
            }


        }
        [HttpPost]
        public ActionResult LoadAgent(Models.WalletLoadRequest WalletLoad)
        {



            //we want to varify that the session Id was correct with what was sent
            User CurrentUser = Session["CurrentUser"] as Models.User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.isBank || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            // the user must be a bank

            String ConfirmationCode = Session["ConfirmationCode"] as String;
            if (String.IsNullOrEmpty(ConfirmationCode))
            {
                TempData["ErrorMessage"] = "Your Confirmation code has expired. Please try again";
                return RedirectToAction("Error", "Home");


            }
            else
            {


                if (Processor.WalletProcessor.ConfirmCode(WalletLoad, ConfirmationCode))
                {
                    WalletLoad.BankId = 1;
                    WalletLoad.CreateDate = DateTime.Now;
                    String ErrorMessage = String.Empty;


                    if (Processor.WalletProcessor.ISValidateLoadReference(WalletLoad.IDReference, CurrentUser))
                    {
                        //Create another table for duplicate check

                        if (!Processor.WalletProcessor.HasDuplicateTransaction(WalletLoad.IDReference))
                        {
                            //
                            //AgentUser ogentUser = Processor.AgenrtProcessor.GetAgentBYID(WalletLoad.AgentId);
                            if (Processor.WalletProcessor.CreditAgentWallet(WalletLoad, CurrentUser, ref ErrorMessage))
                            {
                                Session["ConfirmationCode"] = null;
                                TempData["Message"] = "Your wallet load was successful";
                                return RedirectToAction("LoadSuccess");
                            }
                            else
                            {
                                Processor.WalletProcessor.DeleteDuplicateReference(WalletLoad.IDReference);
                                TempData["ErrorMessage"] = ErrorMessage;
                                return RedirectToAction("Error", "Home");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Please confirm that these transaction has not already been posted";
                            return RedirectToAction("Error", "Home");
                        }

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid Wallet Reference  ";
                        return RedirectToAction("Error", "Home");
                    }


                }
                else
                {
                    TempData["ErrorMessage"] = "Wrong confirmation code entered. Please try again";
                    return RedirectToAction("Error", "Home");
                }
            }


        }

        public ActionResult Transaction()
        {
            return View();
        }

    }
}
