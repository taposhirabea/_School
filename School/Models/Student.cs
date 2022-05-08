using System.ComponentModel.DataAnnotations;

namespace School.Models
{
    public class Student
    {
        public int ID { get; set; }
        [StringLength(50), MinLength(2)]
        public string LastName { get; set; }
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstMidName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EnrollmentDate { get; set; }

        public string FullName
        {
            get
            {
                return FirstMidName + " " + LastName;
            }
        }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
