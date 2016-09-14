using AutoPrintr.Core.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoPrintr.Core.Models
{
    public abstract class ValidatableBaseModel : BaseModel
    {
        private readonly BindableValidator _bindableValidator;

        public ValidatableBaseModel()
        {
            _bindableValidator = new BindableValidator(this);
        }

        public ReadOnlyDictionary<string, ReadOnlyCollection<string>> GetAllErrors()
        {
            return _bindableValidator.GetAllErrors();
        }

        public void SetAllErrors(IDictionary<string, ReadOnlyCollection<string>> entityErrors)
        {
            _bindableValidator.SetAllErrors(entityErrors);
        }

        public bool ValidateProperties()
        {
            return _bindableValidator.ValidateProperties();
        }

        public bool ValidateProperty(string propertyName)
        {
            return _bindableValidator.ValidateProperty(propertyName);
        }
    }
}