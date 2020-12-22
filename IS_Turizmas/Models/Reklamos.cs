using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IS_Turizmas.Models
{
    public partial class Reklamos : IValidatableObject
    {
        public Reklamos()
        {
            ReklamosPlanai = new HashSet<ReklamosPlanai>();
        }

        [Required(ErrorMessage = "Laukas yra privalomas")]
        [StringLength(255, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius 255")]
        public string Pavadinimas { get; set; }
        public string Paveikslelis { get; set; }
        [Required(ErrorMessage = "Laukas yra privalomas")]
        public string Url { get; set; }
        public int Paspaudimai { get; set; }
        public int Id { get; set; }
        public int FkVersloVartotojas { get; set; }

        public virtual VersloVartotojai FkVersloVartotojasNavigation { get; set; }
        public virtual ICollection<ReklamosPlanai> ReklamosPlanai { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(Url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                yield return new ValidationResult("URL netinkamai suformuotas (turi būti pvz. http://www.example.com",
                    new[] { nameof(Url) });
            }
            else
            {
                if (!Url.Contains("."))
                {
                    yield return new ValidationResult("URL netinkamai suformuotas (turi būti pvz. http://www.example.com",
                    new[] { nameof(Url) });
                }
            }

        }
    }
}
