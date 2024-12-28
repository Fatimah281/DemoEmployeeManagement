

namespace BaseLibrary.Entities
{
    public class Employee 
    {
        // Basic Information
        public int Id { get; set; }
        public string? Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Job Details
        public string JobTitle { get; set; }
        public int Salary { get; set; }
        public DateTime HireDate { get; set; }

        // Employee Status
        public bool IsActive { get; set; }

        //Relationship: many to one
        public GeneralDepartment GeneralDepartment { get; set; }
        public int GeneralDepartmentId { get; set; }
        public Department? Department { get; set; }
        public int DepartmentId { get; set; }

        public Branch? Branch { get; set; }
        public int BranchId { get; set; }

        public Town? Town { get; set; }
        public int TownId { get; set; }
    }

}
