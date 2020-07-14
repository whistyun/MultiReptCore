using Avalonia.Media;
using FusianValid;
using ReactiveUI;
using System.Text.RegularExpressions;

namespace MultiReptCore.ViewModels
{
    public class KeywordViewModel : ViewModelBase, IValidationContextHolder<KeywordViewModel>
    {
        ValidationContext IValidationContextHolder.ValidationContext => ValidationContext;
        public ValidationContext<KeywordViewModel> ValidationContext { get; }

        #region MVVM Property

        private Brush _Background;
        public Brush Background
        {
            get => _Background;
            set => this.RaiseAndSetIfChanged(ref _Background, value);
        }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set => this.RaiseAndSetIfChanged(ref _keyword, value);
        }

        private string _replaceWord;
        public string ReplaceWord
        {
            get => _replaceWord;
            set => this.RaiseAndSetIfChanged(ref _replaceWord, value);
        }

        private bool _isPlain = true;
        public bool IsPlain
        {
            get => _isPlain;
            set => this.RaiseAndSetIfChanged(ref _isPlain, value);
        }

        private bool _isWord;
        public bool IsWord
        {
            get => _isWord;
            set => this.RaiseAndSetIfChanged(ref _isWord, value);
        }

        private bool _isRegex;
        public bool IsRegex
        {
            get => _isRegex;
            set => this.RaiseAndSetIfChanged(ref _isRegex, value);
        }

        public FindMethod FindMethod
        {
            get
            {
                return
                    IsPlain ? FindMethod.Plain :
                    IsWord ? FindMethod.Word :
                    IsRegex ? FindMethod.Regex :
                    FindMethod.Plain;
            }
        }

        #endregion

        public KeywordViewModel()
        {
            ValidationContext = FusianValid.ValidationContext.Build(this);

            ValidationContext.Add(
                "nullやん",
                nameof(Keyword),
                Validators.NotNullOrEmpty);


            ValidationContext.AddCombination(
                "ばぐっとるやん",
                (model) =>
                {
                    if (model.IsRegex)
                    {
                        try { new Regex(model.Keyword); return true; }
                        catch { return false; }
                    }
                    else return true;
                },
                nameof(Keyword), nameof(IsRegex));
        }
    }

    public enum FindMethod
    {
        Plain,
        Word,
        Regex
    }
}
