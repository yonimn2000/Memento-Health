using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
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

        private Patient FindPatientByExternalId_Restricted(string externalId)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => f.ExternalPatientId.Equals(externalId)).SingleOrDefault();
        }

        private Patient FindPatientByNameAndBirthday(string fullName, DateTime birthday)
        {
            string userId = User.Identity.GetUserId();
            return Db.Users.Find(userId).Provider.Patients.Where(f => f.FullName.Equals(fullName) && f.Birthday.Equals(birthday)).SingleOrDefault();
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase patientsFile)
        {
            List<string> successList = new List<String>();
            List<string> errorList = new List<string>();
            int existsCounter = 0;

            try
            {
                if (patientsFile.ContentLength > 0)
                {
                    StreamReader fileReader = new StreamReader(patientsFile.InputStream);
                    int providerId = GetCurrentUserProvider().ProviderId;


                    while (!fileReader.EndOfStream)
                    {

                        //  Each line is turned into currentPatient array in the loop
                        string[] currentPatient = fileReader.ReadLine().Split(',');


                        // Validating each line before performing operations
                        if (currentPatient[1].Length > 0 && !currentPatient[1].Any(char.IsDigit) && DateTime.TryParse(currentPatient[2], out DateTime testDate))
                        {
                            // Check if patient with external id exists already
                            Patient testPatient = FindPatientByNameAndBirthday(currentPatient[1], DateTime.Parse(currentPatient[2]));

                            // If statement checks that data being entered is not the header and doesnt exist already (or if it does checks that it updates a record)
                            if ((!currentPatient[0].ToLower().Contains("externalid") && testPatient == null) ||
                                (!currentPatient[0].ToLower().Contains("externalid") && testPatient != null && 
                                (testPatient.FullName != currentPatient[1] || testPatient.Birthday != DateTime.Parse(currentPatient[2]))))
                            {
                                Patient newPatient = new Patient
                                {
                                    ExternalPatientId = currentPatient[0].Length > 0 ? currentPatient[0] : null,
                                    FullName = currentPatient[1],
                                    Birthday = DateTime.Parse(currentPatient[2]),
                                    ProviderId = providerId
                                };
                                Db.Patients.Add(newPatient);
                                successList.Add(string.Join(",", currentPatient));
                            } else 
                            {
                                existsCounter++;
                            }
                        } else
                        {
                            errorList.Add(string.Join(",", currentPatient));
                        }                                           
                    }
                    Db.SaveChanges();
                }

                ViewBag.Message = "File upload was successful";
                ViewBag.Exists = existsCounter > 0 ? existsCounter + " records already exist" : "";
                ViewBag.SuccessString = successList.Count() > 0 ? "Successfuly added records: " : "";
                ViewBag.Success = successList;
                // Remove header from errors list
                errorList.RemoveAt(0);
                ViewBag.FailureString = errorList.Count() > 0 ? "The following records failed to import: " : "";
                ViewBag.Failure = errorList;
                return View();
            }
            catch (Exception e)
            {
                ViewBag.Message = "An error occurred: " + e.Message;
                return View();
            }
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
