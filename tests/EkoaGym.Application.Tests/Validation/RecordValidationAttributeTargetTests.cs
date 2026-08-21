using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EkoaGym.Application.Auth;

namespace EkoaGym.Application.Tests.Validation;

// Regression test: ASP.NET Core throws at request time (not build time) if a record's ValidationAttributes
// are found only on the generated property and not the primary constructor parameter it binds/validates from.
public class RecordValidationAttributeTargetTests
{
    [Fact]
    public void RequestRecordTypes_HaveValidationAttributesOnConstructorParameters()
    {
        var assembly = typeof(RegisterRequest).Assembly;
        var recordTypes = assembly.GetTypes().Where(IsRecordWithPrimaryConstructor);

        var violations = new List<string>();

        foreach (var type in recordTypes)
        {
            var ctor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
            var ctorParams = ctor.GetParameters();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyAttributes = property.GetCustomAttributes<ValidationAttribute>(inherit: false).ToList();
                if (propertyAttributes.Count == 0)
                {
                    continue;
                }

                var matchingParam = ctorParams.FirstOrDefault(p =>
                    string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase));
                var paramAttributes = matchingParam?.GetCustomAttributes<ValidationAttribute>(inherit: false).Count() ?? 0;

                if (paramAttributes == 0)
                {
                    violations.Add($"{type.Name}.{property.Name}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "These record properties have ValidationAttributes only on the property, not the constructor " +
            "parameter (throws InvalidOperationException at request time in ASP.NET Core): " +
            string.Join(", ", violations));
    }

    private static bool IsRecordWithPrimaryConstructor(Type type) =>
        type.IsClass && type.GetMethods().Any(m => m.Name == "<Clone>$");
}
