using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Hubs
{
    public class LikeHub : Hub
    {
        ApplicationDbContext _dbContext;
        UserManager<IdentityUser> _userManager;
        public LikeHub(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        private (int, string, LikeModel) GetValues(HubCallerContext context)
        {
            int chapterId = int.Parse(context.GetHttpContext().Request.Query["chapterId"].ToString().Split("/").Last());
            string userId = _userManager.GetUserId(context.User);
            var like = _dbContext.Likes.Where(l => l.ChapterModelId == chapterId && l.UserId == userId).FirstOrDefault();

            return (chapterId, userId, like);
        }

        public override async Task OnConnectedAsync()
        {
            var (chapterId, userId, like) = GetValues(Context);
            await Clients.Caller.SendAsync("SetLike", like != null);
            await base.OnConnectedAsync();
        }

        public async Task ChangeLike()
        {
            var (chapterId, userId, like) = GetValues(Context);

            bool wasLiked = like != null;
            if (wasLiked)
            {
                _dbContext.Likes.Remove(like);  
            }
            else
            { 
                _dbContext.Likes.Add(new LikeModel() { ChapterModelId = chapterId, UserId = userId });
            }

            _dbContext.SaveChanges();
            await Clients.Caller.SendAsync("SetLike", !wasLiked);
        }
    }
}
