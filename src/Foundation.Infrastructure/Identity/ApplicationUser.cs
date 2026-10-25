using Microsoft.AspNetCore.Identity;

namespace Foundation.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString("D");
    }

    public ApplicationUser(string userName) : this()
    {
        UserName = userName;
    }
}
