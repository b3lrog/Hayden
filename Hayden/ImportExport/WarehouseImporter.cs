using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hayden.Config;
using Hayden.Consumers.HaydenMysql.DB;
using Hayden.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.ComponentModel.DataAnnotations;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Drawing;
using System.Web;
using System.Diagnostics;

namespace Hayden.ImportExport;

public class WarehouseImporter : IImporter
{
	private SourceConfig sourceConfig;
	private ConsumerConfig consumerConfig;
	private DbContextOptions dbContextOptions;

	private ILogger Logger { get; } = SerilogManager.CreateSubLogger("Warehouse");

	public WarehouseImporter(SourceConfig sourceConfig, ConsumerConfig consumerConfig)
	{
		this.sourceConfig = sourceConfig;
		this.consumerConfig = consumerConfig;

		var optionsBuilder = new DbContextOptionsBuilder();

		optionsBuilder.UseMySql(sourceConfig.DbConnectionString,
			ServerVersion.AutoDetect(sourceConfig.DbConnectionString),
			y =>
			{
				y.CommandTimeout(86400);
			});

		dbContextOptions = optionsBuilder.Options;

		using var dbContext = GetDbContext();

		this.dbContext = GetDbContext();
	}

	private WarehouseDbContext GetDbContext() => new(dbContextOptions);

	public Task<string[]> GetBoardList()
	{
		Logger.Debug("GetBoardList");
		return dbContext.Archive.AsNoTracking()
			.Select(p => p.board)
			.Distinct()
			.ToArrayAsync();
	}

	public async IAsyncEnumerable<ThreadPointer> GetThreadList(string board, long? minId = null, long? maxId = null)
	{
		Logger.Debug("GetThreadList " + board.ToString() + " " + minId.ToString() + " " + maxId.ToString());

		var query = dbContext.Archive.AsNoTracking();

		if (minId.HasValue)
			query = query.Where(x => x.postnum >= minId);

		if (maxId.HasValue)
			query = query.Where(x => x.postnum <= maxId);

		var threadIds = query
			.AsNoTracking()
			.Where(x => x.board == board)
			.Where(x => x.thread == null)
			.Select(x => x.postnum);

		await foreach (var threadId in threadIds.AsAsyncEnumerable())
		{
			yield return new ThreadPointer(board, (ulong)threadId);
		}
	}

	public async Task<Thread> RetrieveThread(ThreadPointer pointer)
	{
		Logger.Debug("RetrieveThread " + pointer.Board + " " + pointer.ThreadId.ToString());

		Stopwatch sw = Stopwatch.StartNew();

		var threadPosts = await dbContext.Archive.AsNoTracking()
			.Where(x => x.board == pointer.Board && (x.thread == (int)pointer.ThreadId || x.postnum == (int)pointer.ThreadId))
			.OrderBy(x => x.postnum)
			.ToArrayAsync();

		sw.Stop();

		Logger.Debug("Query took " + sw.ElapsedMilliseconds.ToString() + " ms");

		if (threadPosts.Length == 0)
			return null;

		return new Thread {
			ThreadId = pointer.ThreadId,
			IsArchived = false,
			Title = threadPosts[0].subject,
			Posts = threadPosts.Select(x => new Post {
				PostNumber = (ulong)x.postnum,
				TimePosted = DateTimeOffset.FromUnixTimeSeconds(x.time),
				Author = HttpUtility.HtmlDecode(x.name),
				Tripcode = x.trip,
				Email = x.email,
				Subject = HttpUtility.HtmlDecode(x.subject)?.TrimAndNullify(),
				ContentRaw = null,
				ContentRendered = x.body,
				ContentType = ContentType.Vichan,
				IsDeleted = false,
				OriginalObject = x,
				Media = x.GetMedia(),
				AdditionalMetadata = new()
				{
					Capcode = x.capcode
				}
			}).ToArray()
		};
	}


	private WarehouseDbContext dbContext;

	public class WarehouseDbContext : DbContext
	{
		public WarehouseDbContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasCharSet("utf8mb4");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.LogTo(Console.WriteLine);
		}

		public DbSet<WarehouseDbPost> Archive { get; set; }

		[Table("archive")]
		public class WarehouseDbPost
		{
			[Key]
			public int id { get; set; }

			public int? thread { get; set; }

			public string subject { get; set; }
			public string email { get; set; }
			public string name { get; set; }
			public string trip { get; set; }
			public string capcode { get; set; }
			public string body { get; set; }

			public int time { get; set; }
			public string filename { get; set; }
			public string ext { get; set; }

			public int? replies { get; set; }
			public int? images { get; set; }

			public string board { get; set; }
			public int postnum { get; set; }

			public Media[] GetMedia()
			{
				if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(ext))
				{
					return Array.Empty<Media>();
				}
				List<Media> list = new();
				string[] filenames = filename.Split("/");
				string[] exts = ext.Split("/");
				if (filenames.Length != exts.Length)
					throw new Exception("Filenames and exts do not match");
				for (int i = 0; i < filenames.Length; i++)
				{
					list.Add(new Media {
						Filename = filenames[i],
						FileExtension = exts[i],
						IsSpoiler = null,
						Index = (byte)i,
					});
				}
				return list.ToArray();
			}
		}
	}
}
