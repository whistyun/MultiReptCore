using System.Collections.Generic;
using System.Linq;

namespace FusianValid
{
    internal delegate void RequestAnotherProperty(string property, bool insufficientOnly);

    internal class ValidatorChain<VM>
    {
        private List<ValidatorInfo<VM>> Chains = new List<ValidatorInfo<VM>>();

        private string Property { get; }

        public ValidationResult LastResult { private set; get; }

        public ValidatorChain(string prop)
        {
            Property = prop;
        }

        public void Add(ValidatorInfo<VM> v) => Chains.Add(v);

        public ValidationResult ContinueCheck(VM viewModel, RequestAnotherProperty anotherCheckerRequest)
        {
            for (var i = 0; i < Chains.Count; ++i)
            {
                if (Chains[i].LastResult != Result.Valid)
                {
                    return Check(viewModel, i, anotherCheckerRequest);
                }
            }

            return ValidationResult.Valid(new string[] { Property });
        }


        private ValidationResult Check(VM viewModel, int startAt, RequestAnotherProperty anotherCheckerRequest)
        {
            for (var i = startAt; i < Chains.Count; ++i)
            {
                var chain = Chains[i];

                var relatedProperties = chain.Properties.Where(p => p != Property).ToArray();

                foreach (var rprop in relatedProperties)
                    anotherCheckerRequest(rprop, false);

                var result = LastResult = chain.Check(viewModel);

                if (result.Result != Result.Valid)
                    return result;

                foreach (var rprop in relatedProperties)
                    anotherCheckerRequest(rprop, true);
            }

            return LastResult = ValidationResult.Valid(new string[] { Property });
        }

        public void ClearResult()
        {
            foreach (var chain in Chains)
                chain.ClearResult();
        }
    }
}
