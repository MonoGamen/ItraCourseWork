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
    public class MarkHub : Hub
    {
        ApplicationDbContext _dbContext;
        UserManager<IdentityUser> _userManager;
        public MarkHub(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        private (int, string, MarkModel) GetValues(HubCallerContext context)
        {
            int fanficId = int.Parse(context.GetHttpContext().Request.Query["fanficId"].ToString().Split("/").Last());
            string userId = _userManager.GetUserId(context.User);
            var mark = _dbContext.Marks.Where(l => l.FanficModelId == fanficId && l.UserId == userId).FirstOrDefault();

            return (fanficId, userId, mark);
        }

        public override async Task OnConnectedAsync()
        {
            var (fanficId, userId, mark) = GetValues(Context);
            await Clients.Caller.SendAsync("SetMark", mark != null ? mark.Mark : 0);
            await base.OnConnectedAsync();
        }


        private void DeleteMark(MarkModel mark, int newMark, FanficModel fanfic, int fanficId, string userId)
        {
            if (fanfic.MarkCount == 1)
                fanfic.MarkAverage = 0;
            else
                fanfic.MarkAverage = (Math.Round(fanfic.MarkAverage * fanfic.MarkCount) - newMark) / (fanfic.MarkCount - 1);

            fanfic.MarkCount--;

            _dbContext.Fanfics.Update(fanfic);

            _dbContext.Marks.Remove(mark);
        }
        private void UpdateMark(MarkModel mark, int newMark, FanficModel fanfic, int fanficId, string userId)
        {
            int difference = newMark - mark.Mark;
            fanfic.MarkAverage = (Math.Round(fanfic.MarkAverage * fanfic.MarkCount) + difference) / fanfic.MarkCount;

            mark.Mark = newMark;
            _dbContext.Marks.Update(mark);
        }
        private void AddMark(int newMark, FanficModel fanfic, int fanficId, string userId)
        {
            _dbContext.Marks.Add(new MarkModel() { FanficModelId = fanficId, UserId = userId, Mark = newMark });

            fanfic.MarkAverage = (Math.Round(fanfic.MarkAverage * fanfic.MarkCount) + newMark) / (fanfic.MarkCount + 1);
            fanfic.MarkCount++;

            _dbContext.Fanfics.Update(fanfic);
        }


        private bool ManageMark(MarkModel mark, int newMark, FanficModel fanfic, int fanficId, string userId)
        {
            if (mark != null)
            {
                if (mark.Mark == newMark) 
                {
                    DeleteMark(mark, newMark, fanfic, fanficId, userId);
                    return true;
                }
                else
                {
                    UpdateMark(mark, newMark, fanfic, fanficId, userId);
                }  
            }
            else
            {
                AddMark(newMark, fanfic, fanficId, userId);         
            }
            return false;
        }

        public async Task ChangeMark(int newMark)
        {
            var (fanficId, userId, mark) = GetValues(Context);
            var fanfic = _dbContext.Fanfics.First(f => f.Id == fanficId);

            bool deletedMark = ManageMark(mark, newMark, fanfic, fanficId, userId);            

            _dbContext.SaveChanges();
            await Clients.Caller.SendAsync("SetMark", deletedMark ? 0 : newMark);
        }
    }
}
