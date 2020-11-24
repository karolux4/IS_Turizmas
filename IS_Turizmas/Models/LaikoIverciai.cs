using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class LaikoIverciai
    {
        public LaikoIverciai()
        {
            Marsrutai = new HashSet<Marsrutai>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Marsrutai> Marsrutai { get; set; }
    }
}
