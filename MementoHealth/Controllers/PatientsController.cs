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
using System.Text.RegularExpressions;
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
            return GetCurrentUserProvider().Patients.Where(p => p.PatientId == id).SingleOrDefault();
        }

        private IEnumerable<Patient> FindPatientsByName(string name)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.NameContains(name)).ToList();
        }

        private IEnumerable<Patient> FindPatientsByBirthday(DateTime birthday)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.BirthdayEquals(birthday)).ToList();
        }

        private IEnumerable<Patient> FindPatientsByExternalId(string extId)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.ExtenalIdContains(extId)).ToList();
        }

        private IEnumerable<Patient> FindPatientsByNameAndBirth(string name, DateTime birthday)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.NameContains(name) && p.BirthdayEquals(birthday)).ToList();
        }

        private IEnumerable<Patient> FindPatientsByNameAndId(string name, string extId)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.NameContains(name) && p.ExtenalIdContains(extId)).ToList();
        }

        private IEnumerable<Patient> FindPatientsByBirthAndExternalId(DateTime birthday, string extId)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.BirthdayEquals(birthday) && p.ExtenalIdContains(extId)).ToList();
        }

        private IEnumerable<Patient> FindPatients(string name, DateTime birthday, string extId)
        {
            return GetCurrentUserProvider().Patients.Where(p => p.NameContains(name) && p.BirthdayEquals(birthday) && p.ExtenalIdContains(extId)).ToList();
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
            ImportPatientResultsViewModel model = new ImportPatientResultsViewModel();

            try
            {
                if (patientsFile.ContentLength > 0)
                {
                    StreamReader fileReader = new StreamReader(patientsFile.InputStream);
                    int providerId = GetCurrentUserProvider().ProviderId;


                    while (!fileReader.EndOfStream)
                    {

                        //  Each line is turned into currentPatient array in the loop
                        string currentPatient = fileReader.ReadLine();
                        string[] currentPatientArray = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))").Split(currentPatient);

                        for (int i = 0; i < currentPatientArray.Length; i++)
                        {
                            currentPatientArray[i] = currentPatientArray[i].Trim('"');
                        }

                        // Validating each line before performing operations
                        if (currentPatientArray[1].Length > 0 && DateTime.TryParse(currentPatientArray[2], out DateTime testDate))
                        {
                            // Check if patient with external id exists already
                            Patient testPatient = FindPatientByNameAndBirthday(currentPatientArray[1], testDate);

                            // If statement checks that data being entered is not the header and doesnt exist already (or if it does checks that it updates a record)
                            if ((!currentPatientArray[0].ToLower().Contains("externalid") && testPatient == null) ||
                                (!currentPatientArray[0].ToLower().Contains("externalid") && testPatient != null && 
                                (testPatient.FullName != currentPatientArray[1] || testPatient.Birthday != testDate)))
                            {
                                Patient newPatient = new Patient
                                {
                                    ExternalPatientId = currentPatientArray[0].Length > 0 ? currentPatientArray[0] : null,
                                    FullName = currentPatientArray[1],
                                    Birthday = testDate,
                                    ProviderId = providerId
                                };
                                Db.Patients.Add(newPatient);
                                model.SuccessList.Add(currentPatient);
                            } else 
                            {
                                model.ExistsCounter++;
                            }
                        } else
                        {
                            model.ErrorList.Add(currentPatient);
                        }                                           
                    }
                    Db.SaveChanges();
                }

                // Remove header from errors list
                model.ErrorList.RemoveAt(0);
                model.Message = "File upload was successful";
                model.Exists = model.ExistsCounter > 0 ? model.ExistsCounter + " record(s) already exist" : "";
                model.SuccessHeader = model.SuccessList.Count() > 0 ? "Successfuly added records: " : "";                    
                model.ErrorHeader = model.ErrorList.Count() > 0 ? "The following records failed to import: " : "";            

                return View("ImportResults", model);
            }
            catch (Exception e)
            {
                model.Message = "An error occurred: " + e.Message;
                return View("ImportResults", model);
            }
        }

        public ActionResult Search()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(PatientSearchModel model)
        {
            if (ModelState.IsValid)
            {
                IEnumerable<Patient> foundPatients;
                if ((model.FullName == null) && (model.Birthday == null) && (model.ExternalPatientId == null))
                {
                    ModelState.AddModelError("", "Please enter patient infromation in at least one field.");
                    return View(model);
                }
                else if ((model.FullName == null) && (model.ExternalPatientId == null))
                {
                    foundPatients = FindPatientsByBirthday((DateTime)model.Birthday);
                }
                else if ((model.Birthday == null) && (model.ExternalPatientId == null))
                {
                    foundPatients = FindPatientsByName(model.FullName);
                }
                else if ((model.FullName == null) && (model.Birthday == null))
                {
                    foundPatients = FindPatientsByExternalId(model.ExternalPatientId);
                }
                else if (model.ExternalPatientId == null)
                {
                    foundPatients = FindPatientsByNameAndBirth(model.FullName, (DateTime)model.Birthday);
                }
                else if (model.Birthday == null)
                {
                    foundPatients = FindPatientsByNameAndId(model.FullName, model.ExternalPatientId);
                }
                else if (model.FullName == null)
                {
                    foundPatients = FindPatientsByBirthAndExternalId((DateTime)model.Birthday, model.ExternalPatientId);
                }
                else
                {
                    foundPatients = FindPatients(model.FullName, (DateTime)model.Birthday, model.ExternalPatientId);
                }

                if (foundPatients.Count() == 0)
                {
                    ModelState.AddModelError("", "Patient not found.");
                    return View(model);
                }

                model.Results = foundPatients;
                return View("Search", model);
            }

            return View(model);
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
            Db.FormSubmissions.RemoveRange(patient.Submissions);
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
