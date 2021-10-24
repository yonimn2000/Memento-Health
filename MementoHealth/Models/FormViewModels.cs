using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MementoHealth.Models
{
    public class FormViewModel
    {
        public int FormId { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }

        [DisplayName("Questions")]
        public int NumberOfQuestions { get; set; }
        
        [DisplayName("Submissions")]
        public int NumberOfSubmissions { get; set; }
    }

    public class CreateFormViewModel
    {
        [Required]
        public int FormId { get; set; }


        [Required]
        public string Name { get; set; }
    }

    public class EditFormViewModel
    {
        [Required]
        public int FormId { get; set; }


        [Required]
        public string Name { get; set; }
    }
}