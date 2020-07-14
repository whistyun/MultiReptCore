using Avalonia.Threading;
using DynamicData;
using FusianValid;
using MultiReptCore.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiReptCore.ViewModels
{
    public class FilterPreviewWindowViewModel : ViewModelBase, IValidationContextHolder<FilterPreviewWindowViewModel>
    {
        private CancellationTokenSource tokenSource;

        ValidationContext IValidationContextHolder.ValidationContext => ValidationContext;
        public ValidationContext<FilterPreviewWindowViewModel> ValidationContext { get; }

        #region MVVM Property

        private string _rootDirectory;
        public string RootDirectory
        {
            get => _rootDirectory;
            set
            {
                if (_rootDirectory != value)
                {
                    if (!String.IsNullOrEmpty(value) && Directory.Exists(value))
                    {
                        if (tokenSource != null)
                        {
                            tokenSource.Cancel();

                        }

                        tokenSource = new CancellationTokenSource();
                        Thread.MemoryBarrier();

                        var ct = tokenSource.Token;

                        Task.Run(() =>
                        {
                            Structure(value, ct);
                            tokenSource = null;
                        }, ct);
                    }
                    else
                    {
                        Dispatcher.UIThread.InvokeAsync(() => Entries.Clear());
                    }
                }

                this.RaiseAndSetIfChanged(ref _rootDirectory, value);
            }
        }

        private string _filenameFilter;
        public string FilenameFilter
        {
            get => _filenameFilter;
            set
            {
                FilenameFilterPatterns = value.Split(',').Select(ext => new Wildcard(ext)).ToArray();
                this.RaiseAndSetIfChanged(ref _filenameFilter, value);
            }
        }

        private ObservableCollection<EntryViewModel> _entries;
        public ObservableCollection<EntryViewModel> Entries
        {
            get => _entries;
            set => this.RaiseAndSetIfChanged(ref _entries, value);
        }

        #endregion

        private Wildcard[] _FilenameFilterPatterns;
        private Wildcard[] FilenameFilterPatterns
        {
            get => _FilenameFilterPatterns;
            set
            {
                _FilenameFilterPatterns = value;

                void Seek(IList<EntryViewModel> ents)
                {

                    foreach (var ent in ents)
                    {
                        if (ent is FileViewModel fv)
                        {
                            fv.IsTarget = FilenameFilterPatterns
                                                .Select(wc => wc.IsMatch(fv.Name))
                                                .Aggregate((l, r) => l | r);
                        }
                        else if (ent is DirectoryViewModel dv)
                        {
                            Seek(dv.SubEntries);
                        }
                    }

                }

                Seek(Entries);
            }
        }

        #region MVVM Command

        public void RunApplyFilter() { }

        #endregion

        public FilterPreviewWindowViewModel()
        {
            ValidationContext = FusianValid.ValidationContext.Build(this);

            ValidationContext.Add(
                Message.Get("ErrDirectoryIsNotExist"),
                nameof(RootDirectory),
                Validators.DirectoryExists);

            ValidationContext.Add(
                Message.Get("ErrFilenameFilterIsEmpty"),
                nameof(FilenameFilter),
                Validators.NotNullOrEmpty);


            Entries = new ObservableCollection<EntryViewModel>();


        }

        private async void Structure(string path, CancellationToken ct)
        {
            var dirs = Directory.GetDirectories(path)
                                .Select(sub => new DirectoryViewModel(sub))
                                .ToArray();

            var files = Directory.GetFiles(path)
                                 .Select(sub => new FileViewModel(sub))
                                 .ToArray();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var file in files)
                    file.IsTarget = FilenameFilterPatterns
                            .Select(wc => wc.IsMatch(file.Name))
                            .Aggregate((l, r) => l | r);

                Entries.Clear();
                Entries.AddRange(dirs);
                Entries.AddRange(files);
            });

            foreach (var sub in dirs)
            {
                SubRestructure(sub, ct, 0);
            }
        }

        private void SubRestructure(DirectoryViewModel addTo, CancellationToken ct, int depthLv)
        {
            if (ct.IsCancellationRequested) return;

            DirectoryViewModel[] dirs;
            FileViewModel[] files;
            try
            {
                dirs = Directory.GetDirectories(addTo.FullPath)
                                .Select(sub => new DirectoryViewModel(sub))
                                .ToArray();

                files = Directory.GetFiles(addTo.FullPath)
                                 .Select(sub => new FileViewModel(sub))
                                 .ToArray();
            }
            catch
            {
                // ignore access deny
                return;
            }

            var task = Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var file in files)
                    file.IsTarget = FilenameFilterPatterns
                            .Select(wc => wc.IsMatch(file.Name))
                            .Aggregate((l, r) => l | r);

                addTo.SubEntries.AddRange(dirs);
                addTo.SubEntries.AddRange(files);
            });


            foreach (var sub in dirs)
            {
                SubRestructure(sub, ct, depthLv + 1);
            }
        }
    }

    public class EntryViewModel : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _fullPath;
        public string FullPath
        {
            get => _fullPath;
            set => this.RaiseAndSetIfChanged(ref _fullPath, value);
        }

        public EntryViewModel(string path)
        {
            Name = Path.GetFileName(path);
            FullPath = path;
        }
    }

    public class DirectoryViewModel : EntryViewModel
    {
        private ObservableCollection<EntryViewModel> _subEntries;
        public ObservableCollection<EntryViewModel> SubEntries
        {
            get => _subEntries;
            set => this.RaiseAndSetIfChanged(ref _subEntries, value);
        }

        public DirectoryViewModel(string path) : base(path)
        {
            SubEntries = new ObservableCollection<EntryViewModel>();
        }

    }
    public class FileViewModel : EntryViewModel
    {
        private bool _isTarget;
        public bool IsTarget
        {
            get => _isTarget;
            set => this.RaiseAndSetIfChanged(ref _isTarget, value);
        }

        public FileViewModel(string path) : base(path) { }
    }
}
