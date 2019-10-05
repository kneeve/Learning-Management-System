using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignments
    {
        public Assignments()
        {
            Submission = new HashSet<Submission>();
        }

        public string Name { get; set; }
        public ushort? MaxPoints { get; set; }
        public string Contents { get; set; }
        public DateTime? Due { get; set; }
        public uint AId { get; set; }
        public uint AsgCatId { get; set; }

        public virtual AsgCats AsgCat { get; set; }
        public virtual ICollection<Submission> Submission { get; set; }
    }
}
