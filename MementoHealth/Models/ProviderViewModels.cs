using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MementoHealth.Models
{
    public class StatsViewModel
    {
        public int ProviderCount { get; set; }
        public int PatientCount { get; set; }
        public int FormCount { get; set; }
        public int SubmissionCount { get; set; }
        public int UserCount { get; set; }
        public double AveragePatients { get; set; }
        public double AverageForms { get; set; }
        public double AverageUsers { get; set; }
    }
}