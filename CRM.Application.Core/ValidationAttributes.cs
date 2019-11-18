using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DateNotInThePastAttributeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var futureDate = value as DateTime?;
            var memberNames = new List<string>() { context.MemberName };

            if (futureDate != null)
            {
                if (futureDate.Value.Date <= DateTime.Now.Date)
                {
                    return new ValidationResult("This date must be in the future", memberNames);
                }
            }
            return ValidationResult.Success;
        }
    }
}
