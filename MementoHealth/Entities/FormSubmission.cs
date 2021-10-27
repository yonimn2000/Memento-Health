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

        [DisplayName("Submission Date")]
        public DateTime? SubmissionDate { get; set; }

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

        public FormQuestion GetNextQuestion()
        {
            // TODO: Get correct next question.
            FormQuestion lastAnsweredQuestion = Answers.OrderBy(a => a.Question.Number).Last().Question;
            return Form.Questions.Where(q => q.Number > lastAnsweredQuestion.Number).OrderBy(q => q.Number).FirstOrDefault();
        }
    }
}