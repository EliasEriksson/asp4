using Microsoft.AspNetCore.Identity;
using Quiz.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuizContext>();
builder.Services.AddDefaultIdentity<IdentityUser>(
    options => options.SignIn.RequireConfirmedAccount = false
).AddEntityFrameworkStores<QuizContext>();
builder.Services.AddControllersWithViews();

const string rootUrl = "aspdotnet/moment4";

var app = builder.Build();
app.UsePathBase($"/{rootUrl}");
app.UseRouting();
app.UseAuthorization();
app.UseHttpsRedirection();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        "default", "/{controller}/{action}/{id?}",
        new {controller = "Root", action="Index"}
    );
});
app.UseAuthentication();

app.MapRazorPages();
app.UseStaticFiles($"/static");


app.Run();