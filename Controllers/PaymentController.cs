using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using AssetManagement.Interfaces;
using AssetManagement.Configurations;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace AssetManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPhysicalAssetRepository _physicalAssetRepository;
        private readonly IEmployeeAssetTransactionRepository _transactionRepository;
        private readonly string _webhookSecret;

        public PaymentController(
            IPhysicalAssetRepository physicalAssetRepository,
            IEmployeeAssetTransactionRepository transactionRepository,
            IOptions<StripeSettings> stripeSettings)
        {
            _physicalAssetRepository = physicalAssetRepository;
            _transactionRepository = transactionRepository;
            _webhookSecret = stripeSettings.Value.WebhookSecret;
        }

        //View the transactions

        [HttpGet("transactions")]
        [Authorize(Roles = "1,2")] // Only Super Admin and Admin can view transactions
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _transactionRepository.GetAllTransactionsAsync();

                if (transactions == null || !transactions.Any())
                {
                    return NotFound(new { message = "No transactions found." });
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving transactions", error = ex.Message });
            }
        }


        // Endpoint to create a Stripe checkout session
        [HttpPost("create-checkout-session")]
        [Authorize(Roles = "1,2,3")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequest request)
        {
            try
            {
                // Validate Asset
                var asset = await _physicalAssetRepository.GetAssetByIdAsync(request.AssetID);
                if (asset == null)
                {
                    return NotFound(new { message = "Asset not found." });
                }

                // Validate Employee
                var employee = await _transactionRepository.GetEmployeeByIdAsync(request.EmployeeId);
                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found." });
                }

                // ✅ Check if the employee already owns this asset
                var existingTransaction = await _transactionRepository.HasEmployeeBoughtAsset(request.EmployeeId, request.AssetID);
                if (existingTransaction)
                {
                    return BadRequest(new { message = "You have already purchased this asset." });
                }

                // Create the checkout session options
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "inr",
                        UnitAmount = (long)(asset.PurchaseCost * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = asset.AssetName,
                            Description = asset.Description
                        }
                    },
                    Quantity = 1
                }
            },
                    Mode = "payment",
                    SuccessUrl = "http://localhost:3000/my-assets",
                    CancelUrl = "http://localhost:3000/my-assets",
                    Metadata = new Dictionary<string, string>
            {
                { "EmployeeId", request.EmployeeId.ToString() },
                { "AssetID", request.AssetID.ToString() }
            }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating checkout session", error = ex.Message });
            }
        }


        // Webhook endpoint for Stripe to notify us of payment completion
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            if (!Request.Headers.ContainsKey("Stripe-Signature"))
            {
                return BadRequest(new { message = "Stripe signature header missing" });
            }

            var signatureHeader = Request.Headers["Stripe-Signature"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);

                Console.WriteLine($"Received Stripe Event: {stripeEvent}"); // Log the entire event

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session == null || session.Metadata == null)
                    {
                        return BadRequest(new { message = "Invalid session data" });
                    }

                    if (!session.Metadata.ContainsKey("EmployeeId") || !session.Metadata.ContainsKey("AssetID"))
                    {
                        return BadRequest(new { message = "Metadata missing in Stripe session" });
                    }

                    if (!int.TryParse(session.Metadata["EmployeeId"], out int employeeId) ||
                        !int.TryParse(session.Metadata["AssetID"], out int assetId))
                    {
                        return BadRequest(new { message = "Invalid EmployeeId or AssetID in metadata" });
                    }

                    var amountPaid = Math.Round(session.AmountTotal.HasValue ? session.AmountTotal.Value / 100.0m : 0m, 2);

                    var transaction = new EmployeeAssetTransaction
                    {
                        AssetID = assetId,
                        EmployeeId = employeeId,
                        PurchasePrice = amountPaid,
                        PaymentMethod = "Card",
                        TransactionDate = DateTime.UtcNow
                    };

                    Console.WriteLine($"Transaction to save: {transaction}"); // Log the transaction

                    await _transactionRepository.AddTransactionAsync(transaction);

                    Console.WriteLine("Transaction saved successfully."); // Log success

                    return Ok(new { message = "Payment recorded successfully" });
                }
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe webhook error: {ex.Message}");
                return BadRequest(new { message = "Webhook handling error", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return BadRequest(new { message = "Unexpected error occurred", error = ex.Message });
            }

            return BadRequest(new { message = "Unhandled event type" });
        }

        // Add this route handler for the success URL
        [HttpGet("success")]
        public IActionResult PaymentSuccess(string session_id)
        {
            Console.WriteLine("PaymentSuccess route hit!");
            Console.WriteLine($"Payment Success, Session ID: {session_id}");

            return Ok(new { message = "Payment successful!" });
        }

        public class CheckoutRequest
        {
            public int AssetID { get; set; }
            public int EmployeeId { get; set; }
        }
    }
}