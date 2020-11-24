using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class VartotojoPlanai
    {
        public DateTime DataNuo { get; set; }
        public DateTime DataIki { get; set; }
        public int Tipas { get; set; }
        public int Id { get; set; }
        public int FkRegistruotasVartotojas { get; set; }

        public virtual RegistruotiVartotojai FkRegistruotasVartotojasNavigation { get; set; }
        public virtual VartotojoPlanoTipai TipasNavigation { get; set; }
    }
}
