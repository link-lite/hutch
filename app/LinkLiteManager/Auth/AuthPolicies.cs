using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace LinkLiteManager.Auth;

public static class AuthPolicies
{
  public static AuthorizationPolicy IsClientApp
    => new AuthorizationPolicyBuilder()
        .RequireAssertion(IsSameHost)
        .Build();

  public static AuthorizationPolicy IsAuthenticatedUser
    => new AuthorizationPolicyBuilder()
        .Combine(IsClientApp)
        .RequireAuthenticatedUser()
        .Build();

  private static readonly Func<AuthorizationHandlerContext, bool> IsSameHost =
    context =>
    {
      var request = ((DefaultHttpContext?)context.Resource)?.Request;

      var referer = request?.Headers.Referer.FirstOrDefault();
      if (referer is null) return false;

      // NOTE: this trims the port from the origin
      // which is slightly more lax (same protocol and host, rather than same origin)
      // the following regex is the complete origin: /^http(s?)://[^/\s]*/
      // both regexes also only work safely for a referer header:
      // URLs in other contexts might be formatted differently than the referer header specifies.
      var referringHost = Regex.Match(referer, @"^http(s?)://[^/:\s]*").Value;

      var requestHost = $"{request!.Scheme}://{request!.Host.Host}";

      return requestHost == referringHost;
    };
}
