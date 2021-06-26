using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseWork.Controllers
{
    [Route("Fanfic")]
    [Authorize]
    public class FanficController : Controller
    {
        UserManager<IdentityUser> _userManager;
        ApplicationDbContext _dbContext;
        public FanficController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [Route("Create/{urlUserId}")]
        [HttpGet]
        public async Task<IActionResult> CreateAsync([FromRoute] string urlUserId)
        {
            if (!(await IsAdminOrValidUserAsync(HttpContext.User, urlUserId)))
                return NotFound();
            return View();
        }

        [Route("Create/{urlUserId}")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(FanficModel model, [FromRoute] string urlUserId, string[] tags)
        {
            if (!(await IsAdminOrValidUserAsync(HttpContext.User, urlUserId)))
                return NotFound();

            model.UserId = urlUserId;
            model.Id = 0;

            _dbContext.Fanfics.Add(model);
            _dbContext.SaveChanges();
            SaveTagsAndDb(model.Id, tags);

            return RedirectPermanent($"/User/Index/{urlUserId}");
        }

        [Route("Index/{fanficID:min(1)}")]
        [AllowAnonymous]
        public IActionResult Index(int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            if (fanfic == null)
                return NotFound();

            var chapters = _dbContext.Chapters.Where(c => c.FanficModelId == fanficId).OrderBy(c => c.Index).ToList();
            var tags = _dbContext.Tags.Where(t => t.FanficModelId == fanficId).Select(t => t.Name).ToList();
            var addedToBookmarks = IsAddedToBookmarksAsync(HttpContext.User, fanficId);
            return View((fanfic, chapters, tags, addedToBookmarks));
        }

        [Route("Edit/{urlUserId}/{fanficId:min(1)}")]
        [HttpGet]
        public async Task<IActionResult> EditAsync([FromRoute] string urlUserId, int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            var tags = _dbContext.Tags.Where(t => t.FanficModelId == fanficId).Select(t => t.Name).ToArray();

            if(!await IsValid(HttpContext.User, urlUserId, fanfic))
                return NotFound();

            return View(new EditFanficViewModel() { Fanfic = fanfic, Tags = tags});
        }

        [Route("Edit/{urlUserId}/{fanficId:min(1)}")]
        [HttpPost]
        public async Task<IActionResult> EditAsync(EditFanficViewModel viewModel, [FromRoute] string urlUserId, int fanficId)
        {
            var fanfic = viewModel.Fanfic;
            var tags = viewModel.Tags == null ? new string[0] : viewModel.Tags;

            if(!await IsValid(HttpContext.User, urlUserId, fanfic))
                return NotFound();

            _dbContext.Fanfics.Update(fanfic);
            UpdateTagsAndSave(fanficId, tags);
            return RedirectPermanent($"/User/Index/{urlUserId}");
        }

        [Route("Delete/{urlUserId}/{fanficId:min(1)}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string urlUserId, int fanficId)
        { 
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);

            if(!await IsValid(HttpContext.User, urlUserId, fanfic))
                return NotFound();

            _dbContext.Fanfics.Remove(fanfic);
            _dbContext.SaveChanges();

            return RedirectPermanent($"/User/Index/{urlUserId}");
        }
        
        [Route("Bookmark/{fanficId:min(1)}")]
        public IActionResult Bookmark(int fanficId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            if (userId != null && _dbContext.Fanfics.Where(f => f.Id == fanficId).Count() > 0)
            {
                _dbContext.Bookmarks.Add(new BookmarkModel() { UserId = userId, FanficModelId = fanficId });
                _dbContext.SaveChanges();
            }
            return RedirectPermanent($"/Fanfic/Index/{fanficId}");
        }

        private async Task<bool> IsValid(ClaimsPrincipal principal, string urlUserId, FanficModel model)
        {
            return await IsAdminOrValidUserAsync(principal, urlUserId) && HasAccessToFanfic(urlUserId, model);
        }
        private async Task<bool> IsAdminOrValidUserAsync(ClaimsPrincipal principal, string urlUserId)
        {
            var user = await _userManager.GetUserAsync(principal);
            var roles = await _userManager.GetRolesAsync(user);
            var userFromUrl = await _userManager.FindByIdAsync(urlUserId);

            if (userFromUrl == null)
                return false;
            
            if (user.Id == urlUserId || roles.Contains("admin"))
                return true;
            return false;
        }
        private bool HasAccessToFanfic(string urlUserId, FanficModel model)
        {
            if (model == null || model.UserId != urlUserId)
                return false;
            return true;
        }
        private void SaveTagsAndDb(int fanficId, string[] tags)
        {
            foreach (string tag in tags)
                _dbContext.Tags.Add(new TagModel() { FanficModelId = fanficId, Name = tag });
            _dbContext.SaveChanges();
        }     
        private void UpdateTagsAndSave(int fanficId, string[] tags)
        {
            _dbContext.Tags.RemoveRange(_dbContext.Tags.Where(t => t.FanficModelId == fanficId));
            SaveTagsAndDb(fanficId, tags);
        }
        private bool IsAddedToBookmarksAsync(ClaimsPrincipal principal, int fanficId)
        {
            var userId = _userManager.GetUserId(principal);
            if (userId != null)
            {
                return _dbContext.Bookmarks.Where(b => b.UserId == userId && b.FanficModelId == fanficId).Count() > 0;
            }
            return false;
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
