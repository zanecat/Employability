using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployabilityWebApp.Models;
using JetBrains.Annotations;
using EmployabilityWebApp.ViewModels;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public interface ICoreSkillElementService
    {
        /// <summary>
        /// Create a new simplified element for the given core skill.
        /// If the given core skill has answer, returns true; else then false.
        /// If it is a true, then it creates a new version of self assessment; else, update the existing self assessment.
        /// </summary>
        /// <param name="elementId">Simplified element id</param>
        /// <param name="elementDescription">Simplified element description</param>
        /// <param name="coreSkillId">ID of simplified element's related CoreSkill</param>
        bool CreateSimplifiedElement(int elementId, string elementDescription, int coreSkillId);

        /// <summary>
        /// Edit an existing simplified element.
        /// </summary>
        /// <param name="elementId">Simplified element id</param>
        /// <param name="elementDescription">Simplified element description</param>
        void EditSimplifiedElement(int elementId, string elementDescription);

        /// <summary>
        /// Remove an existing simplified element.
        /// </summary>
        /// <param name="coreSkillId">ID of simplified element's related CoreSkill</param>
        /// <param name="elementId">Simplified element id</param>
        void DeleteSimplifiedElement(int coreSkillId, int elementId);

        /// <summary>
        /// Create a detailed element.
        /// If the given core skill has answer, returns true; else then false.
        /// If it is a true, then it creates a new version of self assessment; else, update the existing self assessment.
        /// </summary>
        /// <param name="elementId">Detailed element id</param>
        /// <param name="elementDescription">Detailed element description</param>
        /// <param name="elementDetailedOptions">Element detailed options</param>
        /// <param name="coreSkillId">ID of detailed element's related CoreSkill</param>
        bool CreateDetailedElement(int elementId, string elementDescription, List<DetailedOptionViewModel> elementDetailedOptions, int coreSkillId);

        /// <summary>
        /// Edit an existing detailed element.
        /// </summary>
        /// <param name="elementId">Detailed element id</param>
        /// <param name="elementDescription">Detailed element description</param>
        /// <param name="detailedOptions">Detailed options of the text element</param>
        void EditDetailedElement(int elementId, string elementDescription, List<DetailedOptionViewModel> detailedOptions);

        /// <summary>
        /// Remove an existing detailed element.
        /// </summary>
        /// <param name="coreSkillId">ID of detailed element's related CoreSkill</param>
        /// <param name="elementId">Detailed element id</param>
        void DeleteDetailedElement(int coreSkillId, int elementId);

        /// <summary>
        /// Create a text element.
        /// If the given core skill has answer, returns true; else then false.
        /// If it is a true, then it creates a new version of self assessment; else, update the existing self assessment.
        /// </summary>
        /// <param name="elementId">Text element id</param>
        /// <param name="elementDescription">Text element description</param>
        /// <param name="coreSkillId">ID of text element's related CoreSkill</param>
        bool CreateTextElement(int elementId, string elementDescription, int coreSkillId);

        /// <summary>
        /// Edit an existing text element.
        /// </summary>
        /// <param name="elementId">Text element id</param>
        /// <param name="elementDescription">Text element description</param>
        void EditTextElement(int elementId, string elementDescription);

        /// <summary>
        /// Remove a text element.
        /// </summary>
        /// <param name="coreSkillId">ID of text element's related CoreSkill</param>
        /// <param name="elementId">Text element id</param>
        void DeleteTextElement(int coreSkillId, int elementId);

        /// <summary>
        /// Get the latest created self-assessment version ID. 
        /// </summary>
        /// <returns></returns>
        int GetLatestSelfAssessmentVersion();
    }
}
