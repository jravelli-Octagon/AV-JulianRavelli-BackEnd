using AmericanVirtual.Weather.Challenge.Database.Audit;
using AmericanVirtual.Weather.Challenge.Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Claims;

namespace AmericanVirtual.Weather.Challenge.Database.Models
{
    public class AmericaVirtualContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AmericaVirtualContext()
        {
        }

        public AmericaVirtualContext(DbContextOptions<AmericaVirtualContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int SaveChanges()
        {
            AddAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            AddAuditInfo();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void AddAuditInfo()
        {
            var entries = ChangeTracker.Entries();

            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.Entity is IFullAuditable full_audit)
                {
                    var user = GetCurrentUser();

                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            full_audit.ModificationDate = now;

                            if (user != null)
                                full_audit.ModifiedBy = user;

                            break;

                        case EntityState.Added:
                            full_audit.CreationDate = now;
                            full_audit.ModificationDate = now;
                            
                            if (user != null)
                            {
                                full_audit.CreatedBy = user;
                                full_audit.ModifiedBy = user;
                            }

                            break;
                    }
                }
                else
                {
                    if (entry.Entity is IPartialAuditable part_audit)
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                var user = GetCurrentUser();
                                part_audit.CreationDate = now;
                                part_audit.CreatedBy = user;
                                break;
                        }
                    }
                }

            }
        }

        private User GetCurrentUser()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if(httpContext != null)
                {
                    var user = _httpContextAccessor.HttpContext.User;
                    if (user != null)
                    {
                        var contextUser = user.FindFirst(ClaimTypes.NameIdentifier);
                        if (contextUser != null)
                            return Users.Find(long.Parse(contextUser.Value));
                    }
                }
                    
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PasswordPolicy PasswordPolicy
        {
            get
            {
                return PasswordPolicies.FirstOrDefault();
            }
        }

        private DbSet<PasswordPolicy> PasswordPolicies { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<UserStateHistory> UserStateHistories { get; set; }
        public DbSet<UserTicket> UserTickets { get; set; }
        public DbSet<User> Users { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var splitStringConverter = new ValueConverter<IEnumerable<int>, string>(v => string.Join(";", v), v => Array.ConvertAll(v.Split(new[] { ';' }), int.Parse));
            
            modelBuilder.Entity<User>()
                .HasOne(a => a.CreatedBy);

            modelBuilder.Entity<User>()
                .HasOne(a => a.ModifiedBy);

            modelBuilder.Entity<User>()
                .HasMany(a => a.Logings)
                .WithOne(b => b.User)
                .HasForeignKey(ab => ab.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(a => a.Tickets)
                .WithOne(b => b.User)
                .HasForeignKey(ab => ab.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(a => a.UserStatesHistory)
                .WithOne(b => b.User)
                .HasForeignKey(ab => ab.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserLogin>()
                .HasOne(a => a.User)
                .WithMany(b => b.Logings)
                .HasForeignKey(ab => ab.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserStateHistory>()
                .HasOne(a => a.CreatedBy);

            modelBuilder.Entity<UserStateHistory>()
                .HasOne(a => a.User)
                .WithMany(b => b.UserStatesHistory)
                .HasForeignKey(ab => ab.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserTicket>()
                .HasOne(a => a.CreatedBy);

            modelBuilder.Entity<UserTicket>()
                .HasOne(a => a.User)
                .WithMany(b => b.Tickets)
                .HasForeignKey(ab => ab.UserID)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}