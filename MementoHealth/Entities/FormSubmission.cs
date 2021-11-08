using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MementoHealth.Entities
{
    public class FormSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [DisplayName("Start")]
        public DateTime SubmissionStartDate { get; set; }

        [DisplayName("End")]
        public DateTime? SubmissionEndDate { get; set; }

        [ForeignKey("Patient")]
        public int? PatientId { get; set; } // Nullable to avoid multiple cascade paths problem.
        public virtual Patient Patient { get; set; }

        [ForeignKey("Form")]
        public int? FormId { get; set; } // Nullable to avoid multiple cascade paths problem.
        public virtual Form Form { get; set; }

        public virtual ICollection<FormQuestionAnswer> Answers { get; set; }

        [NotMapped]
        [DisplayName("Is Complete")]
        public bool IsComplete => Answers.Count > 0 && GetNextQuestion() == null;

        [NotMapped]
        [DisplayName("Time to Complete")]
        public TimeSpan? TimeToComplete
        {
            get
            {
                if (SubmissionEndDate == null)
                    return null;

                TimeSpan diff = (DateTime)SubmissionEndDate - SubmissionStartDate;
                return new TimeSpan(diff.Days, diff.Hours, diff.Minutes, diff.Seconds);
            }
        }

        public FormQuestion GetNextQuestion() => Answers.OrderBy(a => a.Question.Number).Last().GetNextQuestion();
    }
}