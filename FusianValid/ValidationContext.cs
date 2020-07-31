using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;

namespace FusianValid
{
    public abstract class ValidationContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void FirePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(property));
        }

        public abstract ErrorMessageHolder ErrorMessages { get; }

        public abstract bool HasError { get; }
        public abstract string GetMessage(string key);
        public abstract Dictionary<string, string> EnumMessages();

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
        private static readonly Regex SubPtn = new Regex(@"sub(\d+)\.");
        private static readonly Regex SubListPtn = new Regex(@"list(\d+)\[(\d+)\]\.");


        private T TargetViewModel { get; }

        private Dictionary<string, ValidatorChain<T>> PropNm2Validator { get; }

        private List<IValidationContextHolder> SubList { get; }
        private List<IList<IValidationContextHolder>> SubListList { get; }

        private ErrorMessageHolder _ErrorMessages;
        public override ErrorMessageHolder ErrorMessages => _ErrorMessages;

        public override bool HasError
        {
            get
            {
                if (ErrorMessages.Results.Count > 0) return true;

                foreach (var sub in SubList)
                {
                    if (sub.ValidationContext.HasError) return true;
                }

                foreach (var sublst in SubListList)
                {
                    foreach (var sub in sublst)
                    {
                        if (sub.ValidationContext.HasError) return true;
                    }
                }

                return false;
            }
        }

        public override string GetMessage(string key)
        {
            var mch1 = SubPtn.Match(key);
            if (mch1.Success)
            {
                var subIdx = Int32.Parse(mch1.Groups[1].Value);
                if (subIdx < SubList.Count)
                {
                    var subKey = key.Substring(mch1.Index + mch1.Length);
                    return SubList[subIdx].ValidationContext.GetMessage(subKey);
                }
                else return null;
            }

            var mch2 = SubListPtn.Match(key);
            if (mch2.Success)
            {

                var subIdx1 = Int32.Parse(mch2.Groups[1].Value);
                if (subIdx1 < SubListList.Count)
                {
                    var list = SubListList[subIdx1];
                    var subIdx2 = Int32.Parse(mch2.Groups[2].Value);
                    if (subIdx2 < list.Count)
                    {
                        var subKey = key.Substring(mch2.Index + mch2.Length);
                        return list[subIdx2].ValidationContext.GetMessage(subKey);
                    }
                    else return null;
                }
                else return null;
            }

            return ErrorMessages[key];
        }

        public override Dictionary<string, string> EnumMessages()
        {
            var dic = new Dictionary<string, string>();

            foreach (var msg in ErrorMessages.Results)
                dic[msg.Key] = msg.Value.Message;

            for (var i = 0; i < SubList.Count; ++i)
            {
                foreach (var entry in SubList[i].ValidationContext.EnumMessages())
                {
                    dic[$"sub{i}.{entry.Key}"] = entry.Value;
                }
            }

            for (var i = 0; i < SubListList.Count; ++i)
            {
                var list = SubListList[i];
                for (var j = 0; j < list.Count; ++j)
                {
                    foreach (var entry in list[j].ValidationContext.EnumMessages())
                    {
                        dic[$"list{i}[{j}].{entry.Key}"] = entry.Value;
                    }
                }
            }

            return dic;
        }

        public ValidationContext(T viewModel, bool autoValidation)
        {
            TargetViewModel = viewModel;
            PropNm2Validator = new Dictionary<string, ValidatorChain<T>>();
            SubList = new List<IValidationContextHolder>();
            SubListList = new List<IList<IValidationContextHolder>>();

            SetErrorMessage(new ErrorMessageHolder());

            if (autoValidation)
            {
                TargetViewModel.PropertyChanged += ValidationByPropertyChanged;
            }
        }

        protected void SetErrorMessage(ErrorMessageHolder value)
        {
            if (_ErrorMessages == value) return;

            _ErrorMessages = value;
            FirePropertyChanged(nameof(ErrorMessages));
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

                SetErrorMessage(helper.ErrMsg);
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

            SetErrorMessage(helper.ErrMsg);


            foreach (var sub in SubList)
                sub.ValidationContext.Validate();

            foreach (var sublst in SubListList)
                foreach (var sub in sublst)
                    sub.ValidationContext.Validate();
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

        public void ConnectContext<T2>(T2 sub) where T2 : IValidationContextHolder
        {
            SubList.Add(sub);
        }

        public void ConnectContext<T2>(IEnumerable<T2> list) where T2 : IValidationContextHolder
        {
            SubListList.Add(list.OfType<IValidationContextHolder>().ToList());
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
