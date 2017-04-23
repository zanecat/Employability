using EmployabilityWebApp.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmployabilityWebApp.Models
{
    public class SaFeedback
    {
        public const int MinChoice = 1;
        public const int MaxChoice = 5;

        public int Id { get; set; }

        [Required]
        public virtual BasicUser BasicUser { get; set; }

        [Required, Range(MinChoice, MaxChoice)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        [Required]
        public virtual SelfAssessment selfAssessment { get; set; }

        public SaFeedback() { }

        protected SaFeedback(int id, int rating, string comment)
        {
            Id = id;
            Rating = rating;
            Comment = comment;
        }

    }
}