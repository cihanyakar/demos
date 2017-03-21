using CodeAnalyze.Business;

namespace CodeAnalyze.Extensions
{
    public static class Extensions
    {
        public static string AddToken(this string url) => url + (url.Contains("?") ? "&" : "?") + "access_token=" + Constants.AccessToken;
    }
}
