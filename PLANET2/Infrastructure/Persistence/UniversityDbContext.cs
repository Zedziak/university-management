using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class UniversityDbContext : DbContext
    {
        public DbSet<Person> People { get; set; } = default!;
        public DbSet<Student> Students { get; set; } = default!;
        public DbSet<Professor> Professors { get; set; } = default!;

        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<IndexCounter> IndexCounters { get; set; } = default!;
        public DbSet<Enrollment> Enrollments { get; set; } = default!;
        public DbSet<Office> Offices { get; set; } = default!;
        public DbSet<Department> Departments { get; set; } = default!;

        public UniversityDbContext(DbContextOptions<UniversityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexCounter>().HasKey(x => x.Prefix);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MasterStudent>()
                .HasOne(ms => ms.Supervisor)
                .WithMany(p => p.SupervisedStudents)
                .HasForeignKey(ms => ms.SupervisorId)
                .OnDelete(DeleteBehavior.ClientSetNull); 

            modelBuilder.Entity<Enrollment>()
                .Property(e => e.Grade)
                .HasPrecision(4, 2);

            modelBuilder.Entity<IndexCounter>().HasIndex(l => l.Prefix).IsUnique();

            modelBuilder.Entity<Person>().OwnsOne(o => o.ResidentialAddress);

            modelBuilder.Entity<Student>().HasIndex(s => s.UniversityIndex).IsUnique();
            modelBuilder.Entity<Professor>().HasIndex(p => p.UniversityIndex).IsUnique();

            modelBuilder.Entity<Enrollment>().HasKey(e => new { e.StudentId, e.CourseId });
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Prerequisites)
                .WithMany(c => c.RequiredBy)
                .UsingEntity(j => j.ToTable("CoursePrerequisites"));

            modelBuilder.Entity<Professor>()
                .HasOne(p => p.Office)
                .WithOne(o => o.Professor)
                .HasForeignKey<Office>(o => o.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Courses)
                .WithOne(c => c.Department)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}