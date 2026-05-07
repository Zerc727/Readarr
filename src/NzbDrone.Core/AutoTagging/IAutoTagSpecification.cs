using NzbDrone.Core.Books;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging
{
    public interface IAutoTagSpecification
    {
        int Order { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }
        NzbDroneValidationResult Validate();
        IAutoTagSpecification Clone();
        bool IsSatisfiedBy(Author author);
    }
}
