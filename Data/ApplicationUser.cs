using LocalGov360.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace LocalGov360.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Othernames { get; set; }
        public Guid? OrganisationId { get; set; }
        public virtual Organisation? Organisation { get; set; }
    }

}
