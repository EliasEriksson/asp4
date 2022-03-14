using Microsoft.AspNetCore.Identity;
using Quiz.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// add db context
builder.Services.AddDbContext<QuizContext>();
builder.Services.AddDefaultIdentity<IdentityUser>(
    options => options.SignIn.RequireConfirmedAccount = false // no mail server configured 
).AddEntityFrameworkStores<QuizContext>();
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    // ignores circular references in models with navigation properties referring to each other
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// base url where the site will be hosted behind proxy
const string rootUrl = "aspdotnet/moment4";

var app = builder.Build();

// sets the path base to the root url
// allows the site to be served from either / or /aspdotnet/moment4 in development
app.UsePathBase($"/{rootUrl}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// maps static files to /static (will be appended to the PathBase)
app.UseStaticFiles($"/static");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        "default", "/{controller}/{action}/{id?}",
        new {controller = "Root", action="Index"}
    );
});

app.MapRazorPages();

app.Run();