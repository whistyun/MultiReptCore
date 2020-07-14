using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FusianValid
{
    internal class ValidInfo<VM>
    {
        public string Message { get; }
        public string[] Properties { get; }
        public Func<VM, bool> Validator { get; }

        internal ValidInfo<VM> Prev { set; get; }

        [return: NotNull]
        public virtual ValidResult Check(VM viewModel)
        {
            var result = Prev?.Check(viewModel);
            if (result != null && result.Result != Result.Valid) return result;

            return Validator(viewModel) ?
                ValidResult.Pass(Properties) :
                new ValidResult(Result.Invalid, Properties, Message);
        }
    }
}
