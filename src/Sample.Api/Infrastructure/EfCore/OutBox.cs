﻿namespace Sample.Api.Infrastructure.EfCore;

public sealed class OutBox
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public long CreatedAtTimestamp { get; set; }
    public long? ProcessedAtTimestamp { get; set; }
}
