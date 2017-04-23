using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// A single completion to a whole survey.
    public class Answer
    {
        protected Answer() { }

        public Answer(SelfAssessment selfAssessment)
        {
            SelfAssessment = selfAssessment;
            SubAnswers = new List<SubAnswer>();
        }

        protected Answer(int id, DateTime timeStamp)
        {
            Id = id;
            TimeStamp = timeStamp;
        }

        public int Id { get; private set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime TimeStamp { get; private set; }

        [Required]
        public virtual SelfAssessment SelfAssessment { get; protected set; }

        [Required]
        public virtual ICollection<SubAnswer> SubAnswers { get; protected set; }

        [Required]
        public virtual BasicUser BasicUser { get; protected set; }

        public void AddSubAnswer(SubAnswer subAnswer)
        {
            SubAnswers.Add(subAnswer);
        }
    }
}
