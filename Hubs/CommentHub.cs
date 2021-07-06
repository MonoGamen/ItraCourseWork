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
    public class CommentHub : Hub
    {
        ApplicationDbContext _dbContext;
        UserManager<IdentityUser> _userManager;
        public CommentHub(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        private int GetFanficId(HubCallerContext context)
        {
            int fanficId = int.Parse(context.GetHttpContext().Request.Query["fanficId"].ToString().Split("/").Last());
            return fanficId;
        }

        public override async Task OnConnectedAsync()
        {
            int fanficId = GetFanficId(Context);
            await Groups.AddToGroupAsync(Context.ConnectionId, fanficId.ToString());
            await base.OnConnectedAsync();
        }

        public async Task SendComment(string text, string username)
        {
            int fanficId = GetFanficId(Context);
            CommentModel comment = new CommentModel() {
                Text = text, UserId = null, UserName = username,
                Date = DateTime.Now, FanficModelId = fanficId
            };
            _dbContext.Comments.Add(comment);
            _dbContext.SaveChanges();
            await Clients.Group(fanficId.ToString()).SendAsync("AddNewComment", text, username, comment.Date.ToString());
        }
    }
}
