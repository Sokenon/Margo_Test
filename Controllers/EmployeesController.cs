using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Margo_Test.Domain;
using System.Text.Json;
using System.ComponentModel.Design;

namespace Margo_Test.Controllers
{
    public class Filters
    {
        public string? companyId { get; set; }
        public string? departmentId { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private IEmployeeRepository Repository;
        public EmployeesController(IEmployeeRepository repository)
        {
            Repository = repository;
        }

        [HttpGet]
        public async Task<List<Employee>> GetEmployee([FromQuery] Filters filters)
        {
            List<Employee> employees = await Repository.GetEmployees(filters);
            return employees;
        }

        [HttpPost]
        public async Task<string> CreateEmployee([FromBody] Employee employee)
        {
            int id = await Repository.Create(employee);
            string json = JsonSerializer.Serialize(id);
            return json;
        }

        [HttpDelete ("{id}")]
        public async Task DeleteEmployee(int id)
        {
            await Repository.Delete(id);
        }

        [HttpPatch]
        public async Task UpdateEmployee([FromBody] Employee employee)
        {
            await Repository.Update(employee);
        }
    }
}
