using GraphQL;

namespace Depanneur.App.Schema
{
    public class DepanneurSchema : GraphQL.Types.Schema
    {
        public DepanneurSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<DepanneurQuery>();
            Mutation = resolver.Resolve<DepanneurMutation>();

            RegisterType<PurchaseTransactionType>();
            RegisterType<PaymentTransactionType>();
            RegisterType<AdjustmentTransactionType>();
        }
    }
}