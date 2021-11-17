using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MementoHealth.Entities
{
    public class Form
    {
        [Key]
        public int FormId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsPublished { get; set; }

        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }

        public virtual ICollection<FormSubmission> Submissions { get; set; }
        public virtual ICollection<FormQuestion> Questions { get; set; }

        public FormQuestion GetFirstQuestion() => Questions.OrderBy(q => q.Number).FirstOrDefault();

        public Form Clone()
        {
            IDictionary<int, FormQuestion> newQuestions = Questions.Select(q => new FormQuestion
            {
                Number = q.Number,
                Question = q.Question,
                TypeString = q.TypeString,
                JsonData = q.JsonData,
                IsRequired = q.IsRequired
            }).ToDictionary(q => q.Number);

            foreach (FormQuestion question in Questions)
            {
                newQuestions[question.Number].Conditions = question.Conditions.Select(c => new FormQuestionCondition
                {
                    JsonData = c.JsonData,
                    Number = c.Number,
                    GoToQuestion = c.GoToQuestion == null ? null : newQuestions[c.GoToQuestion.Number]
                }).ToList();
            }

            return new Form
            {
                Name = Name,
                ProviderId = ProviderId,
                IsPublished = false,
                Questions = newQuestions.Values
            };
        }
    }
}