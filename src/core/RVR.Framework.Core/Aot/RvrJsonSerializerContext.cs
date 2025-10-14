using System.Text.Json.Serialization;
using RVR.Framework.Core.Pagination;

namespace RVR.Framework.Core.Aot;

/// <summary>
/// Source-generated JSON serializer context for common RIVORA Framework DTOs.
/// Using source generation enables Native AOT compatibility by avoiding
/// runtime reflection for JSON serialization.
/// </summary>
[JsonSerializable(typeof(CursorPageRequest))]
[JsonSerializable(typeof(PagedResult<object>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Guid))]
public partial class RvrJsonSerializerContext : JsonSerializerContext
{
}
