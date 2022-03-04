using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Inspection.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public class MissingCredentials : Exception
    {
    }

    private class DbCredentials
    {
        public string? Role { get; init; }
        public string? Pwd { get; init; }
        public string? Db { get; init; }
        public string? Host { get; init; }
    }

    private static readonly string CredentialsFile = Path.Combine(
        Environment.CurrentDirectory, ".credentials.json"
    );

    private static DbCredentials Load()
    {
        var credentials = JsonConvert.DeserializeObject<DbCredentials>(System.IO.File.ReadAllText(CredentialsFile));
        if (credentials == null)
        {
            throw new MissingCredentials();
        }

        return credentials;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var credentials = Load();
        optionsBuilder.UseNpgsql(
            $"Host={credentials.Host};" +
            $"Username={credentials.Role};" +
            $"Password={credentials.Pwd};" +
            $"Database={credentials.Db}"
        );
    }
}