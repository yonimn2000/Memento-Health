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
                .OrderByDescending(q => q.SubmissionDate).ToList());
        }

        // GET: Submissions/Form/5
        public ActionResult Form(int id) // ID is FormId
        {
            Form form = FindForm_Restricted(id);
            if (form == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (form.Submissions.Count == 0)
                return RedirectToAction("Index", "Forms");

            return View("Form", form.Submissions.OrderByDescending(q => q.SubmissionDate).ToList());
        }

        // GET: Submissions/Form/5
        public ActionResult Patient(int id) // ID is PatientId
        {
            Patient patient = FindPatient_Restricted(id);
            if (patient == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (patient.Submissions.Count == 0)
                return RedirectToAction("Index", "Patients");

            return View("Patient", patient.Submissions.OrderByDescending(q => q.SubmissionDate).ToList());
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
                SubmissionDate = DateTime.Now
            });
            Db.SaveChanges();

            return RedirectToAction("Answer", new { id = submission.SubmissionId });
        }

        // GET: Submissions/Answer/5
        [AllowThroughPinLock]
        public ActionResult Answer(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);
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

            // TODO: Get next question.
            FormQuestion question = submission.Form.Questions.Where(q => q.Number == 6).OrderBy(q => Guid.NewGuid()).First();
            return View(new AnswerViewModel
            {
                SubmissionId = id,
                Patient = submission.Patient,
                Question = question,
                CurrentQuestionNumber = question.Number - 1,
                NumberOfRemainingQuestions = submission.Form.Questions.Count - question.Number
            });
        }

        // POST: Submissions/Answer/5
        [HttpPost]
        [AllowThroughPinLock]
        [ValidateAntiForgeryToken]
        public ActionResult Answer(AnswerViewModel model)
        {
            // TODO: Save answer.
            return RedirectToAction("Answer", new { id = model.SubmissionId });
        }

        // GET: Submissions/Details/5
        public ActionResult Details(int id)
        {
            FormSubmission submission = FindSubmission_Restricted(id);
            if (submission == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(); // TODO: Return details.
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