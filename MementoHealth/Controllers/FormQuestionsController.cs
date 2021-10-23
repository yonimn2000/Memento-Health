using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.ProviderAdmin)]
    public class FormQuestionsController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: FormQuestions/5
        public ActionResult Index(int id)
        {
            Form form = FindForm_Restricted(id);
            if (form == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (form.Questions.Count == 0)
                return RedirectToAction("Index", "Forms");

            return View(form.Questions.OrderBy(q => q.Number).ToList());
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

        // GET: FormQuestions/Add/5
        public ActionResult Add(int id, int insertAfterId = 0)
        {
            Form form = FindForm_Restricted(id);
            if (form == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View("Editor", new FormQuestion { FormId = id, QuestionId = insertAfterId });
        }

        // POST: FormQuestions/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Include = "QuestionId,Question,TypeString,JsonData,IsRequired,FormId")] FormQuestion formQuestion)
        {
            if (!ModelState.IsValid)
                return View("Editor", formQuestion);

            Form form = FindForm_Restricted(formQuestion.FormId);
            if (form == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Check for duplicate questions.
            if (Db.FormQuestions.Any(q => q.Question.Equals(formQuestion.Question) && q.FormId == form.FormId))
            {
                ModelState.AddModelError("", $"The question '{formQuestion.Question}' already exists. Please change the question text.");
                return View("Editor", formQuestion);
            }

            // Validate form question.
            if (!IsQuestionJsonValid(formQuestion))
            {
                ModelState.AddModelError("", "Invalid form question.");
                return View("Editor", formQuestion);
            }

            FormQuestion newFormQuestion = new FormQuestion
            {
                Question = formQuestion.Question,
                IsRequired = formQuestion.IsRequired,
                TypeString = formQuestion.TypeString,
                JsonData = formQuestion.JsonData
            };

            if (formQuestion.QuestionId == 0) // If adding a question.
                newFormQuestion.Number = (form.Questions.OrderByDescending(q => q.Number).FirstOrDefault()?.Number ?? 0) + 1;
            else // Inserting a question
            {
                FormQuestion prevQuestion = form.Questions.Where(q => q.QuestionId == formQuestion.QuestionId).Single();
                foreach (FormQuestion question in form.Questions.Where(q => q.Number > prevQuestion.Number).ToList())
                    question.Number++;
                newFormQuestion.Number = prevQuestion.Number + 1;
            }

            form.Questions.Add(newFormQuestion);
            Db.SaveChanges();
            return RedirectToAction("Index", new { id = form.FormId });
        }

        private bool IsQuestionJsonValid(FormQuestion question)
        {
            try
            {
                switch (question.Type)
                {
                    case QuestionType.Text:
                    case QuestionType.Number:
                    case QuestionType.Date:
                        return true;
                    case QuestionType.Checkboxes:
                    case QuestionType.Radiobuttons:
                        DynamicJsonArray labelsJsonArr = System.Web.Helpers.Json.Decode(question.JsonData).labels as DynamicJsonArray;
                        List<string> labels = labelsJsonArr.Select(l => l.ToString()).ToList();
                        return labels.Count > 0 && labels.All(l => !string.IsNullOrWhiteSpace(l));
                    case QuestionType.Image:
                        dynamic image = System.Web.Helpers.Json.Decode(question.JsonData).image;
                        return (image.url as string).Length < (double)3 / 4 * 5 * 1024 * 1024  // Base64 < 5MB
                        && image.width > 0 && image.height > 0;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
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

            return View("Editor", formQuestion);
        }

        // POST: FormQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionId,Question,JsonData,IsRequired,TypeString")] FormQuestion newFormQuestion)
        {
            FormQuestion formQuestion = FindFormQuestion_Restricted(newFormQuestion.QuestionId);
            if (!ModelState.IsValid)
                return View("Editor", newFormQuestion);

            // Check for duplicate questions.
            if (Db.FormQuestions.Any(q => q.Question.Equals(formQuestion.Question)
                    && q.FormId == formQuestion.FormId && q.QuestionId != formQuestion.QuestionId))
            {
                ModelState.AddModelError("", $"The question '{formQuestion.Question}' already exists. Please change the question text.");
                return View("Editor", formQuestion);
            }

<<<<<<< Updated upstream
            // Validate form question.
            if (!IsQuestionJsonValid(formQuestion))
            {
                ModelState.AddModelError("", $"Invalid form question.");
                return View("Editor", formQuestion);
            }

            if (formQuestion.Type != newFormQuestion.Type || formQuestion.JsonData != newFormQuestion.JsonData)
                Db.FormQuestionConditions.RemoveRange(formQuestion.Conditions);
=======
            if (formQuestion.Type != newFormQuestion.Type || formQuestion.JsonData != newFormQuestion.JsonData)
                formQuestion.Conditions.Clear();
>>>>>>> Stashed changes

            formQuestion.TypeString = newFormQuestion.TypeString;
            formQuestion.Question = newFormQuestion.Question;
            formQuestion.JsonData = newFormQuestion.JsonData;
            formQuestion.IsRequired = newFormQuestion.IsRequired;
            Db.SaveChanges();
            return RedirectToAction("Index", new { id = formQuestion.FormId });
        }

        // POST: FormQuestions/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveUp(int id)
        {
            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion.CanBeMovedUp)
            {
                FormQuestion prevQuestion = formQuestion.Form.Questions.Where(q => q.Number == formQuestion.Number - 1).SingleOrDefault();
                if (prevQuestion == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                formQuestion.Number--;
                prevQuestion.Number++;
                Db.SaveChanges(); 
            }
            return RedirectToAction("Index", new { id = formQuestion.FormId });
        }

        // POST: FormQuestions/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveDown(int id)
        {
            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion.CanBeMovedDown)
            {
                FormQuestion nextQuestion = formQuestion.Form.Questions.Where(q => q.Number == formQuestion.Number + 1).SingleOrDefault();
                if (nextQuestion == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                formQuestion.Number++;
                nextQuestion.Number--;
                Db.SaveChanges(); 
            }
            return RedirectToAction("Index", new { id = formQuestion.FormId });
        }

        // GET: FormQuestions/Insert/5
        public ActionResult Insert(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion == null)
                return HttpNotFound();

            return RedirectToAction("Add", new { id = formQuestion.FormId, insertAfterId = formQuestion.QuestionId });
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
            foreach (FormQuestion question in formQuestion.Form.Questions.Where(q => q.Number > formQuestion.Number).ToList())
                question.Number--;
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