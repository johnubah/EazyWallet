using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Controllers
{
    public class BankController : Controller
    {
        //
        // GET: /Bank/
        [Authorize]
        [HttpPost]
        public ActionResult Index(GeneralSearch search)
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
            TempData["GeneralSearch"] = search;
            return RedirectToAction("Index");

        }
        [HttpGet]
        [Authorize]
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
                return RedirectToAction("Home", "Error");
            }

            GeneralSearch generalSearch = TempData["GeneralSearch"] as Models.GeneralSearch;
            if (generalSearch == null)
            {
                generalSearch = new GeneralSearch();
                generalSearch.SearchCriteria = "ALL";
            }

            generalSearch.Skip = (generalSearch.CurrentPageIndex - 1) * generalSearch.PageSize;


            List<Models.Bank> Banks = null;
            Banks = Processor.BankProcessor.GetAllBanks(generalSearch);


            ViewBag.Banks = Banks;

            List<String> SearchCriteria = new List<string>();

            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Bank Name");
            SearchCriteria.Add("ContactName");
            SearchCriteria.Add("Contact Mobile");

            SelectList SearchList = new SelectList(SearchCriteria);
            ViewBag.SearchList = SearchList;


            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            if (Banks != null && generalSearch.ItemCount > Banks.Count())
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
        [Authorize]
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsManager))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            if (Id == 0)
            {
                return View();
            }
            else
            {
                Models.Bank Bank = Processor.BankProcessor.GetBanksByID(Id);
                if (Bank == null)
                {
                    TempData["ErrorMessage"] = "This bank must have been deleted";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    return View(Bank);
                }
            }
            
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Models.Bank bank)
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
                TempData["ErrorMessage"] = "This bank must have been deleted";
                return RedirectToAction("Home", "Error");
            }

            String ErrorMessage = String.Empty;
            
            if (bank.Id == 0)
            {
                try
                {
                    Processor.BankProcessor.Create(bank, ref ErrorMessage);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ErrorMessage;
                    return View(bank);
                }
            }
            else
            {
                try
                {
                   Processor.BankProcessor.Update(bank);
                   return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    ViewBag.ErrorMessage = ErrorMessage;
                    return View(bank);

                }
            }
        }
    }
}
