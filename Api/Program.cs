using System.Text;
using Application.Interfaces;
using Application.Users;
using FluentValidation.AspNetCore;
using Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connString =  builder.Configuration.GetConnectionString("PostGreSql");
// Add services to the container.

builder.Services.AddDbContext<DataContext>(options => { 
    options.UseNpgsql(connString);
});
builder.Services.AddControllers(
config=>{
var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
    .Build();
config.Filters.Add(new AuthorizeFilter(policy));

}
).AddFluentValidation(cfg =>
    cfg.RegisterValidatorsFromAssemblyContaining<Login.LoginCommand>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(typeof(ListUsers.ListUserQuery).Assembly);
builder.Services.AddAutoMapper(typeof(ListUsers.ListUsersQueryHandler));
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
var secretKey =  builder.Configuration.GetSection("Token:TokenSecret").Value;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateAudience = false,
            ValidateIssuer = false
        };
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCors", builder =>
        builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyCors");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();