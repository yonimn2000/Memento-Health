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
            return new Form
            {
                Name = Name,
                ProviderId = ProviderId,
                IsPublished = false,
                Questions = Questions.Select(q => q.Clone()).ToList()
            };
        }
    }
}