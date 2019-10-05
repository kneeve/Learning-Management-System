using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AsgCats
    {
        public AsgCats()
        {
            Assignments = new HashSet<Assignments>();
        }

        public uint AsgCatId { get; set; }
        public byte? Weight { get; set; }
        public string Name { get; set; }
        public uint? ClassId { get; set; }

        public virtual Classes Class { get; set; }
        public virtual ICollection<Assignments> Assignments { get; set; }
    }
}
