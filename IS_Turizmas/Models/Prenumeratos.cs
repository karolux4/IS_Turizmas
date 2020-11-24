using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class Prenumeratos
    {
        public DateTime Data { get; set; }
        public int Id { get; set; }
        public int FkPrenumeratorius { get; set; }
        public int FkPrenumeruojamasis { get; set; }

        public virtual RegistruotiVartotojai FkPrenumeratoriusNavigation { get; set; }
        public virtual RegistruotiVartotojai FkPrenumeruojamasisNavigation { get; set; }
    }
}
