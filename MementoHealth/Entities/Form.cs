using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MementoHealth.Entities
{
    public class Form
    {
        [Key]
        public int FormId { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }

        public virtual ICollection<FormSubmission> Submissions { get; set; }
        public virtual ICollection<FormQuestion> Questions { get; set; }
    }
}