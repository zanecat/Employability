using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    //organization user, includeing university and employee
    public class OrganisationUser : ApplicationUser
    {
        public const string Role = "Organisation";

        public OrganisationUser(string email) : base(email) { }

        [UsedImplicitly]
        private OrganisationUser() { }
       
        public DateTime? StartTime { get; private set; }

      
        public DateTime? EndTime { get; private set; }

        [Required]
        public string OrganisationName { get; set; }

        [Required]
        public string Domain { get; set; }
    }
}
