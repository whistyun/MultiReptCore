using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FusianValid
{
    internal class CombiValidatorInfo<VM> : ValidatorInfo<VM>
    {
        internal List<Part> Children { get; }

        public CombiValidatorInfo(
            IEnumerable<string> relatedProps,
            Func<VM, bool> validatorFunc,
            string message)
            : base(relatedProps, validatorFunc, message)
        {
            Children = new List<Part>();
        }

        public override ValidationResult Check(VM viewModel)
        {
            var allChildrenExecuted = Children.Select(child => child.IsExecuted).Aggregate((l, r) => l & r);

            if (allChildrenExecuted)
            {
                return Validator(viewModel) ?
                    ValidationResult.Valid(Properties) :
                    ValidationResult.Invalid(Properties, Message);
            }
            else
                return ValidationResult.Insufficient(Properties);
        }

        public Part CreatePart() => new Part(this, Properties, Validator, Message);

        public class Part : ValidatorInfo<VM>
        {
            private CombiValidatorInfo<VM> parent;

            internal bool IsExecuted { private set; get; }

            public Part(
               CombiValidatorInfo<VM> parent,
               IEnumerable<string> relatedProps,
               Func<VM, bool> validatorFunc,
               string message)
               : base(relatedProps, validatorFunc, message)
            {
                this.parent = parent;
                this.parent.Children.Add(this);
            }

            public override ValidationResult Check(VM viewModel)
            {
                IsExecuted = true;

                return parent.Check(viewModel);
            }


            public override void ClearResult() => IsExecuted = false;
        }
    }
}
