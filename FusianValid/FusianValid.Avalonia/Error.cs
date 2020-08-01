using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FusianValid.Avalonia
{
    public class Error
    {
        static Error()
        {
            var style = new Style(x => x.OfType<TextBox>().Class("error"));
            style.Setters.Add(new Setter(TextBox.BackgroundProperty, new SolidColorBrush(Colors.Pink)));

            Application.Current.Styles.Add(style);
        }


        public static readonly AttachedProperty<string> ObserveProperty =
            AvaloniaProperty.RegisterAttached<Error, Control, string>("Observe");

        public static string GetObserve(Control element)
        {
            return element.GetValue(ObserveProperty);
        }

        public static void SetObserve(Control element, string property)
        {
            element.SetValue(ObserveProperty, property);
            element.DataContextChanged += DataContextChanged;

            DataContextChanged(element, null);
        }

        private static void DataContextChanged(object sender, EventArgs e)
        {
            if (sender is Control control)
            {
                if (control.DataContext is IValidationContextHolder holder)
                {
                    holder.ValidationContext.PropertyChanged +=
                        (s2, e2) => ValidationResultChanged(control, s2, e2);
                }
                else if (control.DataContext != null)
                {

                    throw new ArgumentException($"{control.GetType().Name} has DataContext that is not IValidationContextHolder.");
                }
            }
        }

        private static void ValidationResultChanged(Control control, object sender, PropertyChangedEventArgs e)
        {
            if (sender is ValidationContext context
                && e.PropertyName == nameof(ValidationContext.ErrorMessages))
            {
                var propNm = GetObserve(control);

                if (context.ErrorMessages.TryGetValue(propNm, out var message))
                {
                    if (!control.Classes.Contains("error"))
                        control.Classes.Add("error");

                    ToolTip.SetTip(control, message);
                }
                else
                {
                    control.Classes.Remove("error");
                    ToolTip.SetTip(control, null);
                }
            }
        }
    }
}
