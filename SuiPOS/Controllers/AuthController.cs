using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Mvc;
using SuiPOS.DTOs.Auth;
using SuiPOS.Services.Interfaces;

namespace SuiPOS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly JwtHelper _jwtHelper;

        public AuthController(IAuthService authService, JwtHelper jwtHelper)
        {
            _authService = authService;
            _jwtHelper = jwtHelper;
        }

        private bool IsUserAuthenticated()
        {
            var token = CookieHelper.GetCookie(HttpContext, "suipos_ac");
            return !string.IsNullOrEmpty(token) && _jwtHelper.ValidateToken(token) != null;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (IsUserAuthenticated()) return RedirectToAction("Index", "POS");
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return View(loginDto);

            var staff = await _authService.LoginAsync(loginDto);
            if (staff == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(loginDto);
            }

            var accessToken = _jwtHelper.GenerateAccessToken(staff.Username, staff.FullName, staff.Role.Id);
            CookieHelper.SetCookie(HttpContext, "suipos_ac", accessToken, 30, httpOnly: true);

            int persistentExpiry = 10080;

            if (loginDto.RememberMe)
            {
                var refreshToken = _jwtHelper.GenerateRefreshToken();
                CookieHelper.SetCookie(HttpContext, "suipos_rf", refreshToken, persistentExpiry, httpOnly: true);
            }

            var staffInfo = new
            {
                id = staff.Id,
                username = staff.Username,
                name = staff.FullName,
                role = staff.Role?.Name ?? "Staff"
            };
            string staffInfoJson = System.Text.Json.JsonSerializer.Serialize(staffInfo);
            CookieHelper.SetCookie(HttpContext, "staff_data", staffInfoJson, persistentExpiry, httpOnly: false);

            TempData["SuccessMessage"] = $"Chào mừng {staff.FullName}!";
            return RedirectToAction("Index", "POS");
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (IsUserAuthenticated())
            {
                TempData["InfoMessage"] = "Bạn đã đăng nhập rồi, không cần đăng ký tài khoản mới.";
                return RedirectToAction("Index", "POS");
            }
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return View(registerDto);

            var (success, message) = await _authService.RegisterAsync(registerDto);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(registerDto);
            }

            return RedirectToAction("Login", new { username = registerDto.Username });
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            CookieHelper.RemoveCookie(HttpContext, "suipos_ac");
            CookieHelper.RemoveCookie(HttpContext, "suipos_rf");
            CookieHelper.RemoveCookie(HttpContext, "staff_username");
            CookieHelper.RemoveCookie(HttpContext, "staff_data");
            CookieHelper.RemoveCookie(HttpContext, "staff_id");
            CookieHelper.RemoveCookie(HttpContext, "staff_name");
            CookieHelper.RemoveCookie(HttpContext, "staff_role");

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> CheckToken()
        {
            var accessToken = CookieHelper.GetCookie(HttpContext, "suipos_ac");
            if (!string.IsNullOrEmpty(accessToken) && _jwtHelper.ValidateToken(accessToken) != null)
            {
                return Ok(new { success = true, message = "Access token valid" });
            }

            var result = await TryRenewAccessToken();

            if (result.Success)
            {
                return Ok(new { success = true, message = "Access token renewed" });
            }

            return Unauthorized(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await TryRenewAccessToken();
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        private async Task<(bool Success, string Message)> TryRenewAccessToken()
        {
            var refreshToken = CookieHelper.GetCookie(HttpContext, "suipos_rf");
            var staffDataJson = CookieHelper.GetCookie(HttpContext, "staff_data");

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(staffDataJson))
                return (false, "Session expired");

            try
            {
                var staffData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(staffDataJson);
                var username = staffData.GetProperty("username").GetString();

                if (string.IsNullOrEmpty(username))
                    return (false, "Invalid session data");

                var staff = await _authService.GetStaffByUsernameAsync(username);
                if (staff == null) return (false, "User not found");

                var newAccessToken = _jwtHelper.GenerateAccessToken(staff.Username, staff.FullName, staff.Role.Id);

                CookieHelper.SetCookie(HttpContext, "suipos_ac", newAccessToken, 30, httpOnly: true);

                return (true, "Success");
            }
            catch
            {
                return (false, "Invalid session data");
            }
        }
    }
}
