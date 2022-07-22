#nullable enable
// <auto-generated />

using System;
using System.Buffers.Binary;
using System.CodeDom.Compiler;
using Pando.DataSources;
using Pando.Serialization.NodeSerializers;
using SerializerGeneratorUnitTests.TestFiles.TestSubjects;

namespace GeneratedSerializers;

[GeneratedCode("Pando.SerializerGenerator", "1.0.0.0")]
internal class ValidGenericClassSerializer<TGeneric> : INodeSerializer<ValidGenericClass<TGeneric>>
{
	private readonly INodeSerializer<TGeneric> _genericThingSerializer;

	public ValidGenericClassSerializer(
		INodeSerializer<TGeneric> genericThingSerializer
	)
	{
		_genericThingSerializer = genericThingSerializer;

		int? size = 0;
		size += 1 * sizeof(ulong);
		NodeSize = size;
	}

	public int? NodeSize { get; }

	public int NodeSizeForObject(ValidGenericClass<TGeneric> obj)
	{
		if (NodeSize is not null) return NodeSize.Value;

		int size = 0;
		size += 1 * sizeof(ulong);
		return size;
	}

	public void Serialize(ValidGenericClass<TGeneric> obj, Span<byte> writeBuffer, INodeDataSink dataSink)
	{
		ulong genericThingHash = _genericThingSerializer.SerializeToHash(obj.GenericThing, dataSink);
		BinaryPrimitives.WriteUInt64LittleEndian(writeBuffer[..sizeof(ulong)], genericThingHash);
	}

	public ValidGenericClass<TGeneric> Deserialize(ReadOnlySpan<byte> readBuffer, INodeDataSource dataSource)
	{
		ulong genericThingHash = BinaryPrimitives.ReadUInt64LittleEndian(readBuffer[..sizeof(ulong)]);
		var genericThing = _genericThingSerializer.DeserializeFromHash(genericThingHash, dataSource);

		return new ValidGenericClass<TGeneric>(genericThing);
	}
}
