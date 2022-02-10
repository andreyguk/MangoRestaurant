using IdentityModel;


using Mango.Services.IdentityNew.Data;
using Mango.Services.IdentityNew.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Mango.Services.IdentityNew.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public void Initialize()
        {
            if (_roleManager.FindByNameAsync(SD.Admin).Result != null)
            {
                return;
            }
            _roleManager.CreateAsync(new IdentityRole(SD.Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Customer)).GetAwaiter().GetResult();

            ApplicationUser adminUser = new()
            {
                UserName = "admin1@gmail.com",
                Email = "admin1@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "1111111",
                FirstName = "Ben",
                LastName = "Admin"
            };

            _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(adminUser, SD.Admin).GetAwaiter().GetResult();

            var temp1 = _userManager.AddClaimsAsync(adminUser, new Claim[]{
                new Claim(JwtClaimTypes.Name, adminUser.FirstName + " " + adminUser.LastName),
                 new Claim(JwtClaimTypes.GivenName, adminUser.FirstName ),
                 new Claim(JwtClaimTypes.FamilyName, adminUser.LastName ),
                 new Claim(JwtClaimTypes.Role, SD.Admin )
            }).Result;

            ApplicationUser customerUser = new()
            {
                UserName = "customer1@gmail.com",
                Email = "customer@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "1111111",
                FirstName = "Herry",
                LastName = "customer"
            };

            _userManager.CreateAsync(customerUser, "Customer123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(customerUser, SD.Customer).GetAwaiter().GetResult();

            var temp2 = _userManager.AddClaimsAsync(adminUser, new Claim[]{
                new Claim(JwtClaimTypes.Name, adminUser.FirstName + " " + adminUser.LastName),
                 new Claim(JwtClaimTypes.GivenName, adminUser.FirstName ),
                 new Claim(JwtClaimTypes.FamilyName, adminUser.LastName ),
                 new Claim(JwtClaimTypes.Role, SD.Customer )
            }).Result;
        }
    }
}
