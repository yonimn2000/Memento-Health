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
        public bool IsComplete => GetNextQuestion() == null;

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

        public int GetNumberOfAnsweredQuestions(int toQuestionId) => GetAnswers(toQuestionId).Count; // Get current answer graph depth.

        public int GetNumberOfRemainingQuestions(FormQuestion fromQuestion) // Calculate the depth of the graph from a question.
        {
            if (fromQuestion == null)
                return 0;

            ICollection<FormQuestion> nextQuestions = fromQuestion.GetPossibleNextQuestions();
            return nextQuestions.Max(q => GetNumberOfRemainingQuestions(q)) + 1;
        }

        public FormQuestion GetNextQuestion() // Traverse to the last answer and get the next question.
        {
            // If this is a new submission, return the first question of the form.
            if (Answers.Count == 0)
                return Form.GetFirstQuestion();

            return GetAnswers().LastOrDefault()?.GetNextQuestion();
        }

        public IList<FormQuestionAnswer> GetAnswers(int toQuestionId = -1)
        {
            IList<FormQuestionAnswer> answers = new List<FormQuestionAnswer>();

            // Get the first question of the form.
            FormQuestion firstQuestion = Form.GetFirstQuestion();

            // Convert to a dictionary to improve performance.
            Dictionary<FormQuestion, FormQuestionAnswer> answersDict = Answers.ToDictionary(a => a.Question);

            // Get the first node (root) of the answer graph.
            FormQuestionAnswer answerWalker = FindQuestionInDict(answersDict, firstQuestion);
            while (answerWalker != null)
            {
                answers.Add(answerWalker);

                if (answerWalker.QuestionId == toQuestionId)
                    break;

                FormQuestion nextQuestion = answerWalker.GetNextQuestion();
                answerWalker = FindQuestionInDict(answersDict, nextQuestion);
            }

            return answers;
        }

        private static FormQuestionAnswer FindQuestionInDict(Dictionary<FormQuestion, FormQuestionAnswer> answers, FormQuestion question)
        {
            return question == null ? null : answers.TryGetValue(question, out FormQuestionAnswer answer) ? answer : default;
        }

        // Get the next question for a specific question if the prior has been answered.
        public FormQuestion GetNextQuestion(int questionId) => Answers.FirstOrDefault(a => a.QuestionId == questionId)?.GetNextQuestion();

        // Get the previous question for a specific question.
        public FormQuestion GetPreviousQuestion(int questionId)
        {
            IList<FormQuestionAnswer> answers = GetAnswers();
            int answerIndex = -1;
            for (int i = 0; i < answers.Count; i++)
                if (answers[i].QuestionId == questionId)
                {
                    answerIndex = i;
                    break;
                }

            if (answerIndex < 0)
                return null;

            return answers[answerIndex - 1].Question;
        }

        public FormSubmission Clone()
        {
            return new FormSubmission
            {
                FormId = FormId,
                PatientId = PatientId,
                Answers = Answers.Select(a => a.Clone()).ToList()
            };
        }
    }
}