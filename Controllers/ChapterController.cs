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

        private bool HasAccess(ClaimsPrincipal user, int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            var userId = _userManager.GetUserId(user);

            if (fanfic == null || fanfic.UserId != userId)
                return false;
            return true;
        }
        private void SaveChapter(ChapterModel model)
        {
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
            _dbContext.Chapters.Update(model);
            _dbContext.SaveChanges();
        }


        [Route("Create/{fanficId:min(1)}")]
        [HttpGet]
        public IActionResult Create(int fanficId)
        {
            if (!HasAccess(HttpContext.User, fanficId))
                return NotFound();
            return View();
        }

        [Route("Create/{fanficId:min(1)}")]
        [HttpPost]
        public IActionResult Create(ChapterModel model, int fanficId)
        {
            if (!HasAccess(HttpContext.User, fanficId))
                return NotFound();

            SaveNewChapter(model, fanficId);
            return RedirectPermanent("/User/Index");
        }


        [Route("Index/{chapterId:min(1)}")]
        [AllowAnonymous]
        public IActionResult Index(int chapterId)
        {
            var chapter = _dbContext.Chapters.Include(c => c.Fanfic).FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                return NotFound();
            return View(chapter);
        }


        [Route("Edit/{chapterId:min(1)}")]
        [HttpGet]
        public IActionResult Edit(int chapterId)
        {
            var chapter = _dbContext.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null || !HasAccess(HttpContext.User, chapter.FanficModelId))
                return NotFound();
            return View(_dbContext.Chapters.First(c => c.Id == chapterId));
        }

        [Route("Edit/{chapterId:min(1)}")]
        [HttpPost]
        public IActionResult Edit(ChapterModel model, int chapterId)
        {
             if (!HasAccess(HttpContext.User, model.FanficModelId))
                return NotFound();

            UpdateChapter(model);
            return RedirectPermanent("/User/Index");
        }

        
        [Route("Delete/{chapterId:min(1)}")]
        public IActionResult Delete(int chapterId)
        { 
            var chapter = _dbContext.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null || !HasAccess(HttpContext.User, chapter.FanficModelId))
                return NotFound();

            _dbContext.Chapters.Remove(chapter);
            _dbContext.SaveChanges();

            return RedirectPermanent("/User/Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
