using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace IS_Turizmas.Models
{
    public partial class RegistruotiVartotojai :IValidatableObject
    {
        public RegistruotiVartotojai()
        {
            Komentarai = new HashSet<Komentarai>();
            Marsrutai = new HashSet<Marsrutai>();
            PasiulymuPranesimai = new HashSet<PasiulymuPranesimai>();
            PrenumeratosFkPrenumeratoriusNavigation = new HashSet<Prenumeratos>();
            PrenumeratosFkPrenumeruojamasisNavigation = new HashSet<Prenumeratos>();
            Reitingai = new HashSet<Reitingai>();
            VartotojoPlanai = new HashSet<VartotojoPlanai>();
        }

        [Required]
        [StringLength(60, ErrorMessage ="Bloga reikšmė. Maksimalus simbolių skaičius - 60")]
        public string Vardas { get; set; }
        [Required]
        [StringLength(60, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 60")]
        public string Pavarde { get; set; }
        public DateTime? GimimoData { get; set; }
        [Required]
        [EmailAddress]
        public string ElPastas { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Bloga reikšmė turi būti tarp 8 ir 255 simbolių")]
        public string Slaptazodis { get; set; }
        [Required]
        [StringLength(60,MinimumLength = 6, ErrorMessage = "Bloga reikšmė. Simbolių skaičius nuo 6 iki 60")]
        public string Slapyvardis { get; set; }
        [Required]
        public DateTime RegistracijosData { get; set; }
        [Required]
        public DateTime PrisijungimoData { get; set; }
        public string Nuotrauka { get; set; }
        [Required]
        public int AktyvumoTaskai { get; set; }
        public int Id { get; set; }

        public virtual VersloVartotojai VersloVartotojai { get; set; }
        public virtual ICollection<Komentarai> Komentarai { get; set; }
        public virtual ICollection<Marsrutai> Marsrutai { get; set; }
        public virtual ICollection<PasiulymuPranesimai> PasiulymuPranesimai { get; set; }
        public virtual ICollection<Prenumeratos> PrenumeratosFkPrenumeratoriusNavigation { get; set; }
        public virtual ICollection<Prenumeratos> PrenumeratosFkPrenumeruojamasisNavigation { get; set; }
        public virtual ICollection<Reitingai> Reitingai { get; set; }
        public virtual ICollection<VartotojoPlanai> VartotojoPlanai { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingUser = context.RegistruotiVartotojai.FirstOrDefault(o => o.ElPastas == ElPastas &&
                o.Id!=Id);
                if (existingUser!=null) {
                    yield return new ValidationResult("Šitas el. pašto adresas užimtas",
                        new[] { nameof(ElPastas) });
                }
            }

        }

    }

    public class EditViewRegistruotiVartotojai
    {
        [Required]
        [StringLength(60, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 60")]
        public string Vardas { get; set; }
        [Required]
        [StringLength(60, ErrorMessage = "Bloga reikšmė. Maksimalus simbolių skaičius - 60")]
        public string Pavarde { get; set; }
        public DateTime? GimimoData { get; set; }
        [Required]
        [EmailAddress]
        public string ElPastas { get; set; }
        public string Nuotrauka { get; set; }
        [Required]
        public int Id { get; set; }
    }
}
