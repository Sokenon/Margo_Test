using MySqlConnector;
using System.Data;
using Dapper;
using Margo_Test.Controllers;
namespace Margo_Test.Domain
{
    public interface IEmployeeRepository
    {
        Task<int> Create(Employee employee);
        Task Delete(int id);
        Task<List<Employee>> GetEmployees(Filters filters);
        Task Update(Employee employee);
    }

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        public EmployeeRepository(string conn)
        {
            _connectionString = conn;
        }

        public async Task<int> Create(Employee employee)
        {
            return await Task.Run(() =>
            {
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    int idDepartment = db.Query<int>("SELECT ID from Departments WHERE Name = @Name and Phone = @Phone", employee.Department).FirstOrDefault();
                    if (idDepartment == 0)
                    {
                        idDepartment = db.Query<int>("INSERT into Departments (Name, Phone) VALUES(@Name, @Phone) RETURNING ID", employee.Department).FirstOrDefault();
                    }
                    int id = db.Query<int>($"INSERT INTO Employees (Name, Surname, Phone, CompanyID, DepartmentID) VALUES(@Name, @Surname, @Phone, @CompanyID, {idDepartment}) RETURNING ID", employee).FirstOrDefault();
                    db.Execute($"INSERT INTO Passports (Type, Number, EmployeeID) VALUES(@Type, @Number, {id})", employee.Passport);
                    return id;
                }
            });
        }

        public async Task Delete(int id)
        {
            await Task.Run(() =>
            {
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    if (db.Query<Employee>("SELECT * FROM Employees WHERE ID = @id", new {id}).FirstOrDefault() == null)
                    {
                        throw new Exception("404");
                    }
                    db.Execute($"DELETE FROM Employees WHERE ID = @id", new { id });
                }
            });
        }

        public async Task<List<Employee>> GetEmployees(Filters filters) 
        {
            return await Task.Run(() =>
            {
                string fields = "";
                if (filters.companyId != null && filters.departmentId != null)
                {
                    fields += $"CompanyID = @companyId AND DepartmentID = @departmentId";
                }
                else if (filters.companyId != null)
                {
                    fields += $"CompanyID = @companyId";
                }
                else if (filters.departmentId != null)
                {
                    fields += $"DepartmentID = @departmentId";
                }
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    return db.Query<Employee, Passport, Department, Employee>("SELECT Employees.*,Passports.Number,Passports.Type,Passports.ID as passid,Departments.Name,Departments.Phone FROM Employees left join Passports on Passports.EmployeeID = Employees.ID left join Departments on Departments.ID = Employees.DepartmentID" + (fields.Length > 0 ? " where " : "") + fields, (employee, passport, department) => {
                        employee.Department = department;
                        employee.Passport = passport;
                        return employee;
                    }, filters, splitOn: "DepartmentID,passid").ToList();
                }
            });
        }

        public async Task Update(Employee employee)
        {
            await Task.Run(() =>
            { 
                string fields = "";
                if (employee.Name != null)
                {
                    fields += "Name = @Name, ";
                }
                if (employee.Surname != null)
                {
                    fields += "Surname = @Surname, ";
                }
                if (employee.Phone != null)
                {
                    fields += "Phone = @Phone, ";
                }
                if (employee.CompanyID != null)
                {
                    fields += "CompanyID = @CompanyID, ";
                }
                if (fields != "")
                {
                    fields = fields.Substring(0, fields.Length - 2);
                }
                else
                {

                }
                using (IDbConnection db = new MySqlConnection(_connectionString))
                {
                    if (db.Query<Employee>("SELECT * FROM Employees WHERE ID = @ID", employee).FirstOrDefault() == null)
                    {
                        throw new Exception("404");
                    }
                    if (employee.Department != null)
                    {
                        int idDepartment = db.Query<int>("SELECT ID from Departments WHERE Name = @Name and Phone = @Phone", employee.Department).FirstOrDefault();
                        if (idDepartment == 0)
                        {
                            idDepartment = db.Query<int>("INSERT into Departments (Name, Phone) VALUES(@Name, @Phone) RETURNING ID", employee.Department).FirstOrDefault();
                        }
                        if (fields == "")
                        {
                            db.Execute($"UPDATE Employees SET DepartmentID = {idDepartment} WHERE ID = @ID", employee);
                        }
                        else
                        {
                            db.Execute($"UPDATE Employees SET {fields}, DepartmentID = {idDepartment} WHERE ID = @ID", employee);
                        }
                    }
                    else
                    {
                        db.Execute($"UPDATE Employees SET {fields} WHERE ID = @ID", employee);
                    }
                    if (employee.Passport != null)
                    {
                        int idPassport = db.Query<int>($"SELECT ID from Passports WHERE Type = @Type and Number = @Number and EmployeeID = {employee.ID}", employee.Passport).FirstOrDefault();
                        if (idPassport == 0)
                        {
                            db.Execute($"INSERT into Passports (Type, Number, EmployeeID) VALUES(@Type, @Number" +
                                $", {employee.ID})", employee.Passport);
                        }
                        else
                        {
                            db.Execute($"UPDATE Passports SET Type = @Type, Number = @Number WHERE ID = {idPassport}", employee.Passport);
                        }
                    }
                }
            });
        }
    }
}