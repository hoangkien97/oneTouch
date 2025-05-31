using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OneTouch.Models;

public partial class OneTouchDbContext : DbContext
{
    public static OneTouchDbContext Ins = new OneTouchDbContext();
    public OneTouchDbContext()
    {
        if (Ins == null)
        {
            Ins = this;
        }
    }
    public OneTouchDbContext(DbContextOptions<OneTouchDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Clinic> Clinics { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserInsurance> UserInsurances { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("MyCnn"));

        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC22F00CEDB");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Schedule).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("FK__Appointme__Sched__48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Appointme__UserI__49C3F6B7");
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.ClinicId).HasName("PK__Clinics__3347C2DDAF261EDE");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctors__2DC00EBFC519572C");

            entity.HasIndex(e => e.UserId, "UQ__Doctors__1788CC4D3C798963").IsUnique();

            entity.Property(e => e.Certificate).HasMaxLength(255);

            entity.HasOne(d => d.Clinic).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("FK__Doctors__ClinicI__4316F928");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("FK__Doctors__Special__4222D4EF");

            entity.HasOne(d => d.User).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .HasConstraintName("FK__Doctors__UserId__412EB0B6");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD666864702");

            entity.ToTable("Feedback");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Feedback__Appoin__6754599E");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB5FBAB0BFD");

            entity.HasIndex(e => e.AppointmentId, "UQ__Invoices__8ECDFCC3784BECD4").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FinalAmount)
                .HasComputedColumnSql("([TotalAmount]-[InsuranceCoveredAmount])", false)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.InsuranceCoveredAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Appointment).WithOne(p => p.Invoice)
                .HasForeignKey<Invoice>(d => d.AppointmentId)
                .HasConstraintName("FK__Invoices__Appoin__60A75C0F");

            entity.HasOne(d => d.Insurance).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK__Invoices__Insura__619B8048");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78E953B5AC82");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__MedicalRe__Appoi__5812160E");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("PK__Medicine__4F21289038E4EED7");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12EDAF7323");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__6C190EBB");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__40130832D46641B9");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Prescript__Appoi__4E88ABD4");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.PrescriptionDetailId).HasName("PK__Prescrip__6DB766EAE44942C7");

            entity.Property(e => e.Dosage).HasMaxLength(255);
            entity.Property(e => e.Instructions).HasMaxLength(255);

            entity.HasOne(d => d.Medicine).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("FK__Prescript__Medic__5535A963");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("FK__Prescript__Presc__5441852A");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__9C8A5B492FBF6F2A");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK__Schedules__Docto__45F365D3");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.SpecialtyId).HasName("PK__Specialt__D768F6A8F9BC04D4");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C9912098F");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053452887411").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<UserInsurance>(entity =>
        {
            entity.HasKey(e => e.InsuranceId).HasName("PK__UserInsu__74231A24908B27DE");

            entity.Property(e => e.InsuranceNumber).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.UserInsurances)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserInsur__UserI__5BE2A6F2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
