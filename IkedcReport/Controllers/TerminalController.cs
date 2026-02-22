using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.DBConnector;
using WalletReport.Models;
using WalletReport.Processor;

namespace WalletReport.Controllers
{
    public class TerminalController : Controller
    {
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAgent(Int64 DealerID)
        {
          List<AgentUser> AgentUser =  Processor.AgenrtProcessor.GetFullAgentByDealer(DealerID);

            return Json(AgentUser,JsonRequestBehavior.AllowGet);
        }
        
        //
        // GET: /Terminal/
        [Authorize]
        [HttpPost]
        public ActionResult Create(Models.Terminal terminal)
        {

         

            String ErrorMessage = String.Empty;
            try
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
                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsManager))
                {
                    ViewData["ErrorMessage"] = "You do not have permission to view this page";
                    return RedirectToAction("Error", "Home");
                }
                if (terminal.Id == 0)
                {
                    terminal.Create();
                }
                else
                {
                    terminal.Update();
                }
                return RedirectToAction("TerminalList");
            }
            catch (OpenDBException ex)
            {
                ErrorMessage = ex.Message;
                TempData["ErrorMessage"] = ErrorMessage;
            }
            catch (UpdateException ex)
            {
                ErrorMessage = ex.Message;
                TempData["ErrorMessage"] = ErrorMessage;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                TempData["ErrorMessage"] = ErrorMessage;
            }
            if (!String.IsNullOrEmpty(ErrorMessage))
            {
                return RedirectToAction("Error", "Home");
            }
            return View(terminal);
        }
         [Authorize]
        public ActionResult Create(Int64 Id = 0)
        {
            //if (Session["CurrentUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}
            //User CurrentUser = (User)Session["CurrentUser"];
            //if (CurrentUser.ForceChangePassword)
            //{
            //    return RedirectToAction("ChangePassword", "Account");
            //}

            //if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsManager))
            //{
            //    ViewData["ErrorMessage"] = "You do not have permission to view this page";
            //    return RedirectToAction("Error", "Home");
            //}

            //List<Dealers> dealers = DealersProcessor.GetAllDealers();
            ////dealers.Insert(0, new Dealers() { Id = 0, DealerName = "SELECT" });
            //SelectList selectList = new SelectList(dealers, "Id", "DealerName");
            //ViewBag.Dealers = selectList;


            //List<Models.AgentUser> Agents = Processor.UserProcessor.GetAllAgents();
            //SelectList selectListAgent = new SelectList(Agents, "AgentID", "DisplayName");
            //ViewBag.Agents = selectListAgent;

            //ViewBag.ErrorMessage = String.Empty;
            //if (Id == 0)
            //{
            //    return View();
            //}
            //else
            //{
            //    Models.Terminal terminal = Processor.TerminalProcessor.GetTerminalByID(Id);

            //    if (terminal == null)
            //    {
            //        ViewData["ErrorMessage"] = "Was not able to locate Terminal";
            //        return RedirectToAction("Error", "Home");
            //    }
            //    else
            //    {
            //        return View(terminal);
            //    }
            //}

            return View();
        }
         [Authorize]
        [HttpPost]
         public ActionResult TerminalList(Models.GeneralSearch search)
         {
             TempData["GeneralSearch"] = search;
             return RedirectToAction("TerminalList");
         }
         [Authorize]
        public ActionResult TerminalList()
        {

            //if (Session["CurrentUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}
            //User CurrentUser = (User)Session["CurrentUser"];
            //if (CurrentUser.ForceChangePassword)
            //{
            //    return RedirectToAction("ChangePassword", "Account");
            //}
            //if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsManager))
            //{
            //    ViewData["ErrorMessage"] = "You do not have permission to view this page";
            //    return RedirectToAction("Error", "Home");
            //}

            //GeneralSearch generalSearch = TempData["GeneralSearch"] as Models.GeneralSearch;


           

            //if (generalSearch == null)
            //{
            //    generalSearch = new GeneralSearch();
            //    generalSearch.SearchCriteria = "ALL";
            //}

            //generalSearch.Skip = (generalSearch.CurrentPageIndex - 1) * generalSearch.PageSize;
            //try
            //{
            //    List<Models.Terminal> terminals = Processor.TerminalProcessor.GetAllTerminal(generalSearch);
            //    ViewBag.Terminals = terminals;


            //    List<String> SearchCriteria = new List<string>();

            //    SearchCriteria.Add("ALL");
            //    SearchCriteria.Add("TerminalID");
            //    SearchCriteria.Add("Description");

            //    SelectList SearchList = new SelectList(SearchCriteria);

            //    ViewBag.SearchList = SearchList;



            //    SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            //    ViewBag.PageSizeSELECT = PageSizeSELECT;


            //    if (terminals != null && generalSearch.ItemCount > terminals.Count())
            //    {
            //        ViewBag.IsPagination = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsPagination = false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
            return View();


        }

    }
}
