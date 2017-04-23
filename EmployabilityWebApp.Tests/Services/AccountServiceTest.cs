using System;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using Xunit;

namespace EmployabilityWebApp.Tests.Services
{
    public class AccountServiceTest
    {
        [Fact]
        public void FindOrganisation()
        {
            var testEmail = "example@test.com";
            Organisation org = new Organisation()
            {
                Name = "Test",
                Domain = "test.com"
            };
            Assert.NotNull(accountService.FindOrganisation(testEmail));
        }
    }
}
