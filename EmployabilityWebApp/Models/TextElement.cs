using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// A question for a text answer.
    public class TextElement : CoreSkillElement
    {
        public TextElement(int id, string description) 
            : base(id, description)
        {
        }

        public TextElement(string description, int order) 
            : base(description, order)
        {
        }

        /// Used for faking the id of a TextElement.
        public TextElement(int id, string description, int order)
            : base(id, description, order)
        {
        }

        [UsedImplicitly]
        private TextElement() { }
    }
}
