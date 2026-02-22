using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Controllers
{
    public class AgentController : Controller
    {
        //
        // GET: /Agent/

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadAgentByDealer(long Id)
        {

            List<Models.AgentDisplayVM> agents = Processor.AgenrtProcessor.GetAgentByDealer(Id);


            return Json(agents, JsonRequestBehavior.AllowGet);

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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.IsDealer || CurrentUser.isBank))
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


            List<Models.AgentUser> Agents = null;
            Agents = Processor.AgenrtProcessor.GetAllAgent(generalSearch);


            ViewBag.Agents = Agents;

            List<String> SearchCriteria = new List<string>();

            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Agent Name");
            SearchCriteria.Add("ContactName");
            SearchCriteria.Add("Contact Mobile");

            SelectList SearchList = new SelectList(SearchCriteria);
            ViewBag.SearchList = SearchList;


            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            if (Agents != null && generalSearch.ItemCount > Agents.Count())
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.IsDealer || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            List<Dealers> DealersInfor = null;
            DealersInfor =  Processor.DealersProcessor.GetAllDealers(CurrentUser);
            SelectList DealerSelect = new SelectList(DealersInfor, "Id", "DealerName");

            ViewBag.Dealers = DealerSelect;

            List<ListItem> listOfPostingMode = new List<ListItem>();
            ListItem PostingMode = new ListItem();
            PostingMode.DisplayText = "Post Transaction with Dealer Balance why checking Agent Limit";
            PostingMode.ListValue = "N";

            listOfPostingMode.Add(PostingMode);
            PostingMode = new ListItem();
            PostingMode.DisplayText = "Posting using Agent Balance";
            PostingMode.ListValue = "Y";

            listOfPostingMode.Add(PostingMode);


            SelectList agentPostingMode = new SelectList(listOfPostingMode, "ListValue", "DisplayText");

            ViewBag.PostingModeSelectView = agentPostingMode;




            if (Id == 0)
            {
                Models.AgentUser Agent = new AgentUser();
                return View(Agent);
            }
            else
            {
                Models.AgentUser Agent = Processor.AgenrtProcessor.GetAgentBYID(Id);
                if (Agent == null)
                {
                    TempData["ErrorMessage"] = "Was not able to locate Agent";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    return View(Agent);
                }
            }
            
            
        }
        [Authorize]
        [HttpPost]
        public ActionResult Create(Models.AgentUser Agent)
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

            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.IsDealer || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to this page";
                return RedirectToAction("Home", "Error");
            }

            if(Agent.DealerId == 0)
            {
                TempData["ErrorMessage"] = "An agent must belong to a dealer. Please assign agent to a dealer";
                return RedirectToAction("Home", "Error");
            }
            String ErrorMessage = String.Empty;


            if (Agent.Id == 0)
            {
                try
                {

                    Processor.AgenrtProcessor.Create(Agent, ref ErrorMessage);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    ViewBag.ErrorMessage = ErrorMessage;
                    return View(Agent);

                }
            }
            else
            {
                try
                {

                    Processor.AgenrtProcessor.Update(Agent);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    ViewBag.ErrorMessage = ErrorMessage;
                    return View(Agent);

                }
            }
        }

    }
}
