﻿// <auto-generated />
using System;
using GYSTCorpus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GYSTCorpus.Migrations
{
    [DbContext(typeof(TranscriptsContext))]
    [Migration("20250130062533_MigratePartOfSpeechPart1")]
    partial class MigratePartOfSpeechPart1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("GYSTCorpus.Database.Anglicism", b =>
                {
                    b.Property<string>("Word")
                        .HasColumnType("TEXT");

                    b.Property<string>("BaseWord")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EnglishPos")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("EnglishPosTemp")
                        .HasColumnType("INTEGER");

                    b.Property<float>("Entropy")
                        .HasColumnType("REAL");

                    b.Property<string>("GermanPos")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("GermanPosTemp")
                        .HasColumnType("INTEGER");

                    b.HasKey("Word");

                    b.HasIndex("BaseWord");

                    b.ToTable("Anglicisms");
                });

            modelBuilder.Entity("GYSTCorpus.Database.AnglicismContextWindow", b =>
                {
                    b.Property<string>("Anglicism")
                        .HasColumnType("TEXT");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Category")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContextWord")
                        .HasColumnType("TEXT");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.HasKey("Anglicism", "Year", "Category", "ContextWord");

                    b.HasIndex("Anglicism");

                    b.HasIndex("Category");

                    b.HasIndex("ContextWord");

                    b.HasIndex("Year");

                    b.ToTable("AnglicismContextWindows");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Channel", b =>
                {
                    b.Property<string>("ChannelId")
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    b.Property<string>("PlaylistId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "uploadsPlaylist");

                    b.Property<DateTime>("PublishedAt")
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "created");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "title");

                    b.Property<int>("VideoCount")
                        .HasColumnType("INTEGER")
                        .HasAnnotation("Relational:JsonPropertyName", "videoCount");

                    b.HasKey("ChannelId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Transcript", b =>
                {
                    b.Property<string>("VideoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LangCode")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsGenerated")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LanguageName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("VideoId", "LangCode");

                    b.HasIndex("VideoId");

                    b.ToTable("Transcripts");
                });

            modelBuilder.Entity("GYSTCorpus.Database.TranscriptAnglicism", b =>
                {
                    b.Property<string>("VideoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LangCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Word")
                        .HasColumnType("TEXT");

                    b.Property<int>("TranscriptIndex")
                        .HasColumnType("INTEGER");

                    b.HasKey("VideoId", "LangCode", "Word", "TranscriptIndex");

                    b.HasIndex("VideoId");

                    b.HasIndex("Word");

                    b.ToTable("TranscriptAnglicism");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Video", b =>
                {
                    b.Property<string>("VideoId")
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "id");

                    b.Property<bool>("CaptionsEnabled")
                        .HasColumnType("INTEGER")
                        .HasAnnotation("Relational:JsonPropertyName", "captionsEnabled");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER")
                        .HasAnnotation("Relational:JsonPropertyName", "categoryId");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "channelId");

                    b.Property<string>("LiveStreamStatus")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "live");

                    b.Property<DateTime>("PublishedAt")
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "publishedAt");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasAnnotation("Relational:JsonPropertyName", "title");

                    b.HasKey("VideoId");

                    b.HasIndex("ChannelId");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("GYSTCorpus.Database.WordPartOfSpeech", b =>
                {
                    b.Property<string>("Word")
                        .HasColumnType("TEXT");

                    b.Property<int>("EnglishPartOfSpeech")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GermanPartOfSpeech")
                        .HasColumnType("INTEGER");

                    b.HasKey("Word");

                    b.ToTable("WordPartOfSpeech");
                });

            modelBuilder.Entity("GYSTCorpus.Database.AnglicismContextWindow", b =>
                {
                    b.HasOne("GYSTCorpus.Database.Anglicism", "AnglicismNavigation")
                        .WithMany("AnglicismContextWindows")
                        .HasForeignKey("Anglicism")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AnglicismNavigation");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Transcript", b =>
                {
                    b.HasOne("GYSTCorpus.Database.Video", "Video")
                        .WithMany("Transcripts")
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Video");
                });

            modelBuilder.Entity("GYSTCorpus.Database.TranscriptAnglicism", b =>
                {
                    b.HasOne("GYSTCorpus.Database.Anglicism", "Anglicism")
                        .WithMany("TranscriptAnglicisms")
                        .HasForeignKey("Word")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GYSTCorpus.Database.Transcript", "Transcript")
                        .WithMany("TranscriptAnglicisms")
                        .HasForeignKey("VideoId", "LangCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Anglicism");

                    b.Navigation("Transcript");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Video", b =>
                {
                    b.HasOne("GYSTCorpus.Database.Channel", "Channel")
                        .WithMany("Videos")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Anglicism", b =>
                {
                    b.Navigation("AnglicismContextWindows");

                    b.Navigation("TranscriptAnglicisms");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Channel", b =>
                {
                    b.Navigation("Videos");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Transcript", b =>
                {
                    b.Navigation("TranscriptAnglicisms");
                });

            modelBuilder.Entity("GYSTCorpus.Database.Video", b =>
                {
                    b.Navigation("Transcripts");
                });
#pragma warning restore 612, 618
        }
    }
}
