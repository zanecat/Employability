using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EmployabilityWebApp.Models
{
    /// A multiple choice questions with detailed options.
    public class DetailedElement : CoreSkillElement
    {
        [Required]
        public virtual ICollection<DetailedOption> DetailedOptions { get; protected set; }

        public DetailedElement(int id, string description,
                               ICollection<DetailedOption> detailedOptions)
            : base(id, description)
        {
            DetailedOptions = detailedOptions;
        }

        public DetailedElement(string description, ICollection<DetailedOption> options, int position)
            : base(description, position)
        {
            DetailedOptions = options;
        }

        /// <summary>
        /// Used by Entity Framework for proxy generation.
        /// </summary>
        protected DetailedElement() { }

        // Allows doctoring the id for testing.
        public DetailedElement(int id, string description, 
            ICollection<DetailedOption> options, int position)
            : base(id, description, position)
        {
            DetailedOptions = options;
        }

        public bool HasOption(DetailedOption option)
        {
            return DetailedOptions.Contains(option);
        }
    }
}

