#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using School.Models;

namespace School.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext (DbContextOptions<SchoolContext> options)
            : base(options)
        {
        }

        public DbSet<School.Models.Student> Student { get; set; }

        public DbSet<School.Models.Course> Course { get; set; }
        public DbSet<School.Models.Enrollment> Enrollment { get; set; }
        public DbSet<School.Models.Department> Department { get; set; }
        public DbSet<School.Models.Instructor> Instructor { get; set; }
        public DbSet<School.Models.OfficeAssignment> OfficeAssignment { get; set; }
        public DbSet<School.Models.CourseAssignment> CourseAssignment { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Enrollment>().ToTable("Enrollment");
            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Department>().ToTable("Department");
            modelBuilder.Entity<Instructor>().ToTable("Instructor");
            modelBuilder.Entity<OfficeAssignment>().ToTable("OfficeAssignment");
            modelBuilder.Entity<CourseAssignment>().ToTable("CourseAssignment");

            modelBuilder.Entity<CourseAssignment>()
                .HasKey(c => new { c.CourseID, c.InstructorID });
            modelBuilder.Entity<Department>()
    .Property(p => p.RowVersion).IsConcurrencyToken();
        }
    }
}
