namespace LaughAndGroan.Api.Features
{
    using System.Linq;
    using System.Security.Claims;

    public static class Extensions
    {
        public static string ExtractUserId(this ClaimsPrincipal principal) =>
            principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
    }
}
