using System.ComponentModel.DataAnnotations;

namespace MementoHealth.Models
{
    public class FormViewModel
    {
        public int FormId { get; set; }
        public string Name { get; set; }
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