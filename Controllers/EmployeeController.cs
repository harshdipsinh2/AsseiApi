using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using System.Security.Claims;

namespace AssetManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;

        public EmployeeController(IEmployeeRepository employeeRepository, IUserRepository userRepository)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }

        // Helper method to get company ID from token claims
        private int GetCompanyIdFromClaims()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type.Equals("CompanyID", StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrEmpty(companyIdClaim))
                throw new UnauthorizedAccessException("Company ID not found in token.");
            return int.Parse(companyIdClaim);
        }


        // GET: api/Employee
        [HttpGet]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            int companyId = GetCompanyIdFromClaims();
            var employees = await _employeeRepository.GetEmployeesAsync(companyId);

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (roles.Contains("3"))
            {
                foreach (var employee in employees)
                {
                    employee.Salary = null;
                    employee.PhoneNumber = null;
                }
            }

            return Ok(new { message = "Employees retrieved successfully.", data = employees });
        }

        // GET: api/Employee/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            int companyId = GetCompanyIdFromClaims();
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id, companyId);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (roles.Contains("3"))
            {
                employee.Salary = null;
                employee.PhoneNumber = null;
            }

            return Ok(new { message = "Employee retrieved successfully.", data = employee });
        }

        // POST: api/Employee
        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            int companyId = GetCompanyIdFromClaims();
            employee.CompanyID = companyId;

            await _employeeRepository.AddEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.EmployeeId }, new { message = "Employee created successfully.", data = employee });
        }

        // PUT: api/Employee/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
                return BadRequest(new { message = "ID mismatch." });

            int companyId = GetCompanyIdFromClaims();

            bool exists = await _employeeRepository.EmployeeExistsAsync(id, companyId);
            if (!exists)
                return NotFound(new { message = "Employee not found." });

            employee.CompanyID = companyId;

            await _employeeRepository.UpdateEmployeeAsync(employee);
            return Ok(new { message = "Employee updated successfully." });
        }

        // PATCH: api/Employee/{id}
        [HttpPatch("{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> PatchEmployeeEmail(int id, [FromBody] EmailUpdate emailUpdate)
        {
            int companyId = GetCompanyIdFromClaims();

            var employee = await _employeeRepository.GetEmployeeByIdAsync(id, companyId);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            employee.EmailId = emailUpdate.EmailId;
            await _employeeRepository.UpdateEmployeeAsync(employee);

            return Ok(new { message = "Employee email updated successfully." });
        }

        // DELETE: api/Employee/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            int companyId = GetCompanyIdFromClaims();
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id, companyId);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            bool hasAssets = await _employeeRepository.HasAssignedAssetsAsync(id);
            if (hasAssets)
            {
                return BadRequest(new { message = "Cannot delete employee with assigned assets." });
            }

            await _employeeRepository.DeleteEmployeeAsync(id);
            return Ok(new { message = "Employee deleted successfully." });
        }
    }
}
