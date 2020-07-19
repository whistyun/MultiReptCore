using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FusianValid
{
    internal class ValidationResult
    {
        public static ValidationResult Valid(string[] props)
            => new ValidationResult( Result.Valid, props, null);

        public static ValidationResult Invalid(string[] props, string message)
            => new ValidationResult(Result.Invalid, props, message);

        public static ValidationResult Insufficient(string[] props)
            => new ValidationResult(Result.Valid, props, null);


        public Result Result { get; }

        public IReadOnlyCollection<string> RelatedProperty { get; }

        public string Message { get; }


        public ValidationResult(Result r, string[] props, string message)
        {
            Result = r;
            RelatedProperty = props;
            Message = message;
        }

        public override bool Equals(object val)
        {
            if (val is ValidationResult res)
            {
                return Result == res.Result
                    && Message == res.Message
                    && Enumerable.SequenceEqual(RelatedProperty, res.RelatedProperty);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Result.GetHashCode()
                + (Message is null ? 0 : Message.GetHashCode())
                + RelatedProperty.Select(p => p.GetHashCode()).Sum();
        }

        public static bool operator ==(ValidationResult l, object r)
        {
            if (l is null && r is null) return true;
            if (l is null) return false;
            return l.Equals(r);
        }
        public static bool operator ==(object r, ValidationResult l) => l == r;
        public static bool operator ==(ValidationResult l, ValidationResult r) => l == (object)r;

        public static bool operator !=(ValidationResult l, object r) => !(l == r);
        public static bool operator !=(object r, ValidationResult l) => !(l == r);
        public static bool operator !=(ValidationResult l, ValidationResult r) => !(l == (object)r);
    }

    internal enum Result
    {
        Valid,
        Invalid,
        Insufficient,
    }
}
