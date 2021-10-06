﻿using MementoHealth.Attributes;
using MementoHealth.Filters;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController() { }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.ChangePinSuccess ? "Your PIN has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                HasPin = await UserManager.HasPinAsync(userId),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                Role = string.Join(", ", await UserManager.GetRolesAsync(userId))
            };
            return View(model);
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), model.Number);
            if (result.Succeeded)
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to add phone");
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        // GET: /Manage/ChangePin
        [AllowThroughPinLock]
        public ActionResult ChangePin(string returnUrl, bool lockAfterChangingPin = false)
        {
            return View(new ChangePinViewModel
            {
                ReturnUrl = returnUrl,
                LockAfterChangingPin = lockAfterChangingPin
            });
        }

        //
        // POST: /Manage/ChangePin
        [HttpPost]
        [AllowThroughPinLock]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePin(ChangePinViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!model.NewPin.All(char.IsDigit))
                ModelState.AddModelError("", "PIN must contain digits only.");
            else
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (await UserManager.CheckPasswordAsync(user, model.Password))
                {
                    await UserManager.SetPinAsync(user.Id, model.NewPin);
                    if (model.LockAfterChangingPin)
                    {
                        PinLockFilter.Enabled = true;
                        return RedirectToAction("PinUnlock", "Account", new { returnUrl = model.ReturnUrl });
                    }
                    PinLockFilter.Enabled = false;
                    if (string.IsNullOrWhiteSpace(model.ReturnUrl))
                        return RedirectToAction("Index", new { Message = ManageMessageId.ChangePinSuccess });
                    return RedirectToLocal(model.ReturnUrl);
                }
                ModelState.AddModelError("", "Incorrect password");
            }
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
                return user.PasswordHash != null;
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
                return user.PhoneNumber != null;
            return false;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            ChangePinSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}