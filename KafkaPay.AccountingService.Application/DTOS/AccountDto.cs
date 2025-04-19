using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Entities;

namespace KafkaPay.AccountingService.Application.DTOS
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
