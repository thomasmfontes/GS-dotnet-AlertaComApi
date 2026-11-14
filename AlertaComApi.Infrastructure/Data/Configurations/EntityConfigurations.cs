using AlertaComApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlertaComApi.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("TB_CIDADAO");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(120);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Cpf)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(u => u.Bairro)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(u => u.DataCriacao)
                .IsRequired();
        }
    }

    public class OcorrenciaConfiguration : IEntityTypeConfiguration<Ocorrencia>
    {
        public void Configure(EntityTypeBuilder<Ocorrencia> builder)
        {
            builder.ToTable("TB_OCORRENCIA");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedOnAdd();

            builder.Property(o => o.Protocolo)
                .IsRequired()
                .HasMaxLength(40);

            builder.HasIndex(o => o.Protocolo)
                .IsUnique();

            builder.Property(o => o.Titulo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Descricao)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(o => o.Endereco)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(o => o.Bairro)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(o => o.IdempotencyKey)
                .HasMaxLength(100);

            builder.HasIndex(o => o.IdempotencyKey);

            builder.Property(o => o.TipoRisco)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(o => o.Severidade)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasOne(o => o.User)
                .WithMany(u => u.Ocorrencias)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AlertaConfiguration : IEntityTypeConfiguration<AlertaClimatico>
    {
        public void Configure(EntityTypeBuilder<AlertaClimatico> builder)
        {
            builder.ToTable("TB_ALERTA_CLIMATICO");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Titulo)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(a => a.Mensagem)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(a => a.RegiaoAfetada)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Bairro)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(a => a.TipoRisco)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(a => a.Nivel)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }

    public class AuditoriaConfiguration : IEntityTypeConfiguration<AuditoriaOcorrencia>
    {
        public void Configure(EntityTypeBuilder<AuditoriaOcorrencia> builder)
        {
            builder.ToTable("TB_AUDITORIA_OCORRENCIA");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Operacao)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.Usuario)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
