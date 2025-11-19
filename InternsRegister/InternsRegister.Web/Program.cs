using InternsRegister.Application.Interfaces;
using InternsRegister.Application.Services;
using InternsRegister.Infrastructure.Hubs;
using InternsRegister.Persistence;
using InternsRegister.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddDbContext<InternsRegisterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);

builder.Services.AddScoped<IInternService, InternService>();
builder.Services.AddScoped<IDirectionService, DirectionService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddSignalR();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<InternsRegisterDbContext>();

dbContext.Database.Migrate();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<InternsHub>("/internshub");

app.Run();
