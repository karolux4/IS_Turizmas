using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IS_Turizmas.Models
{
    public partial class Marsrutai : IValidatableObject
    {
        public Marsrutai()
        {
            Komentarai = new HashSet<Komentarai>();
            MarsrutoObjektai = new HashSet<MarsrutoObjektai>();
            Reitingai = new HashSet<Reitingai>();
        }

        [Required(ErrorMessage ="Laukas yra privalomas")]
        [StringLength(255, ErrorMessage = "Reikšmė per ilga. Turi būti iki 255 simbolių")]
        public string Pavadinimas { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public string Aprasymas { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public DateTime ModifikavimoData { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public DateTime SukurimoData { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [Range(0, int.MaxValue, ErrorMessage ="Bloga reikšmė. Turi būti didesnė arba lygi 0")]
        public int IslaidosNuo { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [Range(0, int.MaxValue, ErrorMessage = "Bloga reikšmė. Turi būti didesnė arba lygi 0")]
        public int IslaidosIki { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [Range(0, int.MaxValue, ErrorMessage = "Bloga reikšmė. Turi būti didesnė arba lygi 0")]
        public int Perziuros { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public int LaikoIvertis { get; set; }
        public int Id { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public int FkRegistruotasVartotojas { get; set; }

        public virtual RegistruotiVartotojai FkRegistruotasVartotojasNavigation { get; set; }
        public virtual LaikoIverciai LaikoIvertisNavigation { get; set; }
        public virtual ICollection<Komentarai> Komentarai { get; set; }
        public virtual ICollection<MarsrutoObjektai> MarsrutoObjektai { get; set; }
        public virtual ICollection<Reitingai> Reitingai { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IslaidosNuo > IslaidosIki)
            {
                yield return new ValidationResult("Išlaidų apatinė riba negali būti didesnė už viršutinę",
                    new[] { nameof(IslaidosNuo) });
            }

        }
    }
}
