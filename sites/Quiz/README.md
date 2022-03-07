# run in dev
1. Install dependencies
   * Microsoft.EntityFrameworkCore.Design
   * Microsoft.VisualStudio.Web.CodeGeneration.Design
   * Npgsql.EntityFrameworkCore.PostgreSQL
   * Newtonsoft.Json
   * Microsoft.EntityFrameworkCore.SqlServer
   * Microsoft.VisualStudio.Web.CodeGeneration.Design
2. Add `.credentials.json` in project root (`asp4/Quiz/`)
   ```json
   {
       "Role": "role",
       "Pwd": "pdw",
       "Db": "db",
       "Host": "host"
   }
   ```
3. `dotnet ef migrations add init`
4. `dotnet ef database update`
5. `dotnet run`