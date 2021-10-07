using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.SysAdmin + "," + Role.ProviderAdmin)]
    public class UsersController : Controller
    {
        private ApplicationUserManager _userManager;

        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        // GET: Users
        public ActionResult Index()
        {
            IEnumerable<ApplicationUser> users;

            if (User.IsInRole(Role.SysAdmin)) // If SysAdmin, get only other SysAdmins.
                users = Db.Users.Where(u => u.Roles.Any(r => r.Role.Name.Equals(Role.SysAdmin)));
            else if (User.IsInRole(Role.ProviderAdmin)) // If ProviderAdmin, get only users from current provider.
                users = Db.Users.Find(User.Identity.GetUserId()).Provider.Staff;
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Should not be able to reach here at all.

            return View(users.OrderBy(u => u.FullName).ToList().Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                FullName = u.FullName,
                LockOutStatus = u.LockOutStatus,
                LockedOut = u.LockedOut,
                Phone = u.PhoneNumber,
                Role = GetUserRole(u)
            }).ToList());
        }

        private string GetUserRole(ApplicationUser u)
        {
            return UserManager.GetRoles(u.Id).SingleOrDefault();
        }

        // GET: Users/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ApplicationUser applicationUser = FindUser_Restricted(id);

            if (applicationUser == null)
                return HttpNotFound();

            return View(applicationUser);
        }

        private ApplicationUser FindUser_Restricted(string id)
        {
            ApplicationUser applicationUser = null;

            if (User.IsInRole(Role.SysAdmin)) // If SysAdmin, get only other SysAdmins.
                applicationUser = Db.Users.Where(u => u.Id.Equals(id) && u.Roles.Any(r => r.Role.Name.Equals(Role.SysAdmin))).SingleOrDefault();
            else if (User.IsInRole(Role.ProviderAdmin)) // If ProviderAdmin, get only users from current provider.
                applicationUser = GetCurrentUserProvider().Staff.Where(u => u.Id.Equals(id)).SingleOrDefault();

            return applicationUser;
        }

        private Provider GetCurrentUserProvider()
        {
            if (User.IsInRole(Role.SysAdmin))
                return null;
            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Roles = GetRolesSelectListForCurrentUser();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (GetSelectableRolesForCurrentUser().Contains(model.Role))
                {
                    IdentityResult result = await UserManager.CreateAsync(new ApplicationUser
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        UserName = model.Email,
                        PhoneNumber = model.Phone,
                        EmailConfirmed = true,
                        ProviderId = GetCurrentUserProvider().ProviderId
                    });

                    if (result.Succeeded)
                    {
                        var user = await UserManager.FindByEmailAsync(model.Email);
                        UserManager.AddToRole(user.Id, model.Role);
                        string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Memento Account",
                            "An account with Memento has been created for you. Please set your new password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                        TempData.Add("StatusMessage", "User added successfully. The new user should follow the link that was sent to the user's email address.");
                        return RedirectToAction("Index");
                    }
                    else
                        ModelState.AddModelError("", "An error has occurred while adding the user: " + string.Join(" | ", result.Errors));
                }
                else
                    ModelState.AddModelError("", "Invalid role selected.");
            }
            ViewBag.Roles = GetRolesSelectListForCurrentUser();
            return View(model);
        }

        private SelectList GetRolesSelectListForCurrentUser(string selected = "")
        {
            return new SelectList(GetSelectableRolesForCurrentUser().Select(r => new SelectListItem { Text = r, Value = r }).ToList(), "Value", "Text", selected);
        }

        private List<string> GetSelectableRolesForCurrentUser()
        {
            List<string> roles = new List<string>();

            if (User.IsInRole(Role.SysAdmin))
                roles.Add(Role.SysAdmin);
            else if (User.IsInRole(Role.ProviderAdmin))
            {
                roles.Add(Role.Assistant);
                roles.Add(Role.Doctor);
                roles.Add(Role.ProviderAdmin);
            }

            return roles;
        }

        // GET: Users/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser applicationUser = FindUser_Restricted(id);
            if (applicationUser == null)
                return HttpNotFound();

            ViewBag.Roles = GetRolesSelectListForCurrentUser(GetUserRole(applicationUser));

            return View(GetEditUserViewModel(applicationUser));
        }

        private static EditUserViewModel GetEditUserViewModel(ApplicationUser applicationUser)
        {
            return new EditUserViewModel
            {
                Id = applicationUser.Id,
                Email = applicationUser.Email,
                FullName = applicationUser.FullName,
                Phone = applicationUser.PhoneNumber
            };
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserViewModel model)
        {
            // Do not let the current user edit themselves.
            if (model.Id.Equals(User.Identity.GetUserId()))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser applicationUser = FindUser_Restricted(model.Id);
            if (ModelState.IsValid)
            {
                if (GetSelectableRolesForCurrentUser().Contains(model.Role))
                {
                    applicationUser.FullName = model.FullName;
                    applicationUser.PhoneNumber = model.Phone;
                    applicationUser.Roles.Clear();
                    applicationUser.Roles.Add(new ApplicationUserRole
                    {
                        Role = Db.Roles.Where(r => r.Name.Equals(model.Role)).Single()
                    });
                    Db.SaveChanges();
                    TempData.Add("StatusMessage", "User edited successfully.");
                    return RedirectToAction("Index");
                }
                else
                    ModelState.AddModelError("", "Invalid role selected.");
            }
            ViewBag.Roles = GetRolesSelectListForCurrentUser(GetUserRole(applicationUser));
            return View(GetEditUserViewModel(applicationUser));
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser applicationUser = FindUser_Restricted(id);

            if (applicationUser == null)
                return HttpNotFound();

            return View(applicationUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            // Do not let the current user delete themselves.
            if (id.Equals(User.Identity.GetUserId()))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser applicationUser = FindUser_Restricted(id);
            Db.Users.Remove(applicationUser);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Users/ResendEmailConfirmation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResendEmailConfirmation(string id)
        {
            ApplicationUser applicationUser = FindUser_Restricted(id);

            if (applicationUser == null)
                return HttpNotFound();

            string code = UserManager.GenerateEmailConfirmationToken(applicationUser.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = applicationUser.Id, code = code }, protocol: Request.Url.Scheme);
            UserManager.SendEmail(applicationUser.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            TempData.Add("StatusMessage", $"Email confirmation resent successfully to {applicationUser.Email} ({applicationUser.FullName}).");

            return RedirectToAction("Index");
        }

        // POST: Users/LockUnlock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LockUnlock(string id, bool lockOut)
        {
            // Do not let the current user lock themselves.
            if (id.Equals(User.Identity.GetUserId()))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser applicationUser = FindUser_Restricted(id);

            if (applicationUser == null)
                return HttpNotFound();

            if (lockOut)
                applicationUser.LockoutEndDateUtc = DateTime.MaxValue;
            else
                applicationUser.LockoutEndDateUtc = null;

            applicationUser.AccessFailedCount = 0;

            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Db.Dispose();
            base.Dispose(disposing);
        }
    }
}