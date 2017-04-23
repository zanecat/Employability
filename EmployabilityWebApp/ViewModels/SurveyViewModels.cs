using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EmployabilityWebApp.Models;

namespace EmployabilityWebApp.ViewModels
{
    public class SurveyElementViewModel<TElement, TSubAnswer>
        : ISurveyElementViewModel
        where TElement : CoreSkillElementViewModel
        where TSubAnswer : SubAnswerViewModel
    {
        public TElement Element { get; set; }
        public TSubAnswer SubAnswer { get; set; }

        CoreSkillElementViewModel ISurveyElementViewModel.Element => Element;

        SubAnswerViewModel ISurveyElementViewModel.SubAnswer => SubAnswer;
    }

    public interface ISurveyElementViewModel
    {
        CoreSkillElementViewModel Element { get; }
        SubAnswerViewModel SubAnswer { get; }
    }

    public class DetailedSurveyElementViewModel
        : SurveyElementViewModel<DetailedElementViewModel, DetailedAnswerViewModel>
    { }

    public class TextSurveyElementViewModel
        : SurveyElementViewModel<CoreSkillElementViewModel, TextAnswerViewModel>
    { }

    public class SimplifiedSurveyElementViewModel
        : SurveyElementViewModel<CoreSkillElementViewModel, SimplifiedAnswerViewModel>
    { }

    public class SurveyViewModel
    {
        public CoreSkillNavBarData CoreSkillNavBarData { get; set; }
        public CoreSkillViewModel CoreSkill { get; set; }
        public List<ISurveyElementViewModel> SurveyElements { get; set; }
    }

    public class PostSurveyViewModel
    {
        public CoreSkillViewModel CoreSkill { get; set; }

        // Use default values of empty lists for cases where a
        // survey has no elements of a particular type.
        public List<DetailedSurveyElementViewModel> DetailedSurveyElements { get; set; }
            = new List<DetailedSurveyElementViewModel>();
        public List<TextSurveyElementViewModel> TextSurveyElements { get; set; }
            = new List<TextSurveyElementViewModel>();
        public List<SimplifiedSurveyElementViewModel> SimplifiedSurveyElements { get; set; }
            = new List<SimplifiedSurveyElementViewModel>();
    }

    public class SubAnswerViewModel
    {
        [Required]
        public int ElementId { get; set; }
    }

    public class DetailedAnswerViewModel : SubAnswerViewModel
    {
        public int? ChoiceId { get; set; }
    }

    public class TextAnswerViewModel : SubAnswerViewModel
    {
        public string Text { get; set; }
    }

    public class SimplifiedAnswerViewModel : SubAnswerViewModel
    {
        public const int MinChoice = SimplifiedAnswer.MinChoice;
        public const int MaxChoice = SimplifiedAnswer.MaxChoice;

        [Range(MinChoice, MaxChoice)]
        public int Choice { get; set; }
    }
}