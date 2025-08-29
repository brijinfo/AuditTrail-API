using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditTrail_API.Models
{
    public enum AuditAction
    {
        Created = 0,
        Updated = 1,
        Deleted = 2
    }
}
