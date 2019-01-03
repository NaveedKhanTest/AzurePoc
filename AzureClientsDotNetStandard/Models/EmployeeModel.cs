using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureClientsDotNetStandard.Models
{
    public class EmployeeModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public String Created { get; set; }

        public EmployeeModel(string EmployeeId, string Name, int Age, string Created)
        {
            this.Id = EmployeeId;
            this.Name = Name;
            this.Age = Age;
            this.Created = Created;
        }
    }
}
