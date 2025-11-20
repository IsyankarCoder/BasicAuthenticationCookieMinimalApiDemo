using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Claims;

public class BasicAuth
         : AuthenticationHandler<AuthenticationSchemeOptions>
{
    //[Obsolete]
    public BasicAuth()
          :base(new OptionsMonitorFake(),
                      new LoggerFactory()
                     ,UrlEncoder.Default
                     )
    {
        //Context=httpContext;
    }
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
       var header = Request.Headers["Authorization"].ToString();
       if(string.IsNullOrEmpty(header))
          return  Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(header);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter!)).Split(':',2);
            var username = credentials[0];
            var password = credentials[1];

            if(username!="admin" || password!="pas123")
              return Task.FromResult(AuthenticateResult.Fail("Invalid Credentials"));
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,username),
                new Claim(ClaimTypes.Role,"Admin")
            };

            var identity = new ClaimsIdentity(claims,"BasicAuthentication");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal,"BasicAuthentication");

            return Task.FromResult(AuthenticateResult.Success(ticket));


        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Header"));
        }

    }
}

