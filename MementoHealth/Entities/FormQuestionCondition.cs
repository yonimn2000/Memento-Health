using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MementoHealth.Entities
{
    public class FormQuestionCondition
    {
        [Key]
        public int ConditionId { get; set; }

        [Required]
        public string JsonCondition { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual FormQuestion Question { get; set; }
    }
}