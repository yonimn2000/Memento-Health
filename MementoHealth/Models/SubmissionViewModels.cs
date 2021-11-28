using MementoHealth.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MementoHealth.Models
{
    public class ChooseFormViewModel
    {
        public Patient Patient { get; set; }
        public ICollection<Form> Forms { get; set; }
        public int PatientId { get; set; }

        [DisplayName("Pick a Form")]
        public int FormId { get; set; }
    }

    public class AnswerViewModel
    {
        // These are to pass data to the page
        public FormQuestion Question { get; set; }
        public Patient Patient { get; set; }
        public bool AnsweredAllQuestions { get; set; }
        public int CurrentQuestionNumber { get; set; }
        public int NumberOfRemainingQuestions { get; set; }
        public int GetProgress() => NumberOfRemainingQuestions == 0 ? 100 : 100 * CurrentQuestionNumber / (CurrentQuestionNumber + NumberOfRemainingQuestions);

        // These are to gather data from the page.
        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string JsonData { get; set; }

        public string NextAction { get; set; }
    }

    public class SubmissionHistoryViewModel
    {
        public Patient Patient { get; set; }
        public Form Form { get; set; }
        public IList<FormSubmission> Submissions { get; set; }
        public IDictionary<FormQuestion, List<string>> QuestionJsonAnswers { get; set; }
    }
}