using MementoHealth.Classes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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

        public string ToString(bool fullQuestion = false, bool justCondition = false)
        {
            dynamic jsonData = Json.Decode(JsonData);
            if (jsonData == null)
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
                        if (numberOfAreas == 1)
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

            if (justCondition)
                stringBuilder.Append("...");
            else
            {
                stringBuilder.Append(", go to ");
                if (GoToQuestion == null)
                    stringBuilder.Append("end of form");
                else
                {
                    if (fullQuestion)
                        stringBuilder.Append($"'{GoToQuestion.Question}'");
                    else
                        stringBuilder.Append($"question #{GoToQuestion.Number}");
                }
                stringBuilder.Append(".");
            }

            return stringBuilder.ToString();
        }

        internal bool Matches(string answerJsonData)
        {
            dynamic answerData = Json.Decode(answerJsonData);
            dynamic conditionData = Json.Decode(JsonData);

            string conditionOption = (conditionData.option as string).ToLower();

            if (conditionOption.Equals("is anything"))
                return true;

            switch (Question.Type)
            {
                case QuestionType.Text:
                    {
                        string answer = (answerData.answer as string).ToLower();
                        string conditionText = (conditionData.text as string).ToLower();
                        switch (conditionOption)
                        {
                            case "is": return answer.Equals(conditionText);
                            case "is not": return !answer.Equals(conditionText);
                            case "has": return answer.Contains(conditionText);
                            case "does not have": return !answer.Contains(conditionText);
                        }
                    }
                    break;
                case QuestionType.Number:
                    {
                        int answer = int.Parse(answerData.answer);
                        int conditionNumber = int.Parse(conditionData.number);
                        switch (conditionOption)
                        {
                            case "equals": return answer == conditionNumber;
                            case "not equals": return answer != conditionNumber;
                            case "is less than": return answer < conditionNumber;
                            case "is not less than": return answer >= conditionNumber;
                            case "is greater than": return answer > conditionNumber;
                            case "is not greater than": return answer <= conditionNumber;
                        }
                    }
                    break;
                case QuestionType.Date:
                    {
                        DateTime answer = DateTime.Parse(answerData.answer);
                        DateTime conditionDate = DateTime.Parse(conditionData.date);
                        switch (conditionOption)
                        {
                            case "is on": return answer == conditionDate;
                            case "is not on": return answer != conditionDate;
                            case "is before": return answer < conditionDate;
                            case "is not before": return answer >= conditionDate;
                            case "is after": return answer > conditionDate;
                            case "is not after": return answer <= conditionDate;
                        }
                    }
                    break;
                case QuestionType.Checkboxes:
                    {
                        string[] answers = (answerData.answer as DynamicJsonArray).Select(a => (a as string).ToLower()).ToArray();
                        string[] conditionAnswers = (conditionData.checkboxes as DynamicJsonArray).Select(a => (a as string).ToLower()).ToArray();
                        switch (conditionOption)
                        {
                            case "consists of": return answers.Length == conditionAnswers.Length && answers.Intersect(conditionAnswers).Count() == answers.Length;
                            case "does not consist of": return answers.Length != conditionAnswers.Length || answers.Intersect(conditionAnswers).Count() != answers.Length;
                        }
                    }
                    break;
                case QuestionType.Radiobuttons:
                    {
                        string answer = (answerData.answer as string).ToLower();
                        string conditionText = (conditionData.radiobutton as string).ToLower();
                        switch (conditionOption)
                        {
                            case "is": return answer.Equals(conditionText);
                            case "is not": return !answer.Equals(conditionText);
                        }
                    }
                    break;
                case QuestionType.Image:
                    {
                        SelectAreaPoint answer = Json.Decode<SelectAreaPoint>(Json.Encode(answerData.answer));
                        SelectArea[] conditionAreas = Json.Decode<SelectArea[]>(Json.Encode(conditionData.imageAreas));
                        switch (conditionOption)
                        {
                            case "is in": return SelectArea.IsPointInAnyArea(answer, conditionAreas);
                            case "is not in": return !SelectArea.IsPointInAnyArea(answer, conditionAreas);
                        }
                    }
                    break;
            }

            return false;
        }
    }
}