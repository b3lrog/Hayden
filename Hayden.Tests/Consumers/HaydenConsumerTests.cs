﻿using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Hayden.Config;
using Hayden.Consumers;
using Hayden.Consumers.HaydenMysql.DB;
using Hayden.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Hayden.Tests.Consumers
{
	internal class HaydenConsumerTests
	{
		private static DbContextOptions<HaydenDbContext> CreateMemoryContextOptions(bool createTestBoard = true)
		{
			var options = new DbContextOptionsBuilder<HaydenDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
				.Options;

			if (createTestBoard)
			{
				using var context = new HaydenDbContext(options);

				context.Boards.Add(new DBBoard { Id = 1, Category = "Test", ShortName = "test", LongName = "Testing Board" });
				context.SaveChanges();
			}

			return options;
		}

		private static (Thread, ThreadPointer) GenerateThread()
		{
			var thread = new Thread
			{
				ThreadId = 123,
				Title = "Thread Title",
				IsArchived = false,
				Posts = new[]
				{
					new Post
					{
						PostNumber = 123,
						TimePosted = new DateTimeOffset(2020, 02, 02, 02, 02, 02, TimeSpan.Zero),
						Author = "Big",
						Tripcode = "!chungus",
						ContentRaw = "My first post",
						ContentRendered = "<b>My first post</b>",
						Email = "email@example.com",
						IsDeleted = false,
						ContentType = ContentType.Yotsuba, // non-zero
						Media = new[]
						{
							new Media
							{
								Filename = "filename1",
								FileExtension = "jpg",
								ThumbnailExtension = "jpg",
								FileUrl = "test://my.com/test-file1.jpg",
								ThumbnailUrl = "test://my.com/test-file1-thumb.jpg",
								IsSpoiler = true,
								IsDeleted = false,
								Index = 0,
								AdditionalMetadata = new JObject()
								{
									["test-property"] = true
								}
							},
							new Media
							{
								Filename = "file2",
								FileExtension = "png",
								ThumbnailExtension = "webp",
								FileUrl = "test://my.com/test-file2.png",
								ThumbnailUrl = "test://my.com/test-file2.webp",
								IsSpoiler = false,
								IsDeleted = false,
								Index = 1,
								AdditionalMetadata = new JObject()
								{
									["test-property"] = false
								}
							},
						}
					},
					new Post
					{
						PostNumber = 124,
						TimePosted = new DateTimeOffset(2020, 02, 02, 03, 03, 03, TimeSpan.Zero),
						Author = null,
						Tripcode = null,
						ContentRaw = "Reply",
						ContentRendered = "Reply",
						Email = null,
						IsDeleted = false,
						ContentType = ContentType.Yotsuba, // non-zero
						Media = Array.Empty<Media>()
					}
				}
			};
			return (thread, new ThreadPointer("test", 123));
		}

		private const string DownloadPath = @"C:\temp\hayden_test";

		private async Task<TestHaydenConsumer> CreateHaydenConsumerAsync(DbContextOptions<HaydenDbContext> options, IFileSystem fileSystem)
		{
			var consumer = new TestHaydenConsumer(new ConsumerConfig
			{
				DownloadLocation = DownloadPath,
				FullImagesEnabled = true,
				ThumbnailsEnabled = true
			}, () => new HaydenDbContext(options), fileSystem);

			await consumer.InitializeAsync();

			return consumer;
		}

		private void AssertDataIsSame(Thread thread, DBPost dbPost)
		{
			AssertDataIsSame(thread.Posts.Single(x => x.PostNumber == dbPost.PostId), dbPost);
		}

		private void AssertDataIsSame(Post post, DBPost dbPost)
		{
			Assert.AreEqual(post.PostNumber, dbPost.PostId);
			Assert.AreEqual(post.ContentRendered, dbPost.ContentHtml);
			Assert.AreEqual(post.ContentRaw, dbPost.ContentRaw);
			Assert.AreEqual(post.ContentType, dbPost.ContentType);
			Assert.AreEqual(post.Author, dbPost.Author);
			Assert.AreEqual(post.Tripcode, dbPost.Tripcode);
			Assert.AreEqual(post.Email, dbPost.Email);
			Assert.AreEqual(post.TimePosted.UtcDateTime, dbPost.DateTime);
			Assert.AreEqual(post.IsDeleted, dbPost.IsDeleted);

			if (post.AdditionalMetadata == null || post.AdditionalMetadata.Count == 0)
				Assert.AreEqual(null, dbPost.AdditionalMetadata);
			else
				Assert.IsTrue(JToken.DeepEquals(post.AdditionalMetadata, JToken.Parse(dbPost.AdditionalMetadata)));
		}

		private void AssertDataIsSame(Post post, QueuedImageDownload queuedImageDownload)
		{
			AssertDataIsSame(post.Media.Single(x => x.FileUrl == queuedImageDownload.FullImageUri.AbsoluteUri), queuedImageDownload);
		}

		private void AssertDataIsSame(Media media, QueuedImageDownload queuedImageDownload)
		{
			Assert.AreEqual(media.FileUrl, queuedImageDownload.FullImageUri.AbsoluteUri);
			Assert.AreEqual(media.ThumbnailUrl, queuedImageDownload.ThumbnailImageUri.AbsoluteUri);
			// TODO: properties
		}

		private void AssertDataIsSame(Post post, Media media, DBFileMapping[] dbFileMappings)
		{
			AssertDataIsSame(media, dbFileMappings.Single(x => x.PostId == post.PostNumber && x.Index == media.Index));
		}

		private void AssertDataIsSame(Media media, DBFileMapping dbFileMapping)
		{
			Assert.AreEqual(media.Filename, dbFileMapping.Filename);
			Assert.AreEqual(media.Index, dbFileMapping.Index);

			// we want the file id to be null here as we haven't actually downloaded the file yet
			Assert.AreEqual(null, dbFileMapping.FileId);
			
			// TODO: properties
		}

		[Test]
		public async Task IngestNewPostsTest()
		{
			var options = CreateMemoryContextOptions();
			var mockFilesystem = new MockFileSystem();

			using var consumer = await CreateHaydenConsumerAsync(options, mockFilesystem);

			var (thread, threadPointer) = GenerateThread();

			var threadTracker = TrackedThread.StartTrackingThread(consumer.CalculateHash);
			var threadUpdate = threadTracker.ProcessThreadUpdates(threadPointer, thread);
			threadUpdate.IsNewThread = true;

			var pendingDownloads = await consumer.ConsumeThread(threadUpdate);

			Assert.AreEqual(2, pendingDownloads.Count);
			AssertDataIsSame(thread.Posts[0], pendingDownloads[0]);
			AssertDataIsSame(thread.Posts[0], pendingDownloads[1]);


			await using (var context = new HaydenDbContext(options))
			{
				var posts = context.Posts.ToList();

				Assert.AreEqual(2, posts.Count);
				AssertDataIsSame(thread, posts[0]);
				AssertDataIsSame(thread, posts[1]);
				
				var mappings = context.FileMappings.ToArray();

				Assert.AreEqual(2, mappings.Length);
				AssertDataIsSame(thread.Posts[0], thread.Posts[0].Media[0], mappings);
				AssertDataIsSame(thread.Posts[0], thread.Posts[0].Media[1], mappings);
				
				var threads = context.Threads.ToArray();

				Assert.AreEqual(1, threads.Length);
				Assert.AreEqual(thread.ThreadId, threads[0].ThreadId);
				Assert.AreEqual(thread.IsArchived, threads[0].IsArchived);
				Assert.AreEqual(thread.Title, threads[0].Title);
				Assert.AreEqual(null, threads[0].AdditionalMetadata);
				Assert.AreEqual(false, threads[0].IsDeleted);
			}

			const string file1path = @"C:\temp\temp1.jpg";
			const string file1Thumbpath = @"C:\temp\temp1-thumb.jpg";
			const string file2path = @"C:\temp\temp2.png";
			const string file2Thumbpath = @"C:\temp\temp2-thumb.webp";

			mockFilesystem.AddFileFromEmbeddedResource(file1path, typeof(HaydenConsumerTests).Assembly, "Hayden.Tests.TestImages.1.jpg");
			mockFilesystem.AddFileFromEmbeddedResource(file1Thumbpath, typeof(HaydenConsumerTests).Assembly, "Hayden.Tests.TestImages.1-thumb.jpg");

			mockFilesystem.AddFileFromEmbeddedResource(file2path, typeof(HaydenConsumerTests).Assembly, "Hayden.Tests.TestImages.2.png");
			mockFilesystem.AddFileFromEmbeddedResource(file2Thumbpath, typeof(HaydenConsumerTests).Assembly, "Hayden.Tests.TestImages.2-thumb.webp");
			

			async Task testFile(Media media)
			{
				await using var context = new HaydenDbContext(options);

				var fileMapping = context.FileMappings.Single(x => x.BoardId == 1
			                                                   && x.PostId == thread.Posts[0].PostNumber
			                                                   && x.Index == media.Index);

				Assert.IsNotNull(fileMapping.FileId);

				var file = context.Files.Single(x => x.Id == fileMapping.FileId && x.BoardId == fileMapping.BoardId);

				Assert.AreEqual(media.FileExtension, file.Extension);
				Assert.AreEqual(media.ThumbnailExtension, file.ThumbnailExtension);

				// TODO: fix magick/ffprobe not working correctly in tests due to mocked file
				//Assert.IsNotNull(file.ImageHeight);
				//Assert.IsNotNull(file.ImageWidth);

				Assert.IsFalse(file.FileBanned);
				Assert.IsTrue(file.FileExists);
			}

			await consumer.ProcessFileDownload(pendingDownloads.First(x =>
				x.FullImageUri.AbsoluteUri == thread.Posts[0].Media[0].FileUrl), file1path, file1Thumbpath);

			await testFile(thread.Posts[0].Media[0]);

			await consumer.ProcessFileDownload(pendingDownloads.First(x =>
				x.FullImageUri.AbsoluteUri == thread.Posts[0].Media[1].FileUrl), file2path, file2Thumbpath);

			await testFile(thread.Posts[0].Media[1]);

			var expectedPathList = new[]
			{
				@"C:\temp\hayden_test\test\image\g1rpbse4hsaikw0mac17fi5qn76jylm5zsgne31vf6s366n0j3.jpg",
				@"C:\temp\hayden_test\test\thumb\g1rpbse4hsaikw0mac17fi5qn76jylm5zsgne31vf6s366n0j3.jpg",
				@"C:\temp\hayden_test\test\image\y8cdt4omqc6v5opfkx7askwvnd2pru5im985aa8mbh61rzaig2.png",
				@"C:\temp\hayden_test\test\thumb\y8cdt4omqc6v5opfkx7askwvnd2pru5im985aa8mbh61rzaig2.webp"
			};
			CollectionAssert.AreEquivalent(expectedPathList, mockFilesystem.AllFiles, "Unexpected changes were made to the filesystem.");


			//mockFilesystem.AddFile("./temp/file1.jpg", new MockFileData(File.ReadAllBytes()));

			//await consumer.ProcessFileDownload(pendingDownloads[0], )
		}
	}

	internal class TestHaydenConsumer : HaydenMysqlThreadConsumer
	{
		private Func<HaydenDbContext> GetContext { get; set; }

		public TestHaydenConsumer(ConsumerConfig config, Func<HaydenDbContext> getContext, IFileSystem fileSystem) : base(config, fileSystem)
		{
			GetContext = getContext;
		}

		protected override HaydenDbContext GetDBContext() => GetContext();
	}
}