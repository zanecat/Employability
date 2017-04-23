using System.Linq;
using AutoMapper;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.ViewModels;

namespace EmployabilityWebApp
{
    /// <summary>
    /// Configures Automapper which is used throughout the project.
    /// </summary>
    public class AutoMapConfig
    {
        public static IMapper CreateMapper()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                // Basic high level view models.
                config.CreateMap<SelfAssessment, SelfAssessmentViewModel>().ReverseMap();
                config.CreateMap<CoreSkill, CoreSkillViewModel>().ReverseMap();

                // Element view models.
                config.CreateMap<DetailedElement, DetailedElementViewModel>()
                    .ForMember(dest => dest.DetailedOptions, opt => opt.MapFrom(
                        src => src.DetailedOptions.OrderBy(o => o.Position))).ReverseMap();

                config.CreateMap<SimplifiedElement, SimplifiedElementViewModel>().ReverseMap();
                config.CreateMap<TextElement, TextElementViewModel>().ReverseMap();
                config.CreateMap<DetailedOption, DetailedOptionViewModel>().ReverseMap();
                // Map the heirarchy so we can use polymorphism.
                config.CreateMap<CoreSkillElement, CoreSkillElementViewModel>()
                    .Include<DetailedElement, DetailedElementViewModel>()
                    .Include<TextElement, TextElementViewModel>()
                    .Include<SimplifiedElement, SimplifiedElementViewModel>();

                // Survey element view models.
                config.CreateMap<DetailedSurveyElement, DetailedSurveyElementViewModel>();
                config.CreateMap<TextSurveyElement, TextSurveyElementViewModel>();
                config.CreateMap<SimplifiedSurveyElement, SimplifiedSurveyElementViewModel>();
                // Map the heirarchy so we can use polymorphism.
                config.CreateMap<ISurveyElement, ISurveyElementViewModel>()
                    .Include<DetailedSurveyElement, DetailedSurveyElementViewModel>()
                    .Include<TextSurveyElement, TextSurveyElementViewModel>()
                    .Include<SimplifiedSurveyElement, SimplifiedSurveyElementViewModel>();

                // SubAnswer view models.
                config.CreateMap<DetailedAnswer, DetailedAnswerViewModel>()
                    .ForMember(dest => dest.ChoiceId, opt => opt.MapFrom(
                        src => src.Choice.Id));
                config.CreateMap<TextAnswer, TextAnswerViewModel>();
                config.CreateMap<SimplifiedAnswer, SimplifiedAnswerViewModel>();
                // Map the heirarchy so we can use polymorphism.
                config.CreateMap<SubAnswer, SubAnswerViewModel>()
                    .Include<DetailedAnswer, DetailedAnswerViewModel>()
                    .Include<TextAnswer, TextAnswerViewModel>()
                    .Include<SimplifiedAnswer, SimplifiedAnswerViewModel>();
                
                //Map the feedback detail data and the safeedback
                config.CreateMap<SaFeedback, FeedbackDetailData>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(
                        src => src.BasicUser.UserName))
                    .ForMember(dest => dest.selfAssessmentTitle, opt => opt.MapFrom(
                        src => src.selfAssessment.Title));
            });
            return mapperConfig.CreateMapper();
        }
    }
}