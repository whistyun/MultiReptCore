using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hnx8.ReadJEnc;

namespace MultiReptCore.ViewModels
{
    public class EncodeViewModel
    {
        public static readonly EncodeViewModel Initial;
        public static readonly IReadOnlyCollection<EncodeViewModel> List;

        static EncodeViewModel()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var texts = new CharCode.Text[]{
                CharCode.UTF8      ,
                CharCode.UTF32     ,
                CharCode.UTF32B    ,
                CharCode.UTF16     ,
                CharCode.UTF16B    ,
                CharCode.UTF16LE   ,
                CharCode.UTF16BE   ,
                CharCode.UTF8N     ,
                CharCode.ASCII     ,
                CharCode.ANSI      ,
                CharCode.JIS       ,
                CharCode.JIS50222  ,
                CharCode.JISH      ,
                CharCode.ISOKR     ,
                CharCode.SJIS      ,
                CharCode.EUCH      ,
                CharCode.EUC       ,
                CharCode.BIG5TW    ,
                CharCode.EUCTW     ,
                CharCode.GB18030   ,
                CharCode.UHCKR     ,
                CharCode.CP1250    ,
                CharCode.CP1251    ,
                CharCode.CP1253    ,
                CharCode.CP1254    ,
                CharCode.CP1255    ,
                CharCode.CP1256    ,
                CharCode.CP1257    ,
                CharCode.CP1258    ,
                CharCode.TIS620
            };


            List = new ReadOnlyCollection<EncodeViewModel>(
                texts
                    .Where(c =>
                    {
                        try { c.GetEncoding(); return true; }
                        catch { return false; }
                    })
                    .OrderBy(c => c.Name)
                    .Select(c => new EncodeViewModel(c.Name, c.GetEncoding()))
                    .ToList()
            );

            Initial = List.Where(c => c.Name == CharCode.UTF8.Name).FirstOrDefault();
        }

        public string Name { get; }
        public Encoding Encoding { get; }

        public EncodeViewModel(string name, Encoding enc)
        {
            Name = name;
            Encoding = enc;
        }
    }
}
