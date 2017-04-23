using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// SelfAssessment coresponds to a single version of the whole survey.
    public class SelfAssessment
    {
        public int Id { get; [UsedImplicitly] private set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime TimeStamp { get; [UsedImplicitly] private set; }

        public virtual ICollection<CoreSkill> CoreSkills { get; set; }
        public virtual ICollection<Answer> Answers { get; protected set; }

        public SelfAssessment(string title, string description, ICollection<CoreSkill> coreSkills)
        {
            Title = title;
            Description = description;
            CoreSkills = coreSkills;
            Answers = new List<Answer>();
        }

        public SelfAssessment(int id, string title, string description, ICollection<CoreSkill> coreSkills, ICollection<Answer> answers)
        {
            Id = id;
            Title = title;
            Description = description;
            CoreSkills = coreSkills;
            Answers = answers;
        }

        /// <summary>
        /// Used by Entity Framework for proxy generation.
        /// </summary>
        public SelfAssessment() { }

        // Used for spoofing the id in testing.
        public SelfAssessment(int id)
        {
            Id = id;
        }

        protected SelfAssessment(int id, string title, string description, DateTime time,
            ICollection<CoreSkill> coreSkills):this(id)
        {
            Title = title;
            Description = description;
            CoreSkills = coreSkills;
            TimeStamp = time;
        }
    }
}
