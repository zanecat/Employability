using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EmployabilityWebApp.Models;
using System;


namespace EmployabilityWebApp.ViewModels
{
    public class SelfAssessmentViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime TimeStamp { get; set; }
        public ICollection<CoreSkill> CoreSkills { get; set; }
        public ICollection<Answer> Answers { get; set; }

        public SelfAssessmentViewModel()
        {

        }

        public SelfAssessmentViewModel(int id, string title, string description, ICollection<CoreSkill> coreSkills, ICollection<Answer> answers)
        {
            Id = id;
            Title = title;
            Description = description;
            CoreSkills = coreSkills;
            Answers = answers;
        }
    }

    public class SelfAssessmentIndexData
    {
        public IEnumerable<SelfAssessmentViewModel> SelfAssessments { get; set; }
    }

    public class SelfAssessmentDetailsData
    {
        public SelfAssessmentViewModel SelfAssessment;
        public IEnumerable<CoreSkillViewModel> CoreSkills;
    }
}