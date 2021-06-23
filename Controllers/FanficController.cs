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

        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        public IActionResult Create(FanficModel model, string[] tags)
        {
            model.UserId = _userManager.GetUserId(HttpContext.User);
            model.Id = 0;

            _dbContext.Fanfics.Add(model);
            _dbContext.SaveChanges();
            SaveTagsAndDb(model.Id, tags);

            return RedirectPermanent("/User/Index");
        }

        [Route("Index/{fanficID:min(1)}")]
        public IActionResult Index(int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            if (fanfic == null)
                return NotFound();

            var chapters = _dbContext.Chapters.Where(c => c.FanficModelId == fanficId).OrderBy(c => c.Index).ToList();
            var tags = _dbContext.Tags.Where(t => t.FanficModelId == fanficId).Select(t => t.Name).ToList();
            return View((fanfic, chapters, tags));
        }

        [Route("Edit/{fanficId:min(1)}")]
        [HttpGet]
        public IActionResult Edit(int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            var tags = _dbContext.Tags.Where(t => t.FanficModelId == fanficId).Select(t => t.Name).ToArray();

            if (!HasAccess(HttpContext.User, fanfic))
                return NotFound();
            return View(new EditFanficViewModel() { Fanfic = fanfic, Tags = tags});
        }

        [Route("Edit/{fanficId:min(1)}")]
        [HttpPost]
        public IActionResult Edit(EditFanficViewModel viewModel, int fanficId)
        {
            var fanfic = viewModel.Fanfic;
            var tags = viewModel.Tags == null ? new string[0] : viewModel.Tags;

            if (!HasAccess(HttpContext.User, fanfic))
                return NotFound();

            _dbContext.Fanfics.Update(fanfic);
            UpdateTagsAndSave(fanficId, tags);
            return RedirectPermanent("/User/Index");
        }

        [Route("Delete/{fanficId:min(1)}")]
        public IActionResult Delete(int fanficId)
        { 
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            if (!HasAccess(HttpContext.User, fanfic))
                return NotFound();

            _dbContext.Fanfics.Remove(fanfic);
            _dbContext.SaveChanges();

            return RedirectPermanent("/User/Index");
        }


        private bool HasAccess(ClaimsPrincipal user, FanficModel model)
        {
            var userId = _userManager.GetUserId(user);

            if (model == null || model.UserId != userId)
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
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
