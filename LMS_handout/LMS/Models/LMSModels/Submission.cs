using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public DateTime Time { get; set; }
        public uint? Score { get; set; }
        public string Contents { get; set; }
        public uint AId { get; set; }
        public string StudentId { get; set; }

        public virtual Assignments A { get; set; }
        public virtual Students Student { get; set; }
    }
}
