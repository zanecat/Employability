using System.ComponentModel.DataAnnotations;

namespace EmployabilityWebApp.Models
{
    /// DetailedOption coresponds a single option of a detailed element.
    public class DetailedOption
    {
        public DetailedOption(string description, int position)
        {
            Description = description;
            Position = position;
        }

        // Allows doctoring of id for testing.
        public DetailedOption(int id, string description, int position)
            : this(description, position)
        {
            Id = id;
        }

        protected DetailedOption() { }

        public int Id { get; private set; }

        [Required]
        public string Description { get; set; }
        
        public DetailedOption(int id, string description)
        {
            Id = id;
            Description = description;
        }

        [Required]
        [Range(1, 100)]
        public int Position { get; private set; }
    }
}
