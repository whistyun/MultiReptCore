using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    public class ErrorMessageHolder
    {
        internal Dictionary<string, ValidationResult> Results { get; }
        internal Dictionary<string, List<ValidationResult>> IndirectMessages { get; }

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
            if (Results.TryGetValue(property, out var res))
            {
                if (res.Result == Result.Invalid)
                {
                    message = res.Message;
                    return true;
                }
            }
            if (IndirectMessages.TryGetValue(property, out var reslst))
            {
                foreach (var r in reslst)
                {
                    if (r.Result == Result.Invalid)
                    {
                        message = r.Message;
                        return true;
                    }
                }
            }
            message = null;
            return false;
        }

        internal void Set(string property, ValidationResult res)
        {
            if (res is null
                || res.Result == Result.Valid
                || res.Result == Result.Insufficient
                || res.Message is null)
            {
                RemoveOldResult(property);
            }
            else
            {
                RemoveOldResult(property);

                Results[property] = res;

                foreach (var anothProp in res.RelatedProperty)
                {
                    if (!IndirectMessages.TryGetValue(anothProp, out var resultList))
                    {
                        resultList = new List<ValidationResult>();
                        IndirectMessages[anothProp] = resultList;
                    }

                    resultList.Add(res);
                }
            }
        }

        private void RemoveOldResult(string property)
        {
            if (Results.TryGetValue(property, out var result))
            {
                Results.Remove(property);

                foreach (var anothProp in result.RelatedProperty)
                {
                    if (IndirectMessages.TryGetValue(anothProp, out var resultList)
                            && resultList.Remove(result) && resultList.Count == 0)
                        IndirectMessages.Remove(anothProp);
                }
            }
        }


        public ErrorMessageHolder()
        {
            Results = new Dictionary<string, ValidationResult>();
            IndirectMessages = new Dictionary<string, List<ValidationResult>>();
        }

        public ErrorMessageHolder(ErrorMessageHolder cp)
        {
            Results = new Dictionary<string, ValidationResult>(cp.Results);
            IndirectMessages = new Dictionary<string, List<ValidationResult>>(cp.IndirectMessages);
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

                foreach (var entry in IndirectMessages)
                {
                    if (msg.IndirectMessages.TryGetValue(entry.Key, out var tgtVal))
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
