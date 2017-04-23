using System.ComponentModel.DataAnnotations;

namespace EmployabilityWebApp.Models
{
    /// CoreSkillElement coresponds to a single question of CoreSkill.
    /// This class should be inherited by concrete classes for different
    /// types of elements.
    public abstract class CoreSkillElement
    {

        public int Id { get; private set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Range(1, 100)]
        public int Position { get; private set; }

        public CoreSkillElement(string description)
        {
            Description = description;
        }

        protected CoreSkillElement(string description, int order)
        {
            Description = description;
            Position = order;
        }

        public CoreSkillElement(int id, string description)
        {
            Id = id;
            Description = description;
        }

        /// <summary>
        /// Used by Entity Framework, exposed to subclasses for proxy
        /// construction.
        /// </summary>
        protected CoreSkillElement() { }

        // Allows doctoring the id for testing.
        protected CoreSkillElement(int id, string description, int order)
            : this(description, order)
        {
            Id = id;
        }
    }
}
