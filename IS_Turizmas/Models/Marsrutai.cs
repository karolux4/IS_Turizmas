using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class Marsrutai
    {
        public Marsrutai()
        {
            Komentarai = new HashSet<Komentarai>();
            MarsrutoObjektai = new HashSet<MarsrutoObjektai>();
            Reitingai = new HashSet<Reitingai>();
        }

        public string Pavadinimas { get; set; }
        public string Aprasymas { get; set; }
        public DateTime ModifikavimoData { get; set; }
        public DateTime SukurimoData { get; set; }
        public int IslaidosNuo { get; set; }
        public int IslaidosIki { get; set; }
        public int Perziuros { get; set; }
        public int LaikoIvertis { get; set; }
        public int Id { get; set; }
        public int FkRegistruotasVartotojas { get; set; }

        public virtual RegistruotiVartotojai FkRegistruotasVartotojasNavigation { get; set; }
        public virtual LaikoIverciai LaikoIvertisNavigation { get; set; }
        public virtual ICollection<Komentarai> Komentarai { get; set; }
        public virtual ICollection<MarsrutoObjektai> MarsrutoObjektai { get; set; }
        public virtual ICollection<Reitingai> Reitingai { get; set; }
    }
}
