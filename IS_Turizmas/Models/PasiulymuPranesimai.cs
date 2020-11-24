using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class PasiulymuPranesimai
    {
        public string Tekstas { get; set; }
        public int Id { get; set; }
        public int FkRegistruotasVartotojas { get; set; }

        public DateTime Data { get; set; }

        public virtual RegistruotiVartotojai FkRegistruotasVartotojasNavigation { get; set; }
    }
}
