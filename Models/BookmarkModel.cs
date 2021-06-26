using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class BookmarkModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int FanficModelId { get; set; }
        public FanficModel Fanfic { get; set; }
    }
}
