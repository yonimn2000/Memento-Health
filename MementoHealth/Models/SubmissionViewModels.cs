using MementoHealth.Entities;
using System.Collections.Generic;
using System.ComponentModel;

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
        public int SubmissionId { get; set; }
        public FormQuestion Question { get; set; }
        public int CurrentQuestionNumber { get; set; }
        public int NumberOfRemainingQuestions { get; set; }
        public int GetProgress() => 100 * CurrentQuestionNumber / (CurrentQuestionNumber + NumberOfRemainingQuestions);
    }
}