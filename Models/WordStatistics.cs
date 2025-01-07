using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyWordWPF_US5.Models
{
    public class WordStatistics
    {
        public string German { get; set; }
        public string English { get; set; }
        public int CorrectCount { get; set; } = 0;
        public int IncorrectCount { get; set; } = 0;
    }
}
