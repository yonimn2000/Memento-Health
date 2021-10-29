using MementoHealth.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

    public class ImportPatientResultsViewModel
    {
        public bool Success { get; set; } 
        public string Message { get; set; }
        public string SuccessHeader { get; set;  }
        public string ErrorHeader { get; set; }
        public int ExistsCounter { get; set; } = 0;
        public string Exists { get; set; }
        public List<string> SuccessList { get; set; } = new List<String>();
        public List<string> ErrorList { get; set; } = new List<string>();
    }
}