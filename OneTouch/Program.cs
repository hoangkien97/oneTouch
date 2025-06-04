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

// Add DbContext
builder.Services.AddDbContext<OneTouchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddSession();

// Register Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<ImageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

app.UseSession();