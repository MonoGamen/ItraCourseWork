using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class FanficModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public FandomModel Fandom { get; set; }
        public List<TagModel> Tags { get; set; }
        public string UserId { get; set; }
        public FanficModel()
        {
            Tags = new List<TagModel>();
        }
    }
}
