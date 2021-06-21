using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class LikeModel
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int ChapterId { get; set; }
    }
}
