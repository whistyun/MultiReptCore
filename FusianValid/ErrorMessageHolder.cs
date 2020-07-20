using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    public class ErrorMessageHolder
    {
        internal Dictionary<string, List<ValidationResult>> Results { get; }

        public bool HasError
        {
            get => Results.Count() > 0;
        }

        public string this[string property]
        {
            get
            {
                if (TryGetValue(property, out var message))
                {
                    return message;
                }
                else return null;
            }
        }

        public bool TryGetValue(string property, out string message)
        {
            if (Results.TryGetValue(property, out var resLst))
            {
                message = resLst[0].Message;
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        internal void Set(string property, ValidationResult vrslt)
        {
            if (vrslt is null) throw new NullReferenceException();

            if (vrslt.Result == Result.Invalid)
            {
                if (!Results.TryGetValue(property, out var rlst))
                {
                    rlst = new List<ValidationResult>();
                    Results[property] = rlst;
                }

                if (!rlst.Contains(vrslt))
                {
                    rlst.Insert(0, vrslt);
                }

                foreach (var anothProp in vrslt.RelatedProperty.Where(p => p != property))
                {
                    if (!Results.TryGetValue(anothProp, out var arlst))
                    {
                        arlst = new List<ValidationResult>();
                        Results[anothProp] = arlst;
                    }

                    if (!arlst.Contains(vrslt))
                    {
                        arlst.Add(vrslt);
                    }
                }
            }
        }

        internal void Remove(ValidationResult vrslt)
        {
            if (vrslt.Result == Result.Invalid)
            {
                foreach (var key in vrslt.RelatedProperty)
                {
                    if (Results.TryGetValue(key, out var rlst))
                    {
                        if (rlst.Remove(vrslt) && rlst.Count == 0)
                        {
                            Results.Remove(key);
                        }
                    }
                }
            }
        }


        public ErrorMessageHolder()
        {
            Results = new Dictionary<string, List<ValidationResult>>();
        }

        public ErrorMessageHolder(ErrorMessageHolder cp)
        {
            Results = new Dictionary<string, List<ValidationResult>>(cp.Results);
        }

        public ErrorMessageHolder Copy()
            => new ErrorMessageHolder(this);

        public override bool Equals(object val)
        {
            if (val is ErrorMessageHolder msg)
            {
                if (Results.Count != msg.Results.Count)
                    return false;

                foreach (var entry in Results)
                {
                    if (msg.Results.TryGetValue(entry.Key, out var tgtVal))
                    {
                        return entry.Value.Equals(tgtVal);
                    }
                    else return false;
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode() => Results.GetHashCode();

        public static bool operator ==(ErrorMessageHolder l, object r)
        {
            if (l is null && r is null) return true;
            if (l is null) return false;
            return l.Equals(r);
        }
        public static bool operator ==(object r, ErrorMessageHolder l) => l == r;
        public static bool operator ==(ErrorMessageHolder l, ErrorMessageHolder r) => l == (object)r;

        public static bool operator !=(ErrorMessageHolder l, object r) => !(l == r);
        public static bool operator !=(object r, ErrorMessageHolder l) => !(l == r);
        public static bool operator !=(ErrorMessageHolder l, ErrorMessageHolder r) => !(l == (object)r);
    }
}
