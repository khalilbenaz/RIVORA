using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MessagePack;

namespace RVR.Framework.Benchmarks;

/// <summary>
/// Benchmarks comparing System.Text.Json and MessagePack serialization
/// for small and large object graphs.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SerializationBenchmarks
{
    private SmallDto _smallDto = null!;
    private LargeDto _largeDto = null!;
    private string _smallJson = null!;
    private string _largeJson = null!;
    private byte[] _smallMsgPack = null!;
    private byte[] _largeMsgPack = null!;
    private JsonSerializerOptions _jsonOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Small object: a single flat DTO
        _smallDto = new SmallDto
        {
            Id = Guid.NewGuid(),
            Name = "Benchmark Product",
            Price = 29.99m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Large object: nested collection of 100 items with sub-collections
        _largeDto = new LargeDto
        {
            Id = Guid.NewGuid(),
            Title = "Benchmark Order",
            Description = "A large object graph for serialization benchmarking",
            CreatedAt = DateTime.UtcNow,
            Tags = Enumerable.Range(0, 20).Select(i => $"tag-{i}").ToList(),
            Items = Enumerable.Range(0, 100).Select(i => new LargeDto.OrderItem
            {
                ItemId = Guid.NewGuid(),
                ProductName = $"Product_{i}",
                Quantity = i + 1,
                UnitPrice = 9.99m + i,
                Attributes = Enumerable.Range(0, 5)
                    .ToDictionary(k => $"attr_{k}", k => $"value_{k}_{i}")
            }).ToList(),
            Metadata = Enumerable.Range(0, 50)
                .ToDictionary(k => $"meta_{k}", k => $"value_{k}")
        };

        // Pre-serialize for deserialization benchmarks
        _smallJson = JsonSerializer.Serialize(_smallDto, _jsonOptions);
        _largeJson = JsonSerializer.Serialize(_largeDto, _jsonOptions);
        _smallMsgPack = MessagePackSerializer.Serialize(_smallDto);
        _largeMsgPack = MessagePackSerializer.Serialize(_largeDto);
    }

    // ─── Small Object Serialization ────────────────────────────────────

    [Benchmark(Description = "System.Text.Json - Serialize small object")]
    public string JsonSerialize_Small()
    {
        return JsonSerializer.Serialize(_smallDto, _jsonOptions);
    }

    [Benchmark(Description = "MessagePack - Serialize small object")]
    public byte[] MsgPackSerialize_Small()
    {
        return MessagePackSerializer.Serialize(_smallDto);
    }

    [Benchmark(Description = "System.Text.Json - Deserialize small object")]
    public SmallDto? JsonDeserialize_Small()
    {
        return JsonSerializer.Deserialize<SmallDto>(_smallJson, _jsonOptions);
    }

    [Benchmark(Description = "MessagePack - Deserialize small object")]
    public SmallDto? MsgPackDeserialize_Small()
    {
        return MessagePackSerializer.Deserialize<SmallDto>(_smallMsgPack);
    }

    // ─── Large Object Serialization ────────────────────────────────────

    [Benchmark(Description = "System.Text.Json - Serialize large object")]
    public string JsonSerialize_Large()
    {
        return JsonSerializer.Serialize(_largeDto, _jsonOptions);
    }

    [Benchmark(Description = "MessagePack - Serialize large object")]
    public byte[] MsgPackSerialize_Large()
    {
        return MessagePackSerializer.Serialize(_largeDto);
    }

    [Benchmark(Description = "System.Text.Json - Deserialize large object")]
    public LargeDto? JsonDeserialize_Large()
    {
        return JsonSerializer.Deserialize<LargeDto>(_largeJson, _jsonOptions);
    }

    [Benchmark(Description = "MessagePack - Deserialize large object")]
    public LargeDto? MsgPackDeserialize_Large()
    {
        return MessagePackSerializer.Deserialize<LargeDto>(_largeMsgPack);
    }

    // ─── Roundtrip ─────────────────────────────────────────────────────

    [Benchmark(Description = "System.Text.Json - Roundtrip small object")]
    public SmallDto? JsonRoundtrip_Small()
    {
        var json = JsonSerializer.Serialize(_smallDto, _jsonOptions);
        return JsonSerializer.Deserialize<SmallDto>(json, _jsonOptions);
    }

    [Benchmark(Description = "MessagePack - Roundtrip small object")]
    public SmallDto? MsgPackRoundtrip_Small()
    {
        var bytes = MessagePackSerializer.Serialize(_smallDto);
        return MessagePackSerializer.Deserialize<SmallDto>(bytes);
    }

    [Benchmark(Description = "System.Text.Json - Roundtrip large object")]
    public LargeDto? JsonRoundtrip_Large()
    {
        var json = JsonSerializer.Serialize(_largeDto, _jsonOptions);
        return JsonSerializer.Deserialize<LargeDto>(json, _jsonOptions);
    }

    [Benchmark(Description = "MessagePack - Roundtrip large object")]
    public LargeDto? MsgPackRoundtrip_Large()
    {
        var bytes = MessagePackSerializer.Serialize(_largeDto);
        return MessagePackSerializer.Deserialize<LargeDto>(bytes);
    }
}

// ─── DTOs for serialization benchmarks ─────────────────────────────────

[MessagePackObject]
public class SmallDto
{
    [Key(0)]
    public Guid Id { get; set; }

    [Key(1)]
    public string Name { get; set; } = string.Empty;

    [Key(2)]
    public decimal Price { get; set; }

    [Key(3)]
    public bool IsActive { get; set; }

    [Key(4)]
    public DateTime CreatedAt { get; set; }
}

[MessagePackObject]
public class LargeDto
{
    [Key(0)]
    public Guid Id { get; set; }

    [Key(1)]
    public string Title { get; set; } = string.Empty;

    [Key(2)]
    public string? Description { get; set; }

    [Key(3)]
    public DateTime CreatedAt { get; set; }

    [Key(4)]
    public List<string> Tags { get; set; } = [];

    [Key(5)]
    public List<OrderItem> Items { get; set; } = [];

    [Key(6)]
    public Dictionary<string, string> Metadata { get; set; } = [];

    [MessagePackObject]
    public class OrderItem
    {
        [Key(0)]
        public Guid ItemId { get; set; }

        [Key(1)]
        public string ProductName { get; set; } = string.Empty;

        [Key(2)]
        public int Quantity { get; set; }

        [Key(3)]
        public decimal UnitPrice { get; set; }

        [Key(4)]
        public Dictionary<string, string> Attributes { get; set; } = [];
    }
}
