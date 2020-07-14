using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FusianValid
{
    public static class Validators
    {
        public static ValidatorFactory IsDigit = (propNm) => new IsDigitValidator(propNm);
        public static ValidatorFactory IsNumeric = (propNm) => new IsNumericValidator(propNm);
        public static ValidatorFactory NotNullOrEmpty = (propNm) => new NotNullOrEmptyValidator(propNm);
        public static ValidatorFactory FileExists = (propNm) => new FileExistsValidator(propNm, false);
        public static ValidatorFactory DirectoryExists = (propNm) => new DirectoryExistsValidator(propNm, false);
        public static ValidatorFactory FileEntryExists = (propNm) => new FileEntryExistsValidator(propNm, false);

        public static ValidatorFactory FileExistsIgnoreEmpty = (propNm) => new FileExistsValidator(propNm, true);
        public static ValidatorFactory DirectoryExistsIgnoreEmpty = (propNm) => new DirectoryExistsValidator(propNm, true);
        public static ValidatorFactory FileEntryExistsIgnoreEmpty = (propNm) => new FileEntryExistsValidator(propNm, true);

        public static ValidatorFactory PatternMatch(string regex, RegexOptions option)
            => (propNm) => new PatternMatchValidator(propNm, regex, option);

        public static ValidatorFactory PatternMatch(string regex)
            => PatternMatch(regex, RegexOptions.None);

        abstract class ValidatorBase : Validator
        {
            protected ValidatorBase(string propNm)
            {
                base.Property = propNm;
            }
        }

        class IsDigitValidator : ValidatorBase
        {
            public IsDigitValidator(string propNm) : base(propNm) { }

            public override bool Validate(object value)
            {
                return (value is int) ? true :
                       (value is short) ? true :
                       (value is sbyte) ? true :
                       (value is uint) ? true :
                       (value is ushort) ? true :
                       (value is byte) ? true :
                       (value is string txt) ? Int32.TryParse(txt, out var dig) :
                       false;
            }
        }

        class IsNumericValidator : ValidatorBase
        {
            public IsNumericValidator(string propNm) : base(propNm) { }

            public override bool Validate(object value)
            {
                return (value is double) ? true :
                       (value is float) ? true :
                       (value is int) ? true :
                       (value is short) ? true :
                       (value is sbyte) ? true :
                       (value is uint) ? true :
                       (value is ushort) ? true :
                       (value is byte) ? true :
                       (value is string txt) ? Double.TryParse(txt, out var dig) :
                       false;
            }
        }

        class NotNullOrEmptyValidator : ValidatorBase
        {
            public NotNullOrEmptyValidator(string propNm) : base(propNm) { }

            public override bool Validate(object value)
            {
                return (value is string txt) ?
                         !String.IsNullOrEmpty(txt) :
                         !Object.ReferenceEquals(value, null);
            }
        }

        class FileExistsValidator : ValidatorBase
        {
            private bool ignoreEmpty;

            public FileExistsValidator(string propNm, bool ignoreEmpty) : base(propNm)
            {
                this.ignoreEmpty = ignoreEmpty;
            }

            public override bool Validate(object value)
            {
                if (ignoreEmpty)
                {
                    if (value is null) return true;

                    return (value is string txt) ?
                        String.IsNullOrEmpty(txt) || File.Exists(txt) :
                        false;
                }
                else return (value is string txt) ? File.Exists(txt) : false;
            }
        }

        class DirectoryExistsValidator : ValidatorBase
        {
            private bool ignoreEmpty;

            public DirectoryExistsValidator(string propNm, bool ignoreEmpty) : base(propNm)
            {
                this.ignoreEmpty = ignoreEmpty;
            }

            public override bool Validate(object value)
            {
                if (ignoreEmpty)
                {
                    if (value is null) return true;

                    return (value is string txt) ?
                        String.IsNullOrEmpty(txt) || Directory.Exists(txt) :
                        false;
                }
                else return (value is string txt) ? Directory.Exists(txt) : false;
            }

        }

        class FileEntryExistsValidator : ValidatorBase
        {
            private bool ignoreEmpty;

            public FileEntryExistsValidator(string propNm, bool ignoreEmpty) : base(propNm)
            {
                this.ignoreEmpty = ignoreEmpty;
            }

            public override bool Validate(object value)
            {
                if (ignoreEmpty)
                {
                    if (value is null) return true;

                    return (value is string txt) ?
                        String.IsNullOrEmpty(txt) || Directory.Exists(txt) || Directory.Exists(txt) :
                        false;
                }
                else return (value is string txt) ? File.Exists(txt) || Directory.Exists(txt) : false;
            }
        }

        class PatternMatchValidator : ValidatorBase
        {
            Regex regex;

            public PatternMatchValidator(string propNm, string regex, RegexOptions option) : base(propNm)
            {
                this.regex = new Regex(regex, option);
            }

            public override bool Validate(object value)
                => (value is string txt) ? regex.IsMatch(txt) : false;
        }
    }
}
