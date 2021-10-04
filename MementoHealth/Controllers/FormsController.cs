using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.ProviderAdmin + "," + Role.Doctor + "," + Role.Assistant)]
    public class FormsController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: Forms
        public ActionResult Index()
        {
            return View(GetCurrentUserProvider().Forms.OrderBy(u => u.Name).ToList()
                .Select(u => new FormViewModel
                {
                    FormId = u.FormId,
                    Name = u.Name,
                }).ToList());
        }

        // GET: Forms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Form form = FindForm_Restricted(id);
            if (form == null)
                return HttpNotFound();

            return View(form);
        }

        private Form FindForm_Restricted(int? id)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Forms.Where(f => f.FormId == id).SingleOrDefault();
        }

        // GET: Forms/Create
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Forms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Create([Bind(Include = "Name")] Form form)
        {
            if (ModelState.IsValid)
            {
                Form newForm = new Form
                {
                    Name = form.Name,
                    ProviderId = GetCurrentUserProvider().ProviderId
                };
                Db.Forms.Add(newForm);
                Db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(form);
        }

        private Provider GetCurrentUserProvider()
        {
            if (User.IsInRole(Role.SysAdmin))
                return null;
            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // GET: Forms/Edit/5
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Form form = FindForm_Restricted(id);
            if (form == null)
                return HttpNotFound();

            return View(GetEditFormViewModel(form));
        }

        private static EditFormViewModel GetEditFormViewModel(Form form)
        {
            return new EditFormViewModel
            {
                FormId = form.FormId,
                Name = form.Name
            };
        }

        // POST: Forms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Edit(EditFormViewModel model)
        {
            Form form = FindForm_Restricted(model.FormId);
            if (ModelState.IsValid)
            {
                form.Name = model.Name;
                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(GetEditFormViewModel(form));
        }

        // GET: Forms/Delete/5
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Form form = FindForm_Restricted(id);
            if (form == null)
                return HttpNotFound();

            return View(form);
        }

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult DeleteConfirmed(int id)
        {
            Form form = FindForm_Restricted(id);
            Db.Forms.Remove(form);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Forms/Clone/5
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Clone(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Form form = FindForm_Restricted(id);
            if (form == null)
                return HttpNotFound();

            return View(form);
        }


        // POST: Forms/Delete/5
        [HttpPost, ActionName("Clone")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult CloneConfirmed(int id)
        {
            throw new NotImplementedException();
            /*Form form = FindForm_Restricted(id);
            // TODO: Implement
            db.SaveChanges();
            return RedirectToAction("Index");*/
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Db.Dispose();

            base.Dispose(disposing);
        }
    }
}