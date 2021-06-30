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

        [Route("Index/{urlUserId}")]
        public async Task<IActionResult> IndexAsync([FromRoute] string urlUserId)
        { 
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (!(await IsAdminOrValidUserAsync(currentUser, urlUserId)))
                return NotFound();
       
            var fanfics = await GetUserFanficsAsync(urlUserId);
            return View((fanfics, urlUserId));
        }

        [Route("Bookmarks/{urlUserId}")]
        public async Task<IActionResult> BookmarksAsync([FromRoute] string urlUserId)
        { 
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            
            if (!(await IsAdminOrValidUserAsync(currentUser, urlUserId)))
                return NotFound();

            var bookmarks = _dbContext.Bookmarks.Where(b => b.UserId == urlUserId).Include(b => b.Fanfic).ToList();
            return View((bookmarks, urlUserId));
        }

        [Route("BookmarkDelete/{urlUserId}/{fanficId:min(1)}")]
        public async Task<IActionResult> BookmarkDeleteAsync([FromRoute] string urlUserId, int fanficId)
        { 
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (!(await IsAdminOrValidUserAsync(currentUser, urlUserId)))
                return NotFound();

            var bookmark = _dbContext.Bookmarks.Where(b => b.FanficModelId == fanficId && b.UserId == urlUserId).FirstOrDefault();
            if (bookmark != null)
            {
                _dbContext.Bookmarks.Remove(bookmark);
                _dbContext.SaveChanges();
            }
            return RedirectPermanent($"/User/Bookmarks/{urlUserId}");
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
        private async Task<bool> IsAdminOrValidUserAsync(IdentityUser user, string id)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userFromUrl = await _userManager.FindByIdAsync(id);

            if (userFromUrl == null)
                return false;

            if (user.Id == id || roles.Contains("admin"))
                return true;
            return false;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
