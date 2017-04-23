using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployabilityWebApp.Models
{
    /// CoreSkill coresponds to a page in the survey.
    public class CoreSkill
    {
        public int Id { get; private set; }

        [Required]
        public string Name { get; private set; }

        [Required]
        public string Description { get; private set; }

        [Required]
        public virtual ICollection<CoreSkillElement> Elements { get; set; }
        
        public virtual SelfAssessment SelfAssessment { get; set; }

       [NotMapped]
        public bool IsFinished { get; set; }

        public CoreSkill(int id, string name, string description,
                         ICollection<CoreSkillElement> elements)
            : this(id, name, description)
        {
            Elements = elements;
        }

        public CoreSkill(int id, string name, string description,
                         ICollection<CoreSkillElement> elements,
                         SelfAssessment selfAssessment)
            : this(id, name, description)
        {
            Elements = elements;
            SelfAssessment = selfAssessment;
        }

        public CoreSkill(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public CoreSkill(string name, string description,
                         ICollection<CoreSkillElement> elements)
        {
            Elements = elements;
            Name = name;
            Description = description;
        }

        public CoreSkill(string name, string description,
                         ICollection<CoreSkillElement> elements,
                         SelfAssessment selfAssessment)
        {
            Name = name;
            Description = description;
            Elements = elements;
            SelfAssessment = selfAssessment;
        }

        /// <summary>
        /// Used by Entity Framework for proxy generation.
        /// </summary>
        protected CoreSkill() { }
       
    }
}
