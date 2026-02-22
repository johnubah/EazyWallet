using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.Models;

namespace WalletReport.Controllers
{
    public class ClientController : Controller
    {
        //
        // GET: /Client/
        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            User CurrentUser = Session["CurrentUser"] as User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }


            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!CurrentUser.IsSuperUserOrAdmin)
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Error", "Home");
            }
            List<Models.ClientIP> clientIPs = Processor.WalletProcessor.GetClientIP();
            return View(clientIPs);
        }
        [Authorize]
        [HttpGet]
        public ActionResult Delete(int Id)
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
            if (!CurrentUser.IsSuperUserOrAdmin)
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Error", "Home");
            }
            Models.ClientIP clientIP = Processor.WalletProcessor.GetClientIP(Id);
            
           

            if(clientIP == null)
            {
                TempData["ErrorMessage"] = "This record was not found.";
                return RedirectToAction("Error", "Home");
            }
            bool isUpdated = Processor.WalletProcessor.DeleteClient(Id);
            if (isUpdated)
            {
                TempData["ErrorMessage"] = String.Format("Client IP {0} successfully deleted", clientIP.Address);
                return RedirectToAction("Error", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = String.Format("System was unable to delete Client IP {0} ", clientIP.Address);
                return RedirectToAction("Error", "Home");
            }
        }
        [Authorize]
        [HttpGet]
        public ActionResult Create(int? Id)
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
            if (!CurrentUser.IsSuperUserOrAdmin)
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Error", "Home");
            }
            Models.ClientIP clientIP = null;
            if(Id.HasValue)
            {
                clientIP  = Processor.WalletProcessor.GetClientIP(Id.Value);
                if(clientIP == null)
                {
                    TempData["ErrorMessage"] = "Client IP information not found";
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                clientIP = new Models.ClientIP();
            }
            List<String> debitCreditType = new List<string>() { "DR","CR"};
            if(!String.IsNullOrWhiteSpace(clientIP.Trantype))
            {
                ViewBag.Trantype = clientIP.Trantype;
            }
            ViewBag.ListOfTransactionType = new SelectList(debitCreditType);

            return View(clientIP);
        }
        [Authorize]
        [HttpPost]

        public ActionResult Create(Models.ClientIP clientIP)
        {
            bool isUpdated = false;
            if (clientIP.Id > 0)
            {
                isUpdated = Processor.WalletProcessor.UpdateClientIPAddress(clientIP);
               
            }
            else
            {
                isUpdated = Processor.WalletProcessor.CreateClientIPAddress(clientIP);

            }

            if (isUpdated)
            {
                TempData["ErrorMessage"] = String.Format("Client IP {0} successfully updated", clientIP.Address);
                return RedirectToAction("Error", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = String.Format("System was unable to update Client IP {0} ", clientIP.Address);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
