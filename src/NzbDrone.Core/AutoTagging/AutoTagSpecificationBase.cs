using NzbDrone.Core.Books;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging
{
    public abstract class AutoTagSpecificationBase : IAutoTagSpecification
    {
        public abstract int Order { get; }
        public abstract string ImplementationName { get; }

        public virtual string InfoLink => "https://wiki.servarr.com/readarr/settings#auto-tagging";

        public string Name { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }

        public IAutoTagSpecification Clone()
        {
            return (IAutoTagSpecification)MemberwiseClone();
        }

        public abstract NzbDroneValidationResult Validate();

        public bool IsSatisfiedBy(Author author)
        {
            var match = IsSatisfiedByWithoutNegate(author);
            return Negate ? !match : match;
        }

        protected abstract bool IsSatisfiedByWithoutNegate(Author author);
    }
}
