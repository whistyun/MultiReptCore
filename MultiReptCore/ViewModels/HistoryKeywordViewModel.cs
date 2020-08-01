using Avalonia.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MultiReptCore.ViewModels
{
    public class HistoryKeywordViewModel
    {
        [DataMember]
        public string Keyword { get; }
        [DataMember]
        public string ReplaceWord { get; }
        [DataMember]
        public FindMethod FindMethod { get; }

        public string FindMethodName
        {
            get
            {
                switch (FindMethod)
                {
                    case FindMethod.Plain: return Message.Get("Plain");
                    case FindMethod.Word: return Message.Get("Word");
                    case FindMethod.Regex: return Message.Get("Regex");
                    default:
                        return "???";
                }
            }
        }

        [JsonConstructor]
        public HistoryKeywordViewModel(
                string keyword,
                string replaceWord,
                FindMethod findMethod)
        {
            Keyword = keyword;
            ReplaceWord = replaceWord;
            FindMethod = FindMethod;
        }
    }
}