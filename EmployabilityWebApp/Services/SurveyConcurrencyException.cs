using System;

namespace EmployabilityWebApp.Services
{
    public class SurveyConcurrencyException : Exception
    {
        internal SurveyConcurrencyException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}