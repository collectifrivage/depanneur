using GraphQL.Types;

namespace Depanneur.App.Schema.Inputs
{
    public class UserInputType : InputObjectGraphType<UserInputType.Data>
    {
        public class Data
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        public UserInputType()
        {
            Name = "UserInput";
            Description = "Details about a user.";
            
            Field(x => x.Email).Description("Email address to associate with the user");
            Field(x => x.Name).Description("Name of the user");
        }
    }
}