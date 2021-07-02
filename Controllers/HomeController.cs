using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _dbContext;
        public HomeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("/")]
        public IActionResult Index()
        {
            var lastEditChapters = _dbContext.Chapters.OrderByDescending(c => c.LastEdit).Take(5).ToList();
            var topFanfics = _dbContext.Fanfics.OrderByDescending(f => f.MarkAverage).Take(5).ToList();
            return View((lastEditChapters, topFanfics));
        }

        [Route("Menu")]
        public IActionResult Menu(char letter)
        {
            string alphabetInMenu = "ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЭЮЯ";
            IEnumerable<FanficModel> fanfics;

            if (char.IsLetter(letter))    
                fanfics = _dbContext.Fanfics.AsEnumerable().Where(f => char.ToUpper(f.Name[0]) == letter);
            else
                fanfics = _dbContext.Fanfics.AsEnumerable().Where(f => !alphabetInMenu.Contains(char.ToUpper(f.Name[0])));
            return View(fanfics.ToList());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
