using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IS_Turizmas.Models
{
    public partial class VersloVartotojai : IValidatableObject
    {
        public VersloVartotojai()
        {
            Reklamos = new HashSet<Reklamos>();
        }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [StringLength(255, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 2555")]
        public string Imone { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [StringLength(20, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 20")]
        public string PastoKodas { get; set; }
        [StringLength(255, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 255")]
        public string Svetaine { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        [StringLength(255, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 255")]
        public string Adresas { get; set; }
        public int FkRegistruotasVartotojas { get; set; }

        public virtual RegistruotiVartotojai FkRegistruotasVartotojasNavigation { get; set; }
        public virtual ICollection<Reklamos> Reklamos { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Svetaine != null)
            {
                Uri uriResult;
                bool result = Uri.TryCreate(Svetaine, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    yield return new ValidationResult("URL netinkamai suformuotas (turi būti pvz. http://www.example.com",
                        new[] { nameof(Svetaine) });
                }
                else
                {
                    if (!Svetaine.Contains("."))
                    {
                        yield return new ValidationResult("URL netinkamai suformuotas (turi būti pvz. http://www.example.com",
                        new[] { nameof(Svetaine) });
                    }
                }
            }
        }
    }
}
