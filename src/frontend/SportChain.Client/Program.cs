using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SportChain.Client;
using SportChain.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Cấu hình HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Đăng ký Api Service & Realtime SignalR Service
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<ICourtRealtimeService, CourtRealtimeService>();

await builder.Build().RunAsync();
