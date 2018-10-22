namespace Depanneur.App
{
    public static class Roles
    {
        public const string Users = "USERS";
        public const string Products = "PRODUCTS";
        public const string Balances = "BALANCES";

        public static string[] All => new[] { Users, Products, Balances };
    }
}
