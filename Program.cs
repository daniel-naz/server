
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

class MyServer : Server
{
    readonly MyDatabase db;
    readonly DatabaseFactory<MyDatabase> factory;

    public MyServer()
    {
        db = new MyDatabase("database");
        factory = new DatabaseFactory<MyDatabase>(db);
    }

    public override Task HandleConnectionAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        string filePath = request.Url!.AbsolutePath.TrimStart('/');
        string fileExtension = Path.GetExtension(filePath).ToLower();

        var database = factory.CreateInstance();

        if (filePath == "Zanoh")
        {
            ClimbingSite? site = GetSite(database, 1);
            if (site == null) return Task.CompletedTask;

            var json = JsonConvert.SerializeObject(site);

            // Set the response content type to JSON
            response.SendJson(json);
        }

        else if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
        {
            response.SendImage(filePath);
        }

        else
        {
            response.SendFile($"Pages/{filePath}");
        }

        return Task.CompletedTask;
    }

    static ClimbingSite? GetSite(MyDatabase db, int id)
    {
        return db.ClimbingSites
            .Include(s => s.ClimbingWalls)
            .ThenInclude(w => w.ClimbingRoutes)
            .FirstOrDefault(s => s.Id == id);
    }
}

class MyDatabase : Database
{
    public DbSet<ClimbingSite> ClimbingSites { get; set; }

    public MyDatabase(string filename) : base(filename)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One ClimbingSite -> Many ClimbingWalls
        modelBuilder.Entity<ClimbingWall>()
            .HasOne(w => w.Site)
            .WithMany(s => s.ClimbingWalls)
            .HasForeignKey(w => w.SiteId);

        // One ClimbingWall -> Many ClimbingRoutes
        modelBuilder.Entity<ClimbingRoute>()
            .HasOne(r => r.ClimbingWall)
            .WithMany(w => w.ClimbingRoutes)
            .HasForeignKey(r => r.ClimbingWallId);
    }
}

public class ClimbingSite
{
    [JsonProperty]
    public int Id { get; set; }
    [JsonProperty]
    public string? Name { get; set; }
    [JsonProperty]
    public ICollection<ClimbingWall> ClimbingWalls { get; set; } = new List<ClimbingWall>(); // One-to-Many
}

public class ClimbingWall
{
    [JsonProperty]
    public int Id { get; set; }

    [JsonProperty]
    public string? ImgUrl { get; set; }

    // Foreign key for ClimbingSite
    [JsonIgnore]
    public int SiteId { get; set; }
    [JsonIgnore]
    public ClimbingSite? Site { get; set; } // Navigation property

    [JsonProperty]
    public ICollection<ClimbingRoute> ClimbingRoutes { get; set; } = new List<ClimbingRoute>(); // One-to-Many
}

public class ClimbingRoute
{
    [JsonProperty]
    public int Id { get; set; }
    [JsonProperty]
    public int Grade { get; set; }

    // Foreign key for ClimbingWall
    [JsonIgnore]
    public int ClimbingWallId { get; set; }
    [JsonIgnore]
    public ClimbingWall? ClimbingWall { get; set; } // Navigation property
}

class Program
{
    static void Main()
    {
        Server server = new MyServer();
        server.Start();

        while (true) ;
    }
}