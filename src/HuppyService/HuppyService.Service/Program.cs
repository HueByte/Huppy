using HuppyService.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
//builder.Services.AddHttpClient();
builder.Services.AddHttpClient("GPT", httpclient =>
{
    var token = "";
    httpclient.BaseAddress = new Uri("https://api.openai.com/v1/engines/");
    httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

    //if (!string.IsNullOrEmpty(_appSettings?.GPT?.Orgranization))
    //    httpclient.DefaultRequestHeaders.Add("OpenAI-Organization", _appSettings?.GPT!.Orgranization);
});

var app = builder.Build();

app.UseHttpLogging();
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<GPTService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
