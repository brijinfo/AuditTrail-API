using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditTrail_API.Models
{
    public class ChangeLog
    {
        public string Path { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
