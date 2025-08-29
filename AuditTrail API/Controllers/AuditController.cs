using AuditTrail_API.Data;
using AuditTrail_API.Models;
using AuditTrail_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuditTrail_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly AuditDbContext _db;
        private readonly IAuditService _auditService;

        public AuditController(AuditDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
        }

        [HttpPost("compare")]
        public async Task<IActionResult> Compare([FromBody] CompareRequest request)
        {
            if (request == null) return BadRequest();

            var action = request.Before is null || request.Before.Value.ValueKind == JsonValueKind.Null
                ? AuditAction.Created
                : (request.After is null || request.After.Value.ValueKind == JsonValueKind.Null
                    ? AuditAction.Deleted
                    : AuditAction.Updated);

            var result = _auditService.BuildAuditResult(request.Before, request.After, action, request.Metadata);

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = result.Timestamp,
                UserId = result.UserId,
                EntityName = result.EntityName,
                EntityId = result.EntityId,
                Action = result.Action,
                ChangesJson = JsonSerializer.Serialize(result.Changes)
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();

            result.AuditId = log.Id;
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _db.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            return entity == null ? NotFound() : Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _db.AuditLogs.AsNoTracking().OrderByDescending(a => a.Timestamp);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { page, pageSize, total, items });
        }
        [HttpGet("logs/{entityName}/{entityId}")]
        public IActionResult GetLogs(string entityName, string entityId)
        {
            var logs = _db.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            var result = logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                Timestamp = l.Timestamp,
                UserId = l.UserId,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                CorrelationId = l.CorrelationId,
                Changes = JsonSerializer.Deserialize<List<ChangeLog>>(l.ChangesJson)
            });

            return Ok(result);
        }

    }

}