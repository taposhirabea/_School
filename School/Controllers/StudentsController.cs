#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School.Data;
using School.Models;
using School.Models.SchoolViewModel;

namespace School.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;
         
        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber, string currentFilter)
        {
            ViewData["NameSort"] = String.IsNullOrEmpty(sortOrder) ? "nameDesc" : "";
            ViewData["DateSort"] = sortOrder == "Date" ? "dateDesc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }


            var students = from s in _context.Student
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FirstMidName.StartsWith(searchString) ||
                s.LastName.StartsWith(searchString));
            }

            switch (sortOrder)
            {
                case "nameDesc":
                    students = _context.Student.OrderByDescending(i => i.LastName);
                    break;
                case "dateDesc":
                    students = _context.Student.OrderByDescending(i => i.EnrollmentDate);
                    break;
                case "Date":
                    students = _context.Student.OrderBy(i => i.EnrollmentDate);
                    break;
                default:
                    students = _context.Student.OrderBy(i => i.LastName);
                    break;
            }
            int pageSize = 4;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .Include(i => i.Enrollments)
                    .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return PartialView("_Index", student);
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
            "Try again, and if the problem persists, " +
            "see your system administrator.");
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [Bind("ID,EnrollmentDate,FirstMidName,LastName")] Student student)
        {
            if (id != student.ID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    return PartialView("_Index", student);
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            var student = await _context.Student.FindAsync(id);
            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return Json(true);
        }

        public async Task<IActionResult> _AddCourseModal(int studentID)
        {
            ViewBag.StudentID = studentID;
            var courses = await _context.Course.Where(x => x.Enrollments.Any(y => y.StudentID == studentID) == false).ToListAsync();
            return PartialView(courses);
        }
        public async Task<IActionResult> addEnrollment(Enrollment enrollment)//int StudentID,int CourseID)
        {
            enrollment.Course = await _context.Course.SingleAsync(x => x.CourseID == enrollment.CourseID);
            _context.Enrollment.Add(enrollment);
            await _context.SaveChangesAsync();
            return PartialView("_Details", enrollment);
        }
        public async Task<IActionResult> deleteEnrollment(int enrollmentID)
        {
            var enrollment = await _context.Enrollment.SingleAsync(x => x.EnrollmentID == enrollmentID);
            _context.Enrollment.Remove(enrollment);
            return Json(await _context.SaveChangesAsync() > 0);
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.ID == id);
        }
    }
}
