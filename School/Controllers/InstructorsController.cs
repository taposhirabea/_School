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
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Instructors
        public async Task<IActionResult> Index()
        {
            var viewModel = await _context.Instructor
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                //        .ThenInclude(i => i.Enrollments)
                //            .ThenInclude(i => i.Student)
                //.Include(i => i.CourseAssignments)
                //    .ThenInclude(i => i.Course)
                //        .ThenInclude(i => i.Department)
                //.AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();


            return View(viewModel);
        }
        public async Task<IActionResult> _Index1(int id)
        {
            var Courses = await _context.CourseAssignment.Where(x => x.InstructorID == id)
                .Include(x => x.Course)
                .ThenInclude(x => x.Department).ToListAsync();

                //ViewData["InstructorID"] = id.Value;
                //Instructor instructor = viewModel.Instructors.Where(
                //    i => i.ID == id.Value).Single();
                //viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);

            return PartialView(Courses.Select(s => s.Course));
        }
        public async Task<IActionResult> _Index2(int courseID)
        {
            //ViewData["CourseID"] = courseID.Value;
            //viewModel.Enrollments = viewModel.Courses.Where(
            //    x => x.CourseID == courseID).Single().Enrollments;

            return PartialView(await _context.Enrollment.Where(x => x.CourseID == courseID)
                .Include(x => x.Student).ToListAsync());
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var instructor = await _context.Instructor
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id); ;
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,HireDate,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment { InstructorID = instructor.ID, CourseID = int.Parse(course) };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructor
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id); ;
            if (instructor == null)
            {
                return NotFound();
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);

        }
        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Course;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(i => i.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = viewModel;
        }

        // POST: Instructors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return NotFound();
            }

            var updateToInstructor = await _context.Instructor.
                Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                .FirstOrDefaultAsync(i => i.ID == id);
            if (await TryUpdateModelAsync<Instructor>(updateToInstructor, "",
                c => c.LastName, c => c.FirstMidName, c => c.HireDate, c => c.OfficeAssignment))
            {
                if (String.IsNullOrWhiteSpace(updateToInstructor.OfficeAssignment?.Location))
                {
                    updateToInstructor.OfficeAssignment = null;
                }
                UpdateInstructorCourses(selectedCourses, updateToInstructor);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            UpdateInstructorCourses(selectedCourses, updateToInstructor);
            PopulateAssignedCourseData(updateToInstructor);
            return View(updateToInstructor);
        }
        private void UpdateInstructorCourses(string[] selectedCourses, Instructor updateToInstructor)
        {
            if (selectedCourses == null)
            {
                updateToInstructor.CourseAssignments = new List<CourseAssignment>();
                return;
            }
            var selectedCourseHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (updateToInstructor.CourseAssignments.Select(i => i.Course.CourseID));
            foreach (var course in _context.Course)
            {
                if (selectedCourseHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        updateToInstructor.CourseAssignments.Add(new CourseAssignment { InstructorID = updateToInstructor.ID, CourseID = course.CourseID });
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        CourseAssignment courseRemove = updateToInstructor.CourseAssignments.FirstOrDefault(i => i.CourseID == course.CourseID);
                        _context.Remove(courseRemove);
                    }
                }

            }
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructor
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructor.FindAsync(id);
            _context.Instructor.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructor.Any(e => e.ID == id);
        }
    }
}
