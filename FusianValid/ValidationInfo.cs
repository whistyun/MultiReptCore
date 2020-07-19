using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    internal class ValidatorInfo<VM>
    {
        public string Message { get; }
        public string[] Properties { get; }
        public Func<VM, bool> Validator { get; }


        public ValidatorInfo(
            string relatedProps,
            Func<VM, bool> validatorFunc,
            string message)
            : this(
                  new[] { relatedProps },
                  validatorFunc,
                  message)
        { }

        public ValidatorInfo(
            IEnumerable<string> relatedProps,
            Func<VM, bool> validatorFunc,
            string message)
        {
            Properties = relatedProps.ToArray();
            Validator = validatorFunc;
            Message = message;
        }


        public ValidatorInfo(
            Type viewModelType,
            string targetPropertyName,
            Func<object, bool> validatatorFunc,
            string message)
            : this(
                    new[] { targetPropertyName },
                    viewModelType,
                    targetPropertyName,
                    validatatorFunc,
                    message)
        { }

        public ValidatorInfo(
            IEnumerable<string> relatedProps,
            Type viewModelType,
            string targetPropertyName,
            Func<object, bool> validatatorFunc,
            string message)
        {
            Properties = relatedProps.ToArray();
            Message = message;

            var property = viewModelType.GetProperty(targetPropertyName);
            Validator = (vm) => validatatorFunc(property.GetValue(vm));
        }

        public virtual ValidationResult Check(VM viewModel)
        {
            return Validator(viewModel) ?
                ValidationResult.Valid(Properties) :
                ValidationResult.Invalid(Properties, Message);
        }

        public virtual void ClearResult() { }
    }
}
