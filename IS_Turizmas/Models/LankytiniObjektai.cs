using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_Turizmas.Models
{
    public partial class LankytiniObjektai
    {
        public LankytiniObjektai()
        {
            MarsrutoObjektai = new HashSet<MarsrutoObjektai>();
        }

        [Required(ErrorMessage = "Laukas yra privalomas")]
        [Range(-180,180, ErrorMessage = "Bloga reikšmė. Reikšmė turi būti nuo -180 iki 180")]
        public double XKoordinate { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [Range(-90, 90, ErrorMessage = "Bloga reikšmė. Reikšmė turi būti nuo -90 iki 90")]
        public double YKoordinate { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public string Pavadinimas { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public int FkValstybe { get; set; }

        public virtual Valstybes FkValstybeNavigation { get; set; }
        public virtual ICollection<MarsrutoObjektai> MarsrutoObjektai { get; set; }
    }
}
