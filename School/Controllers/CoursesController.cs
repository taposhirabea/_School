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

namespace School.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext _context;

        public CoursesController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var schoolContext = _context.Course.Include(c => c.Department);
            return View(await schoolContext.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            PopulateDepartmentsDropDownList();
            return View();
        }
        public IActionResult _Create()
        {
            PopulateDepartmentsDropDownList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> _Create(int[] CourseID, string[] Title, int[] Credits, int[] DepartmentID)
        {
            List<Course> courses = new List<Course>();
            for (int i = 0; i < CourseID.Length; i++)
            {
                Course course = new Course();
                course.CourseID = CourseID[i];
                course.Title = Title[i];
                course.Credits = Credits[i];
                course.DepartmentID = DepartmentID[i];
                course.Department = _context.Department.SingleOrDefault(x => x.DepartmentID == DepartmentID[i]);
                courses.Add(course);
            }

            if (ModelState.IsValid)
            {
                _context.Course.AddRange(courses);
                await _context.SaveChangesAsync();
                return View("_Index", courses);
            }
            return null;
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID,Title,Credits,DepartmentID")] Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
            "Try again, and if the problem persists, " +
            "see your system administrator.");
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View("_Edit", course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var updateToCourse = await _context.Course
                .Include(x=>x.Department)
                .SingleOrDefaultAsync(i => i.CourseID == id);
            if (await TryUpdateModelAsync<Course>(updateToCourse, "",
                c => c.Title, c => c.Credits, c => c.DepartmentID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.");
                }
                IList<Course> courses = new List<Course>();
                courses.Add(updateToCourse);
                return View("_Index", courses);
                //return RedirectToAction("Index");
            }
            PopulateDepartmentsDropDownList(updateToCourse.DepartmentID);
            return View(updateToCourse);
        }
        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in _context.Department
                                   orderby d.Name
                                   select d;
            ViewBag.DepartmentID = new SelectList(departmentsQuery.AsNoTracking(), "DepartmentID", "Name", selectedDepartment);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var course = await _context.Course.SingleOrDefaultAsync(m => m.CourseID == id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return Json(true);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.SingleOrDefaultAsync(m => m.CourseID == id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.CourseID == id);
        }
    }
}
