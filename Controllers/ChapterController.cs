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
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseWork.Controllers
{
    [Route("Chapter")]
    [Authorize]
    public class ChapterController : Controller
    {
        UserManager<IdentityUser> _userManager;
        ApplicationDbContext _dbContext;
        public ChapterController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [Route("Create/{urlUserId}/{fanficId:min(1)}")]
        [HttpGet]
        public async Task<IActionResult> CreateAsync([FromRoute] string urlUserId, int fanficId)
        {   
            if (!await IsValid(HttpContext.User, urlUserId, fanficId))
                return NotFound();
            return View();
        }

        [Route("Create/{urlUserId}/{fanficId:min(1)}")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(ChapterModel model, [FromRoute] string urlUserId, int fanficId)
        {
            if (!await IsValid(HttpContext.User, urlUserId, fanficId))
                return NotFound();

            SaveNewChapter(model, fanficId);
            return RedirectPermanent($"/User/Index/{urlUserId}");
        }


        [Route("Index/{chapterId:min(1)}")]
        [AllowAnonymous]
        public IActionResult Index(int chapterId)
        {
            var chapter = _dbContext.Chapters.Include(c => c.Fanfic).FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                return NotFound();

            return View((chapter, GetPrevAndNext(chapter)));
        }


        [Route("Edit/{urlUserId}/{chapterId:min(1)}")]
        [HttpGet]
        public async Task<IActionResult> EditAsync([FromRoute] string urlUserId, int chapterId)
        {
            var chapter = _dbContext.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null || !await IsValid(HttpContext.User, urlUserId, chapter.FanficModelId))
                return NotFound();
            return View(chapter);
        }

        [Route("Edit/{urlUserId}/{chapterId:min(1)}")]
        [HttpPost]
        public async Task<IActionResult> EditAsync(ChapterModel model, [FromRoute] string urlUserId, int chapterId)
        {
             if (!await IsValid(HttpContext.User, urlUserId, model.FanficModelId))
                return NotFound();

            UpdateChapter(model);
            return RedirectPermanent($"/User/Index/{urlUserId}");
        }
        
        [Route("Delete/{urlUserId}/{chapterId:min(1)}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string urlUserId, int chapterId)
        { 
            var chapter = _dbContext.Chapters.FirstOrDefault(c => c.Id == chapterId);
            int fanficId = chapter.FanficModelId;

            if (chapter == null || !await IsValid(HttpContext.User, urlUserId, chapter.FanficModelId))
                return NotFound();

            _dbContext.Chapters.Remove(chapter);
            _dbContext.SaveChanges();
            UpdateOrder(fanficId);

            return RedirectPermanent($"/User/Index/{urlUserId}");
        }

        private async Task<bool> IsValid(ClaimsPrincipal principal, string urlUserId, int fanficId)
        {
            return await IsAdminOrValidUserAsync(principal, urlUserId) && HasAccessToFanfic(urlUserId, fanficId);
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
        private bool HasAccessToFanfic(string urlUserId, int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);

            if (fanfic == null || fanfic.UserId != urlUserId)
                return false;
            return true;
        }
        private void UpdateOrder(int fanficId)
        {
            var chapters = _dbContext.Chapters.Where(c => c.FanficModelId == fanficId).OrderBy(c => c.Index).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                if(chapters[i].Index != i)
                {
                    chapters[i].Index = i;
                    _dbContext.Chapters.Update(chapters[i]);
                }
            }
            _dbContext.SaveChanges();
        }
        private void SaveChapter(ChapterModel model)
        {
            model.LastEdit = DateTime.Now;
            _dbContext.Chapters.Add(model);
            _dbContext.SaveChanges();
        }
        private void SaveNewChapter(ChapterModel model, int fanficId)
        {
            var fanfic = _dbContext.Fanfics.First(f => f.Id == fanficId);

            model.Index = _dbContext.Chapters.Include(c => c.Fanfic).Where(c => c.Fanfic == fanfic).Count();
            model.Fanfic = fanfic;
            model.Id = 0;

            SaveChapter(model);
        }
        private void UpdateChapter(ChapterModel model)
        {
            model.LastEdit = DateTime.Now;
            _dbContext.Chapters.Update(model);
            _dbContext.SaveChanges();
        }
        private (int, int) GetPrevAndNext(ChapterModel chapter)
        {
            int count = _dbContext.Chapters.Where(c => c.FanficModelId == chapter.FanficModelId).Count();
            int prevIndex = chapter.Index - 1 < 0 ? 0 : chapter.Index - 1;
            int nextIndex = chapter.Index + 1 == count ? chapter.Index : chapter.Index + 1;
            int prevId = _dbContext.Chapters.Where(c => c.FanficModelId == chapter.FanficModelId && c.Index == prevIndex).First().Id;
            int nextId = _dbContext.Chapters.Where(c => c.FanficModelId == chapter.FanficModelId && c.Index == nextIndex).First().Id; 
            return (prevId, nextId);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
