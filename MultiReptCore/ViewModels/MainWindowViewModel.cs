
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Text;
using System.IO;
using MultiReptCore.Views;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace MultiReptCore.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region MVVM Property

        private string _rootDirectory;
        public string RootDirectory
        {
            get => _rootDirectory;
            set => this.RaiseAndSetIfChanged(ref _rootDirectory, value);
        }

        private string _filenameFilter;
        public string FilenameFilter
        {
            get => _filenameFilter;
            set => this.RaiseAndSetIfChanged(ref _filenameFilter, value);
        }

        private EncodeViewModel _selectedEncode;
        public EncodeViewModel SelectedEncode
        {
            get => _selectedEncode;
            set => this.RaiseAndSetIfChanged(ref _selectedEncode, value);
        }


        private bool _isEncodeAutoDetect;
        public bool IsEncodeAutoDetect
        {
            get => _isEncodeAutoDetect;
            set => this.RaiseAndSetIfChanged(ref _isEncodeAutoDetect, value);
        }

        private ObservableCollection<KeywordViewModel> _Keywords;
        public ObservableCollection<KeywordViewModel> Keywords
        {
            get => _Keywords;
            set => this.RaiseAndSetIfChanged(ref _Keywords, value);
        }

        private KeywordViewModel _SelectedKeyword;
        public KeywordViewModel SelectedKeyword
        {
            get => _SelectedKeyword;
            set => this.RaiseAndSetIfChanged(ref _SelectedKeyword, value);
        }

        #endregion

        #region MVVM Command

        private async void RunBrowseRootDirectory()
        {
            var app = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;

            var win = new OpenFolderDialog();

            if (Directory.Exists(RootDirectory))
            {
                win.Directory = Path.GetDirectoryName(RootDirectory);
            }

            var dir = await win.ShowAsync(app.MainWindow);
            if (dir != null)
            {
                RootDirectory = dir;
            }
        }

        private async void RunPreviewFilter()
        {
            var app = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;

            var filterWinVM = new FilterPreviewWindowViewModel();
            filterWinVM.RootDirectory = RootDirectory;
            filterWinVM.FilenameFilter = FilenameFilter;

            var filterWin = new FilterPreviewWindow();
            filterWin.DataContext = filterWinVM;

            var dialogResult = await filterWin.ShowDialog<bool?>(app.MainWindow);

            if (dialogResult.HasValue && dialogResult.Value)
            {
                RootDirectory = filterWinVM.RootDirectory;
                FilenameFilter = filterWinVM.FilenameFilter;
            }
        }

        private void RunAdd()
        {
            Keywords.Add(new KeywordViewModel());
            Keywords.Last().Background = CreateColor(Keywords.Count % 2 == 0);
        }

        private void RunRemove()
        {
            if (SelectedKeyword != null)
            {
                Keywords.Remove(SelectedKeyword);

                for (var i = 0; i < Keywords.Count; ++i)
                    Keywords[i].Background = CreateColor(i % 2 != 0);
            }
            else if (Keywords.Count > 1)
                Keywords.RemoveAt(Keywords.Count - 1);
        }

        private SolidColorBrush CreateColor(bool even)
        {
            return new SolidColorBrush(
                 even ?
                    new Color(0x7f, 0xAA, 0xAA, 0xAA) :
                    new Color(0x7f, 0xFF, 0xFF, 0xFF)
           );
        }

        #endregion


        public MainWindowViewModel()
        {
            RootDirectory = "";
            FilenameFilter = "*.*";
            SelectedEncode = EncodeViewModel.Initial;
            IsEncodeAutoDetect = true;
            Keywords = new ObservableCollection<KeywordViewModel>();
            RunAdd();
        }
    }
}
