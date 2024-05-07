using Microsoft.AspNetCore.Identity;

namespace MyBackend.Data
{
    public class Role : IdentityRole
    {
        public Role()
        {
        }

        public Role(string roleName) : base(roleName)
        {
        }
    }
}
