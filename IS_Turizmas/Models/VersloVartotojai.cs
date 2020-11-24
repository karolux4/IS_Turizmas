using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class VersloVartotojai
    {
        public VersloVartotojai()
        {
            Reklamos = new HashSet<Reklamos>();
        }

        public string Imone { get; set; }
        public string PastoKodas { get; set; }
        public string Svetaine { get; set; }
        public string Adresas { get; set; }
        public int FkRegistruotasVartotojas { get; set; }

        public virtual RegistruotiVartotojai FkRegistruotasVartotojasNavigation { get; set; }
        public virtual ICollection<Reklamos> Reklamos { get; set; }
    }
}
