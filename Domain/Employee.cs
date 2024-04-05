namespace Margo_Test.Domain
{
    public class Employee
    {
        public int? ID { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public int? CompanyID { get; set; }
        public Passport? Passport { get; set; }
        public Department? Department { get; set; }
    }
}
