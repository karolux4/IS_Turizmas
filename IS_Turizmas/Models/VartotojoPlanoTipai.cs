using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class VartotojoPlanoTipai
    {
        public VartotojoPlanoTipai()
        {
            VartotojoPlanai = new HashSet<VartotojoPlanai>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<VartotojoPlanai> VartotojoPlanai { get; set; }
    }
}
