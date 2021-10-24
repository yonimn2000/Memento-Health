using MementoHealth.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MementoHealth.Models
{
    public class PatientSearchModel
    {
        [DisplayName("Full Name Contains")]
        public string FullName { get; set; }

        [DisplayName("Birthday Equals")]
        public DateTime? Birthday { get; set; }

        [DisplayName("External Id Contains")]
        public string ExternalPatientId { get; set; }

        public IEnumerable<Patient> Results { get; set; }
    }
}