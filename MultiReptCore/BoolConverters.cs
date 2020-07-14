using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiReptCore
{
    public static class BoolConverters
    {
        /// <summary>
        /// A multi-value converter that returns true if all inputs are true.
        /// </summary>
        public static readonly IValueConverter Not =
            new FuncValueConverter<bool, bool>(x => !x);
    }
}
