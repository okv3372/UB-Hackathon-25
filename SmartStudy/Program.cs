using SmartStudy.Components;
using SmartStudy.API.Services;
using SmartStudy.API.SemanticKernel;
using SmartStudy.Services;

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
// Breadcrumb helper service
builder.Services.AddScoped<SmartStudy.Services.BreadcrumbService>();
builder.Services.AddSingleton<AssignmentService>();

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

// Perform a lightweight backfill to ensure any historical assignments with PracticeSetIds
// have corresponding PracticeSet records so UI pages don't show 'Failed to load test data'.
try
{
    var created = await PracticeSetsService.BackfillMissingPracticeSetsAsync();
    if (created > 0)
    {
        Console.WriteLine($"[Startup] Backfilled {created} missing practice set(s).");
    }
}
catch (Exception ex)
{
    Console.WriteLine("[Startup] Practice set backfill failed: " + ex.Message);
}

app.Run();
