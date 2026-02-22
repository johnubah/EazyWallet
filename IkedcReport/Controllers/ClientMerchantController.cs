using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.Models;

namespace WalletReport.Controllers
{
    public class ClientMerchantController : Controller
    {

        [Authorize]
        [HttpGet]
        public ActionResult Index()
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
            GeneralSearch generalSearch = TempData["GeneralSearch"] as Models.GeneralSearch;
            if (generalSearch == null)
            {
                generalSearch = new GeneralSearch();
                generalSearch.SearchCriteria = "ALL";
            }

            generalSearch.Skip = (generalSearch.CurrentPageIndex - 1) * generalSearch.PageSize;


            List<String> SearchCriteria = new List<string>();

            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Client Id");
            SearchCriteria.Add("Client Name");
          

            SelectList SearchList = new SelectList(SearchCriteria);
            ViewBag.SearchList = SearchList;

            var clientMerchantList = WalletReport.Processor.ClientMerchantInformationService.GetList(generalSearch);

            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            if (clientMerchantList != null && generalSearch.ItemCount > clientMerchantList.Count())
            {
                ViewBag.IsPagination = true;
            }
            else
            {
                ViewBag.IsPagination = false;
            }
            ViewBag.ClientBankInformation = clientMerchantList;
            return View(generalSearch);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(GeneralSearch search)
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.IsAgent))
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Error", "Home");
            }

            TempData["GeneralSearch"] = search;


            return RedirectToAction("Index");

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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            if(Id > 0)
            {
                ClientBankInformation clientInformation = WalletReport.Processor.ClientMerchantInformationService.GetClientById(Id);
                if(clientInformation == null)
                {
                    TempData["ErrorMessage"] = "Client id not found";
                    return RedirectToAction("Error", "Home");
                }
                return View(clientInformation);
            }
            return View(new ClientBankInformation());

            
        }


        [Authorize]
        [HttpPost]
        public ActionResult SaveChanges(ClientBankInformation clientBankInformation)
        {
            try
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
                if (String.IsNullOrWhiteSpace(clientBankInformation.Name) ||
                   String.IsNullOrWhiteSpace(clientBankInformation.ClientId) ||
                    String.IsNullOrWhiteSpace(clientBankInformation.SecretKey)
                   )
                {
                    TempData["ErrorMessage"] = "client name or client id or secret cannot be empty ";
                    return RedirectToAction("Error", "Home");
                }

                bool ischangesSave = WalletReport.Processor.ClientMerchantInformationService.SaveChanges(clientBankInformation);

                if (ischangesSave)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while processing request";
                    return RedirectToAction("Error", "Home");
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }
           
          
        }
    }
}