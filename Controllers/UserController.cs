using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Controllers
{
    [Route("User")]
    [Authorize]
    public class UserController : Controller
    {
        UserManager<IdentityUser> _userManager;
        ApplicationDbContext _dbContext;
        public UserController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        private async Task<List<(FanficModel, List<ChapterModel>)>> GetUserFanficsAsync(string userId)
        {
            var fanfics = _dbContext.Fanfics.Where(f => f.UserId == userId).ToList();
            var output = new List<(FanficModel, List<ChapterModel>)>();
            foreach (var f in fanfics)
            {
                var chapters = await _dbContext.Chapters.Include(c => c.Fanfic).Where(c => c.Fanfic.Id == f.Id).ToListAsync();
                output.Add((f, chapters));
            }
            return output;
        }


        [Route("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var fanfics = await GetUserFanficsAsync(user.Id);
            return View((fanfics, true));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
