using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS_Turizmas.Models
{
    public partial class MarsrutoObjektai
    {
        [Required]
        [Range(1,int.MaxValue, ErrorMessage ="Bloga reikšmė. Reikšmė turi būti didesnė arba lygi 1")]
        public int EilesNr { get; set; }
        [Required]
        public int Id { get; set; }
        [Required]
        public int FkLankytinasObjektas { get; set; }
        [Required]
        public int FkMarsrutas { get; set; }

        public virtual LankytiniObjektai FkLankytinasObjektasNavigation { get; set; }
        public virtual Marsrutai FkMarsrutasNavigation { get; set; }
    }
}
