using System.ComponentModel.DataAnnotations;

namespace School.Models
{
    public class Instructor
    {
        public int ID { get; set; }
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }
        [StringLength(50, MinimumLength = 2)]
        public string FirstMidName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime HireDate { get; set; }
        public string FullName
        {
            get
            {
                return FirstMidName + " " + LastName;
            }
        }

        public ICollection<CourseAssignment>? CourseAssignments { get; set; }
        public OfficeAssignment? OfficeAssignment { get; set; }
    }
}
