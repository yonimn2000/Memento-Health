using MementoHealth.Classes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Web.Helpers;

namespace MementoHealth.Entities
{
    public class FormQuestionCondition
    {
        [Key]
        public int ConditionId { get; set; }

        public int Number { get; set; }

        [Required]
        public string JsonData { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual FormQuestion Question { get; set; }

        [ForeignKey("GoToQuestion")]
        public int? GoToQuestionId { get; set; }
        public virtual FormQuestion GoToQuestion { get; set; }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool fullQuestion = false)
        {
            dynamic jsonData = Json.Decode(JsonData);
            if (jsonData == null || GoToQuestion == null)
                return "Invalid condition";

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("If answer ");

            string option = jsonData.option as string;
            stringBuilder.Append(option);

            if (!option.Equals("is anything", StringComparison.InvariantCultureIgnoreCase))
            {
                stringBuilder.Append(" ");
                switch (Question.Type)
                {
                    case QuestionType.Text:
                        stringBuilder.Append($"'{jsonData.text}'");
                        break;
                    case QuestionType.Number:
                        stringBuilder.Append(jsonData.number);
                        break;
                    case QuestionType.Date:
                        stringBuilder.Append(jsonData.date);
                        break;
                    case QuestionType.Checkboxes:
                        stringBuilder.Append($"'{string.Join("' and '", jsonData.checkboxes as DynamicJsonArray)}'");
                        break;
                    case QuestionType.Radiobuttons:
                        stringBuilder.Append($"'{jsonData.radiobutton}'");
                        break;
                    case QuestionType.Image:
                        int numberOfAreas = (jsonData.imageAreas as DynamicJsonArray).Length;
                        if(numberOfAreas == 1)
                        {
                            stringBuilder.Append(numberOfAreas);
                            stringBuilder.Append(" area");
                        }
                        else
                        {
                            stringBuilder.Append("one of ");
                            stringBuilder.Append(numberOfAreas);
                            stringBuilder.Append(" areas");
                        }
                        break;
                }
            }

            stringBuilder.Append(", go to ");
            if(fullQuestion)
                stringBuilder.Append($"'{GoToQuestion.Question}'");
            else
                stringBuilder.Append($"question #{GoToQuestion.Number}");
            stringBuilder.Append(".");
            return stringBuilder.ToString();
        }
    }
}