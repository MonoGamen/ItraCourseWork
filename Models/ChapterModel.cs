using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class ChapterModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        [Required]
        public string Name { get; set; }
        [UIHint("Boolean")]
        public bool IsMarkdown { get; set; }
        [Required]
        public string Text { get; set; }
        public string Image { get; set; }
        public string ImagePublicId { get; set; }
        public int FanficModelId { get; set; }
        public FanficModel Fanfic { get; set; } 
        public DateTime LastEdit { get; set; }
    }
}
