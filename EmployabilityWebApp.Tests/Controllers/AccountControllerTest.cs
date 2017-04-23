using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EmployabilityWebApp.Controllers;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.ViewModels;
using Xunit;
using Microsoft.AspNet.Identity.Owin;
using Moq;

namespace EmployabilityWebApp.Tests.Controllers
{
    //basic tests for account controller, login and register method
    public class AccountControllerTest
    {
        AccountController target;
        private String returnUrl = "test.com";

        //To test the return view when calling login() method
        // GET: /Account/Login
        [Fact]
        public void GetLogin()
        {
     
        }

        //To test the return view when calling register() method
        // GET: /Account/Register
        [Fact]
        public void GetRegister()
        {
      
        }

        [Fact]
        public void GetRegisterBasicReturnCorrectViewName()
        {
   
        }

        [Fact]
        public void GetRegisterBasicReturnCorrectViewModel()
        {
      
        }
    }
}
