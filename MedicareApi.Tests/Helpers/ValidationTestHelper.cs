using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Tests.Helpers
{
    public static class ValidationTestHelper
    {
        public static List<ValidationResult> ValidateObject(object obj)
        {
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(obj, context, results, true);
            return results;
        }
    }
}