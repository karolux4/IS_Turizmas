using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class ReklamosPlanai
    {
        public ReklamosPlanai()
        {
            ReklamavimoLaikai = new HashSet<ReklamavimoLaikai>();
        }

        public double Kaina { get; set; }
        public int Id { get; set; }
        public int FkReklama { get; set; }

        public DateTime Laikas_nuo { get; set; }

        public DateTime Laikas_iki { get; set; }

        public virtual Reklamos FkReklamaNavigation { get; set; }
        public virtual ICollection<ReklamavimoLaikai> ReklamavimoLaikai { get; set; }
    }
}
