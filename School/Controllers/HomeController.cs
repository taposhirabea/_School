//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using School.Data;
//using School.Models;
//using School.Models.SchoolViewModel;
//using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using School.Data;
using School.Models;
using School.Models.SchoolViewModel;
using Microsoft.Extensions.Logging;

namespace School.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SchoolContext _context;

        public HomeController(ILogger<HomeController> logger, SchoolContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> About()
        {
            IQueryable<EnrollmentDateGroup> data =
                from student in _context.Student
                group student by student.EnrollmentDate into datagroup
                select new EnrollmentDateGroup()
                {
                    EnrollmentDate = datagroup.Key,
                    StudentCount = datagroup.Count()
                };
            return View(await data.AsNoTracking().ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}