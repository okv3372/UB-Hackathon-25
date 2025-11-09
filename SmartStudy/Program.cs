using SmartStudy.Components;
using SmartStudy.API.Services;
using SmartStudy.API.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<UsersService>();
// Bind model settings from configuration section "Model" and register SemanticKernelService
builder.Services.Configure<ModelSettings>(builder.Configuration.GetSection("Model"));
builder.Services.AddSingleton<SemanticKernelService>();
builder.Services.AddSingleton<ClassesService>();
builder.Services.AddSingleton<EnrollmentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
