using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace AzureClientsDotNetStandard
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                QueueClientTests.Tests();


            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }
    }
}
