using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class UserSettingsModel
    {
        public int Id { get; set; }
        public string UserId{ get; set; }
        public bool IsOnboarded { get; set; } = false;
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "light";
    }
}
