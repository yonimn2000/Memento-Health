using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize]
    public class ProvidersController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: Providers
        [Authorize(Roles = Role.SysAdmin)]
        public ActionResult Index()
        {
            return View(Db.Providers.ToList());
        }

        // GET: Providers/Details/5
        public ActionResult Details(int? id)
        {
            Provider provider = GetProvider(id);

            if (provider == null)
                return HttpNotFound();

            return View(provider);
        }

        // GET: Providers/Edit/5
        [Authorize(Roles = Role.SysAdmin + "," + Role.ProviderAdmin)]
        public ActionResult Edit(int? id)
        {
            Provider provider = GetProvider(id);

            if (provider == null)
                return HttpNotFound();

            return View(provider);
        }

        private Provider GetProvider(int? id)
        {
            if (User.IsInRole(Role.SysAdmin))
            {
                if (id == null)
                    return null;

                return Db.Providers.Find(id);
            }

            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // POST: Providers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.SysAdmin + "," + Role.ProviderAdmin)]
        public ActionResult Edit([Bind(Include = "ProviderId,Name,Phone,Address,Email")] Provider model)
        {
            if (ModelState.IsValid)
            {
                if (!Db.Providers.Any(p => p.ProviderId != model.ProviderId && p.Name.Equals(model.Name)))
                {
                    Provider provider = GetProvider(model.ProviderId);
                    provider.Name = model.Name;
                    provider.Phone = model.Phone;
                    provider.Address = model.Address;
                    provider.Email = model.Email;
                    Db.SaveChanges();
                    return RedirectToAction(User.IsInRole(Role.SysAdmin) ? "Index" : "Details");
                }
                ModelState.AddModelError("", $"A provider with the name of '{model.Name}' already exists. " +
                    "Please pick a different name.");
            }
            return View(model);
        }

        // GET: Providers/Delete/5
        [Authorize(Roles = Role.SysAdmin)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Provider provider = Db.Providers.Find(id);
            if (provider == null)
                return HttpNotFound();

            return View(provider);
        }

        // POST: Providers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.SysAdmin)]
        public ActionResult DeleteConfirmed(int id)
        {
            Provider provider = Db.Providers.Find(id);
            Db.Providers.Remove(provider);
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