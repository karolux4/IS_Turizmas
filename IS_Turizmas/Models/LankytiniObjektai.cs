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

        [Required]
        [Range(-180,180, ErrorMessage = "Bloga reikšmė. Reikšmė turi būti nuo -180 iki 180")]
        public double XKoordinate { get; set; }
        [Required]
        [Range(-90, 90, ErrorMessage = "Bloga reikšmė. Reikšmė turi būti nuo -90 iki 90")]
        public double YKoordinate { get; set; }
        [Required]
        public string Pavadinimas { get; set; }
        [Required]
        public int Id { get; set; }
        [Required]
        public int FkValstybe { get; set; }

        public virtual Valstybes FkValstybeNavigation { get; set; }
        public virtual ICollection<MarsrutoObjektai> MarsrutoObjektai { get; set; }
    }
}
