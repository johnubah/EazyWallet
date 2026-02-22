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
    public class HomeController : Controller
    {


        public ActionResult Error()
        {
            String Error = TempData["ErrorMessage"] as String;
            if (Error != null)
            {
                ViewBag.ErrorMessage = Error;
            }
            else
            {
                Error = ViewData["ErrorMessage"] as String;
                ViewBag.ErrorMessage = String.IsNullOrEmpty(Error) ? String.Empty : Error;
            }
            TempData.Clear();
            return View();
        }
        [HttpPost]
        public ActionResult Reverse(FormCollection c)
        {
            try
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
                    WalletTransaction trans = TransactionQueryProcessor.GetWalletTransactionByID(c["tranID"]);

                    if(trans.IsReversed)
                    {
                        return RedirectToAction("TransHasBeenReversed");
                    }
                    Models.Response resp = WalletProcessor.ReverseTransaction(trans);
                }
                
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "System was unable to retrieve transaction detail";
               
            }
            return Error();

        }
        [Authorize]
        public ActionResult Detail(String id)
        {
            try
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
                    ViewBag.CanReverse = true;
                }
                TransactionDetail detail =  Processor.IndexProcessor.GetTransactionDetail(id);

                if(detail != null)
                {
                    return View(detail);
                }
                else
                {
                    TempData["ErrorMessage"] = "System was unable to retrieve transaction detail";
                    return Error();
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "System was unable to retrieve transaction detail";
                return Error();
            }
            
        }
        [Authorize]
        public ActionResult Index()
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
            
            DashBoardModel dashBoard = new DashBoardModel();
            dashBoard.CurrentUser = CurrentUser;

            if(CurrentUser.IsDealer)
            {
                Dealers dealer = DealersProcessor.GetDealersByID(CurrentUser.DealerID);
                if(dealer != null)
                {
                    dashBoard.Dealer = dealer;
                }
            }
            else if(CurrentUser.IsAgent)
            {
                if(CurrentUser.AgentID > 0)
                {
                    AgentUser agentUser = AgenrtProcessor.GetAgentBYID(CurrentUser.AgentID);
                    if (agentUser != null)
                    {
                        dashBoard.AgentUser = agentUser;
                    }
                }
            }




            //DateTime todayStart = DateTime.Now;
            //todayStart = new DateTime(todayStart.Year, todayStart.Month, todayStart.Day, 0, 0, 0);
            //DateTime todayEnd = new DateTime(todayStart.Year, todayStart.Month, todayStart.Day, 23, 59, 59);

            //Models.Summary TodaySurmmary = Processor.IndexProcessor.GetTotalTransaction(todayStart, todayEnd, CurrentUser);
            //dashBoard.Today = TodaySurmmary;



            //DateTime weeklyDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            //DateTime endWeekDay = weeklyDate.AddDays(7);
            //endWeekDay = new DateTime(endWeekDay.Year, endWeekDay.Month, endWeekDay.Day, 23, 59, 59);


            //Models.Summary WeeklySurmmary = Processor.IndexProcessor.GetTotalTransaction( new DateTime(weeklyDate.Year, weeklyDate.Month, weeklyDate.Day), endWeekDay, CurrentUser);
            //dashBoard.WeeklySummary = WeeklySurmmary;



            //DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            //DateTime endDate = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
            //endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            //Models.Summary MonthlySummary = Processor.IndexProcessor.GetTotalTransaction( startDate, endDate, CurrentUser);
            //dashBoard.Monthly = MonthlySummary;



            //var totalDebitCredit = Processor.IndexProcessor.GetTotalDebitCreditTransaction(CurrentUser);
            //dashBoard.TotalCredit = totalDebitCredit.CreditTotal;
            //dashBoard.TotalDebit = totalDebitCredit.DebitTotal;



            DateTime todayStart = DateTime.Now;


            DateTime twoDayAgo = todayStart.AddDays(-2);

            twoDayAgo = new DateTime(twoDayAgo.Year, twoDayAgo.Month, twoDayAgo.Day, 0, 0, 0);
            DateTime twoDayAgoEndDate = new DateTime(twoDayAgo.Year, twoDayAgo.Month, twoDayAgo.Day, 23, 59, 59);
            Models.Summary TwoDaysSurmmary = Processor.IndexProcessor.GetTotalTransaction(twoDayAgo, twoDayAgoEndDate, CurrentUser);
            dashBoard.TwoDaysSummary = TwoDaysSurmmary;


            DateTime yesterdayDate = todayStart.AddDays(-1);

            yesterdayDate = new DateTime(yesterdayDate.Year, yesterdayDate.Month, yesterdayDate.Day, 0, 0, 0);
            DateTime yesterdayEnd = new DateTime(yesterdayDate.Year, yesterdayDate.Month, yesterdayDate.Day, 23, 59, 59);
            Models.Summary YesterdaySurmmary = Processor.IndexProcessor.GetTotalTransaction(yesterdayDate, yesterdayEnd, CurrentUser);
            dashBoard.YesterdaySummary = YesterdaySurmmary;


            todayStart = new DateTime(todayStart.Year, todayStart.Month, todayStart.Day, 0, 0, 0);
            DateTime todayEnd = new DateTime(todayStart.Year, todayStart.Month, todayStart.Day, 23, 59, 59);
            Models.Summary TodaySurmmary = Processor.IndexProcessor.GetTotalTransaction(todayStart, todayEnd, CurrentUser);
            dashBoard.Today = TodaySurmmary;



            DateTime weeklyDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            DateTime endWeekDay = weeklyDate.AddDays(7);
            endWeekDay = new DateTime(endWeekDay.Year, endWeekDay.Month, endWeekDay.Day, 23, 59, 59);


            Models.Summary WeeklySurmmary = Processor.IndexProcessor.GetTotalTransaction(new DateTime(weeklyDate.Year, weeklyDate.Month, weeklyDate.Day), endWeekDay, CurrentUser);
            dashBoard.WeeklySummary = WeeklySurmmary;



            List<WalletTransaction> transaction = Processor.IndexProcessor.GetTransactionHistory(CurrentUser);


            dashBoard.WalletTransaction = transaction;
            return View(dashBoard);
        }

    }
}
