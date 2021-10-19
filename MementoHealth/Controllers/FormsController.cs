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
                .Select(f => FormToViewModel(f)).ToList());
        }

        private static FormViewModel FormToViewModel(Form form)
        {
            return new FormViewModel
            {
                FormId = form.FormId,
                Name = form.Name,
                NumberOfQuestions = form.Questions.Count,
                NumberOfSubmissions = form.Submissions.Count,
                IsPublished = form.IsPublished
            };
        }

        private Form FindForm_Restricted(int? id)
        {
            return GetCurrentUserProvider().Forms.Where(f => f.FormId == id).SingleOrDefault();
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
                int providerId = GetCurrentUserProvider().ProviderId;
                if (!Db.Forms.Any(f => f.Name.Equals(form.Name) && f.ProviderId == providerId))
                {
                    Form newForm = new Form
                    {
                        Name = form.Name,
                        ProviderId = providerId
                    };

                    Db.Forms.Add(newForm);
                    Db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", $"A from with the name of '{form.Name}' already exists." +
                    "Please pick a different name.");
            }
            return View(form);
        }

        private Provider GetCurrentUserProvider()
        {
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
            return GetFormViewModelResult(id);
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
            return GetFormViewModelResult(id);
        }


        // POST: Forms/Clone/5
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

        // GET: Forms/Publish/5
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Publish(int? id)
        {
            return GetFormViewModelResult(id);
        }


        // POST: Forms/Publish/5
        [HttpPost, ActionName("Publish")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult PublishConfirmed(int id)
        {
            Form form = FindForm_Restricted(id);
            form.IsPublished = true;
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Forms/Unpublish/5
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult Unpublish(int? id)
        {
            return GetFormViewModelResult(id);
        }

        private ActionResult GetFormViewModelResult(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Form form = FindForm_Restricted(id);
            if (form == null)
                return HttpNotFound();

            return View(FormToViewModel(form));
        }


        // POST: Forms/Unpublish/5
        [HttpPost, ActionName("Unpublish")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.ProviderAdmin)]
        public ActionResult UnpublishConfirmed(int id)
        {
            Form form = FindForm_Restricted(id);
            form.IsPublished = false;
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