using System;
using System.Linq;

namespace Depanneur.App.Helpers
{
    public static class StringExtensions
    {
        public static string DeduceFirstName(this string fullName)
        {
            return fullName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }
    }
}