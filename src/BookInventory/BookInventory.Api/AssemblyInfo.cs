using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

using BookInventory.Common;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<CustomSerializationContext>))]