using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Controllers
{
    [Authorize]
    public class DealersController : Controller
    {

        [AcceptVerbs( HttpVerbs.Get)]
        public JsonResult GetDealerByBank(Int64 Id)
        {

            List<Models.DealerVM> Dealers = Processor.DealersProcessor.GetDealerByBank(Id);

            return Json(Dealers, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Transfer(WalletLoadRequest walletLoad)
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
            if (!(CurrentUser.IsDealer))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            String ConfirmationCode = Session["ConfirmationCode"] as String;
            if (String.IsNullOrEmpty(ConfirmationCode))
            {
                TempData["ErrorMessage"] = "Your Confirmation code has expired. Please try again";
                return RedirectToAction("Error", "Home");
            }
            if (!Processor.WalletProcessor.ConfirmCode(walletLoad, ConfirmationCode))
            {
                TempData["ErrorMessage"] = "Your Confirmation code is incorrect or has expired. Please try again";
                return RedirectToAction("Error", "Home");
            }

            try
            {
                if (Processor.WalletProcessor.Transfer(walletLoad, CurrentUser))
                {
                    TempData["ErrorMessage"] = "Your transfer  is successful";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Your Transfer Request failed";
                    return RedirectToAction("Error", "Home");
                }
            }
            catch(Exception ex)
            {
                if(ex.Message.ToLower().Contains("insufficient"))
                {
                    TempData["ErrorMessage"] = "Insufficient Fund";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Your Transfer Request failed";
                    return RedirectToAction("Error", "Home");
                }

            }
           


        }

        [Authorize]
        [HttpGet]
        public ActionResult Transfer()
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
            if (!(CurrentUser.IsDealer ))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            WalletLoadRequest walletLoad = new WalletLoadRequest();

            var listOfAgentUser = Processor.AgenrtProcessor.GetAgentByDealer(CurrentUser.DealerID);

            //listOfAgentUser[0].Id;
            //listOfAgentUser[0].AgenName;
            listOfAgentUser.Insert(0, new AgentDisplayVM() { Id = 0, AgenName = "Please Select" });
            SelectList selectAgentList = new SelectList(listOfAgentUser, "Id", "AgenName");
    
            ViewBag.AgentSelect = selectAgentList;

            return View(walletLoad);

        }

        [Authorize]
        [HttpGet]
        public ActionResult Create(Int64 Id = 0)
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            List<Bank> Banks = Processor.BankProcessor.GetALLBanks(CurrentUser);

            if (CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank)
            {
                Banks.Insert(0,new Bank() { Id = 0, BankName = "ALL Banks" });
            }
            SelectList BankSelect = new SelectList(Banks, "Id", "BankName");
            ViewBag.Banks = BankSelect;


            List<ClientBankInformation> clientInformation = Processor.ClientMerchantInformationService.GetList(new GeneralSearch()
            {
                SearchCriteria = "ALL"
            });

            clientInformation.Insert(0, new ClientBankInformation() { Id = 0, Name = "Select Merchant" });
            ViewBag.MerchantsList = new SelectList(clientInformation, "Id", "Name"); 




            if (Id == 0)
            {
                return View();
            }
            else
            {
                Models.Dealers dealer = Processor.DealersProcessor.GetDealersByID(Id);
                if (dealer == null)
                {
                    TempData["ErrorMessage"] = "Was not able to locate Dealer";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    if (CurrentUser.isBank)
                    {
                        if (dealer.BankId > 0)
                        {
                            if (CurrentUser.BankID != dealer.BankId)
                            {
                                TempData["ErrorMessage"] = "You do not have permission to view this bank";
                                return RedirectToAction("Error", "Home");
                            }
                        }
                    }
                    return View(dealer);
                }
            }
            
        }
        [Authorize]
        [HttpPost]
        public ActionResult Create(Models.Dealers dealers)
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop  || CurrentUser.isBank ))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            if(dealers.MerchantId == 0)
            {
                TempData["ErrorMessage"] = "Please select a merchant for the dealer";
                return RedirectToAction("Error", "Home");
            }
            String ErrorMessage = String.Empty;
            try
            {
                //if (CurrentUser.isBank)
                //{
                //    if (dealers.BankId > 0)
                //    {
                //        if (dealers.BankId != CurrentUser.BankID)
                //        {
                //            TempData["ErrorMessage"] = "You have selected wrong bank";
                //            return RedirectToAction("Error", "Home");
                //        }
                //    }
                //}
                //if(String.IsNullOrWhiteSpace( dealers.SettleAccountNumber))
                //{
                //    TempData["ErrorMessage"] = "Account Number is empty";
                //    return RedirectToAction("Error", "Home");
                //}
                //if (dealers.SettleAccountNumber.Trim().Length < 10)
                //{
                //    TempData["ErrorMessage"] = "Account Number is not correct. Account Number Length must not be less than 10";
                //    return RedirectToAction("Error", "Home");
                //}
                if (dealers.Id == 0)
                {
                    //we need to verify the account number



                    dealers.Create();
                }
                else
                {
                    bool isSettlementAccountExists = Processor.DealersProcessor.SettlementAccountExistsForOtherDealer(dealers);
                    if (isSettlementAccountExists)
                    {
                        TempData["ErrorMessage"] = "Settlemt account aleady exists. Please Account must be unique";
                        return RedirectToAction("Error", "Home");
                    }
                    dealers.Update();
                }

                return RedirectToAction("DealerList");
            }
            catch (OpenDBException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (UpdateException ex)
            {
                ErrorMessage = ex.Message;
                TempData["ErrorMessage"] = ErrorMessage;
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                TempData["ErrorMessage"] = ErrorMessage;
                return RedirectToAction("Error", "Home");
            }
            return View(dealers);
        }
        [Authorize]
        [HttpPost]
        public ActionResult DealerList(Models.GeneralSearch search)
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Error", "Home");
            }

            TempData["GeneralSearch"] = search;
            return RedirectToAction("DealerList");
        }
        [Authorize]
        [HttpGet]
        public ActionResult DealerList()
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank || CurrentUser.IsManager || CurrentUser.IsDealer))
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Error", "Home");
            }


            GeneralSearch generalSearch = TempData["GeneralSearch"] as Models.GeneralSearch;
            if (generalSearch == null)
            {
                generalSearch = new GeneralSearch();
                generalSearch.SearchCriteria = "ALL";
            }

            generalSearch.Skip = (generalSearch.CurrentPageIndex - 1) * generalSearch.PageSize;


            List<Models.Dealers> dealers = null;
            dealers = Processor.DealersProcessor.GetAllDealers(generalSearch);


            ViewBag.Dealers = dealers;

            List<String> SearchCriteria = new List<string>();

            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Dealer Name");
            SearchCriteria.Add("ContactName");
            SearchCriteria.Add("Contact Mobile");

            SelectList SearchList = new SelectList(SearchCriteria);
            ViewBag.SearchList = SearchList;


            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            if (dealers != null && generalSearch.ItemCount > dealers.Count())
            {
                ViewBag.IsPagination = true;
            }
            else
            {
                ViewBag.IsPagination = false;
            }
            return View(generalSearch);
        }

    }
}
