using MementoHealth.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MementoHealth.Entities
{
    public class FormQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        public int Number { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        [Column("Type")]
        [DisplayName("Answer Type")]
        public string TypeString
        {
            get => Type.ToString();
            set => Type = (QuestionType)Enum.Parse(typeof(QuestionType), value, true);
        }

        [NotMapped]
        public QuestionType Type { get; set; }

        [DisplayName("Data")]
        public string JsonData { get; set; }

        [DisplayName("Is Required")]
        public bool IsRequired { get; set; }

        [ForeignKey("Form")]
        public int FormId { get; set; }
        public virtual Form Form { get; set; }

        public virtual ICollection<FormQuestionAnswer> Answers { get; set; }

        [InverseProperty("Question")]
        public virtual ICollection<FormQuestionCondition> Conditions { get; set; }

        [InverseProperty("GoToQuestion")]
<<<<<<< Updated upstream
        public virtual ICollection<FormQuestionCondition> ConditionComeFroms { get; set; }

        [NotMapped]
        public ICollection<FormQuestion> PossibleNextQuestions
        {
            get
            {
                ICollection<FormQuestion> questions = new HashSet<FormQuestion>();
                foreach (FormQuestionCondition condition in Conditions)
                    questions.Add(condition.GoToQuestion);
                questions.Add(NextSequentialQuestion);
                return questions;
            }
        }

        [NotMapped]
        public ICollection<FormQuestionEdge> GraphEdges
        {
            get
            {
                ICollection<FormQuestionEdge> edges = new HashSet<FormQuestionEdge>();
                foreach (FormQuestionCondition condition in Conditions)
                    edges.Add(new FormQuestionEdge
                    {
                        Question = condition.GoToQuestion,
                        Condition = condition
                    });
                if (!edges.Any(e => e.Question == NextSequentialQuestion
                    || e.Condition.ToString(justCondition: true).Equals("If answer is anything...")))
                    edges.Add(new FormQuestionEdge
                    {
                        Question = NextSequentialQuestion,
                        Condition = null
                    });
                return edges;
            }
        }

        [NotMapped]
        public FormQuestion NextSequentialQuestion
            => Form.Questions.Where(q => q.Number == Number + 1).FirstOrDefault();

        [NotMapped]
        public bool CanBeMovedUp => !IsTopQuestion && ReferencedConditionOfAboveQuestion == null;
        
        [NotMapped]
        public bool CanBeMovedDown => !IsBottomQuestion && ConditionReferencingBottomQuestion == null;

        [NotMapped]
        public bool IsTopQuestion => Number <= 1;
        
        [NotMapped]
        public bool IsBottomQuestion => Number == (Form?.Questions.Max(q => q.Number) ?? 0);

        [NotMapped]
        public FormQuestionCondition ReferencedConditionOfAboveQuestion =>
            ConditionComeFroms.Where(c => c.Question.Number == Number - 1).FirstOrDefault();
        
        [NotMapped]
        public FormQuestionCondition ConditionReferencingBottomQuestion =>
            Conditions.Where(c => c.GoToQuestion != null && c.GoToQuestion.Number == Number + 1).FirstOrDefault();

        public string GetCannotMoveUpReason()
        {
            if (IsTopQuestion)
                return "The current question is already the top question.";

            FormQuestionCondition referencedCondition = ReferencedConditionOfAboveQuestion;
            if (referencedCondition != null)
                return $"The current question is referenced by condition #{referencedCondition.Number}" +
                    " of the previous question.";

            return null;
        }

        public string GetCannotMoveDownReason()
        {
            if (IsBottomQuestion)
                return "The current question is already the bottom question.";

            FormQuestionCondition referencingCondition = ConditionReferencingBottomQuestion;
            if (referencingCondition != null)
                return $"Condition #{referencingCondition.Number} of the current question" +
                    $" is referencing the next question.";

            return null;
        }

        public override string ToString() => Question;
=======
        public virtual ICollection<FormQuestionCondition> ConditionGoTos { get; set; }
>>>>>>> Stashed changes
    }
}