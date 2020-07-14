using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace FusianValid
{
    [return: NotNull] public delegate Validator ValidatorFactory([NotNull]string propNm);
}
