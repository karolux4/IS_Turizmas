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
        [Required]
        public string Pavadinimas { get; set; }
        [Required]
        public string Trumpinys { get; set; }
        [Required]
        public string Zemynas { get; set; }
        [Required]
        public int Id { get; set; }

        public virtual ICollection<LankytiniObjektai> LankytiniObjektai { get; set; }
    }
}
