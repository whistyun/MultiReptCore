using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FusianValid
{
    public abstract class Validator
    {
        public string Property { get; protected set; }

        public abstract bool Validate(object value);
    }
}
