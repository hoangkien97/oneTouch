using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using OneTouch.Repositories;
using OneTouch.Repositories.Interfaces;
using OneTouch.Services;
using OneTouch.Services.Interfaces;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<OneTouchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddSession(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, VonageSmsService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IVietQrService, VietQrService>();
//builder.Services.AddHttpClient<ChatbotService>();
//builder.Services.AddScoped<ChatbotService>();



builder.Services.AddHttpClient("Gemini", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60); // Increase timeout for AI responses
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
    );

    // Optional: Add User-Agent
    client.DefaultRequestHeaders.UserAgent.ParseAdd("OneTouch-Medical/1.0");
});

// Add general HttpClient for test controller
builder.Services.AddHttpClient();
builder.Services.AddScoped<DoctorScheduleService>();
builder.Services.AddHostedService<CronJobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Always use HTTPS redirection, even in development
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.Run();