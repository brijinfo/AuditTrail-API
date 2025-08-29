using AuditTrail_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AuditTrail_API.Services
{
    public class AuditService:IAuditService
    {
        public CompareResponse BuildAuditResult(JsonElement? before, JsonElement? after, AuditAction action, Metadata? meta)
        {
            var timestamp = meta?.Timestamp ?? DateTime.UtcNow;

            var changes = action switch
            {
                AuditAction.Created => ComputeAdded(null, after),
                AuditAction.Deleted => ComputeRemoved(before, null),
                _ => ComputeDiff(before, after)
            };

            return new CompareResponse
            {
                Timestamp = timestamp,
                UserId = meta?.UserId,
                EntityName = meta?.EntityName,
                EntityId = meta?.EntityId,
                Action = action,
                Changes = changes
            };
        }

        // --- Diff helpers ---
        private static List<ChangeItem> ComputeDiff(JsonElement? before, JsonElement? after)
        {
            var list = new List<ChangeItem>();
            WalkDiff(before, after, path: string.Empty, list);
            return list;
        }

        private static List<ChangeItem> ComputeAdded(JsonElement? before, JsonElement? after)
        {
            var list = new List<ChangeItem>();
            WalkAdded(after, path: string.Empty, list);
            return list;
        }

        private static List<ChangeItem> ComputeRemoved(JsonElement? before, JsonElement? after)
        {
            var list = new List<ChangeItem>();
            WalkRemoved(before, path: string.Empty, list);
            return list;
        }

        private static void WalkDiff(JsonElement? a, JsonElement? b, string path, List<ChangeItem> acc)
        {
            if (a is null && b is null) return;
            if (a is null) { WalkAdded(b, path, acc); return; }
            if (b is null) { WalkRemoved(a, path, acc); return; }

            var va = a.Value;
            var vb = b.Value;

            if (va.ValueKind != vb.ValueKind)
            {
                acc.Add(new ChangeItem { Path = path, OldValue = va, NewValue = vb });
                return;
            }

            switch (va.ValueKind)
            {
                case JsonValueKind.Object:
                    var aProps = va.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                    var bProps = vb.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                    var keys = aProps.Keys.Union(bProps.Keys).OrderBy(k => k);

                    foreach (var key in keys)
                    {
                        aProps.TryGetValue(key, out var av);
                        bProps.TryGetValue(key, out var bv);

                        var childPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";

                        WalkDiff(
                            av.ValueKind == JsonValueKind.Undefined ? null : av,
                            bv.ValueKind == JsonValueKind.Undefined ? null : bv,
                            childPath, acc
                        );
                    }
                    break;

                case JsonValueKind.Array:
                    var aArr = va.EnumerateArray().ToList();
                    var bArr = vb.EnumerateArray().ToList();
                    var max = Math.Max(aArr.Count, bArr.Count);

                    for (int i = 0; i < max; i++)
                    {
                        var childPath = $"{path}[{i}]";
                        JsonElement? ai = i < aArr.Count ? aArr[i] : (JsonElement?)null;
                        JsonElement? bi = i < bArr.Count ? bArr[i] : (JsonElement?)null;
                        WalkDiff(ai, bi, childPath, acc);
                    }
                    break;

                default:
                    if (!JsonElementDeepEquals(va, vb))
                        acc.Add(new ChangeItem { Path = path, OldValue = va, NewValue = vb });
                    break;
            }
        }

        private static void WalkAdded(JsonElement? b, string path, List<ChangeItem> acc)
        {
            if (b is null) return;
            var vb = b.Value;
            switch (vb.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var p in vb.EnumerateObject())
                    {
                        var childPath = string.IsNullOrEmpty(path) ? p.Name : $"{path}.{p.Name}";
                        WalkAdded(p.Value, childPath, acc);
                    }
                    break;

                case JsonValueKind.Array:
                    int i = 0;
                    foreach (var item in vb.EnumerateArray())
                    {
                        WalkAdded(item, $"{path}[{i++}]", acc);
                    }
                    break;

                default:
                    acc.Add(new ChangeItem { Path = path, OldValue = null, NewValue = vb });
                    break;
            }
        }

        private static void WalkRemoved(JsonElement? a, string path, List<ChangeItem> acc)
        {
            if (a is null) return;
            var va = a.Value;
            switch (va.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var p in va.EnumerateObject())
                    {
                        var childPath = string.IsNullOrEmpty(path) ? p.Name : $"{path}.{p.Name}";
                        WalkRemoved(p.Value, childPath, acc);
                    }
                    break;

                case JsonValueKind.Array:
                    int i = 0;
                    foreach (var item in va.EnumerateArray())
                    {
                        WalkRemoved(item, $"{path}[{i++}]", acc);
                    }
                    break;

                default:
                    acc.Add(new ChangeItem { Path = path, OldValue = va, NewValue = null });
                    break;
            }
        }

        private static bool JsonElementDeepEquals(JsonElement a, JsonElement b)
        {
            // Compare via serialization (good enough for audit)
            return a.ValueKind == b.ValueKind && a.ToString() == b.ToString();
        }
    }
}
