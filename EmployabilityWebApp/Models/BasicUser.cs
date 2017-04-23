using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    //basic user, incliuding student and employees, complete survy and generate reports
    public class BasicUser : ApplicationUser
    {
        public const string Role = "Basic";

        public BasicUser(string email) : base(email)
        {
            Answers = new List<Answer>();
        }

        [UsedImplicitly]
        protected BasicUser() { Answers = new List<Answer>();  }

        public string GivenName { get; set; }
        public string FamilyName { get; set; }

        [Required]
        public virtual ICollection<Answer> Answers { get; protected set; }

        public void AddAnswer(Answer answer)
        {
            Answers.Add(answer);
        }
    }
}