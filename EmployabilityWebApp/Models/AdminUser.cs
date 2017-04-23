using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// <summary>
    /// A non-unique adminstrator of the entire system.
    /// </summary>
    public class AdminUser : ApplicationUser
    {
        public const string Role = "Admin";

        public AdminUser(string email) : base(email) { }

        [UsedImplicitly]
        protected AdminUser() { }
    }
}