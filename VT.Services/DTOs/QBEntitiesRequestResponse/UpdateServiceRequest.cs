using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class UpdateServiceRequest
    {
        public bool sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public string Name { get; set; }
        public IncomeAccountRef IncomeAccountRef { get; set; }
        public ExpenseAccountRef ExpenseAccountRef { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
