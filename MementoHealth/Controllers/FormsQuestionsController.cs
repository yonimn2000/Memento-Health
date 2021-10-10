using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.ProviderAdmin)]
    public class FormQuestionsController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: Forms
        public ActionResult Index(int? id)
        {
            Form form = FindForm_Restricted(id);
            if (form == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(form.Questions.ToList());
        }

        // GET: FormQuestions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion == null)
                return HttpNotFound();

            return View(formQuestion);
        }

        private Form FindForm_Restricted(int? id)
        {
            return GetCurrentUserProvider().Forms.Where(f => f.FormId == id).SingleOrDefault();
        }

        private FormQuestion FindFormQuestion_Restricted(int? id)
        {
            int providerId = GetCurrentUserProvider().ProviderId;
            return Db.FormQuestions.Where(q => q.QuestionId == id && q.Form.ProviderId == providerId).SingleOrDefault();
        }

        // GET: FormQuestions/Create
        public ActionResult Create(int id)
        {
            return View(new FormQuestion { FormId = id });
        }

        // POST: FormQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Question,TypeString,JsonData,IsRequired,FormId")] FormQuestion formQuestion)
        {
            if (ModelState.IsValid)
            {
                Form form = FindForm_Restricted(formQuestion.FormId);
                if (form == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                if (!Db.FormQuestions.Any(q => q.Question.Equals(formQuestion.Question) && q.FormId == form.FormId))
                {
                    form.Questions.Add(new FormQuestion
                    {
                        Question = formQuestion.Question,
                        IsRequired = formQuestion.IsRequired,
                        TypeString = formQuestion.TypeString,
                        JsonData = formQuestion.JsonData
                    });
                    Db.SaveChanges();
                    return RedirectToAction("Index", new { id = form.FormId });
                }
                ModelState.AddModelError("", $"The question '{formQuestion.Question}' already exists. Please change the question text.");
            }
            return View(formQuestion);
        }

        private Provider GetCurrentUserProvider()
        {
            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // GET: FormQuestions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion == null)
                return HttpNotFound();

            return View(formQuestion);
        }

        // POST: FormQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionId,Question,JsonData,IsRequired")] FormQuestion newFormQuestion)
        {
            FormQuestion formQuestion = FindFormQuestion_Restricted(newFormQuestion.QuestionId);
            if (ModelState.IsValid)
            {
                formQuestion.Question = newFormQuestion.Question;
                formQuestion.JsonData = newFormQuestion.JsonData;
                formQuestion.IsRequired = newFormQuestion.IsRequired;
                Db.SaveChanges();
                return RedirectToAction("Index", new { id = newFormQuestion.FormId });
            }
            return View(newFormQuestion);
        }

        // GET: FormQuestions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion == null)
                return HttpNotFound();

            return View(formQuestion);
        }

        // POST: FormQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            Db.FormQuestions.Remove(formQuestion);
            Db.SaveChanges();
            return RedirectToAction("Index", new { id = formQuestion.FormId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Db.Dispose();
            base.Dispose(disposing);
        }
    }
}