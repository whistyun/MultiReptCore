using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MultiReptCore.Models
{
    public class Wildcard
    {
        public string Expression { get; }
        public Regex Pattern { get; }

        public Wildcard(string expression)
        {
            Expression = expression;

            var builder = new StringBuilder();

            builder.Append("^");

            foreach (char c in expression)
                builder.Append(
                    c == '?' ? "." :
                    c == '*' ? ".*" :
                    Regex.Escape(c.ToString()));

            builder.Append("$");

            Pattern = new Regex(builder.ToString());
        }

        public bool IsMatch(string value) => Pattern.IsMatch(value);
    }
}
