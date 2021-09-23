using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
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
            return View(Db.Users.OrderBy(u => u.FullName).ToList().Select(u => new UserViewModel
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = Db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Roles = GetRolesSelectList();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = UserManager.CreateAsync(new ApplicationUser
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.Phone,
                    EmailConfirmed = true
                }).Result;

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByNameAsync(model.Email);
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
            ViewBag.Roles = GetRolesSelectList();
            return View(model);
        }

        private SelectList GetRolesSelectList(string selected = "")
        {
            return new SelectList(Db.Roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name }).ToList(), "Value", "Text", selected);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser applicationUser = Db.Users.Find(id);
            if (applicationUser == null)
                return HttpNotFound();

            ViewBag.Roles = GetRolesSelectList(GetUserRole(applicationUser));

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

            ApplicationUser applicationUser = Db.Users.Find(model.Id);
            if (ModelState.IsValid)
            {
                applicationUser.FullName = model.FullName;
                applicationUser.PhoneNumber = model.Phone;
                applicationUser.Roles.Clear();
                applicationUser.Roles.Add(new IdentityUserRole
                {
                    RoleId = Db.Roles.Where(r => r.Name.Equals(model.Role)).Single().Id
                });
                Db.SaveChanges();
                TempData.Add("StatusMessage", "User edited successfully.");
                return RedirectToAction("Index");
            }
            ViewBag.Roles = GetRolesSelectList(GetUserRole(applicationUser));
            return View(GetEditUserViewModel(applicationUser));
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = Db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = Db.Users.Find(id);
            Db.Users.Remove(applicationUser);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Users/ResendEmailConfirmation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResendEmailConfirmation(string id)
        {
            ApplicationUser applicationUser = Db.Users.Find(id);

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
            ApplicationUser applicationUser = Db.Users.Find(id);

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
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
