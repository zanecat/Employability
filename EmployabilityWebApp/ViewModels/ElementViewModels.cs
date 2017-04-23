using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployabilityWebApp.ViewModels
{
    public class CoreSkillElementViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Position { get; set; }
    }

    public class DetailedElementViewModel : CoreSkillElementViewModel
    {
        [Required]
        public List<DetailedOptionViewModel> DetailedOptions { get; set; }

        public DetailedElementViewModel()
        {

        }

        public DetailedElementViewModel(int id, string description, List<DetailedOptionViewModel> detailedOptions, int position)
        {
            Id = id;
            Description = description;
            DetailedOptions = detailedOptions;
            Position = position;
        }
    }

    public class TextElementViewModel : CoreSkillElementViewModel
    {
        public TextElementViewModel()
        {

        }

        public TextElementViewModel(int id, string description, int position)
        {
            Id = id;
            Description = description;
            Position = position;
        }
    }

    public class SimplifiedElementViewModel : CoreSkillElementViewModel
    {
        public SimplifiedElementViewModel()
        {

        }

        public SimplifiedElementViewModel(int id, string description, int position)
        {
            Id = id;
            Description = description;
            Position = position;
        }
    }

    public class DetailedOptionViewModel : CoreSkillElementViewModel
    {
        public DetailedOptionViewModel()
        {

        }

        public DetailedOptionViewModel(int id, string description, int position)
        {
            Id = id;
            Description = description;
            Position = position;
        }
    }

    public class CoreSkillSimplifiedElementViewModel : CoreSkillElementViewModel
    {
        [Required]
        public int CoreSkillId { get; set; }

        public CoreSkillSimplifiedElementViewModel()
        {

        }

        public CoreSkillSimplifiedElementViewModel(int id, string description, int position, int coreSkillId)
        {
            Id = id;
            Description = description;
            Position = position;
            CoreSkillId = coreSkillId;
        }
    }

    public class CoreSkillTextElementViewModel : CoreSkillElementViewModel
    {
        [Required]
        public int CoreSkillId { get; set; }

        public CoreSkillTextElementViewModel()
        {

        }

        public CoreSkillTextElementViewModel(int id, string description, int position, int coreSkillId)
        {
            Id = id;
            Description = description;
            Position = position;
            CoreSkillId = coreSkillId;
        }
    }

    public class CoreSkillDetailedElementViewModel : CoreSkillElementViewModel
    {
        [Required]
        public List<DetailedOptionViewModel> DetailedOptions { get; set; }

        [Required]
        public int CoreSkillId { get; set; }

        public CoreSkillDetailedElementViewModel()
        {

        }

        public CoreSkillDetailedElementViewModel(int id, string description, List<DetailedOptionViewModel> detailedOptions, int position, int coreSkillId)
        {
            Id = id;
            Description = description;
            DetailedOptions = detailedOptions;
            Position = position;
            CoreSkillId = coreSkillId;
        }
    }
}