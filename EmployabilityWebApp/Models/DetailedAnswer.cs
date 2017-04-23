using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// A user's answer to a DetailedElement survey question.
    [Table("DetailedAnswer")]
    [ImplicitNotNull]
    public class DetailedAnswer : SubAnswer
    {
        [UsedImplicitly]
        protected DetailedAnswer() {}

        public DetailedAnswer(DetailedElement detailedElement)
        {
            DetailedElement = detailedElement;
        }

        [Required]
        public virtual DetailedElement DetailedElement { get; protected set; }

        [Required]
        public virtual DetailedOption Choice { get; protected set; }

        public void SetChoiceAndValidate(DetailedOption choice)
        {
            // If I have a DetailedElement, validate the option against it.
            if (DetailedElement != null && !DetailedElement.HasOption(choice))
            {
                throw new ArgumentException(
                    "element " + DetailedElement.Id
                    + " does not contain option " + choice.Id, nameof(choice));
            }
            Choice = choice;
        }

        [NotMapped]
        public override CoreSkillElement Element => DetailedElement;

        [NotMapped]
        public override bool IsFinished => DetailedElement.HasOption(Choice);
    }
}