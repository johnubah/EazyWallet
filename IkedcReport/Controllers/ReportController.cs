using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using WalletReport.Models;
using WalletReport.Processor;


namespace WalletReport.Controllers
{
    public class ReportController : Controller
    {
        public ReportController()
        {

        }
        //
        // GET: /Report/
        [HttpPost]
        public ActionResult TransactionDetail(WalletTransaction trans)
        {
            bool canReverse = false;
            User CurrentUser = Session["CurrentUser"] as Models.User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            if (CurrentUser.IsSuperUserOrAdmin)
            {
                try
                {
                    String[] emailAddresses = WebConfigurationManager.AppSettings["BaseEmailAddress"].Split(new char[] { ';', ',' });

                    foreach (String emailAddress in emailAddresses)
                    {
                        if (emailAddress.Trim().Equals(CurrentUser.EmailAddress.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            canReverse = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (canReverse)
            {
                WalletTransaction walletDebit = TransactionQueryProcessor.GetWalletTransactionByID(trans.TranID);

                Models.Response resp = WalletProcessor.ReverseTransaction(walletDebit);
            }
            return View();
        }

        public ActionResult TransactionDetail(String Id)
        {
            User CurrentUser = Session["CurrentUser"] as Models.User;
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            if (CurrentUser.IsSuperUserOrAdmin)
            {
                try
                {
                    String[] emailAddresses = WebConfigurationManager.AppSettings["BaseEmailAddress"].Split(new char[] { ';', ',' });

                    foreach (String emailAddress in emailAddresses)
                    {
                        if (emailAddress.Trim().Equals(CurrentUser.EmailAddress.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            ViewBag.CanReverse = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {

                }


            }


            WalletTransaction transaction = Processor.TransactionQueryProcessor.GetWalletTransactionByID(Id);
            return View(transaction);
        }
        //
        // GET: /Report/

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public void DownloadExcel(FormCollection m)
        {

            Models.SearchModel search = new SearchModel();
            try
            {
                DateTime startdate = DateTime.Now;

               

                if (m["CurrentPageIndex"] != null)
                {
                    search.CurrentPageIndex = Convert.ToInt32(m["CurrentPageIndex"]);
                }
                User oUser = (User)Session["CurrentUser"];

                search.StartDate = m["StartDate"];
                search.EndDate = m["EndDate"];

                if (m["DealerID"] != null && Convert.ToInt64(m["DealerID"]) > 0)
                {
                    search.DealerID = Convert.ToInt64(m["DealerID"]);
                }
                else
                {
                    search.DealerID = 0;
                }


                if (m["AgentID"] != null && m["AgentID"].ToString() != "0")
                {
                    search.AgentID = Convert.ToInt32(m["DistrictID"]);
                }

                DateTime? Startdate = IkedcHeper.GetDate(search.StartDate);
                DateTime? EndDate = IkedcHeper.GetDate(search.EndDate);

                TempData["Startdate"] = Startdate.Value.ToString("dd/MM/yyyy");
                TempData["EndDate"] = EndDate.Value.ToString("dd/MM/yyyy");


                if (m["DealerID"] != null && m["DealerID"].ToString() != "0")
                {
                    search.DealerID = Convert.ToInt32(m["DealerID"]);
                }


                if (m["BankID"] != null && m["BankID"].ToString() != "0")
                {
                    search.BankID = Convert.ToInt64(m["BankID"]);
                }





                if (EndDate.HasValue)
                {
                    EndDate = new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, 23, 59, 59);

                }

                int CurrentPageIndex = search.CurrentPageIndex;
                if (CurrentPageIndex == 0)
                {
                    CurrentPageIndex = 1;
                }

                search.Skip = (CurrentPageIndex - 1) * search.PageSize;

                search.CurrentPageIndex = CurrentPageIndex;

                search.CustomSearch = m["CustomSearch"].ToString();
                search.CustomSearchValue = m["CustomSearchValue"].ToString();
                List<WalletTransaction> Transactions = TransactionQueryProcessor.GetWalletTransactionDownload(oUser, Startdate.Value, EndDate.Value, search.BankID, search.DealerID, search.AgentID, search);

                System.IO.StringWriter sw = new System.IO.StringWriter();


                DateTime t = DateTime.Now;
                String filename = String.Format("TRANSREPORT_{0}_{1}_{2}_{3}_{4}_{5}_{6}.xls",t.Year.ToString(),t.Month.ToString(),t.Day.ToString(),t.Hour.ToString(),t.Minute.ToString(),t.Second.ToString(),t.Millisecond.ToString());

                Response.ClearContent();
                Response.AddHeader("content-disposition", String.Format("attachment;filename={0}", filename));
                Response.ContentType = "application/excel";




                if (Transactions != null)
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();

                    String AccountNumber = String.Empty;
                    String TransactionAmount = String.Empty;
                    String ReceiptNumber = String.Empty;
                    sw.Write("<table>");
                    sw.Write("<thead>");
                    sw.Write("<tr>");
                    sw.Write("<th>SN</th>");
                    sw.Write("<th>Date</th>");
                    sw.Write("<th>Terminal</th>");
                    sw.Write("<th>Customer Ref</th>");
                    sw.Write("<th>Reference</th>");
                    sw.Write("<th>Description</th>");
                    sw.Write("<th>Transaction Amt</th>");
                    sw.Write("<th>TranType</th>");
                    sw.Write("<th>Dealer Name</th>");
                    sw.Write("</tr>");
                    sw.Write("</thead>");
                    sw.Write("<tbody>");

                    int count = 0;
                    foreach (Models.WalletTransaction transact in Transactions)
                    {

                        sw.Write("<tr>");
                        sw.Write(String.Format("<td>{0}/td>", (++count).ToString()));
                        sw.Write(String.Format("<td>{0}</td>", transact.TransactionDateBuild));
                        sw.Write(String.Format("<td>{0}</td>",transact.TerminalID));
                        sw.Write(String.Format("<td>{0}</td>",transact.CustomerRef));
                        sw.Write(String.Format("<td>{0}</td>",transact.Reference));
                        sw.Write(String.Format("<td>{0}</td>", transact.Description));

                       sw.Write(String.Format("<td>{0}</td>",transact.LastTransactionAmt));
                       sw.Write(String.Format("<td>{0}</td>", transact.TransactionType));
                       sw.Write(String.Format("<td>{0}</td>", transact.DealerName));
                    }
                    sw.Write("</table>");
                }
              Response.Write(sw.ToString());
              Response.End();

            }
            catch (Exception ex)
            {
            }
        }

        [Authorize]
        [HttpPost]
        public void DownloadExcelArch(FormCollection m)
        {

            Models.SearchModel search = new SearchModel();
            try
            {
                DateTime startdate = DateTime.Now;



                if (m["CurrentPageIndex"] != null)
                {
                    search.CurrentPageIndex = Convert.ToInt32(m["CurrentPageIndex"]);
                }
                User oUser = (User)Session["CurrentUser"];

                search.StartDate = m["StartDate"];
                search.EndDate = m["EndDate"];

                if (m["DealerID"] != null && Convert.ToInt64(m["DealerID"]) > 0)
                {
                    search.DealerID = Convert.ToInt64(m["DealerID"]);
                }
                else
                {
                    search.DealerID = 0;
                }


                if (m["AgentID"] != null && m["AgentID"].ToString() != "0")
                {
                    search.AgentID = Convert.ToInt32(m["DistrictID"]);
                }

                DateTime? Startdate = IkedcHeper.GetDate(search.StartDate);
                DateTime? EndDate = IkedcHeper.GetDate(search.EndDate);

                TempData["Startdate"] = Startdate.Value.ToString("dd/MM/yyyy");
                TempData["EndDate"] = EndDate.Value.ToString("dd/MM/yyyy");


                if (m["DealerID"] != null && m["DealerID"].ToString() != "0")
                {
                    search.DealerID = Convert.ToInt32(m["DealerID"]);
                }


                if (m["BankID"] != null && m["BankID"].ToString() != "0")
                {
                    search.BankID = Convert.ToInt64(m["BankID"]);
                }





                if (EndDate.HasValue)
                {
                    EndDate = new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, 23, 59, 59);

                }

                int CurrentPageIndex = search.CurrentPageIndex;
                if (CurrentPageIndex == 0)
                {
                    CurrentPageIndex = 1;
                }

                search.Skip = (CurrentPageIndex - 1) * search.PageSize;

                search.CurrentPageIndex = CurrentPageIndex;

                search.CustomSearch = m["CustomSearch"].ToString();
                search.CustomSearchValue = m["CustomSearchValue"].ToString();
                List<WalletTransaction> Transactions = TransactionQueryProcessor.GetWalletTransactionDownloadArch(oUser, Startdate.Value, EndDate.Value, search.BankID, search.DealerID, search.AgentID, search);

                System.IO.StringWriter sw = new System.IO.StringWriter();


                DateTime t = DateTime.Now;
                String filename = String.Format("TRANSREPORT_{0}_{1}_{2}_{3}_{4}_{5}_{6}.xls", t.Year.ToString(), t.Month.ToString(), t.Day.ToString(), t.Hour.ToString(), t.Minute.ToString(), t.Second.ToString(), t.Millisecond.ToString());

                Response.ClearContent();
                Response.AddHeader("content-disposition", String.Format("attachment;filename={0}", filename));
                Response.ContentType = "application/excel";




                if (Transactions != null)
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();

                    String AccountNumber = String.Empty;
                    String TransactionAmount = String.Empty;
                    String ReceiptNumber = String.Empty;
                    sw.Write("<table>");
                    sw.Write("<thead>");
                    sw.Write("<tr>");
                    sw.Write("<th>SN</th>");
                    sw.Write("<th>Date</th>");
                    sw.Write("<th>Terminal</th>");
                    sw.Write("<th>Customer Ref</th>");
                    sw.Write("<th>Reference</th>");
                    sw.Write("<th>Description</th>");
                    sw.Write("<th>Transaction Amt</th>");
                    sw.Write("<th>TranType</th>");
                    sw.Write("<th>Dealer Name</th>");
                    sw.Write("</tr>");
                    sw.Write("</thead>");
                    sw.Write("<tbody>");

                    int count = 0;
                    foreach (Models.WalletTransaction transact in Transactions)
                    {

                        sw.Write("<tr>");
                        sw.Write(String.Format("<td>{0}/td>", (++count).ToString()));
                        sw.Write(String.Format("<td>{0}</td>", transact.TransactionDateBuild));
                        sw.Write(String.Format("<td>{0}</td>", transact.TerminalID));
                        sw.Write(String.Format("<td>{0}</td>", transact.CustomerRef));
                        sw.Write(String.Format("<td>{0}</td>", transact.Reference));
                        sw.Write(String.Format("<td>{0}</td>", transact.Description));

                        sw.Write(String.Format("<td>{0}</td>", transact.LastTransactionAmt));
                        sw.Write(String.Format("<td>{0}</td>", transact.TransactionType));
                        sw.Write(String.Format("<td>{0}</td>", transact.DealerName));
                    }
                    sw.Write("</table>");
                }
                Response.Write(sw.ToString());
                Response.End();

            }
            catch (Exception ex)
            {
            }
        }
        [Authorize]
        [HttpPost]
        public ActionResult TransReportArch(FormCollection m)
        {

            Models.SearchModel search = new SearchModel();
            try
            {
                DateTime startdate = DateTime.Now;

                if (Session["CurrentUser"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                User CurrentUser = (User)Session["CurrentUser"];
                if (CurrentUser.ForceChangePassword)
                {
                    return RedirectToAction("ChangePassword", "Account");
                }

                if (m["CurrentPageIndex"] != null)
                {
                    search.CurrentPageIndex = Convert.ToInt32(m["CurrentPageIndex"]);
                }
                User oUser = (User)Session["CurrentUser"];

                search.StartDate = m["StartDate"];
                search.EndDate = m["EndDate"];

                if (m["DealerID"] != null && Convert.ToInt64(m["DealerID"]) > 0)
                {
                    search.DealerID = Convert.ToInt64(m["DealerID"]);
                }
                else
                {
                    search.DealerID = 0;
                }


                if (m["AgentID"] != null && m["AgentID"].ToString() != "0")
                {
                    search.AgentID = Convert.ToInt32(m["AgentID"]);
                }

                DateTime? Startdate = IkedcHeper.GetDate(search.StartDate);
                DateTime? EndDate = IkedcHeper.GetDate(search.EndDate);

                TempData["Startdate"] = Startdate.Value.ToString("dd/MM/yyyy");
                TempData["EndDate"] = EndDate.Value.ToString("dd/MM/yyyy");


                if (m["DealerID"] != null && m["DealerID"].ToString() != "0")
                {
                    search.DealerID = Convert.ToInt32(m["DealerID"]);
                }


                if (m["BankID"] != null && m["BankID"].ToString() != "0")
                {
                    search.BankID = Convert.ToInt64(m["BankID"]);
                }





                if (EndDate.HasValue)
                {
                    EndDate = new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, 23, 59, 59);

                }

                int CurrentPageIndex = search.CurrentPageIndex;
                if (CurrentPageIndex == 0)
                {
                    CurrentPageIndex = 1;
                }

                search.Skip = (CurrentPageIndex - 1) * search.PageSize;

                search.CurrentPageIndex = CurrentPageIndex;

                search.CustomSearch = m["CustomSearch"].ToString();
                search.CustomSearchValue = m["CustomSearchValue"].ToString();


                List<WalletTransaction> Transactions = TransactionQueryProcessor.GetWalletTransactionPagingArch(oUser, Startdate.Value, EndDate.Value, search.BankID, search.DealerID, search.AgentID, search);


                if (search.ItemCount > Transactions.Count())
                {
                    TempData["IsPagination"] = "1";
                }

                TempData["Transactions"] = Transactions;
                TempData["Criteria"] = search;
                return RedirectToAction("TransReportArch");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }
        [Authorize]
        [HttpPost]
        public ActionResult TransReport(FormCollection m)
        {

            Models.SearchModel search = new SearchModel();
            try
            {
                DateTime startdate = DateTime.Now;

                if (Session["CurrentUser"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                User CurrentUser = (User)Session["CurrentUser"];
                if (CurrentUser.ForceChangePassword)
                {
                    return RedirectToAction("ChangePassword", "Account");
                }

                if (m["CurrentPageIndex"] != null)
                {
                    search.CurrentPageIndex = Convert.ToInt32(m["CurrentPageIndex"]);
                }
                User oUser = (User)Session["CurrentUser"];

                search.StartDate = m["StartDate"];
                search.EndDate = m["EndDate"];

                if (oUser.IsAgent || oUser.AgentID > 0)
                {
                    search.DealerID = 0;
                    search.AgentID = oUser.AgentID;
                }
                else if (oUser.IsDealer && oUser.DealerID > 0)
                {
                    search.DealerID = oUser.DealerID;
                    if (!String.IsNullOrWhiteSpace(m["AgentID"]) && m["AgentID"].ToString() != "0")
                    {
                        search.AgentID = Convert.ToInt32(m["AgentID"]);
                    }
                }
                else {
                    if (!String.IsNullOrWhiteSpace(m["DealerID"]) && Convert.ToInt64(m["DealerID"]) > 0)
                    {
                        search.DealerID = Convert.ToInt64(m["DealerID"]);
                    }
                    else
                    {
                        search.DealerID = 0;
                    }


                    if (!String.IsNullOrWhiteSpace( m["AgentID"]) && m["AgentID"].ToString() != "0")
                    {
                        search.AgentID = Convert.ToInt32(m["AgentID"]);
                    }
                }
                

                DateTime? Startdate = IkedcHeper.GetDate(search.StartDate);
                DateTime? EndDate = IkedcHeper.GetDate(search.EndDate);

                TempData["Startdate"] = Startdate.Value.ToString("dd/MM/yyyy");
                TempData["EndDate"] = EndDate.Value.ToString("dd/MM/yyyy");


                if (!String.IsNullOrWhiteSpace(m["DealerID"]) && m["DealerID"].ToString() != "0")
                {
                    search.DealerID = Convert.ToInt32(m["DealerID"]);
                }


                if (!String.IsNullOrWhiteSpace(m["BankID"]) && m["BankID"].ToString() != "0")
                {
                    search.BankID = Convert.ToInt64(m["BankID"]);
                }

                if(m["TransactionType"] != null)
                {
                    search.TransactionType = m["TransactionType"].ToString();
                }

                if (EndDate.HasValue)
                {
                    EndDate = new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, 23, 59, 59);

                }

                int CurrentPageIndex = search.CurrentPageIndex;
                if (CurrentPageIndex == 0)
                {
                    CurrentPageIndex = 1;
                }

                search.Skip = (CurrentPageIndex - 1) * search.PageSize;

                search.CurrentPageIndex = CurrentPageIndex;

                search.CustomSearch = m["CustomSearch"].ToString();
                search.CustomSearchValue = m["CustomSearchValue"].ToString();


                List<WalletTransaction> Transactions = TransactionQueryProcessor.GetWalletTransactionPagingV2(oUser, Startdate.Value, EndDate.Value, search.BankID, search.DealerID, search.AgentID, search);
               

                if (search.ItemCount > Transactions.Count())
                {
                    TempData["IsPagination"] = "1";
                }

                TempData["Transactions"] = Transactions;
                TempData["Criteria"] = search;
                return RedirectToAction("TransReport");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }
        [Authorize]
        [HttpGet]
        public ActionResult TransReport()
        {
            String Error = TempData["ErrorMessage"] as String;
            if (!String.IsNullOrEmpty(Error))
            {
                TempData["ErrorMessage"] = Error;
                return RedirectToAction("Error", "Home");
            }
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            User CurrentUser = (User)Session["CurrentUser"];
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }


            User oUser = (User)Session["CurrentUser"];


            List<Models.Bank> banks = null;
            List<Models.Dealers> dealers = null;
            List<Models.AgentUser> Agents = null;


            banks = Processor.BankProcessor.GetBankByUser(oUser);
            if (oUser.IsSuperUserOrAdmin || oUser.IsEtop || oUser.IsManager || oUser.isBank)
            {
                banks.Insert(0, new Bank() { Id = 0, BankName = "ALL" });
            }
            SelectList BankSelect = new SelectList(banks, "Id", "BankName");
            ViewBag.Banks = BankSelect;




            dealers = Processor.DealersProcessor.GetDealersByUser(oUser);
            if (oUser.IsSuperUserOrAdmin || oUser.IsEtop  || oUser.IsManager || oUser.isBank)
            {
                dealers.Insert(0, new Dealers() { Id = 0, DealerName = "ALL" });
            }
            if(dealers == null)
            {
                dealers = new List<Dealers>();
            }
            SelectList DealerSelect = new SelectList(dealers, "Id", "DealerName");
            ViewBag.Dealers = DealerSelect;


            List<SelectListItem> listOfTransactionTypes = new List<SelectListItem>();
            SelectListItem transactionType = new SelectListItem();
            transactionType.Text = "ALL";
            transactionType.Value = "ALL";
            listOfTransactionTypes.Add(transactionType);

            transactionType = new SelectListItem();
            transactionType.Text = "DEBIT";
            transactionType.Value = "DR";
            listOfTransactionTypes.Add(transactionType);

            transactionType = new SelectListItem();
            transactionType.Text = "CREDIT";
            transactionType.Value = "CR";
            listOfTransactionTypes.Add(transactionType);

            SelectList transactionSelectList = new SelectList(listOfTransactionTypes, "Value", "Text");
            ViewBag.TransactionType = transactionSelectList;



            Agents = Processor.AgenrtProcessor.GetAgentByUser(oUser);
            if (oUser.IsSuperUserOrAdmin || oUser.IsEtop || oUser.IsManager || oUser.isBank)
            {
                Agents.Insert(0, new AgentUser() { Id = 0, AgenName = "ALL" });
            }
            else
            {
                if (oUser.IsDealer)
                {
                    Agents.Insert(0, new AgentUser() { Id = 0, AgenName = "ALL" });
                }
            }
            SelectList AgentsSelect = new SelectList(Agents, "Id", "AgenName");
            ViewBag.Agents = AgentsSelect;



            String Startdate = TempData["Startdate"] as String;
            if (String.IsNullOrEmpty(Startdate))
            {
                ViewBag.Startdate = DateTime.Now.ToString("dd/MM/yyyy");
            }
            else
            {
                ViewBag.Startdate = Startdate;
            }
            String EndDate = TempData["EndDate"] as String;
            if (String.IsNullOrEmpty(EndDate))
            {
                ViewBag.EndDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            else
            {
                ViewBag.EndDate = EndDate;
            }


            List<String> AdditionalCriteria = new List<string>();
            AdditionalCriteria.Add("ALL");
            AdditionalCriteria.Add("Terminal ID");
            AdditionalCriteria.Add("Customer Ref");
            AdditionalCriteria.Add("Reference");



            SelectList AdditionalCriteriaSELECT = new SelectList(AdditionalCriteria);
            ViewBag.AdditionalCriteria = AdditionalCriteriaSELECT;
            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            List<WalletTransaction> transactions = TempData["Transactions"] as List<WalletTransaction>;
            Models.SearchModel Search = TempData["Criteria"] as Models.SearchModel;

            if (transactions != null)
            {
                ViewBag.Transaction = transactions;
            }


            String IsPagination = TempData["IsPagination"] as String;
            if (IsPagination != null && IsPagination == "1")
            {
                ViewBag.IsPagination = true;
            }
            else
            {
                ViewBag.IsPagination = false;
            }

            if (Search != null)
            {
                return View(Search);
            }
            else
            {
                return View();
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult TransReportArch()
        {
            String Error = TempData["ErrorMessage"] as String;
            if (!String.IsNullOrEmpty(Error))
            {
                TempData["ErrorMessage"] = Error;
                return RedirectToAction("Error", "Home");
            }
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            User CurrentUser = (User)Session["CurrentUser"];
            if (CurrentUser.ForceChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }


            User oUser = (User)Session["CurrentUser"];


            List<Models.Bank> banks = null;
            List<Models.Dealers> dealers = null;
            List<Models.AgentUser> Agents = null;


            banks = Processor.BankProcessor.GetBankByUser(oUser);
            if (oUser.IsSuperUserOrAdmin || oUser.IsEtop || oUser.IsManager || oUser.isBank)
            {
                banks.Insert(0, new Bank() { Id = 0, BankName = "ALL" });
            }
            SelectList BankSelect = new SelectList(banks, "Id", "BankName");
            ViewBag.Banks = BankSelect;




            dealers = Processor.DealersProcessor.GetDealersByUser(oUser);
            if (oUser.IsSuperUserOrAdmin || oUser.IsEtop || oUser.IsManager || oUser.isBank)
            {
                dealers.Insert(0, new Dealers() { Id = 0, DealerName = "ALL" });
            }
            SelectList DealerSelect = new SelectList(dealers, "Id", "DealerName");
            ViewBag.Dealers = DealerSelect;




            Agents = Processor.AgenrtProcessor.GetAgentByUser(oUser);
            if (oUser.IsSuperUserOrAdmin || oUser.IsEtop || oUser.IsManager || oUser.isBank)
            {
                Agents.Insert(0, new AgentUser() { Id = 0, AgenName = "ALL" });
            }
            else
            {
                if (oUser.IsDealer)
                {
                    Agents.Insert(0, new AgentUser() { Id = 0, AgenName = "ALL" });
                }
            }
            SelectList AgentsSelect = new SelectList(Agents, "Id", "AgenName");
            ViewBag.Agents = AgentsSelect;



            String Startdate = TempData["Startdate"] as String;
            if (String.IsNullOrEmpty(Startdate))
            {
                ViewBag.Startdate = new DateTime(2020,10,1).ToString("dd/MM/yyyy");
            }
            else
            {
                ViewBag.Startdate = Startdate;
            }
            String EndDate = TempData["EndDate"] as String;
            if (String.IsNullOrEmpty(EndDate))
            {
                ViewBag.EndDate = new DateTime(2020, 10, 2).ToString("dd/MM/yyyy");
            }
            else
            {
                ViewBag.EndDate = EndDate;
            }


            List<String> AdditionalCriteria = new List<string>();
            AdditionalCriteria.Add("ALL");
            AdditionalCriteria.Add("Terminal ID");
            AdditionalCriteria.Add("Customer Ref");
            AdditionalCriteria.Add("Reference");



            SelectList AdditionalCriteriaSELECT = new SelectList(AdditionalCriteria);
            ViewBag.AdditionalCriteria = AdditionalCriteriaSELECT;
            SelectList PageSizeSELECT = new SelectList(new List<Int32> { 15, 20, 25, 30, 35, 40, 45, 50 });
            ViewBag.PageSizeSELECT = PageSizeSELECT;

            List<WalletTransaction> transactions = TempData["Transactions"] as List<WalletTransaction>;
            Models.SearchModel Search = TempData["Criteria"] as Models.SearchModel;

            if (transactions != null)
            {
                ViewBag.Transaction = transactions;
            }


            String IsPagination = TempData["IsPagination"] as String;
            if (IsPagination != null && IsPagination == "1")
            {
                ViewBag.IsPagination = true;
            }
            else
            {
                ViewBag.IsPagination = false;
            }

            if (Search != null)
            {
                return View(Search);
            }
            else
            {
                return View();
            }
        }

    }
}
