using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FusianValid
{
    public abstract class ValidationContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ErrorMessageHolder _ErrorMessages;
        public ErrorMessageHolder ErrorMessages
        {
            get => _ErrorMessages;
            protected set
            {
                if (value != null && ErrorMessages != null)
                    if (Enumerable.SequenceEqual(
                            value.OrderBy(entry => entry.Key),
                            ErrorMessages.OrderBy(entry => entry.Key)))
                        return;

                _ErrorMessages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessages)));
            }
        }

        public abstract void Validate();

        public static ValidationContext<T> Build<T>(T viewModel, bool autoValidation = true)
                where T : INotifyPropertyChanged
        {
            return new ValidationContext<T>(viewModel, autoValidation);
        }
    }

    public class ValidationContext<T> : ValidationContext
        where T : INotifyPropertyChanged
    {
        private T TargetViewModel { get; }

        private Dictionary<string, ValidationChain> PropNm2Validator { get; }

        public ValidationContext(T viewModel, bool autoValidation)
        {
            TargetViewModel = viewModel;
            PropNm2Validator = new Dictionary<string, ValidationChain>();
            ErrorMessages = new Dictionary<string, string>();

            if (autoValidation)
            {
                TargetViewModel.PropertyChanged += ValidationTime;
            }
        }

        private void ValidationTime(object sender, PropertyChangedEventArgs e)
        {
            var propNm = e.PropertyName;

            if (String.IsNullOrEmpty(propNm))
            {
                // All Properties Changed
                Validate();
            }
            else if (PropNm2Validator.TryGetValue(propNm, out var validator))
            {
                var errMsgs = new Dictionary<string, string>(ErrorMessages);

                Validate(propNm, validator, errMsgs);

                ErrorMessages = errMsgs;
            }
        }

        public override void Validate()
        {
            foreach (var v in PropNm2Validator.Values)
                v.ClearResult();

            var errMsgs = new Dictionary<string, string>();

            foreach (var v in PropNm2Validator)
            {
                Validate(v.Key, v.Value, errMsgs);
            }

            ErrorMessages = errMsgs;
        }

        private void Validate(string key, ValidationChain chain, Dictionary<string, string> errMsgs)
        {
            string msg;
            var isValid = chain.Last.Validation(TargetViewModel, out msg);

            if (isValid || msg is null) errMsgs.Remove(key);
            else errMsgs[key] = msg;
        }

        public void Add(
            [NotNull]string message,
            [NotNull]string property,
            [NotNull]ValidatorFactory validator) => Add(message, validator(property));


        public void Add(
            [NotNull]string message,
            [NotNull]Validator validator)
        {
            if (String.IsNullOrEmpty(message))
                throw new NullReferenceException($"{nameof(message)} is null or empty");

            if (validator is null)
                throw new NullReferenceException($"{nameof(validator)} is null");


            if (!PropNm2Validator.TryGetValue(validator.Property, out var chain))
            {
                chain = new ValidationChain();
                PropNm2Validator[validator.Property] = chain;
            }

            chain.Add(new ValidationPart(message, TargetViewModel.GetType(), validator));
        }

        public void Add(
            [NotNull]string message,
            [NotNull]string property,
            [NotNull]Func<T, bool> validator)
        {
            if (String.IsNullOrEmpty(message))
                throw new NullReferenceException($"{nameof(message)} is null or empty");

            if (String.IsNullOrEmpty(property))
                throw new NullReferenceException($"{nameof(property)} is null");

            if (validator is null)
                throw new NullReferenceException($"{nameof(validator)} is null");


            if (!PropNm2Validator.TryGetValue(property, out var chain))
            {
                chain = new ValidationChain();
                PropNm2Validator[property] = chain;
            }

            chain.Add(new ValidationPart(message, validator));
        }

        public void AddCombination(
            [NotNull]string message,
            [NotNull]Func<T, bool> validator,
            [NotNull]params string[] properties)
        {
            if (String.IsNullOrEmpty(message))
                throw new NullReferenceException($"{nameof(message)} is null or empty");

            if (validator is null)
                throw new NullReferenceException($"{nameof(validator)} is null");

            if (properties is null || properties.Length == 0)
                throw new ArgumentException($"{nameof(properties)} has no property");

            var validationParent = new ValidationCombinationPart(message, validator);

            foreach (var property in properties)
            {
                if (!PropNm2Validator.TryGetValue(property, out var chain))
                {
                    chain = new ValidationChain();
                    PropNm2Validator[property] = chain;
                }

                chain.Add(new ValidationCombinationPartChild(validationParent));
            }
        }


        class ValidationPart
        {
            public string Message { get; }
            protected Func<T, bool> ValidatorFunc { get; }

            internal ValidationPart Prev { set; get; }

            public ValidationPart(string message, Type viewModelType, Validator validator)
            {
                var propInfo = viewModelType.GetProperty(validator.Property);

                ValidatorFunc = (viewModel) => validator.Validate(propInfo.GetValue(viewModel));
                Message = message;
            }

            public ValidationPart(string message, Func<T, bool> validatorFunc)
            {
                ValidatorFunc = validatorFunc;
                Message = message;
            }

            public virtual bool Validation(T viewModel, out string message)
            {
                var isValid =
                        (Prev is null || Prev.Validation(viewModel, out message))
                        && ValidatorFunc(viewModel);

                if (isValid)
                {
                    message = null;
                    return true;
                }
                else
                {
                    message = Message;
                    return false;
                }
            }

            public virtual void ClearResult() { }
        }


        class ValidationCombinationPart : ValidationPart
        {
            internal List<ValidationCombinationPartChild> Children { get; }

            public ValidationCombinationPart(string message, Func<T, bool> validatorFunc)
                : base(message, validatorFunc)
            {
                Children = new List<ValidationCombinationPartChild>();
            }

            public override bool Validation(T viewModel, out string message)
            {
                var allChildrenExecuted = Children.Select(child => child.IsExecuted).Aggregate((l, r) => l & r);

                if (allChildrenExecuted)
                {
                    if (ValidatorFunc(viewModel))
                    {
                        message = null;
                        return true;
                    }
                    else
                    {
                        message = Message;
                        return false;
                    }
                }
                else
                {
                    message = null;
                    return false;
                }
            }
        }


        class ValidationCombinationPartChild : ValidationPart
        {
            private ValidationCombinationPart parent;

            internal bool IsExecuted { private set; get; }


            public ValidationCombinationPartChild(ValidationCombinationPart parent) : base(parent.Message, null)
            {
                this.parent = parent;
                this.parent.Children.Add(this);
            }

            public override bool Validation(T viewModel, out string message)
            {
                message = null;

                var isValid = (Prev is null || Prev.Validation(viewModel, out message));

                if (isValid)
                {
                    IsExecuted = true;

                    return parent.Validation(viewModel, out message);
                }

                return false;
            }

            public override void ClearResult()
            {
                IsExecuted = false;
            }
        }


        class ValidationChain
        {
            private List<ValidationPart> Chains;

            public ValidationChain()
            {
                Chains = new List<ValidationPart>();
            }

            internal void Add(ValidationPart part)
            {
                part.Prev = Last;
                Chains.Add(part);
            }

            internal ValidationPart Last
            {
                get => Chains.Count == 0 ? null : Chains[Chains.Count - 1];
            }

            internal void ClearResult()
            {
                foreach (var chain in Chains)
                    chain.ClearResult();
            }
        }
    }

}
