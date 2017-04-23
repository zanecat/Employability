using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using EmployabilityWebApp.Models;

namespace EmployabilityWebApp.ViewModels
{
    public class CoreSkillViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public virtual ICollection<CoreSkillElement> Elements { get; set; }
        
        public virtual SelfAssessment SelfAssessment { get; set; }
        public bool IsFinished { get; set; }

        public CoreSkillViewModel()
        {

        }

        public CoreSkillViewModel(int id, string name, string description, ICollection<CoreSkillElement> elements)
        {
            Id = id;
            Name = name;
            Description = description;
            Elements = elements;
        }

        public CoreSkillViewModel(int id, string name, string description, ICollection<CoreSkillElement> elements, SelfAssessment selfAssessment)
        {
            Id = id;
            Name = name;
            Description = description;
            Elements = elements;
            SelfAssessment = selfAssessment;
        }
    }

    public class CoreSkillIndexData
    {
        public IList<CoreSkillViewModel> CoreSkills { get; set; }
        public int SelfAssessmentId { get; set; }
    }

    public class CoreSkillDetailData
    {
        public CoreSkillNavBarData CoreSkillNavBarData { get; set; }
        public IList<CoreSkillViewModel> CoreSkills { get; set; }
        public CoreSkillViewModel CoreSkill { get; set; }
        public IList<DetailedElementViewModel> DetailedElements;
        public IList<SimplifiedElementViewModel> SimplifiedElements;
        public IList<TextElementViewModel> TextElements;
    }

    public class CoreSkillNavBarData
    {
        public IList<CoreSkillViewModel> CoreSkills { get; set; }
        public int CurrentCoreSkillId { get; set; }
        public int SelfAssessmentId { get; set; }
    }
}
