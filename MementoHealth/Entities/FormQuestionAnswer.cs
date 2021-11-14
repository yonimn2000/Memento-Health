using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        public FormQuestion GetNextQuestion()
        {
            foreach (FormQuestionCondition condition in Question.Conditions.OrderBy(c => c.Number).ToList())
                if (condition.Matches(JsonData))
                    return condition.GoToQuestion;

            // If no conditions matched, go to the next ordinal question.
            return Question.NextOrdinalQuestion;
        }

        public FormQuestionAnswer Clone()
        {
            return new FormQuestionAnswer
            {
                QuestionId = QuestionId,
                JsonData = JsonData
            };
        }
    }
}