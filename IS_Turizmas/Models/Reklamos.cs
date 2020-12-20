using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class Reklamos
    {
        public Reklamos()
        {
            ReklamosPlanai = new HashSet<ReklamosPlanai>();
        }

        public string Pavadinimas { get; set; }
        public string Paveikslelis { get; set; }
        public string Url { get; set; }
        public int Paspaudimai { get; set; }
        public int Id { get; set; }
        public int FkVersloVartotojas { get; set; }

        public virtual VersloVartotojai FkVersloVartotojasNavigation { get; set; }
        public virtual ICollection<ReklamosPlanai> ReklamosPlanai { get; set; }
    }
}
