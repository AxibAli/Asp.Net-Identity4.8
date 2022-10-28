using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace Database
{

    public class UserRole : IdentityUserRole<int>
    {
    }

    public class UserClaim : IdentityUserClaim<int>
    {
    }

    public class UserLogin : IdentityUserLogin<int>
    {
    }

    public class Role : IdentityRole<int, UserRole>
    {
        public Role() { }
        public Role(string name) { Name = name; }
    }

    public class UserStore : UserStore<ApplicationUser, Role, int,
        UserLogin, UserRole, UserClaim>
    {
        public UserStore(DatabaseContext context) : base(context)
        {
        }
    }


    public class RoleStore : RoleStore<Role, int, UserRole>
    {
        public RoleStore(DatabaseContext context) : base(context)
        {
        }
    }

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {
        //public DateTime? ActiveUntil;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(AppUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        public ApplicationSignInManager(AppUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((AppUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<AppUserManager>(), context.Authentication);
        }
    }

    public class ApplicationRoleManager : RoleManager<Role,int>
    {
        public ApplicationRoleManager(IRoleStore<Role,int> store) : base(store)
        {
        }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var roleStore = new RoleStore(context.Get<DatabaseContext>());
            return new ApplicationRoleManager(roleStore);
        }
    }

    public class DatabaseContext : IdentityDbContext<ApplicationUser, Role, int,
        UserLogin, UserRole, UserClaim>//DbContext
    {

        public static DatabaseContext Create()
        {
            return new DatabaseContext();
        }


        public DatabaseContext() : base("DBConnection")
        {
            
        }

        public DatabaseContext(string connectionString) : base(connectionString)
        {

        }
       
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        }
    }

    public class AppUserManager : UserManager<ApplicationUser, int>
    {
        public AppUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }

        // this method is called by Owin therefore this is the best place to configure your User Manager
        public static AppUserManager Create(
            IdentityFactoryOptions<AppUserManager> options, IOwinContext context)
        {
            var manager = new AppUserManager(
                new UserStore(context.Get<DatabaseContext>()));

            // optionally configure your manager
            // ...

            return manager;
        }
    }

}
