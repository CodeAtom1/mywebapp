using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // add custom fields if needed
    public string? FullName { get; set; }
}
