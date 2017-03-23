using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ToDo.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        public ApplicationUser()
        {
            Venues = new HashSet<Venue>();
        }
        
        public virtual ICollection<Venue> Venues { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<File> Files { get; set; }
        public DbSet<VenueFile> VenueFiles { get; set; }
        public DbSet<BandFile> BandFiles { get; set; }
        public DbSet<AdminSettings> AdminSettings { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<Venue_Type> VenueCategories { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Band> Bands { get; set; }
        public DbSet<MusicGenre> MusicGenres { get; set; }
        public DbSet<VenueMailingList> VenueMailingList { get; set; }



        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<ToDo.Models.Event> Events { get; set; }

        public System.Data.Entity.DbSet<ToDo.Models.Venue> Venues { get; set; }
    }
}