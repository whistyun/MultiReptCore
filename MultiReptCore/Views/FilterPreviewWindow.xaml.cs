using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MultiReptCore.Views
{
    public class FilterPreviewWindow : Window
    {
        public FilterPreviewWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Ok_Clicked(object sender, RoutedEventArgs e)
        {
            Close(true);
        }
        public void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
