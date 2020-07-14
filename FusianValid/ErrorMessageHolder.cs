using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    public class ErrorMessageHolder
    {
        internal Dictionary<string, ValidResult> Results
            = new Dictionary<string, ValidResult>();

        public bool HasError
        {
            get => Results.Where(entry => entry.Value.Result == Result.Invalid)
                          .Count() > 0;
        }

        public string this[string property]
        {
            get
            {
                if (Results.TryGetValue(property, out var res))
                {
                    return res.Result == Result.Invalid ?
                        res.Message :
                        null;
                }
                else return null;
            }
        }

        public bool Equals(object val)
        {
            if (val is ErrorMessageHolder msg)
            {
                if (Results.Count != msg.Results.Count)
                    return false;


            }
            return false;
        }
    }
}
