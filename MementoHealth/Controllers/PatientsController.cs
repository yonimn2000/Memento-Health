using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MementoHealth.Controllers
{
    [Authorize(Roles = Role.ProviderAdmin + "," + Role.Doctor + "," + Role.Assistant)]
    public class PatientsController : Controller
    {
        private ApplicationDbContext Db { get; } = new ApplicationDbContext();

        // GET: Patients
        public ActionResult Index()
        {
            return View(GetCurrentUserProvider().Patients.OrderBy(p => p.FullName).ToList());
        }

        private Provider GetCurrentUserProvider()
        {
            if (User.IsInRole(Role.SysAdmin))
                return null;
            return Db.Users.Find(User.Identity.GetUserId()).Provider;
        }

        // GET: Patients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Patient patient = FindPatient_Restricted(id);
            if (patient == null)
                return HttpNotFound();

            return View(patient);
        }

        private Patient FindPatient_Restricted(int? id)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => f.PatientId == id).SingleOrDefault();
        }

        private IEnumerable<Patient> FindPatients_Name(string name)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => f.FullName.StartsWith(name)).ToList();
        }

        private IEnumerable<Patient> FindPatients_Birthday(DateTime birthday)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => f.Birthday == birthday).ToList();
        }

        private IEnumerable<Patient> FindPatients_ExternalId(string ExtId)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => f.ExternalPatientId.StartsWith(ExtId)).ToList();
        }

        private IEnumerable<Patient> FindPatients_NameAndBirth(string name, DateTime birthday)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => (f.FullName.StartsWith(name))&&(f.Birthday == birthday)).ToList();
        }

        private IEnumerable<Patient> FindPatients_NameAndId(string name, string ExtId)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => (f.FullName.StartsWith(name)) && (f.ExternalPatientId.StartsWith(ExtId))).ToList();
        }

        private IEnumerable<Patient> FindPatients_BirthAndId(DateTime birthday, string ExtId)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => (f.Birthday == birthday) && (f.ExternalPatientId.StartsWith(ExtId))).ToList();
        }

        private IEnumerable<Patient> FindPatients_All(string name, DateTime birthday, string ExtId)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => (f.FullName.StartsWith(name)) && (f.Birthday == birthday) && (f.ExternalPatientId.StartsWith(ExtId))).ToList();
        }

        // GET: Patients/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ExternalPatientId,FullName,Birthday")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                Patient newPatient = new Patient
                {
                    FullName = patient.FullName,
                    Birthday = patient.Birthday.Date,
                    ExternalPatientId = patient.ExternalPatientId,
                    ProviderId = GetCurrentUserProvider().ProviderId
                };
                Db.Patients.Add(newPatient);
                Db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(patient);
        }

        // GET: Patients/Import
        public ActionResult Import()
        {
            throw new NotImplementedException();
        }

        public ActionResult Search()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(PatientSearchModel patient)
        {
            if (ModelState.IsValid)
            {
                IEnumerable<Patient> foundPatients;
                if ((patient.FullName == null) && (patient.Birthday == null) && (patient.ExternalPatientId == null))
                {
                    ModelState.AddModelError("", "Please enter patient infromation in at least one field.");
                    return View(patient);
                }
                else if ((patient.FullName == null) && (patient.ExternalPatientId == null))
                {
                    foundPatients = FindPatients_Birthday((DateTime)patient.Birthday);
                }
                else if ((patient.Birthday == null) && (patient.ExternalPatientId == null))
                {
                    foundPatients = FindPatients_Name(patient.FullName);
                }
                else if ((patient.FullName == null) && (patient.Birthday == null))
                {
                    foundPatients = FindPatients_ExternalId(patient.ExternalPatientId);
                }
                else if (patient.ExternalPatientId == null)
                {
                    foundPatients = FindPatients_NameAndBirth(patient.FullName, (DateTime)patient.Birthday);
                }
                else if (patient.Birthday == null)
                {
                    foundPatients = FindPatients_NameAndId(patient.FullName, patient.ExternalPatientId);
                }
                else if (patient.FullName == null)
                {
                    foundPatients = FindPatients_BirthAndId((DateTime)patient.Birthday, patient.ExternalPatientId);
                }
                else
                {
                    foundPatients = FindPatients_All(patient.FullName, (DateTime)patient.Birthday, patient.ExternalPatientId);
                }

                if (foundPatients.Count() == 0)
                {
                    ModelState.AddModelError("", "Patient not found.");

                    return View(patient);
                }
                if (foundPatients.Count() == 1)
                {
                    return RedirectToAction("Details", new { id = foundPatients.SingleOrDefault().PatientId });
                }
                if (foundPatients.Count() > 1)
                {
                    return View("Index", foundPatients);
                }
            }

            return View(patient);
        }

        // GET: Patients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Patient patient = FindPatient_Restricted(id);
            if (patient == null)
                return HttpNotFound();

            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PatientId,ExternalPatientId,FullName,Birthday")] Patient patient)
        {
            Patient originalPatient = FindPatient_Restricted(patient.PatientId);
            if (originalPatient == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                originalPatient.ExternalPatientId = patient.ExternalPatientId;
                originalPatient.FullName = patient.FullName;
                originalPatient.Birthday = patient.Birthday.Date;
                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(patient);
        }

        // GET: Patients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Patient patient = FindPatient_Restricted(id);
            if (patient == null)
                return HttpNotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Patient patient = FindPatient_Restricted(id);
            Db.Patients.Remove(patient);
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
