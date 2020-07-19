using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FusianValid
{
    internal class ValidatorChain<VM>
    {
        private List<ValidatorInfo<VM>> Chains = new List<ValidatorInfo<VM>>();

        public ValidatorInfo<VM> LastValidation { private set; get; }
        public ValidationResult LastResult { private set; get; }

        private string Property { get; }

        public ValidatorChain(string prop)
        {
            Property = prop;
        }

        public void Add(ValidatorInfo<VM> v) => Chains.Add(v);

        public ValidationResult ContinueCheck(VM viewModel)
            => Check(viewModel, LastValidation is null ? 0 : Chains.IndexOf(LastValidation));

        private ValidationResult Check(VM viewModel, int startAt)
        {
            ValidationResult result = ValidationResult.Valid(new string[] { Property });

            for (var i = startAt; i < Chains.Count; ++i)
            {
                var chain = Chains[i];
                result = chain.Check(viewModel);

                LastValidation = chain;
                LastResult = result;

                if (result.Result != Result.Valid)
                    break;
            }

            return result;
        }

        public IEnumerable<string> ClearResult()
        {
            var lastIdx = LastValidation is null ? 0 : Chains.IndexOf(LastValidation);
            LastValidation = null;

            foreach (var chain in Chains)
                chain.ClearResult();

            return new HashSet<string>(
                Chains
                    .Take(lastIdx)
                    .SelectMany(p => p.Properties)
                    .Distinct());
        }
    }
}
