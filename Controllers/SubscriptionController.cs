using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using AssetManagement.Configurations;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using AssetManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AssetManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly string _webhookSecret;
        private readonly AppDbContext _context; //  Add your DbContext here
        private readonly IConfiguration _configuration;


        public SubscriptionController(
            ISubscriptionRepository subscriptionRepository,
            IOptions<StripeSettings> stripeSettings,
            AppDbContext context,
            IConfiguration configuration) //  Inject it in the constructor
        {
            _subscriptionRepository = subscriptionRepository;
            _webhookSecret = stripeSettings.Value.WebhookSecret;
            _context = context; //  Assign it here
            _configuration = configuration;
        }

        private int GetCurrentUserCompanyId()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyID")?.Value;
            return companyIdClaim != null ? int.Parse(companyIdClaim) : 0;
        }


        [HttpGet("check-user-exists/{email}")]
        public IActionResult CheckUserExists(string email)
        {
            var user = _context.tb_Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return Ok(false); // Not a user yet
            return Ok(true); // User already exists
        }

        [HttpGet("check-subscription/{email}")]
        public async Task<IActionResult> CheckSubscription(string email)
        {
            // Log email input and query result
            Console.WriteLine($"Checking subscription for: {email}");

            var subscription = await _subscriptionRepository.GetSubscriptionByEmailAsync(email);

            if (subscription == null)
            {
                Console.WriteLine("No subscription found.");
                return Ok(false);
            }

            // Log the retrieved subscription status
            Console.WriteLine($"Subscription found: SubscriptionId = {subscription.SubscriptionId}, Status = {subscription.Status}");

            if (subscription.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) && subscription.EndDate > DateTime.UtcNow)
            {
                return Ok(true);
            }

            return Ok(false);
        }


        [HttpPost("create-subscription-session")]
        [Authorize]
        public IActionResult CreateSubscriptionSession([FromBody] SubscriptionRequest request)
        {
            try
            {
                var companyId = GetCurrentUserCompanyId();

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "subscription",
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = "price_1R9gDqR355WsWBFp2egcsLgB", // Replace with your actual Price ID
                    Quantity = 1
                }
            },
                    SuccessUrl = "http://localhost:3000/dashboard",
                    CancelUrl = "http://localhost:3000/subscribe",
                    Metadata = new Dictionary<string, string>
            {
                { "Email", request.Email },
                { "Type", "Subscription" },
                { "CompanyID", companyId.ToString() }
            }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create session", error = ex.Message });
            }
        }


        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            if (!Request.Headers.ContainsKey("Stripe-Signature"))
            {
                return BadRequest(new { message = "Missing Stripe-Signature header" });
            }

            var signatureHeader = Request.Headers["Stripe-Signature"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
                Console.WriteLine($"Received Stripe Event: {stripeEvent.Type}");

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session == null || session.Metadata == null)
                    {
                        return BadRequest(new { message = "Invalid session data" });
                    }

                    if (!session.Metadata.TryGetValue("Email", out string email) || string.IsNullOrEmpty(email))
                    {
                        return BadRequest(new { message = "Invalid or missing Email in metadata" });
                    }

                    var subscriptionId = session.SubscriptionId;
                    var customerId = session.CustomerId;

                    var subscriptionService = new SubscriptionService();
                    var subscription = await subscriptionService.GetAsync(subscriptionId);

                    if (subscription == null)
                    {
                        return BadRequest(new { message = "Subscription not found" });
                    }

                    if (!session.Metadata.TryGetValue("CompanyID", out string companyIdStr) || string.IsNullOrEmpty(companyIdStr))
                    {
                        return BadRequest(new { message = "Missing CompanyID in metadata" });
                    }

                    int companyId = int.Parse(companyIdStr);

                    // Create subscription with CompanyID
                    var newSubscription = new EmployeeSubscription
                    {
                        Email = email,
                        CompanyID = companyId,
                        SubscriptionId = subscription?.Id ?? "default-subscription-id",
                        ProductId = subscription?.Items?.Data?.FirstOrDefault()?.Plan?.ProductId ?? "default-product-id",
                        Status = subscription?.Status ?? "Active",
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(12),
                    };

                    await _subscriptionRepository.AddSubscriptionAsync(newSubscription);

                    // Find the user associated with this email and update their role to Super Admin (or as required)
                    var user = await _context.tb_Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user != null)
                    {
                        // Set RoleID based on the subscription (assuming 1 is Super Admin, adjust as needed)
                        user.RoleID = 1; // Set RoleID to 1 for Super Admin

                        _context.tb_Users.Update(user);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"User {email} is now a Super Admin.");
                    }

                    Console.WriteLine("Subscription saved to DB.");
                    return Ok(new { message = "Subscription recorded and user role updated to Super Admin" });
                }
                else
                {
                    Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
                    return Ok(new { message = "Event type not handled" });
                }
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe webhook error: {ex.Message}");
                return BadRequest(new { message = "Webhook error", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return BadRequest(new { message = "Unexpected error occurred", error = ex.Message });
            }
        }

        [HttpGet("regenerate-token")]
        [Authorize]
        public async Task<IActionResult> RegenerateToken()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Email not found in token.");

            var user = await _context.tb_Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Unauthorized("User not found.");

            // Generate a new token with updated claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("EmployeeID", user.EmployeeID.ToString()),
                new Claim(ClaimTypes.Role, user.RoleID.ToString()),
                new Claim("RoleId", user.RoleID.ToString()), //  Explicit custom claim
                new Claim("CompanyID", user.CompanyID.ToString())
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourIssuer",
                audience: "yourAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(jwtToken); // instead of wrapping in { token = jwtToken }
        }



        public class SubscriptionRequest
        {
            public string Email { get; set; }
        }
    }
}