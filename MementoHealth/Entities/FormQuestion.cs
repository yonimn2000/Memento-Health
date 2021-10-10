using MementoHealth.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MementoHealth.Entities
{
    public class FormQuestion
    {
        [Key]
        public int QuestionId { get; set; }

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

        [ForeignKey("NextQuesiton")]
        public int? NextQuestionId { get; set; }
        public virtual FormQuestion NextQuesiton { get; set; }

        public virtual ICollection<FormQuestionAnswer> Answers { get; set; }
        public virtual ICollection<FormQuestionCondition> Conditions { get; set; }
    }
}