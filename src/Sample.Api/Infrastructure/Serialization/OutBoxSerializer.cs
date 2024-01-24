using System.Text.Json.Serialization;
using Sample.Api.Core.Model;

namespace Sample.Api.Infrastructure.Serialization;



[JsonSerializable(typeof(OutBoxEvent))]
[JsonSerializable(typeof(OrderSaved))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class OutBoxSerializationConfig : JsonSerializerContext;
