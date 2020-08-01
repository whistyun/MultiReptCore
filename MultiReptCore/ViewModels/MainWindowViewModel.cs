
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.IO;
using MultiReptCore.Views;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia.Media;
using FusianValid;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;
using MultiReptCore.Models;
using Avalonia.Collections;
using System;
using Avalonia.Input;

namespace MultiReptCore.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IValidationContextHolder<MainWindowViewModel>
    {
        ValidationContext IValidationContextHolder.ValidationContext => ValidationContext;
        public ValidationContext<MainWindowViewModel> ValidationContext { get; }

        #region MVVM Property

        private int _ViewTabIndex;
        public int ViewTabIndex
        {
            get => _ViewTabIndex;
            set => this.RaiseAndSetIfChanged(ref _ViewTabIndex, value);
        }


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

        private bool _IgnoreHiddenFile;
        public bool IgnoreHiddenFile
        {
            get => _IgnoreHiddenFile;
            set => this.RaiseAndSetIfChanged(ref _IgnoreHiddenFile, value);
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


        private HistoryViewModel _SelectedHistory;
        public HistoryViewModel SelectedHistory
        {
            get => _SelectedHistory;
            set => this.RaiseAndSetIfChanged(ref _SelectedHistory, value);
        }


        private AvaloniaList<HistoryViewModel> _Histories;
        public AvaloniaList<HistoryViewModel> Histories
        {
            get => _Histories;
            set => this.RaiseAndSetIfChanged(ref _Histories, value);
        }


        #endregion

        #region MVVM Command

        public async void RunBrowseRootDirectory()
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

        public async void RunPreviewFilter()
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

        public void RunAdd()
        {
            Keywords.Add(new KeywordViewModel());
            Keywords.Last().Background = CreateColor(Keywords.Count % 2 == 0);
        }

        public void RunRemove()
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


        public void RunRevertReplace()
        {
        }

        public void RunExecReplace()
        {
            ValidationContext.Validate();

            if (ValidationContext.HasError)
            {
                var app = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;

                var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
                    new MessageBoxStandardParams
                    {
                        ButtonDefinitions = ButtonEnum.Ok,
                        ContentTitle = "Error",
                        ContentMessage = Message.Get("CannotReplaceBecauseError"),
                        Icon = Icon.Warning,
                    });
                msBoxStandardWindow.ShowDialog(app.MainWindow);

                return;
            }

            HistoryHelper.Register(this);



        }

        public void RemoveHistory()
        {
            HistoryHelper.Remove(SelectedHistory);
        }

        public void ReuseHistory()
        {
            var vm = SelectedHistory;

            RootDirectory = vm.RootDirectory;
            FilenameFilter = vm.FilenameFilter;
            SelectedEncode =
                EncodeViewModel.List
                    .Where(enc => enc.Name == vm.SelectedEncode)
                    .Concat(new[] { EncodeViewModel.List.FirstOrDefault() })
                    .First();

            IgnoreHiddenFile = vm.IgnoreHiddenFile;
            IsEncodeAutoDetect = vm.IsEncodeAutoDetect;

            Keywords.Clear();
            foreach (var key in vm.Keywords)
            {
                Keywords.Add(
                    new KeywordViewModel()
                    {
                        Keyword = key.Keyword,
                        ReplaceWord = key.ReplaceWord,
                        IsPlain = key.FindMethod == FindMethod.Plain,
                        IsWord = key.FindMethod == FindMethod.Word,
                        IsRegex = key.FindMethod == FindMethod.Regex,
                    });

                Keywords.Last().Background = CreateColor(Keywords.Count % 2 == 0);
            }

            ViewTabIndex = 0;
        }

        #endregion


        public MainWindowViewModel()
        {
            RootDirectory = "";
            FilenameFilter = "*.*";
            SelectedEncode = EncodeViewModel.Initial;
            IgnoreHiddenFile = true;
            IsEncodeAutoDetect = true;
            Keywords = new ObservableCollection<KeywordViewModel>();
            RunAdd();

            Histories = HistoryHelper.Cache;
            SelectedHistory = Histories.FirstOrDefault();

            ValidationContext = FusianValid.ValidationContext.Build(this);
            ValidationContext.ConnectContext(Keywords);

            ValidationContext.Add(
                Message.Get("ItisEmpty"),
                nameof(RootDirectory),
                Validators.NotNullOrEmpty);

            ValidationContext.Add(
                Message.Get("ErrDirectoryIsNotExist"),
                nameof(RootDirectory),
                Validators.DirectoryExists);

            ValidationContext.Add(
                Message.Get("ErrFilenameFilterIsEmpty"),
                nameof(FilenameFilter),
                Validators.NotNullOrEmpty);

        }
    }
}
