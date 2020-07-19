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
                if (_ErrorMessages == value) return;

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

        private Dictionary<string, ValidatorChain<T>> PropNm2Validator { get; }

        public ValidationContext(T viewModel, bool autoValidation)
        {
            TargetViewModel = viewModel;
            PropNm2Validator = new Dictionary<string, ValidatorChain<T>>();
            ErrorMessages = new ErrorMessageHolder();

            if (autoValidation)
            {
                TargetViewModel.PropertyChanged += ValidationByPropertyChanged;
            }
        }

        private void ValidationByPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propNm = e.PropertyName;

            if (String.IsNullOrEmpty(propNm))
            {
                // All Properties Changed
                Validate();
            }
            else
            {
                ContinueValidate(propNm);
            }
        }

        private void ContinueValidate(string propNm)
        {
            if (PropNm2Validator.TryGetValue(propNm, out var validator))
            {
                var errMsgs = ErrorMessages.Copy();

                var anotherProps = validator.ClearResult().Where(p => p != propNm);

                errMsgs.Set(propNm, validator.ContinueCheck(TargetViewModel));

                foreach (var aPropNm in anotherProps)
                    if (PropNm2Validator.TryGetValue(propNm, out var aChain))
                        errMsgs.Set(aPropNm, aChain.ContinueCheck(TargetViewModel));
                ErrorMessages = errMsgs;
            }
        }

        public override void Validate()
        {
            foreach (var v in PropNm2Validator.Values)
                v.ClearResult();

            var errMsgs = new ErrorMessageHolder();

            foreach (var v in PropNm2Validator)
                errMsgs.Set(v.Key, v.Value.ContinueCheck(TargetViewModel));

            ErrorMessages = errMsgs;
        }

        public void Add(
            string message,
            string property,
            ValidatorFactory validator)
        => Add(message, validator(property));


        public void Add(
            string message,
            Validator validator)
        {
            if (String.IsNullOrEmpty(message))
                throw new NullReferenceException($"{nameof(message)} is null or empty");

            if (validator is null)
                throw new NullReferenceException($"{nameof(validator)} is null");


            ValidatorChain<T> chain = GetChain(validator.Property);

            chain.Add(
                new ValidatorInfo<T>(
                    TargetViewModel.GetType(),
                    validator.Property,
                    validator.Validate,
                    message));
        }

        public void Add(
            string message,
            string property,
            Func<T, bool> validator)
        {
            if (String.IsNullOrEmpty(message))
                throw new NullReferenceException($"{nameof(message)} is null or empty");

            if (String.IsNullOrEmpty(property))
                throw new NullReferenceException($"{nameof(property)} is null");

            if (validator is null)
                throw new NullReferenceException($"{nameof(validator)} is null");


            ValidatorChain<T> chain = GetChain(property);

            chain.Add(
                new ValidatorInfo<T>(
                    property,
                    validator,
                    message));
        }

        public void AddCombination(
            string message,
            Func<T, bool> validator,
            params string[] properties)
        {
            if (String.IsNullOrEmpty(message))
                throw new NullReferenceException($"{nameof(message)} is null or empty");

            if (validator is null)
                throw new NullReferenceException($"{nameof(validator)} is null");

            if (properties is null || properties.Length == 0)
                throw new ArgumentException($"{nameof(properties)} has no property");

            var validationParent = new CombiValidatorInfo<T>(
                properties, validator, message);

            foreach (var property in properties)
            {
                ValidatorChain<T> chain = GetChain(property);
                chain.Add(validationParent.CreatePart());
            }
        }


        private ValidatorChain<T> GetChain(string property)
        {
            if (!PropNm2Validator.TryGetValue(property, out var chain))
            {
                chain = new ValidatorChain<T>(property);
                PropNm2Validator[property] = chain;
            }
            return chain;
        }
    }
}
