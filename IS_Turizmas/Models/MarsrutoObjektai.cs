using System;
using System.Collections.Generic;

namespace IS_Turizmas.Models
{
    public partial class MarsrutoObjektai
    {
        public int EilesNr { get; set; }
        public int Id { get; set; }
        public int FkLankytinasObjektas { get; set; }
        public int FkMarsrutas { get; set; }

        public virtual LankytiniObjektai FkLankytinasObjektasNavigation { get; set; }
        public virtual Marsrutai FkMarsrutasNavigation { get; set; }
    }
}
