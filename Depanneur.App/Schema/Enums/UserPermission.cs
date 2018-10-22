using System;
using GraphQL.Types;

namespace Depanneur.App.Schema.Enums
{
    public enum UserPermission { Users, Products, Balances }

    public class UserPermissionEnumType : EnumerationGraphType 
    {
        public UserPermissionEnumType()
        {
            Name = "UserPermission";
            Description = "The different roles that can be granted to a user.";

            AddValue("USERS", "Allows managing the list of users.", UserPermission.Users);
            AddValue("PRODUCTS", "Allows managing the list of products, and seeing the list of purchases associated with them.", UserPermission.Products);
            AddValue("BALANCES", "Allows to view the balances of all users, the list of transactions that they made, and recording new payments or adjustments.", UserPermission.Balances);
        }
    }

    public static class UserPermissionExtensions 
    {
        public static string GetRoleName(this UserPermission permission) 
        {
            switch (permission) 
            {
                case UserPermission.Users: return Roles.Users;
                case UserPermission.Products: return Roles.Products;
                case UserPermission.Balances: return Roles.Balances;
                default: throw new ArgumentException($"Unknown permission {permission}", nameof(permission));
            }
        }
    }
}