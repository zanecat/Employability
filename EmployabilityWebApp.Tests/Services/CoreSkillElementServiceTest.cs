using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using Xunit;
using Moq;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.Tests.TestHelpers;
using System;
using System.Data.Entity;
using AutoMapper;
using EmployabilityWebApp.ViewModels;

namespace EmployabilityWebApp.Tests.Services
{
    public class CoreSkillElementServiceTest
    {
        private readonly CoreSkillElementService target;

        private readonly Mock<CoreSkillService> coreSkillService =
            new Mock<CoreSkillService>();

        private readonly Mock<EmployabilityDbContext> db =
            new Mock<EmployabilityDbContext>();
        private readonly Mock<IMapper> mapper =
            new Mock<IMapper>();

        private readonly Mock<CoreSkill> coreSkill =
            new Mock<CoreSkill>();
        private readonly Mock<SelfAssessment> selfAssessment =
            new Mock<SelfAssessment>(RIGHT_ID);
        private readonly Mock<Answer> answer =
            new Mock<Answer>();
        private const int RIGHT_ID = 123;

        private readonly Mock<BasicUser> user = new Mock<BasicUser>();
        
        private readonly List<TextElement> textElements =
            new List<TextElement>();
        private readonly List<SimplifiedElement> simplifiedElements =
            new List<SimplifiedElement>();
        private readonly List<DetailedElement> detailedElements =
            new List<DetailedElement>();
        private readonly List<DetailedOption> options =
            new List<DetailedOption>();

        public CoreSkillElementServiceTest()
        {
            target = new CoreSkillElementService(db.Object, mapper.Object);
        }

        [Fact]
        public void CreateSimplifiedElement_ShouldCreateNewSelfAssessmentVersionIfHasAnswer()
        {
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithAnswers();

            Assert.Equal(target.CreateSimplifiedElement(456, "element description", coreSkill.Object.Id), true);
        }

        [Fact]
        public void CreateSimplifiedElement_ShouldUpdateSelfAssessmentIfHasNoAnswer()
        {
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithoutAnswers();

            Assert.Equal(target.CreateSimplifiedElement(456, "element description", coreSkill.Object.Id), false);
        }

        [Fact]
        public void CreateTextElement_ShouldCreateNewSelfAssessmentVersionIfHasAnswer()
        {
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithAnswers();

            Assert.Equal(target.CreateTextElement(456, "element description", coreSkill.Object.Id), true);
        }

        [Fact]
        public void CreateTextElement_ShouldUpdateSelfAssessmentIfHasNoAnswer()
        {
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithoutAnswers();

            Assert.Equal(target.CreateTextElement(456, "element description", coreSkill.Object.Id), false);
        }

        [Fact]
        public void CreateDetailedElement_ShouldCreateNewSelfAssessmentVersionIfHasAnswer()
        {
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithAnswers();

            Mock<List<DetailedOptionViewModel>> elementDetailedOptions = new Mock<List<DetailedOptionViewModel>>();

            Assert.Equal(target.CreateDetailedElement(456, "element description", elementDetailedOptions.Object, coreSkill.Object.Id), true);
        }

        [Fact]
        public void CreateDetailedElement_ShouldUpdateSelfAssessmentIfHasNoAnswer()
        {
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithoutAnswers();

            Mock<List<DetailedOptionViewModel>> elementDetailedOptions = new Mock<List<DetailedOptionViewModel>>();
            Assert.Equal(target.CreateDetailedElement(456, "element description", elementDetailedOptions.Object, coreSkill.Object.Id), false);
        }

        // Make sure the original self assessment has an associated answer
        private void GivenASelfAssessmentWithAnswers()
        {
            var ans = new List<Answer>();
            
            ans.Add(new Mock<Answer>().Object);
            selfAssessment.SetupGet(s => s.Answers).Returns(ans);

            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);

            var selfAssessmentDbSet = new Mock<DbSet<SelfAssessment>>();
            selfAssessmentDbSet.Setup(s => s.Find(selfAssessment.Object.Id)).Returns(selfAssessment.Object);
            db.Setup(d => d.SelfAssessments).Returns(selfAssessmentDbSet.Object);

            var cs_list = new List<CoreSkill>();
            cs_list.Add(coreSkill.Object);
            selfAssessment.Setup(s => s.CoreSkills).Returns(cs_list);
        }

        private void GivenASelfAssessmentWithoutAnswers()
        {
            var sa = new Mock<SelfAssessment>();
            sa.SetupGet(s => s.Answers).Returns(new List<Answer>());

            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(sa.Object);

            var selfAssessmentDbSet = new Mock<DbSet<SelfAssessment>>();
            selfAssessmentDbSet.Setup(s => s.Find(sa.Object.Id)).Returns(sa.Object);
            db.Setup(d => d.SelfAssessments).Returns(selfAssessmentDbSet.Object);
        }

        private void SetupCoreSkill(int nElements, int nOptions)
        {
            // Given the core skill has nElements detailed elements.
            options.SetUpOptions(nElements, nOptions);
            detailedElements.SetUpDetailedElements(nElements, nOptions, options);

            Mock<DbSet<DetailedOption>> do_dbset = new Mock<DbSet<DetailedOption>>();
            foreach (DetailedOption d_o in options)
            {
                do_dbset.Setup(d => d.Find(d_o.Id)).Returns(d_o);
            }
            db.Setup(d => d.DetailedOptions).Returns(do_dbset.Object);

            Mock<DbSet<CoreSkillElement>> coreSkillElementSet = new Mock<DbSet<CoreSkillElement>>();
            foreach (DetailedElement d_e in detailedElements)
            {
                coreSkillElementSet.Setup(c => c.Find(d_e.Id)).Returns(d_e);
            }
            db.Setup(d => d.CoreSkillElements).Returns(coreSkillElementSet.Object);

            coreSkill.SetupGet(cs => cs.Elements).Returns(
                detailedElements.OfType<CoreSkillElement>().ToList());

            Mock<DbSet<CoreSkill>> coreSkillSet = new Mock<DbSet<CoreSkill>>();
            coreSkillSet.Setup(c => c.Find(coreSkill.Object.Id)).Returns(coreSkill.Object);
            db.Setup(d => d.CoreSkills).Returns(coreSkillSet.Object);
        }
    }
}
