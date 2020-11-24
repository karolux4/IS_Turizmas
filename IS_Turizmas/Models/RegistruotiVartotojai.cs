using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class RegistruotiVartotojai
    {
        public RegistruotiVartotojai()
        {
            Komentarai = new HashSet<Komentarai>();
            Marsrutai = new HashSet<Marsrutai>();
            PasiulymuPranesimai = new HashSet<PasiulymuPranesimai>();
            PrenumeratosFkPrenumeratoriusNavigation = new HashSet<Prenumeratos>();
            PrenumeratosFkPrenumeruojamasisNavigation = new HashSet<Prenumeratos>();
            Reitingai = new HashSet<Reitingai>();
            VartotojoPlanai = new HashSet<VartotojoPlanai>();
        }

        public string Vardas { get; set; }
        public string Pavardė { get; set; }
        public DateTime? GimimoData { get; set; }
        public string ElPastas { get; set; }
        public string Slaptazodis { get; set; }
        public string Slapyvardis { get; set; }
        public DateTime RegistracijosData { get; set; }
        public DateTime PrisijungimoData { get; set; }
        public string Nuotrauka { get; set; }
        public int AktyvumoTaskai { get; set; }
        public int Id { get; set; }

        public virtual VersloVartotojai VersloVartotojai { get; set; }
        public virtual ICollection<Komentarai> Komentarai { get; set; }
        public virtual ICollection<Marsrutai> Marsrutai { get; set; }
        public virtual ICollection<PasiulymuPranesimai> PasiulymuPranesimai { get; set; }
        public virtual ICollection<Prenumeratos> PrenumeratosFkPrenumeratoriusNavigation { get; set; }
        public virtual ICollection<Prenumeratos> PrenumeratosFkPrenumeruojamasisNavigation { get; set; }
        public virtual ICollection<Reitingai> Reitingai { get; set; }
        public virtual ICollection<VartotojoPlanai> VartotojoPlanai { get; set; }
    }
}
