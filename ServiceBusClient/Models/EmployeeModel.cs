using System;
using System.Collections.Generic;
using System.Text;

namespace AzureClients.Models
{
    public class EmployeeModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public EmployeeModel(string EmployeeId, string Name, int Age)
        {
            this.Id = EmployeeId;
            this.Name = Name;
            this.Age = Age;
        }
    }
}
