namespace EmployabilityWebApp.Models
{
    //data user, the 3rd party organizations just want to analyze data
    public class DataUser : ApplicationUser
    {
        public const string Role = "Data";
    }
}