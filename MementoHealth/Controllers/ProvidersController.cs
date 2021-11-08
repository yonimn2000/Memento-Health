using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.SysAdmin)]
    public class ProvidersController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: Providers
        public ActionResult Index()
        {
            return View(Db.Providers.ToList());
        }

        // GET: Providers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Provider provider = Db.Providers.Find(id);
            if (provider == null)
                return HttpNotFound();

            return View(provider);
        }

        // GET: Providers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Provider provider = Db.Providers.Find(id);
            if (provider == null)
                return HttpNotFound();

            return View(provider);
        }

        // POST: Providers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProviderId,Name,Phone,Address,Email")] Provider provider)
        {
            if (ModelState.IsValid)
            {
                if (!Db.Providers.Any(f => f.Name.Equals(provider.Name)))
                {
                    Db.Entry(provider).State = EntityState.Modified;
                    Db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", $"A provider with the name of '{provider.Name}' already exists." +
                    "Please pick a different name.");
            }
            return View(provider);
        }

        // GET: Providers/Delete/5
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

        public ActionResult Stats()
        {
            StatsViewModel statsViewModel = new StatsViewModel();

            statsViewModel.Providers = Db.Providers.ToList();
            statsViewModel.PatientCount = Db.Patients.Count();
            statsViewModel.FormCount = Db.Forms.Count();
            statsViewModel.SubmissionCount = Db.FormSubmissions.Count();
            statsViewModel.UserCount = Db.Users.Count();

            return View(statsViewModel);
        }
    }
}
