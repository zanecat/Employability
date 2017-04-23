using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// SubAnswer is a user's answer for a single question.
    /// Meant to be inherited by classes which represent answers to
    /// specified element types.
    public abstract class SubAnswer
    {
        protected SubAnswer() { }

        // Allows doctoring of id for testing.
        protected SubAnswer(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        [Required]
        public Answer Answer { get; [UsedImplicitly] private set; }

        [NotMapped]
        public abstract CoreSkillElement Element { get; }

        [NotMapped]
        public abstract bool IsFinished { get; }
    }
}
