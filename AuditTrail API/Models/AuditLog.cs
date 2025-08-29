using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditTrail_API.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public AuditAction Action { get; set; }
        public string? CorrelationId { get; set; }
        public string? ChangesJson { get; set; }
    }
}
