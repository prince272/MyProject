using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MyBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }
    }

    public static class IdentityExtensions
    {
        public static string GetMessage(this IEnumerable<IdentityError> errors)
        {
            if (errors is null) throw new ArgumentNullException(nameof(errors));
            return "Operation failed: " + string.Join(string.Empty, errors.Select(x => $"{Environment.NewLine} -- {x.Code}: {x.Description}"));
        }
    }
}
