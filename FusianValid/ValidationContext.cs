using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        public bool HasError => ErrorMessages.HasError;

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
                validator.ClearResult();

                var errMsg = ErrorMessages.Copy();
                errMsg.Remove(propNm);

                var helper = new ValidatorHelper(
                    TargetViewModel,
                    PropNm2Validator,
                    errMsg);

                helper.CheckRelatedProperty(propNm, false);

                ErrorMessages = helper.ErrMsg;
            }
        }

        public override void Validate()
        {
            foreach (var v in PropNm2Validator.Values)
                v.ClearResult();

            var helper = new ValidatorHelper(TargetViewModel, PropNm2Validator);

            foreach (var v in PropNm2Validator)
            {
                helper.PropLog.Clear();

                helper.CheckRelatedProperty(v.Key, false);
            }

            ErrorMessages = helper.ErrMsg;
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

        class ValidatorHelper
        {
            private T TargetViewModel;
            private Dictionary<string, ValidatorChain<T>> PropNm2Validator { get; }
            public HashSet<string> PropLog { get; }
            public ErrorMessageHolder ErrMsg { get; }

            public ValidatorHelper(T vm, Dictionary<string, ValidatorChain<T>> pn2v)
                : this(vm, pn2v, new ErrorMessageHolder())
            {
            }
            public ValidatorHelper(T vm, Dictionary<string, ValidatorChain<T>> pn2v, ErrorMessageHolder errMsg)
            {
                TargetViewModel = vm;
                PropNm2Validator = pn2v;
                ErrMsg = errMsg;
                PropLog = new HashSet<string>();
            }

            public void CheckRelatedProperty(string property, bool insufficientOnly)
            {
                if (!PropLog.Contains(property)
                    && PropNm2Validator.TryGetValue(property, out var validator))
                {
                    if (insufficientOnly && validator.LastResult != Result.Insufficient)
                        return;

                    try
                    {
                        PropLog.Add(property);
                        ErrMsg.Set(
                            property,
                            validator.ContinueCheck(TargetViewModel, CheckRelatedProperty));
                    }
                    finally
                    {
                        PropLog.Remove(property);
                    }

                }
            }
        }

    }
}
