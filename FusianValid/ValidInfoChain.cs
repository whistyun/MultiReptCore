using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    internal class ValidInfoChain<VM>
    {
        private List<ValidInfo<VM>> Chains = new List<ValidInfo<VM>>();

        public ValidInfo<VM> Last
        {
            get => Chains.Count == 0 ? null : Chains[Chains.Count - 1];
        }
    }
}
