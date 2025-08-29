using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AuditTrail_API.Models
{
    public record CompareRequest(
JsonElement? Before,
JsonElement? After,
Metadata? Metadata
);


    public class Metadata
    {
        public string? UserId { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? CorrelationId { get; set; }
        public DateTime? Timestamp { get; set; } // optional override
    }
    public class ChangeItem
    {
        public string Path { get; set; } = string.Empty; // e.g., "address.city" or "phones[0]"
        public JsonElement? OldValue { get; set; }
        public JsonElement? NewValue { get; set; }
    }


    public class CompareResponse
    {
        public Guid AuditId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public AuditAction Action { get; set; }
        public List<ChangeItem> Changes { get; set; } = new();
    }
}
