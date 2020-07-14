using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace MultiReptCore
{
    // In 0.9.10, it's looks that you need to declare the class that derives MarkupExtension as a public class.
    // Otherwise VS designer throw exception.

    public class MessageExtension : MarkupExtension
    {
        public MessageExtension(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }

        public string FallBack { get; set; }

        public string Context { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rscMan = new ResourceManager("MultiReptCore.Assets.Messages", typeof(MessageExtension).Assembly);
            return rscMan.GetString(this.Key);
        }
    }

    public class Message
    {
        static ResourceManager rscMan = new ResourceManager("MultiReptCore.Assets.Messages", typeof(MessageExtension).Assembly);

        public static string Get(string key)
        {
            return rscMan.GetString(key);
        }
    }
}
