using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.ProviderAdmin)]
    public class FormQuestionConditionsController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: FormQuestionConditions/5
        public ActionResult Index(int id)
        {
            FormQuestion formQuestion = FindFormQuestion_Restricted(id);
            if (formQuestion == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (formQuestion.Conditions.Count == 0)
                return RedirectToAction("Index", "FormQuestions", new { id = formQuestion.FormId });

            return View(formQuestion.Conditions.OrderBy(q => q.Number).ToList());
        }

        private FormQuestion FindFormQuestion_Restricted(int? id)
        {
            int providerId = GetCurrentUserProvider().ProviderId;
            return Db.FormQuestions.Where(q => q.QuestionId == id && q.Form.ProviderId == providerId).SingleOrDefault();
        }
        private FormQuestionCondition FindFormQuestionCondition_Restricted(int? id)
        {
            int providerId = GetCurrentUserProvider().ProviderId;
            return Db.FormQuestionConditions.Where(c => c.ConditionId == id && c.Question.Form.ProviderId == providerId).SingleOrDefault();
        }

        // GET: FormQuestionConditions/Add/5
        public ActionResult Add(int id, int insertAfterId = 0)
        {
            FormQuestion question = FindFormQuestion_Restricted(id);
            if (question == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            return View("Editor", new FormQuestionCondition { QuestionId = id, Question = question, ConditionId = insertAfterId });
        }

        // POST: FormQuestionConditions/Create/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Include = "ConditionId,Number,JsonData,GoToQuestionId,QuestionId")] FormQuestionCondition questionCondition)
        {
            if (!ModelState.IsValid)
                return View("Editor", questionCondition);

            FormQuestion question = FindFormQuestion_Restricted(questionCondition.QuestionId);
            if (question == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            questionCondition.Question = question;

            // Check for duplicate conditions. Fixme: Needs more checks.
            if (Db.FormQuestionConditions.Any(c => c.JsonData.Equals(questionCondition.JsonData)
                && c.QuestionId == question.QuestionId && c.ConditionId != questionCondition.ConditionId))
            {
                ModelState.AddModelError("", "The condition already exists. Please modify the condition and try again.");
                return View("Editor", questionCondition);
            }

            if (!IsConditionJsonValid(questionCondition))
            {
                ModelState.AddModelError("", "Invalid question condition.");
                return View("Editor", questionCondition);
            }

            FormQuestionCondition newCondition = new FormQuestionCondition
            {
                JsonData = questionCondition.JsonData,
                GoToQuestionId = questionCondition.GoToQuestionId == 0
                    || !question.Form.Questions.Any(q => q.QuestionId == questionCondition.GoToQuestionId)
                    || question.Number > Db.FormQuestions.Find(questionCondition.GoToQuestionId).Number
                    ? null : questionCondition.GoToQuestionId
            };

            if (questionCondition.ConditionId == 0) // If adding a question.
                newCondition.Number = (question.Conditions.OrderByDescending(c => c.Number).FirstOrDefault()?.Number ?? 0) + 1;
            else // Inserting a question
            {
                FormQuestionCondition prevCondition = question.Conditions.Where(c => c.ConditionId == questionCondition.ConditionId).Single();
                foreach (FormQuestionCondition condition in question.Conditions.Where(c => c.Number > prevCondition.Number).ToList())
                    condition.Number++;
                newCondition.Number = prevCondition.Number + 1;
            }

            question.Conditions.Add(newCondition);
            Db.SaveChanges();
            return RedirectToAction("Index", new
            {
                id = question.QuestionId
            });
        }

        private Provider GetCurrentUserProvider()
        {
            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // GET: FormQuestionConditions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestionCondition formQuestionCondition = FindFormQuestionCondition_Restricted(id);
            if (formQuestionCondition == null)
                return HttpNotFound();

            return View("Editor", formQuestionCondition);
        }

        // POST: FormQuestionConditions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConditionId,Number,JsonData,GoToQuestionId,QuestionId")] FormQuestionCondition newCondition)
        {
            FormQuestionCondition formQuestionCondition = FindFormQuestionCondition_Restricted(newCondition.ConditionId);
            if (!ModelState.IsValid)
                return View("Editor", newCondition);

            if (!IsConditionJsonValid(newCondition))
            {
                ModelState.AddModelError("", "Invalid question condition.");
                return View("Editor", newCondition);
            }

            formQuestionCondition.JsonData = newCondition.JsonData;
            formQuestionCondition.GoToQuestionId = newCondition.GoToQuestionId == 0 
                || !formQuestionCondition.Question.Form.Questions.Any(q => q.QuestionId == newCondition.GoToQuestionId)
                || formQuestionCondition.Question.Number > Db.FormQuestions.Find(newCondition.GoToQuestionId).Number
                ? null : newCondition.GoToQuestionId;

            Db.SaveChanges();
            return RedirectToAction("Index", new { id = formQuestionCondition.QuestionId });
        }

        private bool IsConditionJsonValid(FormQuestionCondition condition)
        {
            try
            {
                // TODO: Someday...
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        // POST: FormQuestionConditions/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveUp(int id)
        {
            FormQuestionCondition questionCondition = FindFormQuestionCondition_Restricted(id);
            FormQuestionCondition prevCondition = questionCondition.Question.Conditions.Where(c => c.Number == questionCondition.Number - 1).SingleOrDefault();
            if (prevCondition == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            questionCondition.Number--;
            prevCondition.Number++;
            Db.SaveChanges();
            return RedirectToAction("Index", new { id = questionCondition.QuestionId });
        }

        // POST: FormQuestionConditions/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveDown(int id)
        {
            FormQuestionCondition questionCondition = FindFormQuestionCondition_Restricted(id);
            FormQuestionCondition nextCondition = questionCondition.Question.Conditions.Where(c => c.Number == questionCondition.Number + 1).SingleOrDefault();
            if (nextCondition == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            questionCondition.Number++;
            nextCondition.Number--;
            Db.SaveChanges();
            return RedirectToAction("Index", new { id = questionCondition.QuestionId });
        }

        // GET: FormQuestionConditions/Insert/5
        public ActionResult Insert(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestionCondition formQuestionCondition = FindFormQuestionCondition_Restricted(id);
            if (formQuestionCondition == null)
                return HttpNotFound();

            return RedirectToAction("Add", new { id = formQuestionCondition.QuestionId, insertAfterId = formQuestionCondition.ConditionId });
        }

        // GET: FormQuestionConditions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestionCondition formQuestionCondition = FindFormQuestionCondition_Restricted(id);
            if (formQuestionCondition == null)
                return HttpNotFound();

            return View(formQuestionCondition);
        }

        // POST: FormQuestionConditions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FormQuestionCondition questionCondition = FindFormQuestionCondition_Restricted(id);
            foreach (FormQuestionCondition condition in questionCondition.Question.Conditions.Where(c => c.Number > questionCondition.Number).ToList())
                condition.Number--;
            Db.FormQuestionConditions.Remove(questionCondition);
            Db.SaveChanges();
            return RedirectToAction("Index", new { id = questionCondition.QuestionId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Db.Dispose();
            base.Dispose(disposing);
        }
    }
}