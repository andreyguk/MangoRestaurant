using Mango.Services.IdentityNew;
using Mango.Services.IdentityNew.Data;
using Mango.Services.IdentityNew.Initializer;
using Mango.Services.IdentityNew.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    

    var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var _userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
    var _roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
    var a = scope.ServiceProvider.GetService<ApplicationDbContext>();
    var dbInitializer = new DbInitializer(a, _userManager, _roleManager);
    dbInitializer.Initialize();


    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (args.Contains("/seed"))
    {
        Log.Information("Seeding database...");
        SeedData.EnsureSeedData(app);
        Log.Information("Done seeding database. Exiting.");
        return;
    }

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}