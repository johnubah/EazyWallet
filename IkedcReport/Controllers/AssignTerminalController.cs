using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WalletReport.Models;

namespace WalletReport.Controllers
{
    public class AssignTerminalController : Controller
    {
        [Authorize]
        [HttpGet]
        public ActionResult Activate(String Id)
        {
            bool isactivate = Processor.TerminalProcessor.ActivateTerminal(Id);
            if (isactivate)
            {
                TempData["ErrorMessage"] = String.Format("{0} Terminal was successfully activated", Id);

            }
            else
            {
                TempData["ErrorMessage"] = String.Format("{0} Terminal  activation failed", Id);
            }
            return RedirectToAction("Error", "Home");
        }
        [Authorize]
        [HttpGet]
        public ActionResult Deactivate(String Id)
        {
            bool isDeactivate = Processor.TerminalProcessor.DeActivateTerminal(Id);
            if(isDeactivate)
            {
                TempData["ErrorMessage"] = String.Format("{0} Terminal was successfully deactivated", Id);

            }
            else
            {
                TempData["ErrorMessage"] = String.Format("{0} Terminal  deactivated failed", Id);
            }
            return RedirectToAction("Error", "Home");
        }
        [HttpPost]
        [Authorize]
        public ActionResult Create(Models.TerminalAssignment Terminal)
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank || CurrentUser.IsDealer))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            if (CurrentUser.IsDealer)
            {
                if (!(!String.IsNullOrWhiteSpace(CurrentUser.AllowTerminalConfig) && CurrentUser.AllowTerminalConfig.Equals("Y", StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["ErrorMessage"] = "You do not have permission to view this page";
                    return RedirectToAction("Error", "Home");
                }
            }
            String ErrorMessage = String.Empty;
            try
            {
               
               TerminalAssignment assignment = Processor.TerminalProcessor.GetTerminalBYID(Terminal.TerminalID);

               if (String.IsNullOrEmpty(Terminal.OldTerminalID) || assignment == null)
               {
                   Processor.TerminalProcessor.Create(Terminal, ref ErrorMessage);
               }
               else
               {
                   if (String.IsNullOrEmpty(Terminal.OldTerminalID))
                   {
                       Terminal.OldTerminalID = assignment.TerminalID;
                   }
                   Processor.TerminalProcessor.Update(Terminal);
               }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error Saving Terminal. Please Review your setup";
            }

            return View(Terminal);
            
        }
        [HttpPost]
        public ActionResult ManagePIN(TerminalPIN terminalPIN)
        {

            if (String.IsNullOrWhiteSpace(terminalPIN.Pin))
            {
                TempData["ErrorMessage"] = "Terminal PIN Cannot be empty";
                return RedirectToAction("Error", "Home");
            }
            if (String.IsNullOrWhiteSpace(terminalPIN.ConfirmPin))
            {
                TempData["ErrorMessage"] = "Confirm PIN cannot be empty";
                return RedirectToAction("Error", "Home");
            }
            if (terminalPIN.Pin.Length < 4)
            {
                TempData["ErrorMessage"] = "PIN length must be greater than 3 digit";
                return RedirectToAction("Error", "Home");
            }
            if(terminalPIN.Pin != terminalPIN.ConfirmPin)
            {
                TempData["ErrorMessage"] = "PIN must be equal to Confirm PIN";
                return RedirectToAction("Error", "Home");
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }
            Models.TerminalAssignment Terminal = Processor.TerminalProcessor.GetTerminalBYID(terminalPIN.TerminalID);
            if (Terminal == null)
            {
                TempData["ErrorMessage"] = "Terminal has not been assigned to Agent";
                return RedirectToAction("Error", "Home");
            }


            if(Processor.TerminalProcessor.HasTerminalPIN(terminalPIN.TerminalID))
            {
                bool successful = Processor.TerminalProcessor.UpdateTerminalPIN(terminalPIN);
                if (successful)
                {
                    TempData["ErrorMessage"] = "PIN was successfully Created";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "PIN creation failed. Please contact admin";
                    return RedirectToAction("Error", "Home");
                }
             
            }
            else
            {
                bool successful = Processor.TerminalProcessor.CreateTerminalPIN(terminalPIN);
                if (successful)
                {
                    TempData["ErrorMessage"] = "PIN was successfully Created";
                    return RedirectToAction("Error", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "PIN creation failed. Please contact admin";
                    return RedirectToAction("Error", "Home");
                }


            }


        }
        public ActionResult ManagePIN(String Id)
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
            Models.TerminalAssignment Terminal = Processor.TerminalProcessor.GetTerminalBYID(Id);
            if (Terminal == null)
            {
                TempData["ErrorMessage"] = "Terminal has not been assigned to Agent";
                return RedirectToAction("Error", "Home");
            }
            Dealers dealer = Processor.DealersProcessor.GetDealersByID(Terminal.DealerID);
            ViewBag.Dealer = dealer == null ? String.Empty : dealer.DealerName;

            AgentUser agenUser =  Processor.AgenrtProcessor.GetAgentBYID(Terminal.AgentID);
            ViewBag.Agent = agenUser == null ? String.Empty : agenUser.AgenName;


            ViewBag.HasTerminalPIN = Processor.TerminalProcessor.HasTerminalPIN(Id);

            return View(Terminal);

        }
        [HttpGet]
        [Authorize]
        public ActionResult Create(String Id )
        {


            //we need to create a unique reference and insert into a table



            User CurrentUser = Session["CurrentUser"] as User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }


            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsDealer || CurrentUser.isBank))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page";
                return RedirectToAction("Error", "Home");
            }

            if(CurrentUser.IsDealer)
            {
                if(!(!String.IsNullOrWhiteSpace(CurrentUser.AllowTerminalConfig) && CurrentUser.AllowTerminalConfig.Equals("Y", StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["ErrorMessage"] = "You do not have permission to view this page";
                    return RedirectToAction("Error", "Home");
                }
            }

            

            List<Dealers> DealersInfor = Processor.DealersProcessor.GetAllDealers(CurrentUser);

            DealersInfor.Insert(0, new Dealers() { Id = 0, DealerName = "PLEASE SELECT DEALER" });
            SelectList DealerSelect = new SelectList(DealersInfor, "Id", "DealerName");
            ViewBag.Dealers = DealerSelect;


            List<Models.AgentDisplayVM> Agents = null;

            List<AgentUser> listOfAgents = null;

            if(CurrentUser.IsDealer)
            {
                Agents = Processor.AgenrtProcessor.GetAgentByDealer(CurrentUser.DealerID);
            }
            else
            {
                listOfAgents = Processor.AgenrtProcessor.GetALLAGent();
                Agents = (from c in listOfAgents
                          select new AgentDisplayVM()
                          {
                              Id = c.Id,
                              AgenName = c.AgenName
                          }).ToList();
            }
            

          

            Models.TerminalAssignment Terminal = null;
            if (!String.IsNullOrWhiteSpace(Id))
            {
                Terminal = Processor.TerminalProcessor.GetTerminalBYID(Id);
                ViewBag.OldTerminalID = Id;

            }
            else
            {
                Terminal = new TerminalAssignment();


            }
            if(Terminal != null && Terminal.DealerID > 0)
            {
                ViewBag.DealerID = Terminal.DealerID;
            }
            if (Terminal != null && Terminal.AgentID > 0)
            {
                ViewBag.AgentID = Convert.ToInt32(Terminal.AgentID.ToString());
            }
            else
            {
                Agents.Add(new AgentDisplayVM() { Id = 0, AgenName = "Please Select an Agent" });
            }

            SelectList AgentSelect = new SelectList(Agents, "Id", "AgenName");
            ViewBag.AgentListView = AgentSelect;

            return View(Terminal);
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.IsDealer))
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
            if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.IsDealer || CurrentUser.isBank))
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


            List<Models.TerminalAssignmentVM> Terminals = null;
            Terminals = Processor.TerminalProcessor.GetAllTerminal(generalSearch);


            ViewBag.Terminals = Terminals;

            List<String> SearchCriteria = new List<string>();

            SearchCriteria.Add("ALL");
            SearchCriteria.Add("Agent Name");
            SearchCriteria.Add("Dealer Name");
            SearchCriteria.Add("Terminal ID");

            SelectList SearchList = new SelectList(SearchCriteria);
            ViewBag.SearchList = SearchList;


            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            if (Terminals != null && generalSearch.ItemCount > Terminals.Count())
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
