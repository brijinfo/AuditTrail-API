**Audit Trail Web API built with .NET 5 that tracks changes in objects and keeps audit logs in memory (no database required).**


Swagger UI will be available at:
https://localhost:44306/swagger/index.html
sample data for **https://localhost:44306/api/audit/compare** end point
{
  "before": { "id": "123", "firstName": "Brij", "isActive": true },
  "after": { "id": "123", "firstName": "Brijesh", "isActive": false },
  "metadata": {
    "userId": "bkumar",
    "entityName": "Customer",
    "entityId": "123",
    "correlationId": "req-001"
  }
}
