using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class Valstybes
    {
        public Valstybes()
        {
            LankytiniObjektai = new HashSet<LankytiniObjektai>();
        }

        public string Pavadinimas { get; set; }
        public string Trumpinys { get; set; }
        public string Zemynas { get; set; }
        public int Id { get; set; }

        public virtual ICollection<LankytiniObjektai> LankytiniObjektai { get; set; }
    }
}
