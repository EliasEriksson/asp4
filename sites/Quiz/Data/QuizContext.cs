using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Quiz.Data
{   
    public class QuizContext : IdentityDbContext
    {
        public QuizContext(DbContextOptions<QuizContext> options) : base(options)
        {
        }
        
        /**
         * exception to throw if the .credentials.json is missing
         */
        public class MissingCredentials : Exception
        {
        }

        /**
         * class representing contents of .credentials.json
         */
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

        /**
         * loads the .credentials.json file into an object
         */
        private static DbCredentials Load()
        {
            var credentials = JsonConvert.DeserializeObject<DbCredentials>(System.IO.File.ReadAllText(CredentialsFile));
            if (credentials == null)
            {
                throw new MissingCredentials();
            }

            return credentials;
        }
        
        /**
         * configure context to use postgres
         *
         * logs into the postgres server using the credentials in
         * .credentials.json
         */
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
        
        public DbSet<Models.ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Models.Quiz> Quizzes { get; set; }
        public DbSet<Models.QuizResult> QuizResults { get; set; }
        
        public DbSet<Models.MissingWord> MissingWords { get; set; }
    }
}