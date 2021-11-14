using MementoHealth.Attributes;
using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Filters;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.ProviderAdmin + "," + Role.Doctor + "," + Role.Assistant)]
    public class SubmissionsController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        private FormSubmission FindSubmission_Restricted(int? id)
        {
            int providerId = GetCurrentUserProvider().ProviderId;
            return Db.FormSubmissions.Where(s => s.SubmissionId == id && s.Form.ProviderId == providerId).SingleOrDefault();
        }

        private Form FindForm_Restricted(int? id)
        {
            return GetCurrentUserProvider().Forms.Where(f => f.FormId == id).SingleOrDefault();
        }

        private Patient FindPatient_Restricted(int? id)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.PatientId == id).SingleOrDefault();
        }

        private Provider GetCurrentUserProvider()
        {
            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // GET: Submissions
        public ActionResult Index()
        {
            int providerId = GetCurrentUserProvider().ProviderId;
            return View("All", Db.FormSubmissions
                .Where(s => s.Form.ProviderId == providerId || s.Patient.ProviderId == providerId)
                .OrderByDescending(q => q.SubmissionStartDate).ToList());
        }

        // GET: Submissions/Form/5
        public ActionResult Form(int id) // ID is FormId
        {
            Form form = FindForm_Restricted(id);
            if (form == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (form.Submissions.Count == 0)
                return RedirectToAction("Index", "Forms");

            return View("Form", form.Submissions.OrderByDescending(q => q.SubmissionStartDate).ToList());
        }

        // GET: Submissions/Form/5
        public ActionResult Patient(int id) // ID is PatientId
        {
            Patient patient = FindPatient_Restricted(id);
            if (patient == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (patient.Submissions.Count == 0)
                return RedirectToAction("Index", "Patients");

            return View("Patient", patient.Submissions.OrderByDescending(q => q.SubmissionStartDate).ToList());
        }

        // GET: Submissions/Start/5
        public ActionResult Start(int id) // PatientId
        {
            Patient patient = FindPatient_Restricted(id);
            if (patient == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Provider provider = GetCurrentUserProvider();

            return View(new ChooseFormViewModel
            {
                PatientId = patient.PatientId,
                Patient = patient,
                Forms = provider.Forms.Where(f => f.IsPublished).OrderBy(f => f.Name).ToList()
            });
        }

        // POST: Submissions/Start
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Start(ChooseFormViewModel model)
        {
            Form form = FindForm_Restricted(model.FormId);
            if (!ModelState.IsValid || form == null)
            {
                ModelState.AddModelError("", "Invalid data selected.");
                return RedirectToAction("Start", new { id = model.PatientId });
            }

            Patient patient = FindPatient_Restricted(model.PatientId);
            if (patient == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormSubmission submission = Db.FormSubmissions.Add(new FormSubmission
            {
                FormId = form.FormId,
                PatientId = patient.PatientId,
                SubmissionStartDate = DateTime.Now
            });
            Db.SaveChanges();

            return RedirectToAction("Answer", new { id = submission.SubmissionId });
        }

        // GET: Submissions/Answer/5
        [AllowThroughPinLock]
        public ActionResult Answer(int id, int? questionId)
        {
            FormSubmission submission = FindSubmission_Restricted(id);
            FormQuestionAnswer answer = questionId == null ? null : submission.Answers.SingleOrDefault(a => a.QuestionId == questionId);
            FormQuestion question = answer?.Question;

            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (Db.Users.Find(User.Identity.GetUserId()).PinHash == null)
                return RedirectToAction("ChangePin", "Manage", new
                {
                    returnUrl = Url.Action("Answer", new { id = submission.SubmissionId }),
                    lockAfterChangingPin = true
                });
            else
                PinLockFilter.Enabled = true;


            // If no specific question has been requested:
            if (question == null)
            {
                question = submission.GetNextQuestion();

                // If reached the end of the form, go to the review page.
                if (question == null)
                    return View("Review", submission);

                // Get the current answer if has already been answered.
                answer = submission.Answers.SingleOrDefault(a => a.QuestionId == question.QuestionId);
            }
            else
                question = answer.Question;

            return View(new AnswerViewModel
            {
                SubmissionId = id,
                QuestionId = question.QuestionId,
                Patient = submission.Patient,
                Question = question,
                IsComplete = submission.IsComplete,
                CurrentQuestionNumber = submission.GetNumberOfAnsweredQuestions(question.QuestionId),
                NumberOfRemainingQuestions = submission.GetNumberOfRemainingQuestions(question),
                JsonData = answer?.JsonData
            });
        }

        // POST: Submissions/Answer/5
        [HttpPost]
        [AllowThroughPinLock]
        [ValidateAntiForgeryToken]
        public ActionResult Answer(AnswerViewModel model)
        {
            FormSubmission submission = FindSubmission_Restricted(model.SubmissionId);
            FormQuestion question = submission.Form.Questions.Where(q => q.QuestionId == model.QuestionId).SingleOrDefault();

            if (question == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormQuestionAnswer answer = submission.Answers.Where(a => a.QuestionId == model.QuestionId).SingleOrDefault();

            if (answer != null) // If this question has been answered before,
                answer.JsonData = model.JsonData; // Update it.
            else
                submission.Answers.Add(new FormQuestionAnswer
                {
                    JsonData = model.JsonData,
                    QuestionId = model.QuestionId
                });

            Db.SaveChanges();

            int? goToQuestionId = null;
            switch (model.NextAction)
            {
                case "next":
                    // Get the ID of the next question if it has been answered.
                    goToQuestionId = submission.GetNextQuestion(question.QuestionId)?.QuestionId;
                    break;
                case "previous":
                    // Get the ID of the previous question.
                    goToQuestionId = submission.GetPreviousQuestion(question.QuestionId)?.QuestionId;
                    break;
                    // case "review": break; // This will be automatically handled in the GET action.
            }

            return RedirectToAction("Answer", new
            {
                id = model.SubmissionId,
                questionId = goToQuestionId
            });
        }

        // POST: Submissions/Submit/5
        [HttpPost]
        [AllowThroughPinLock]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);

            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!submission.IsComplete)
                return RedirectToAction("Answer", new { id });

            if (submission.SubmissionEndDate == null)
            {
                submission.SubmissionEndDate = DateTime.Now;
                Db.SaveChanges();
            }

            return View("ThankYou", submission);
        }

        // GET: Submissions/Clone/5
        public ActionResult Clone(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);

            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(submission);
        }

        // POST: Submissions/Clone/5
        [HttpPost]
        [ActionName("Clone")]
        [ValidateAntiForgeryToken]
        public ActionResult CloneConfirmed(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);

            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            FormSubmission newSubmission = Db.FormSubmissions.Add(submission.Clone());
            newSubmission.SubmissionStartDate = DateTime.Now;
            Db.SaveChanges();

            return RedirectToAction("Answer", new { id = newSubmission.SubmissionId, questionId = newSubmission.Answers.FirstOrDefault()?.QuestionId });
        }

        // GET: Submissions/Details/5
        public ActionResult Details(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);
            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(submission);
        }

        // GET: Submissions/Delete/5
        public ActionResult Delete(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);
            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(submission);
        }

        // POST: Submissions/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);
            Db.FormSubmissions.Remove(submission);
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