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
        public IActionResult Create(FanficModel model)
        {
            model.UserId = _userManager.GetUserId(HttpContext.User);
            model.Id = 0;

            _dbContext.Fanfics.Add(model);
            _dbContext.SaveChanges();

            return RedirectPermanent("/User/Index");
        }


        [Route("Index/{fanficID:min(1)}")]
        public IActionResult Index(int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);
            if (fanfic == null)
                return NotFound();

            var chapters = _dbContext.Chapters.Where(c => c.FanficModelId == fanficId).OrderBy(c => c.Index).ToList();
            return View((fanfic, chapters));
        }

        private bool HasAccess(ClaimsPrincipal user, FanficModel model)
        {
            var userId = _userManager.GetUserId(user);

            if (model == null || model.UserId != userId)
                return false;
            return true;
        }


        [Route("Edit/{fanficId:min(1)}")]
        [HttpGet]
        public IActionResult Edit(int fanficId)
        {
            var fanfic = _dbContext.Fanfics.FirstOrDefault(f => f.Id == fanficId);

            if (!HasAccess(HttpContext.User, fanfic))
                return NotFound();
            return View(fanfic);
        }

        [Route("Edit/{fanficId:min(1)}")]
        [HttpPost]
        public IActionResult Edit(FanficModel model, int fanficId)
        {
             if (!HasAccess(HttpContext.User, model))
                return NotFound();

            _dbContext.Fanfics.Update(model);
            _dbContext.SaveChanges();
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
