using MementoHealth.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MementoHealth.Entities
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Email { get; set; }

        public virtual ICollection<ApplicationUser> Staff { get; set; }
        public virtual ICollection<Patient> Patients { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
    }
}