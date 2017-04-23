using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployabilityWebApp.Models
{
    /// A user's answer to a SimplifiedElement survey question.
    [Table("SimplifiedAnswer")]
    public class SimplifiedAnswer : SubAnswer
    {
        public const int MinChoice = 1;
        public const int MaxChoice = 9;

        public SimplifiedAnswer(SimplifiedElement element)
        {
            SimplifiedElement = element;
        }

        protected SimplifiedAnswer() { }

        [Required]
        public virtual SimplifiedElement SimplifiedElement { get; protected set; }

        [Required]
        [Range(MinChoice, MaxChoice)]
        public int Choice { get; private set; }

        [NotMapped]
        public override CoreSkillElement Element => SimplifiedElement;

        [NotMapped]
        public override bool IsFinished => !(MinChoice > Choice || MaxChoice < Choice);

        public void SetChoiceAndValidate(int choice)
        {
            if (MinChoice > choice || MaxChoice < choice)
            {
                throw new ArgumentException("can't choose outside " + MinChoice + " to " 
                    + MaxChoice + " for a SimplifiedAnswers");
            }
            Choice = choice;
        }
    }
}
