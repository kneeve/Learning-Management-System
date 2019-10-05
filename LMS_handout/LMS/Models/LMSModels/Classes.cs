using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Classes
    {
        public Classes()
        {
            AsgCats = new HashSet<AsgCats>();
            Enrolled = new HashSet<Enrolled>();
        }

        public uint ClassId { get; set; }
        public uint? CatalogId { get; set; }
        public string ProfessorId { get; set; }
        public string Semester { get; set; }
        public string Location { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public uint? Year { get; set; }

        public virtual Courses Catalog { get; set; }
        public virtual Professors Professor { get; set; }
        public virtual ICollection<AsgCats> AsgCats { get; set; }
        public virtual ICollection<Enrolled> Enrolled { get; set; }
    }
}
