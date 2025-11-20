using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;


//Bu sınıf sadece OptionsMonitor ihtiyacını doldurmak için
public class OptionsMonitorFake() : IOptionsMonitor<AuthenticationSchemeOptions>
{
    public AuthenticationSchemeOptions CurrentValue =>new();

    public AuthenticationSchemeOptions Get(string? name)
    {
        return new();
    }

    public IDisposable? OnChange(Action<AuthenticationSchemeOptions, string?> listener)
    {
         return null!;
    }
}