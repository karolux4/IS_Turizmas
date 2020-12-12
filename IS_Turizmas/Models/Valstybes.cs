using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_Turizmas.Models
{
    public partial class Valstybes
    {
        public Valstybes()
        {
            LankytiniObjektai = new HashSet<LankytiniObjektai>();
        }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public string Pavadinimas { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public string Trumpinys { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public string Zemynas { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public int Id { get; set; }

        public virtual ICollection<LankytiniObjektai> LankytiniObjektai { get; set; }
    }
}
