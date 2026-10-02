using ArgosyUpdaterConsoleBLZ.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArgosyUpdaterConsoleBLZ.Server
{
    public class MyDbContext : DbContext
    {
        // Configured in Program.cs from ConnectionStrings:ArgosyUpdater.
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Machines.Machine>(entity =>
            {
                entity.HasKey(e => new { e.MachineName });

                //entity.HasOne(d => d.Course)
                //    .WithMany(p => p.StudentCourse)
                //    .HasForeignKey(d => d.CourseId)
                //    .OnDelete(DeleteBehavior.ClientSetNull)
                //    .HasConstraintName("FK_StudentCourse_Course");

                //entity.HasOne(d => d.Student)
                //    .WithMany(p => p.StudentCourse)
                //    .HasForeignKey(d => d.StudentId)
                //    .HasConstraintName("FK_StudentCourse_Student");
            });
        }
        //entities
        public DbSet<Machines.Machine> ArgosyUpdaterMachines { get; set; }

    }

}
