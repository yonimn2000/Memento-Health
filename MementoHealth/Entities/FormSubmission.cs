using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MementoHealth.Entities
{
    public class FormSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [DisplayName("Submission Date")]
        public DateTime? SubmissionDate { get; set; }

        [ForeignKey("Patient")]
        public int? PatientId { get; set; } // "Temporary" solution to multiple cascade paths problem.
        public virtual Patient Patient { get; set; }

        [ForeignKey("Form")]
        public int? FormId { get; set; } // "Temporary" solution to multiple cascade paths problem.
        public virtual Form Form { get; set; }

        public virtual ICollection<FormQuestionAnswer> Answers { get; set; }

        [NotMapped]
        [DisplayName("Is Complete")]
        public bool IsComplete => false; // TODO
    }
}