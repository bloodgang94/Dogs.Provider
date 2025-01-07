using Dogs;
using Dogs.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace DogsTests;

public class ContractsWithConsumerTests : IDisposable
{
    private readonly Uri _serverUri = new("http://localhost:5000");
    private readonly Mock<IDogsService> _dogsService;
    private readonly PactVerifier _pactVerifier;
    private bool _disposedValue; 

    private readonly IHostBuilder _serverBuilder;
    private IHost _server = null!;

    public ContractsWithConsumerTests(ITestOutputHelper outputHelper)
    {
        _dogsService = new Mock<IDogsService>();

        var config = new PactVerifierConfig
        {
            Outputters = new[] { new XunitOutput(outputHelper) }
        };

        _pactVerifier = new PactVerifier("backend", config);
        _serverBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls(_serverUri.ToString());
                webBuilder.UseStartup<Startup>();
            });
    }

    [Fact]
    public void Verify_RestDemoConsumerContacts()
    {
        _dogsService.Setup(x => x.GetDogs("Corgi"))
            .ReturnsAsync([new Dog("Corgi", 11)]);
        _dogsService.Setup(x => x.GetDogs("human"))
            .ReturnsAsync([]);

        _serverBuilder.ConfigureServices(services =>
            services.AddTransient(_ => _dogsService.Object));

        _server = _serverBuilder.Build();
        _server.StartAsync();

        _pactVerifier
            .WithHttpEndpoint(_serverUri)
            .WithPactBrokerSource(new Uri("http://localhost:9292"), options =>
            {
                options.BasicAuthentication("admin", "pass");
                options.PublishResults("1.0.1" + $" {Guid.NewGuid().ToString().Substring(0, 5)}");
            })
            .Verify();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _server.StopAsync().GetAwaiter().GetResult();
                _server.Dispose();
                _pactVerifier.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose() => Dispose(true);
}