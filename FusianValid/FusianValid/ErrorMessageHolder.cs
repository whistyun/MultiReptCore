using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    public class ErrorMessageHolder
    {
        internal Dictionary<string, ErrorMessageEntry> Results { get; }

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
                message = resLst.Message;
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
                if (Results.TryGetValue(property, out var rlst))
                {
                    rlst.AddToTop(vrslt);
                }
                else
                {
                    rlst = new ErrorMessageEntry(vrslt);
                    Results[property] = rlst;

                }


                foreach (var anothProp in vrslt.RelatedProperty.Where(p => p != property))
                {
                    if (Results.TryGetValue(anothProp, out var arlst))
                    {
                        arlst.Add(vrslt);
                    }
                    else
                    {
                        arlst = new ErrorMessageEntry(vrslt);
                        Results[anothProp] = arlst;
                    }
                }
            }
        }

        internal void Remove(string property)
        {
            if (Results.TryGetValue(property, out var message))
                foreach (var p in message.RelatedProperties)
                    Results.Remove(p);
        }


        public ErrorMessageHolder()
        {
            Results = new Dictionary<string, ErrorMessageEntry>();
        }

        public ErrorMessageHolder(ErrorMessageHolder cp) : this()
        {
            foreach (var entry in cp.Results)
                Results[entry.Key] = new ErrorMessageEntry(entry.Value);
        }

        public ErrorMessageHolder Copy()
            => new ErrorMessageHolder(this);


        #region equals

        public override bool Equals(object val)
        {
            if (val is ErrorMessageHolder msg)
            {
                if (Results.Count != msg.Results.Count)
                    return false;

                foreach (var myEntry in Results)
                {
                    if (msg.Results.TryGetValue(myEntry.Key, out var yourVal))
                    {
                        if (myEntry.Value != yourVal)
                            return false;
                    }
                    else return false;
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            foreach (var entry in Results)
            {
                hash += entry.Key.GetHashCode();
                hash += entry.Value.GetHashCode();
                hash <<= 1;
            }

            return hash;
        }

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

        #endregion
    }

    internal class ErrorMessageEntry
    {
        public string Message { set; get; }
        public HashSet<string> RelatedProperties { private set; get; }

        public ErrorMessageEntry(ValidationResult result)
        {
            Message = result.Message;
            RelatedProperties = new HashSet<string>(result.RelatedProperty.Distinct());
        }

        public ErrorMessageEntry(ErrorMessageEntry entry)
        {
            Message = entry.Message;
            RelatedProperties = new HashSet<string>(entry.RelatedProperties);
        }


        public void AddToTop(ValidationResult result)
        {
            Message = result.Message;
            Add(result);
        }

        public void Add(ValidationResult result)
        {
            if (String.IsNullOrEmpty(Message))
                Message = result.Message;

            foreach (var key in result.RelatedProperty)
                if (!RelatedProperties.Contains(key))
                    RelatedProperties.Add(key);
        }

        #region equals 

        public override bool Equals(object val)
        {
            if (val is ErrorMessageEntry msg)
            {
                if (Message != msg.Message) return false;

                return RelatedProperties.SetEquals(msg.RelatedProperties);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = Message.GetHashCode();

            foreach (var prop in RelatedProperties.OrderBy(p => p))
            {
                hash += prop.GetHashCode();
                hash <<= 1;
            }

            return hash;
        }

        public static bool operator ==(ErrorMessageEntry l, object r)
        {
            if (l is null && r is null) return true;
            if (l is null) return false;
            return l.Equals(r);
        }
        public static bool operator ==(object r, ErrorMessageEntry l) => l == r;
        public static bool operator ==(ErrorMessageEntry l, ErrorMessageEntry r) => l == (object)r;

        public static bool operator !=(ErrorMessageEntry l, object r) => !(l == r);
        public static bool operator !=(object r, ErrorMessageEntry l) => !(l == r);
        public static bool operator !=(ErrorMessageEntry l, ErrorMessageEntry r) => !(l == (object)r);

        #endregion
    }
}
