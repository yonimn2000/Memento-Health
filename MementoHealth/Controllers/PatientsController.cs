using MementoHealth.Classes;
using MementoHealth.Entities;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
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

        private IEnumerable<Patient> FindPatientsByNameAndBirthday(string name, DateTime birthday)
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

        // POST: Patients/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase patientsFile)
        {
            ImportPatientsResultsViewModel model = new ImportPatientsResultsViewModel();
            StreamReader fileReader = new StreamReader(patientsFile.InputStream);

            try
            {
                // Skip first line.
                fileReader.ReadLine();

                int currentProviderId = GetCurrentUserProvider().ProviderId;

                while (!fileReader.EndOfStream)
                {
                    string currentLine = fileReader.ReadLine();

                    // Each line is turned into an array.
                    string[] currentLineTokens = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")
                        .Split(currentLine).Select(t => t.Trim('"').Trim()).ToArray();

                    string patientExternalId = currentLineTokens[0];
                    string patientName = currentLineTokens[1];
                    string patientBirthdayString = currentLineTokens[2];

                    // Validate name and birthday.
                    if (string.IsNullOrWhiteSpace(patientName) || !DateTime.TryParse(patientBirthdayString, out DateTime patientBirthday))
                    {
                        model.ErrorLines.Add(currentLine);
                        continue;
                    }

                    // If external ID is given:
                    if (!string.IsNullOrWhiteSpace(patientExternalId))
                    {
                        // Try to find the patient.
                        Patient patient = FindPatientsByExternalId(patientExternalId).FirstOrDefault();

                        // If found:
                        if (patient != null)
                        {
                            // Update patient details if needed:
                            if (patient.Birthday != patientBirthday || !patient.FullName.Equals(patientName))
                            {
                                patient.Birthday = patientBirthday;
                                patient.FullName = patientName;
                                model.UpdatedLines.Add(currentLine);
                                continue;
                            }

                            // Else:
                            model.ExistingLines.Add(currentLine);
                            continue;
                        }
                    }

                    // If not found by the external ID, try by name and birthday:
                    if (FindPatientsByNameAndBirthday(patientName, patientBirthday).Any())
                    {
                        model.ExistingLines.Add(currentLine);
                        continue;
                    }

                    // If not found by name and birthday, create a new patient
                    Patient newPatient = new Patient
                    {
                        ExternalPatientId = patientExternalId,
                        FullName = patientName,
                        Birthday = patientBirthday,
                        ProviderId = currentProviderId
                    };

                    Db.Patients.Add(newPatient);
                    model.ImportedLines.Add(currentLine);
                }

                Db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                model.ExceptionThrown = true;
            }
            finally
            {
                fileReader.Close();
            }

            return View("ImportResults", model);
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
                    foundPatients = FindPatientsByNameAndBirthday(model.FullName, (DateTime)model.Birthday);
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
