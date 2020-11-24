using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class ReklamavimoLaikai
    {
        public int SavaitesDiena { get; set; }
        public TimeSpan ValandaNuo { get; set; }
        public TimeSpan ValandaIki { get; set; }
        public int Id { get; set; }
        public int FkReklamosPlanas { get; set; }

        public virtual ReklamosPlanai FkReklamosPlanasNavigation { get; set; }
    }
}
