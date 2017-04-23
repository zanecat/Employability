using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmployabilityWebApp.Models
{
    public class Organisation
    {
        public int Id { get; private set; }

        public virtual ICollection<BasicUser> Users { get; set;}

        [Required]
        public virtual OrganisationUser OrganisationUser { get; set; }

        public virtual ICollection<SelfAssessment> SelfAssessments { get;  set; }
    }
}