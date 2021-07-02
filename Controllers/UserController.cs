using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
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

        [Route("Settings/{urlUserId}")]
        [HttpGet]
        public async Task<IActionResult> SettingsAsync([FromRoute] string urlUserId)
        { 
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (!(await IsAdminOrValidUserAsync(currentUser, urlUserId)))
                return NotFound();

            var settings = _dbContext.Settings.Where(s => s.UserId == urlUserId).FirstOrDefault() ?? new UserSettingsModel();
            settings.UserId = urlUserId;
            return View(settings);
        }

        [Route("Settings/{urlUserId}")]
        [HttpPost]
        public async Task<IActionResult> SettingsAsync([FromRoute] string urlUserId, UserSettingsModel userSettings)
        { 
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (!(await IsAdminOrValidUserAsync(currentUser, urlUserId)))
                return NotFound();

            userSettings.IsOnboarded = true;
            SaveSettings(urlUserId, userSettings);
            SetCookie(HttpContext.Response.Cookies, userSettings);

            return RedirectPermanent($"/User/Settings/{urlUserId}");
        }

        private void SaveSettings(string urlUserId, UserSettingsModel userSettings)
        { 
            if (_dbContext.Settings.Where(s => s.UserId == urlUserId).Count() == 0)
                _dbContext.Settings.Add(userSettings);
            else
                _dbContext.Settings.Update(userSettings);
            _dbContext.SaveChanges();
        }
        private void SetCookie(IResponseCookies cookies, UserSettingsModel userSettings)
        {
            cookies.Append("theme", userSettings.Theme, new CookieOptions() { Expires = DateTime.MaxValue, MaxAge = TimeSpan.MaxValue });
            cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(userSettings.Language)),
                new CookieOptions { Expires = DateTime.MaxValue, IsEssential = true }
            );
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
