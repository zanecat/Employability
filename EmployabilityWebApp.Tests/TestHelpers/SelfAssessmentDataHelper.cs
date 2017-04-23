using System;
using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using Moq;

namespace EmployabilityWebApp.Tests.TestHelpers
{
    internal static class SelfAssessmentDataHelper
    {
        internal static void SetUpSelfAssessments(
            this List<SelfAssessment> selfAssessments, int nSelfAssessments)
        {
            for (var i = 0; i < nSelfAssessments; i += 1)
            {
                var selfAssessment = new Mock<SelfAssessment>(100 + i + 1, $"Title{i}", 
                    $"Description{i}", DateTime.Now, null as ICollection<CoreSkill>);
                selfAssessments.Add(selfAssessment.Object);
            }
        }

        internal static void SetUpAnswersWithSelfAssessments(
            this List<Answer> answers, int nAnswers, List<SelfAssessment> selfAssessments)
        {
            for (var i = 0; i < nAnswers; i += 1)
            {
                var answer = new Mock<Answer>(100 + i + 1, DateTime.Now);
                answer.SetupGet(a => a.SelfAssessment).Returns(selfAssessments[i % selfAssessments.Count]);
                answers.Add(answer.Object);
            }
        }

        internal static void SetUpAnswersWithSimplifiedAnswers(this List<Answer> answers, int nAnswers,
            List<SimplifiedAnswer> simplifiedAnswers, Mock<SelfAssessment> selfAssessment)
        {
            for (var i = 0; i < nAnswers; i += 1)
            {
                var answer = new Mock<Answer>(100 + i + 1, DateTime.Now);
                var basicUser = new Mock<BasicUser>();
                basicUser.SetupGet(u => u.Id).Returns($"{i}");
                answer.SetupGet(a => a.BasicUser).Returns(basicUser.Object);
                answer.SetupGet(a => a.SelfAssessment).Returns(selfAssessment.Object);
                answer.SetupGet(a => a.SubAnswers).Returns(simplifiedAnswers.OfType<SubAnswer>().ToList());
                answers.Add(answer.Object);
            }
        }

        internal static void SetUpAnswersWithBasicUsers(this List<Answer> answers, int nAnswers)
        {
            for (var i = 0; i < nAnswers; i += 1)
            {
                var answer = new Mock<Answer>(100 + i + 1, DateTime.Now);
                var basicUser = new Mock<BasicUser>();
                basicUser.SetupGet(u => u.Id).Returns($"{i}");
                answer.SetupGet(a => a.BasicUser).Returns(basicUser.Object);
                answers.Add(answer.Object);
            }
        }

        internal static void SetUpCoreSkills(
            this List<CoreSkill> coreSkills, int nCoreSkills)
        {
            for (var i = 0; i < nCoreSkills; i += 1)
            {
                var coreSkill = new Mock<CoreSkill>(100 + i + 1, "coreskill" + i, "coreskill" + i,
                    null as ICollection<CoreSkillElement>);
                coreSkills.Add(coreSkill.Object);
            }
        }
    }
}
