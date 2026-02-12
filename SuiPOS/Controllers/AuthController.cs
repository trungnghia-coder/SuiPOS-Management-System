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
            CookieHelper.SetCookie(HttpContext, "suipos_ac", accessToken, 30);

            if (loginDto.RememberMe)
            {
                var refreshToken = _jwtHelper.GenerateRefreshToken();
                CookieHelper.SetCookie(HttpContext, "suipos_rf", refreshToken, 10080);
            }

            CookieHelper.SetCookie(HttpContext, "staff_name", staff.FullName, 30);

            CookieHelper.SetCookie(HttpContext, "staff_role", staff.Role?.Name ?? "Staff", 30);

            TempData["SuccessMessage"] = $"Chào mừng {staff.FullName}!";
            return RedirectToAction("Index", "POS");
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (IsUserAuthenticated()) return RedirectToAction("Index", "POS");
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

            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Login");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            CookieHelper.RemoveCookie(HttpContext, "suipos_ac");
            CookieHelper.RemoveCookie(HttpContext, "suipos_rf");
            CookieHelper.RemoveCookie(HttpContext, "staff_name");
            CookieHelper.RemoveCookie(HttpContext, "staff_role");

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = CookieHelper.GetCookie(HttpContext, "suipos_rf");

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { success = false, message = "Refresh token không tồn tại" });
            }

            var username = CookieHelper.GetCookie(HttpContext, "staff_name");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { success = false, message = "Session đã hết hạn" });
            }

            try
            {
                var staff = await _authService.GetStaffByUsernameAsync(username);

                if (staff == null)
                {
                    return Unauthorized(new { success = false, message = "User không tồn tại" });
                }

                var newAccessToken = _jwtHelper.GenerateAccessToken(staff.Username, staff.FullName, staff.Role.Id);
                CookieHelper.SetCookie(HttpContext, "suipos_ac", newAccessToken, 30);

                return Ok(new { success = true, message = "Token refreshed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
