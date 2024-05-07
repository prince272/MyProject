using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Data;
using MyBackend.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Globalization;
using System.Security.Claims;

namespace MyBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UsersController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("[action]"), AllowAnonymous]
        [SwaggerOperation(nameof(Register), OperationId = nameof(Register), Summary = "Registers a new user.")]
        public async Task<IActionResult> Register([FromBody] RegisterForm form)
        {
            // Validate the form.

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create the user if does not exist.
            var user = await _userManager.FindByEmailAsync(form.Email);

            if (user != null)
            {
                return ValidationProblem(title: "User already exists");
            }

            user = new User
            {
                UserName = form.Email,
                Email = form.Email,
                FirstName = form.FirstName,
                LastName = form.LastName
            };


            var result = await _userManager.CreateAsync(user, form.Password);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());

            // Find or create the role.
            var role = await _roleManager.FindByNameAsync(form.Role);
            if (role == null)
            {
                role = new Role(form.Role);
                await _roleManager.CreateAsync(role);
            }

            // Add the user to the role.
            result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());

            // return response with success message
            return Ok(new
            {
                message = "Registration successful",
                data = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    Roles = await _userManager.GetRolesAsync(user)
                }
            });
        }

        [HttpPost("[action]"), AllowAnonymous]
        [SwaggerOperation(nameof(Login), OperationId = nameof(Login), Summary = "Logs in a user.")]
        public async Task<IActionResult> Login([FromBody] LoginForm form)
        {
            // Validate the form.

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find the user.
            var user = await _userManager.FindByEmailAsync(form.Email);

            if (user == null)
            {
                return ValidationProblem(title: "User not found");
            }

            // Check the password.
            if (!await _userManager.CheckPasswordAsync(user, form.Password))
            {
                return ValidationProblem(title: "Invalid password");
            }

            // 
            var issuedAt = DateTimeOffset.UtcNow;
            var expirngAt = issuedAt.AddDays(30);

            // Create claims principal for user.
            var cookieClaims = await CreateClaimsPrincipalAsync(user);

            // Sign in the user.
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cookieClaims, new AuthenticationProperties
            {
                IsPersistent = true, // "Remember Me"
                IssuedUtc = issuedAt,
                ExpiresUtc = expirngAt
            });

            return Ok(new
            {
                message = "Login successful",
                data = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    Roles = await _userManager.GetRolesAsync(user)
                }
            });
        }

        [HttpPost("[action]")]
        [SwaggerOperation(nameof(Logout), OperationId = nameof(Logout), Summary = "Logs out the current user.")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("[action]"), AllowAnonymous]
        [SwaggerOperation(nameof(IsAuthenticated), OperationId = nameof(IsAuthenticated), Summary = "Checks if the user is authenticated.")]
        public bool IsAuthenticated()
        {
            return User.Identity?.IsAuthenticated ?? false;
        }

        [HttpGet("[action]")]
        [SwaggerOperation(nameof(GetUserInfo), OperationId = nameof(GetUserInfo), Summary = "Gets the information of the current user.")]
        public async Task<IActionResult> GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }

        private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
            identity.AddClaim(new Claim(ClaimTypes.SerialNumber, user.SecurityStamp));

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            roles.ForEach(role => identity.AddClaim(new Claim(ClaimTypes.Role, role)));

            return new ClaimsPrincipal(identity);
        }
    }
}
