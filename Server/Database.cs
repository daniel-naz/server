
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;

public static partial class Utils
{
    public const string ConnectionString = "Data Source={Path}";
}

public abstract class Database : DbContext
{
    public string FileName { get; }
    public string Path { get; }

    public Database(string filename)
    {
        var folder = Directory.GetCurrentDirectory();
        Path = System.IO.Path.Join(folder, $"{filename}.db");
        FileName = filename;

        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(Utils.ConnectionString.Replace("{Path}", Path));
    }

    public virtual object[] GetInOrderConstructorArgs()
    {
        return [FileName];
    }
}

public class DatabaseFactory<DB> where DB : Database
{
    private Database db;

    public DatabaseFactory(Database database)
    {
        db = database;
    }

    public DB CreateInstance()
    {
        var result = Activator.CreateInstance(typeof(DB), db.GetInOrderConstructorArgs()) as DB;

        return result ?? throw new Exception("Couldn't create database :" +
            $"\n\tType : {typeof(DB)}" +
            $"\n\tFile name : {db.FileName}" +
            $"\n\tPath : {db.Path}" +
            $"\n\tMake sure to override GetInOrderConstructorArgs().");
    }
}