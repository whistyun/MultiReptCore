using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FusianValid
{
    public interface IValidationContextHolder
    {
        ValidationContext ValidationContext { get; }
    }

    public interface IValidationContextHolder<T> : IValidationContextHolder
        where T : INotifyPropertyChanged
    {
        new ValidationContext<T> ValidationContext { get; }
    }
}
