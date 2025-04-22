using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Application.DTOS
{
    public class TransactionDTO
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string ReferenceNumber { get; private set; }
        public int StatusId { get; set; }
        public DateTimeOffset Created { get; set; }
        public TransactionStatusDto Status { get; set; }

    }
}
