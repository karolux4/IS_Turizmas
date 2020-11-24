using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class LankytiniObjektai
    {
        public LankytiniObjektai()
        {
            MarsrutoObjektai = new HashSet<MarsrutoObjektai>();
        }

        public double XKoordinate { get; set; }
        public double YKoordinate { get; set; }
        public string Pavadinimas { get; set; }
        public int Id { get; set; }
        public int FkValstybe { get; set; }

        public virtual Valstybes FkValstybeNavigation { get; set; }
        public virtual ICollection<MarsrutoObjektai> MarsrutoObjektai { get; set; }
    }
}
