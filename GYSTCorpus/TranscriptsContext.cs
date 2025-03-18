using GYSTCorpus.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus;
public class TranscriptsContext : DbContext
{
	private readonly string _connectionString = "DataSource=application.db";
	private readonly bool _useLazyLoading = true;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public TranscriptsContext() : base() { }

	public TranscriptsContext(string connectionString, bool useLazyLoading = true)
	{
		_connectionString = connectionString;
		_useLazyLoading = useLazyLoading;
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	public DbSet<Channel> Channels { get; set; }
	public DbSet<Video> Videos { get; set; }
	public DbSet<Transcript> Transcripts { get; set; }
	public DbSet<Anglicism> Anglicisms { get; set; }
	public DbSet<TranscriptAnglicism> TranscriptAnglicism { get; set; }
	public DbSet<AnglicismContextWindow> AnglicismContextWindows { get; set; }
	public DbSet<WordPartOfSpeech> WordPartOfSpeech { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<Channel>(entity =>
		{
			entity.HasKey(c => c.ChannelId);
		});

		builder.Entity<Video>(entity =>
		{
			entity.HasKey(v => v.VideoId);

			entity.Property(e => e.LiveStreamStatus)
				.HasConversion<string>()
				.ValueGeneratedNever();

			//entity.Property(e => e.CaptionsEnabled)
			//	.HasConversion(new ValueConverter<string, bool>(v => bool.Parse(v), v => v.ToString()))
			//	.ValueGeneratedNever();

			entity.HasOne(v => v.Channel)
				.WithMany(c => c.Videos)
				.HasForeignKey(v => v.ChannelId);
		});

		builder.Entity<Transcript>(entity =>
		{
			entity.HasKey(t => new { t.VideoId, t.LangCode });

			entity.HasIndex(t => t.VideoId);

			entity.HasOne(t => t.Video)
				.WithMany(v => v.Transcripts)
				.HasForeignKey(t => t.VideoId);
		});

		builder.Entity<Anglicism>(entity =>
		{
			entity.HasKey(e => e.Word);

			entity.HasIndex(e => e.BaseWord);

			entity.Property(e => e.EnglishPos)
				.HasConversion(p => (int)p, i => (PartOfSpeech)i)
				.ValueGeneratedNever();

			entity.Property(e => e.GermanPos)
				.HasConversion(p => (int)p, i => (PartOfSpeech)i)
				.ValueGeneratedNever();
		});

		builder.Entity<TranscriptAnglicism>(entity =>
		{
			entity.HasKey(e => new { e.VideoId, e.LangCode, e.Word, e.TranscriptIndex });

			entity.HasIndex(e => e.Word);
			entity.HasIndex(e => e.VideoId);

			entity.Property(e => e.GermanPos)
				.HasConversion(p => (int)p, i => (TreeTagger.Wrapper.PartOfSpeech)i)
				.ValueGeneratedNever();

			entity.HasOne(e => e.Anglicism)
				.WithMany(a => a.TranscriptAnglicisms)
				.HasForeignKey(e => e.Word);

			entity.HasOne(e => e.Transcript)
				.WithMany(t => t.TranscriptAnglicisms)
				.HasForeignKey(e => new { e.VideoId, e.LangCode });

		});

		builder.Entity<AnglicismContextWindow>(entity =>
		{
			entity.HasKey(e => new { e.Anglicism, e.Year, e.Category, e.ContextWord, e.GermanPos });

			entity.HasIndex(e => e.Anglicism);
			entity.HasIndex(e => e.Year);
			entity.HasIndex(e => e.Category);
			entity.HasIndex(e => e.ContextWord);
			entity.HasIndex(e => e.GermanPos);

			entity.Property(e => e.GermanPos)
				.HasConversion(p => (int)p, i => (TreeTagger.Wrapper.PartOfSpeech)i);

			entity.HasOne(e => e.AnglicismNavigation)
				.WithMany(a => a.AnglicismContextWindows)
				.HasForeignKey(e => e.Anglicism);
		});

		builder.Entity<WordPartOfSpeech>(entity =>
		{
			entity.HasKey(e => e.Word);

			entity.Property(e => e.GermanPartOfSpeech)
				.HasConversion(p => (int)p, i => (PartOfSpeech)i)
				.ValueGeneratedNever();

			entity.Property(e => e.EnglishPartOfSpeech)
				.HasConversion(p => (int)p, i => (PartOfSpeech)i)
				.ValueGeneratedNever();
		});

		base.OnModelCreating(builder);
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseSqlite(_connectionString);

			optionsBuilder.UseLazyLoadingProxies(_useLazyLoading);

			optionsBuilder.EnableSensitiveDataLogging();
		}
	}
}
