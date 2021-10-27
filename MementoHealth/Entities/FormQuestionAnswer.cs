using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MementoHealth.Entities
{
    public class FormQuestionAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public string JsonData { get; set; }

        [ForeignKey("FormSubmission")]
        public int SubmissionId { get; set; }
        public virtual FormSubmission FormSubmission { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual FormQuestion Question { get; set; }
    }
}