using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WalletReport.Models;
using WalletReport.Processor;
using com.ujc.StringHelper;
using System.Text;

namespace WalletReport.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(Models.LocalPasswordModel Changepassword)
        {
            Models.User Ouser = Session["CurrentUser"] as Models.User;
            if (Ouser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                String OldPassword = UserProcessor.GetOldPassword(Ouser.Id);

                String HashOld = Changepassword.OldPassword.Hash(HashType.SHA512, System.Text.Encoding.ASCII);
                if (HashOld == OldPassword)
                {
                    bool isChangePassword = UserProcessor.ChangePassword(Ouser, Changepassword.NewPassword,false);
                    if (isChangePassword)
                    {
                        Ouser.ForceChangePassword = false;
                        Session["CurrentUser"] = Ouser;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("Error Updating Profile", "Error Updating Profile");
                        return View(Changepassword);
                    }

                    
                }
                else
                {
                    ModelState.AddModelError("Wrong password", "Wrong password");
                    return View(Changepassword);
                }


                
            }
            else
            {
              
                return View(Changepassword);
            }
        }
        [Authorize]
        public ActionResult ChangePassword(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassword(ForgetPassword model)
        {
            if (String.IsNullOrWhiteSpace(model.EmailAddress))
            {
                TempData["ErrorMessage"] = "Empty Email Address";
                return RedirectToAction("ForgetPassword", "Account");
            }
            Models.User user =  Processor.UserProcessor.GetUserByUserNameOrEmail(model.EmailAddress);
            if(user != null)
            {
                String guid = Processor.UserProcessor.InsertResetPassword(user.Username, user.EmailAddress);

                StringBuilder builder = new StringBuilder();
                String htmlTemplate = String.Format(@"<html><head></head><body>
                <br>You can change your password by clicking on the link below<br>
                <a href='https://portal.netopng.com/EazyWalletReport/Account/ResetMyPassword?reference={0}'></a>
                
                </body>
                </html>",guid);

                builder.Append(htmlTemplate);
                NotificationManager.NotifyCustomer(builder, user.EmailAddress, String.Format("PASSWORD RESET FOR {0}", user.DisplayName));
                ViewBag.ErrorMessage = "A link has been sent to your email to reset your password";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["ErrorMessage"] = "Empty Email Address";
                return RedirectToAction("ForgetPassword", "Account");
            }
        }
        [HttpGet]
        [AllowAnonymous]
     
        public ActionResult ForgetPassword()
        {
            String errorMessage = TempData["ErrorMessage"] as String;
            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }
            return  View(new ForgetPassword());
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, String returnUrl)
        {
            String ErrorText = null;
            try
            {

                String password1 = model.Password.Hash(HashType.SHA512, Encoding.ASCII);

                Models.User oUser = Processor.UserProcessor.AuthenticateUser(model.UserName, model.Password, ref ErrorText);

                if (oUser != null)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false);
                    String AuthName = Guid.NewGuid().ToString();
                    Session["AuthName"] = AuthName;
                    Session["CurrentUser"] = oUser;

                    Processor.UserProcessor.SetUserRoles(oUser);






                    //if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    //{

                    //    return RedirectToLocal(returnUrl);
                    //}
                    //else
                    //{
                    //    return RedirectToAction("Index", "Home");
                    //}
                    return RedirectToAction("TransReport", "Report");

                }
                else
                {
                    ViewBag.LoginError = ErrorText;
                    ModelState.AddModelError("", ErrorText);
                    return View(model);
                }

            }
            catch (DBConnector.OpenDBException ex)
            {
                ErrorText = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", ErrorText);
            return View(model);
        }
        public ActionResult SignOff()
        {
            Session["CurrentUser"] = null;
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        private ActionResult RedirectToLocal(String returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
