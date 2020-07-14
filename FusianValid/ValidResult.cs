using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace FusianValid
{
    internal class ValidResult
    {
        public static ValidResult Pass(string[] props) => new ValidResult(Result.Valid, props, null);
        public static ValidResult Insufficient(string[] props) => new ValidResult(Result.Valid, props, null);


        public Result Result { get; }

        public IReadOnlyCollection<string> RelatedProperty { get; }

        public string Message { get; }


        public ValidResult(Result r, string[] props, string message)
        {
            Result = r;
            RelatedProperty = props;
            Message = message;
        }
    }

    internal enum Result
    {
        Valid,
        Invalid,
        Insufficient,
    }
}
