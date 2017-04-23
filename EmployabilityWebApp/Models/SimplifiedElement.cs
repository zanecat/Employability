using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// A question with fixed options from strongly disagree
    /// to to strongly agree.
    public class SimplifiedElement : CoreSkillElement
    {
        public SimplifiedElement(int id, string description) 
            : base(id, description)
        {
        }

        public SimplifiedElement(string description, int order)
            : base(description, order)
        {
        }

        public SimplifiedElement(int id, string description, int order)
            : base(id, description, order)
        {
        }

        [UsedImplicitly]
        private SimplifiedElement() { }
    }
}
