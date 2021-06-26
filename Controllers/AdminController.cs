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
    [Route("Admin")]
    [Authorize(Roles="admin")]
    public class AdminController : Controller
    {
        UserManager<IdentityUser> _userManager;
        SignInManager<IdentityUser> _signInManager;
        public AdminController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("Index")]
        public IActionResult Index(string search = "")
        {
            var users = _userManager.Users.Select(u => new EditUserViewModel()
            {
                Id = u.Id,
                Email = u.Email,
                IsBlocked = u.LockoutEnd != null,
                IsAdmin = _userManager.GetRolesAsync(u).Result.Contains("admin")
            });

            if (search != "")
                users = users.Where(u => u.Email.ToLower().Contains(search.Trim().ToLower()));

            return View((users.ToList(), search));
        }

        [Route("Block/{id}")]
        public async Task<IActionResult> BlockAsync([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnd = DateTime.MaxValue;
                await _userManager.UpdateAsync(user);
            }

            if (_userManager.GetUserId(HttpContext.User) == id)
                await _signInManager.SignOutAsync();

            return RedirectPermanent("/Admin/Index");
        }

        [Route("Unblock/{id}")]
        public async Task<IActionResult> UnblockAsync([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
            }
            return RedirectPermanent("/Admin/Index");
        }
        
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && _userManager.GetUserId(HttpContext.User) != id)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectPermanent("/Admin/Index");
        }

        [Route("AddAdmin/{id}")]
        public async Task<IActionResult> AddAdminAsync([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "admin");
            }
            return RedirectPermanent("/Admin/Index");
        }

        [Route("RemoveAdmin/{id}")]
        public async Task<IActionResult> RemoveAdminAsync([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "admin");
            }
            await _signInManager.SignInAsync(user, false);
            return RedirectPermanent("/Admin/Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
