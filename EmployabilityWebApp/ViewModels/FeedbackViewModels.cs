using EmployabilityWebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmployabilityWebApp.ViewModels
{
    public class FeedbackCreateData
    {
        [Range(SaFeedback.MinChoice , SaFeedback.MaxChoice)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public int selfAssessmentId { get; set; }
    }

    public class FeedbackDetailData
    {
        [Range(SaFeedback.MinChoice, SaFeedback.MaxChoice)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        [DisplayName("SelfAddessment Title")]
        public string selfAssessmentTitle { get; set; }

        public string UserName { get; set; }
    }
}