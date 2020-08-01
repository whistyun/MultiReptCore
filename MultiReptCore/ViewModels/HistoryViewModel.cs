using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MultiReptCore.ViewModels
{

    public class HistoryViewModel : ViewModelBase
    {
        #region MVVM Property

        [DataMember]
        public long Id { get; }

        private string _Description;
        [DataMember]
        public string Description
        {
            get => _Description;
            set => this.RaiseAndSetIfChanged(ref _Description, value);
        }

        private DateTime _ExecutedTime;
        [DataMember]
        public DateTime ExecutedTime
        {
            get => _ExecutedTime;
            set => this.RaiseAndSetIfChanged(ref _ExecutedTime, value);
        }

        private string _rootDirectory;
        [DataMember]
        public string RootDirectory
        {
            get => _rootDirectory;
            set => this.RaiseAndSetIfChanged(ref _rootDirectory, value);
        }

        private string _filenameFilter;
        [DataMember]
        public string FilenameFilter
        {
            get => _filenameFilter;
            set => this.RaiseAndSetIfChanged(ref _filenameFilter, value);
        }

        private string _selectedEncode;
        [DataMember]
        public string SelectedEncode
        {
            get => _selectedEncode;
            set => this.RaiseAndSetIfChanged(ref _selectedEncode, value);
        }

        private bool _IgnoreHiddenFile;
        [DataMember]
        public bool IgnoreHiddenFile
        {
            get => _IgnoreHiddenFile;
            set => this.RaiseAndSetIfChanged(ref _IgnoreHiddenFile, value);
        }

        private bool _isEncodeAutoDetect;
        [DataMember]
        public bool IsEncodeAutoDetect
        {
            get => _isEncodeAutoDetect;
            set => this.RaiseAndSetIfChanged(ref _isEncodeAutoDetect, value);
        }

        private IReadOnlyList<HistoryKeywordViewModel> _Keywords;
        [DataMember]
        public IReadOnlyList<HistoryKeywordViewModel> Keywords
        {
            get => _Keywords;
            set => this.RaiseAndSetIfChanged(ref _Keywords, value);
        }

        #endregion

        [JsonConstructor]
        public HistoryViewModel(
                long id,
                string description,
                DateTime executedTime,
                string rootDirectory,
                string filenameFilter,
                string selectedEncode,
                bool ignoreHiddenFile,
                bool isEncodeAutoDetect,
                IReadOnlyList<HistoryKeywordViewModel> keywords)
        {
            Id = id;
            Description = description;
            ExecutedTime = executedTime;
            RootDirectory = rootDirectory;
            FilenameFilter = filenameFilter;
            SelectedEncode = selectedEncode;
            IgnoreHiddenFile = ignoreHiddenFile;
            IsEncodeAutoDetect = isEncodeAutoDetect;
            Keywords = keywords;
        }
    }
}
