using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MementoHealth.Models
{
    public class PatientSearchModel
    {
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        public DateTime? Birthday { get; set; }

        public string ExternalId { get; set; }
    }
}