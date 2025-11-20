using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Authentication ve Cookie Ekle
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme=CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme=CookieAuthenticationDefaults.AuthenticationScheme; 
})
.AddCookie()
.AddScheme<AuthenticationSchemeOptions,BasicAuth>("BasicAuthentication",null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

//Public EndPoint
app.MapGet("/",()=>"Public endpoint (no auth)");

//Basic login endpoint -cookie üretir
app.MapGet("/login",async (HttpContext context) =>
{
   var header = context.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(header))
    {
        context.Response.Headers["WWW-Authenticate"] ="Basic realm = \"login \" ";
        return Results.Unauthorized();
    }

    //var handler = new BasicAuth();
    //var result = await handler.AuthenticateAsync();

     // Use the registered authentication handler via the authentication system
    var result = await context.AuthenticateAsync("BasicAuthentication");
    if(!result.Succeeded)
      return Results.Unauthorized();

    if(!result.Succeeded)
      return Results.Unauthorized();

    //Kullanıcı Doğrulandı , şimdi cookie oluştur
    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        result.Principal!,
        new AuthenticationProperties()
        {
            IsPersistent=true,
            ExpiresUtc= DateTime.UtcNow.AddMinutes(5)
        }
    );

    return Results.Ok("Basic ile doğrulandin, artik cookie set edildi ");

});

app.MapGet("/secure",[Authorize] (ClaimsPrincipal user) =>
{ 
     return $"Hoş geldin {user.Identity?.Name}  artik cookie üzerinden doğrulandin";
});

app.MapGet("/logout",[Authorize] async (HttpContext context,ClaimsPrincipal principal)=>{
       await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok("Çikiş yapildi");
});

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
