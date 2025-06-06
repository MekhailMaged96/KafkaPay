﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Domain.Common
{
    public abstract class BaseAuditableEntity<T> : BaseEntity<T>,IAuditableEntity
    {
        public DateTimeOffset Created { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
