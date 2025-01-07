using Dogs.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dogs;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/dogs", async ([FromQuery(Name = "breed")] string breed, IDogsService dogsService) =>
                {
                    var result = await dogsService.GetDogs(breed);
                    if (result.Length != 0) return Results.Ok(result);
                    return Results.NotFound(new { error = "not found!" });
                })
                .WithName("Dogs")
                .WithOpenApi();
        });


    }
}