namespace ECommerceMVC.Helpers
{
    public static class CookieHelper
    {
        public static string? GetCookie(HttpContext context, string key)
        {
            return context.Request.Cookies[key];
        }

        public static bool HasToken(HttpContext context)
        {
            return !string.IsNullOrEmpty(GetCookie(context, "suipos_ac"));
        }

        public static void SetCookie(HttpContext context, string key, string value, int? expireTimeInMinutes, bool httpOnly = true)
        {
            var options = new CookieOptions
            {
                HttpOnly = httpOnly, // ? Allow override for client-side access
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expireTimeInMinutes.HasValue
                    ? DateTime.UtcNow.AddMinutes(expireTimeInMinutes.Value)
                    : null
            };
            context.Response.Cookies.Append(key, value, options);
        }


        public static void RemoveCookie(HttpContext context, string key)
        {
            context.Response.Cookies.Delete(key);
        }
    }
}
