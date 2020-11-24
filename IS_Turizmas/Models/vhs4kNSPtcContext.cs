using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;

namespace IS_Turizmas.Models
{
    public partial class vhs4kNSPtcContext : DbContext
    {
        public vhs4kNSPtcContext()
        {
        }

        public vhs4kNSPtcContext(DbContextOptions<vhs4kNSPtcContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AktyvumoLygiai> AktyvumoLygiai { get; set; }
        public virtual DbSet<Komentarai> Komentarai { get; set; }
        public virtual DbSet<LaikoIverciai> LaikoIverciai { get; set; }
        public virtual DbSet<LankytiniObjektai> LankytiniObjektai { get; set; }
        public virtual DbSet<Marsrutai> Marsrutai { get; set; }
        public virtual DbSet<MarsrutoObjektai> MarsrutoObjektai { get; set; }
        public virtual DbSet<PasiulymuPranesimai> PasiulymuPranesimai { get; set; }
        public virtual DbSet<Prenumeratos> Prenumeratos { get; set; }
        public virtual DbSet<RegistruotiVartotojai> RegistruotiVartotojai { get; set; }
        public virtual DbSet<Reitingai> Reitingai { get; set; }
        public virtual DbSet<ReklamavimoLaikai> ReklamavimoLaikai { get; set; }
        public virtual DbSet<Reklamos> Reklamos { get; set; }
        public virtual DbSet<ReklamosPlanai> ReklamosPlanai { get; set; }
        public virtual DbSet<Valstybes> Valstybes { get; set; }
        public virtual DbSet<VartotojoPlanai> VartotojoPlanai { get; set; }
        public virtual DbSet<VartotojoPlanoTipai> VartotojoPlanoTipai { get; set; }
        public virtual DbSet<VersloVartotojai> VersloVartotojai { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var myJsonString = System.IO.File.ReadAllText("..\\config.json");
                var myJObject = JObject.Parse(myJsonString);
                var conf_string = myJObject.SelectToken("ConfigurationString").Value<string>();
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySQL(conf_string);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AktyvumoLygiai>(entity =>
            {
                entity.ToTable("Aktyvumo_lygiai");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Iki)
                    .HasColumnName("iki")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Nuo)
                    .HasColumnName("nuo")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Pavadinimas)
                    .IsRequired()
                    .HasColumnName("pavadinimas")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Komentarai>(entity =>
            {
                entity.HasIndex(e => e.FkMarsrutas)
                    .HasName("fkc_Marsrutas2");

                entity.HasIndex(e => e.FkRegistruotasVartotojas)
                    .HasName("fkc_Registruotas_vartotojas3");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Data)
                    .HasColumnName("data")
                    .HasColumnType("date");

                entity.Property(e => e.FkMarsrutas)
                    .HasColumnName("fk_Marsrutas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkRegistruotasVartotojas)
                    .HasColumnName("fk_Registruotas_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Tekstas)
                    .IsRequired()
                    .HasColumnName("tekstas");

                entity.HasOne(d => d.FkMarsrutasNavigation)
                    .WithMany(p => p.Komentarai)
                    .HasForeignKey(d => d.FkMarsrutas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Marsrutas2");

                entity.HasOne(d => d.FkRegistruotasVartotojasNavigation)
                    .WithMany(p => p.Komentarai)
                    .HasForeignKey(d => d.FkRegistruotasVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Registruotas_vartotojas3");
            });

            modelBuilder.Entity<LaikoIverciai>(entity =>
            {
                entity.ToTable("Laiko_iverciai");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(6)
                    .IsFixedLength();
            });

            modelBuilder.Entity<LankytiniObjektai>(entity =>
            {
                entity.ToTable("Lankytini_Objektai");

                entity.HasIndex(e => e.FkValstybe)
                    .HasName("fkc_Valstybe");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkValstybe)
                    .HasColumnName("fk_Valstybe")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Pavadinimas)
                    .IsRequired()
                    .HasColumnName("pavadinimas")
                    .HasMaxLength(255);

                entity.Property(e => e.XKoordinate).HasColumnName("x_koordinate");

                entity.Property(e => e.YKoordinate).HasColumnName("y_koordinate");

                entity.HasOne(d => d.FkValstybeNavigation)
                    .WithMany(p => p.LankytiniObjektai)
                    .HasForeignKey(d => d.FkValstybe)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Valstybe");
            });

            modelBuilder.Entity<Marsrutai>(entity =>
            {
                entity.HasIndex(e => e.FkRegistruotasVartotojas)
                    .HasName("fkc_Registruotas_vartotojas1");

                entity.HasIndex(e => e.LaikoIvertis)
                    .HasName("laiko_ivertis");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Aprasymas)
                    .IsRequired()
                    .HasColumnName("aprasymas");

                entity.Property(e => e.FkRegistruotasVartotojas)
                    .HasColumnName("fk_Registruotas_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IslaidosIki)
                    .HasColumnName("islaidos_iki")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IslaidosNuo)
                    .HasColumnName("islaidos__nuo")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LaikoIvertis)
                    .HasColumnName("laiko_ivertis")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ModifikavimoData)
                    .HasColumnName("modifikavimo_data")
                    .HasColumnType("date");

                entity.Property(e => e.Pavadinimas)
                    .IsRequired()
                    .HasColumnName("pavadinimas")
                    .HasMaxLength(255);

                entity.Property(e => e.Perziuros)
                    .HasColumnName("perziuros")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SukurimoData)
                    .HasColumnName("sukurimo_data")
                    .HasColumnType("date");

                entity.HasOne(d => d.FkRegistruotasVartotojasNavigation)
                    .WithMany(p => p.Marsrutai)
                    .HasForeignKey(d => d.FkRegistruotasVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Registruotas_vartotojas1");

                entity.HasOne(d => d.LaikoIvertisNavigation)
                    .WithMany(p => p.Marsrutai)
                    .HasForeignKey(d => d.LaikoIvertis)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Marsrutai_ibfk_1");
            });

            modelBuilder.Entity<MarsrutoObjektai>(entity =>
            {
                entity.ToTable("Marsruto_Objektai");

                entity.HasIndex(e => e.FkLankytinasObjektas)
                    .HasName("fkc_Lankytinas_Objektas");

                entity.HasIndex(e => e.FkMarsrutas)
                    .HasName("fkc_Marsrutas1");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EilesNr)
                    .HasColumnName("eiles_nr")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkLankytinasObjektas)
                    .HasColumnName("fk_Lankytinas_Objektas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkMarsrutas)
                    .HasColumnName("fk_Marsrutas")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.FkLankytinasObjektasNavigation)
                    .WithMany(p => p.MarsrutoObjektai)
                    .HasForeignKey(d => d.FkLankytinasObjektas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Lankytinas_Objektas");

                entity.HasOne(d => d.FkMarsrutasNavigation)
                    .WithMany(p => p.MarsrutoObjektai)
                    .HasForeignKey(d => d.FkMarsrutas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Marsrutas1");
            });

            modelBuilder.Entity<PasiulymuPranesimai>(entity =>
            {
                entity.ToTable("Pasiulymu_pranesimai");

                entity.HasIndex(e => e.FkRegistruotasVartotojas)
                    .HasName("fkc_Registruotas_vartotojas5");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkRegistruotasVartotojas)
                    .HasColumnName("fk_Registruotas_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Tekstas)
                    .IsRequired()
                    .HasColumnName("tekstas");

                entity.Property(e => e.Data).HasColumnName("data").HasColumnType("date");

                entity.HasOne(d => d.FkRegistruotasVartotojasNavigation)
                    .WithMany(p => p.PasiulymuPranesimai)
                    .HasForeignKey(d => d.FkRegistruotasVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Registruotas_vartotojas5");
            });

            modelBuilder.Entity<Prenumeratos>(entity =>
            {
                entity.HasIndex(e => e.FkPrenumeratorius)
                    .HasName("fkc_prenumeratorius");

                entity.HasIndex(e => e.FkPrenumeruojamasis)
                    .HasName("fkc_prenumeruojamasis");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Data)
                    .HasColumnName("data")
                    .HasColumnType("date");

                entity.Property(e => e.FkPrenumeratorius)
                    .HasColumnName("fk_prenumeratorius")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkPrenumeruojamasis)
                    .HasColumnName("fk_prenumeruojamasis")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.FkPrenumeratoriusNavigation)
                    .WithMany(p => p.PrenumeratosFkPrenumeratoriusNavigation)
                    .HasForeignKey(d => d.FkPrenumeratorius)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_prenumeratorius");

                entity.HasOne(d => d.FkPrenumeruojamasisNavigation)
                    .WithMany(p => p.PrenumeratosFkPrenumeruojamasisNavigation)
                    .HasForeignKey(d => d.FkPrenumeruojamasis)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_prenumeruojamasis");
            });

            modelBuilder.Entity<RegistruotiVartotojai>(entity =>
            {
                entity.ToTable("Registruoti_vartotojai");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AktyvumoTaskai)
                    .HasColumnName("aktyvumo_taskai")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ElPastas)
                    .IsRequired()
                    .HasColumnName("el_pastas")
                    .HasMaxLength(255);

                entity.Property(e => e.GimimoData)
                    .HasColumnName("gimimo_data")
                    .HasColumnType("date");

                entity.Property(e => e.Nuotrauka)
                    .HasColumnName("nuotrauka")
                    .HasMaxLength(255);

                entity.Property(e => e.Pavardė)
                    .IsRequired()
                    .HasColumnName("pavardė")
                    .HasMaxLength(60);

                entity.Property(e => e.PrisijungimoData)
                    .HasColumnName("prisijungimo_data")
                    .HasColumnType("date");

                entity.Property(e => e.RegistracijosData)
                    .HasColumnName("registracijos_data")
                    .HasColumnType("date");

                entity.Property(e => e.Slaptazodis)
                    .IsRequired()
                    .HasColumnName("slaptazodis")
                    .HasMaxLength(255);

                entity.Property(e => e.Slapyvardis)
                    .IsRequired()
                    .HasColumnName("slapyvardis")
                    .HasMaxLength(60);

                entity.Property(e => e.Vardas)
                    .IsRequired()
                    .HasColumnName("vardas")
                    .HasMaxLength(60);
            });

            modelBuilder.Entity<Reitingai>(entity =>
            {
                entity.HasIndex(e => e.FkMarsrutas)
                    .HasName("fkc_Marsrutas3");

                entity.HasIndex(e => e.FkRegistruotasVartotojas)
                    .HasName("fkc_Registruotas_vartotojas2");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Data)
                    .HasColumnName("data")
                    .HasColumnType("date");

                entity.Property(e => e.FkMarsrutas)
                    .HasColumnName("fk_Marsrutas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkRegistruotasVartotojas)
                    .HasColumnName("fk_Registruotas_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Reitingas)
                    .HasColumnName("reitingas")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.FkMarsrutasNavigation)
                    .WithMany(p => p.Reitingai)
                    .HasForeignKey(d => d.FkMarsrutas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Marsrutas3");

                entity.HasOne(d => d.FkRegistruotasVartotojasNavigation)
                    .WithMany(p => p.Reitingai)
                    .HasForeignKey(d => d.FkRegistruotasVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Registruotas_vartotojas2");
            });

            modelBuilder.Entity<ReklamavimoLaikai>(entity =>
            {
                entity.ToTable("Reklamavimo_laikai");

                entity.HasIndex(e => e.FkReklamosPlanas)
                    .HasName("fkc_Reklamos_planas");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkReklamosPlanas)
                    .HasColumnName("fk_Reklamos_planas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SavaitesDiena)
                    .HasColumnName("savaites_diena")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ValandaIki).HasColumnName("valanda_iki");

                entity.Property(e => e.ValandaNuo).HasColumnName("valanda_nuo");

                entity.HasOne(d => d.FkReklamosPlanasNavigation)
                    .WithMany(p => p.ReklamavimoLaikai)
                    .HasForeignKey(d => d.FkReklamosPlanas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Reklamos_planas");
            });

            modelBuilder.Entity<Reklamos>(entity =>
            {
                entity.HasIndex(e => e.FkVersloVartotojas)
                    .HasName("fkc_Verslo_vartotojas");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkVersloVartotojas)
                    .HasColumnName("fk_Verslo_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Paspaudimai)
                    .HasColumnName("paspaudimai")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Paveikslelis)
                    .IsRequired()
                    .HasColumnName("paveikslelis")
                    .HasMaxLength(255);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasMaxLength(255);

                entity.HasOne(d => d.FkVersloVartotojasNavigation)
                    .WithMany(p => p.Reklamos)
                    .HasForeignKey(d => d.FkVersloVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Verslo_vartotojas");
            });

            modelBuilder.Entity<ReklamosPlanai>(entity =>
            {
                entity.ToTable("Reklamos_planai");

                entity.HasIndex(e => e.FkReklama)
                    .HasName("fkc_Reklama");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FkReklama)
                    .HasColumnName("fk_Reklama")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Kaina).HasColumnName("kaina");

                entity.Property(e => e.Paspaudimai)
                    .HasColumnName("paspaudimai")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Laikas_nuo).HasColumnName("laikas_nuo").HasColumnType("date");

                entity.Property(e => e.Laikas_iki).HasColumnName("laikas_iki").HasColumnType("date");

                entity.HasOne(d => d.FkReklamaNavigation)
                    .WithMany(p => p.ReklamosPlanai)
                    .HasForeignKey(d => d.FkReklama)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Reklama");
            });

            modelBuilder.Entity<Valstybes>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Pavadinimas)
                    .IsRequired()
                    .HasColumnName("pavadinimas")
                    .HasMaxLength(255);

                entity.Property(e => e.Trumpinys)
                    .IsRequired()
                    .HasColumnName("trumpinys")
                    .HasMaxLength(3)
                    .IsFixedLength();

                entity.Property(e => e.Zemynas)
                    .IsRequired()
                    .HasColumnName("zemynas")
                    .HasMaxLength(60);
            });

            modelBuilder.Entity<VartotojoPlanai>(entity =>
            {
                entity.ToTable("Vartotojo_planai");

                entity.HasIndex(e => e.FkRegistruotasVartotojas)
                    .HasName("fkc_Registruotas_vartotojas4");

                entity.HasIndex(e => e.Tipas)
                    .HasName("tipas");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DataIki)
                    .HasColumnName("data_iki")
                    .HasColumnType("date");

                entity.Property(e => e.DataNuo)
                    .HasColumnName("data_nuo")
                    .HasColumnType("date");

                entity.Property(e => e.FkRegistruotasVartotojas)
                    .HasColumnName("fk_Registruotas_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Tipas)
                    .HasColumnName("tipas")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.FkRegistruotasVartotojasNavigation)
                    .WithMany(p => p.VartotojoPlanai)
                    .HasForeignKey(d => d.FkRegistruotasVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Registruotas_vartotojas4");

                entity.HasOne(d => d.TipasNavigation)
                    .WithMany(p => p.VartotojoPlanai)
                    .HasForeignKey(d => d.Tipas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Vartotojo_planai_ibfk_1");
            });

            modelBuilder.Entity<VartotojoPlanoTipai>(entity =>
            {
                entity.ToTable("Vartotojo_plano_tipai");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(9)
                    .IsFixedLength();
            });

            modelBuilder.Entity<VersloVartotojai>(entity =>
            {
                entity.HasKey(e => e.FkRegistruotasVartotojas)
                    .HasName("PRIMARY");

                entity.ToTable("Verslo_vartotojai");

                entity.Property(e => e.FkRegistruotasVartotojas)
                    .HasColumnName("fk_Registruotas_vartotojas")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Adresas)
                    .IsRequired()
                    .HasColumnName("adresas")
                    .HasMaxLength(255);

                entity.Property(e => e.Imone)
                    .IsRequired()
                    .HasColumnName("imone")
                    .HasMaxLength(255);

                entity.Property(e => e.PastoKodas)
                    .IsRequired()
                    .HasColumnName("pasto_kodas")
                    .HasMaxLength(20);

                entity.Property(e => e.Svetaine)
                    .HasColumnName("svetaine")
                    .HasMaxLength(255);

                entity.HasOne(d => d.FkRegistruotasVartotojasNavigation)
                    .WithOne(p => p.VersloVartotojai)
                    .HasForeignKey<VersloVartotojai>(d => d.FkRegistruotasVartotojas)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fkc_Registruotas_vartotojas6");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
