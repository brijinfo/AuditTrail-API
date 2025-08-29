using AuditTrail_API.Models;
using System.Text.Json;

namespace AuditTrail_API.Services
{
    public interface IAuditService
    {
        CompareResponse BuildAuditResult(JsonElement? before, JsonElement? after, AuditAction action, Metadata? meta);
    }
}
