using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// A user's answer to a TextElement survey question.
    [Table("TextAnswer")]
    public class TextAnswer : SubAnswer
    {
        [UsedImplicitly]
        public TextAnswer() {}

        public TextAnswer(TextElement element)
        {
            TextElement = element;
        }

        [Required]
        public virtual TextElement TextElement { get; protected set; }

        [Required]
        public string Text { get; private set; }

        [NotMapped]
        public override CoreSkillElement Element => TextElement;

        [NotMapped]
        public override bool IsFinished => Text.Length != 0;

        public void SetTextAndValidate([NotNull] string text)
        {
            if (0 == text.Length)
            {
                throw new ArgumentException("text for a TextAnswer cannot be empty");
            }
            Text = text;
        }
    }
}