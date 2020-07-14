using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace FusianValid.Avalonia
{
    public class ErrorTextBlock : TextBlock
    {
        public static readonly StyledProperty<string> PathProperty =
            AvaloniaProperty.Register<ErrorTextBlock, string>(nameof(Path));

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }


        public ErrorTextBlock()
        {
            this.DataContextChanged += ErrorTextBox_DataContextChanged;

            this.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void ErrorTextBox_DataContextChanged(object sender, EventArgs e)
        {
            if (DataContext is IValidationContextHolder holder)
            {
                holder.ValidationContext.PropertyChanged += ValidationContext_PropertyChanged;

                ValidationContext_PropertyChanged(
                    holder.ValidationContext,
                    new System.ComponentModel.PropertyChangedEventArgs(nameof(ValidationContext.ErrorMessages)));
            }
        }

        private void ValidationContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is ValidationContext context
                && e.PropertyName == nameof(ValidationContext.ErrorMessages))
            {
                if (context.ErrorMessages.TryGetValue(Path, out var message))
                {
                    Text = message;
                }
                else
                {
                    Text = null;
                }
            }
        }
    }
}
