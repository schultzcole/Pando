#nullable enable
// <auto-generated />

using System;
using System.Buffers.Binary;
using System.CodeDom.Compiler;
using Pando.DataSources;
using Pando.Serialization.NodeSerializers;
using Pando.Serialization.PrimitiveSerializers;
using SerializerGeneratorUnitTests.TestClasses;

namespace GeneratedSerializers;

[GeneratedCode("Pando.SerializerGenerator", "1.0.0.0")]
internal class ValidStructSerializer : INodeSerializer<ValidStruct>
{
	private readonly INodeSerializer<object> _objectPropSerializer;
	private readonly IPrimitiveSerializer<float> _floatPropSerializer;

	public ValidStructSerializer(
		INodeSerializer<object> objectPropSerializer,
		IPrimitiveSerializer<float> floatPropSerializer
	)
	{
		_objectPropSerializer = objectPropSerializer;
		_floatPropSerializer = floatPropSerializer;

		int? size = 0;
		size += _floatPropSerializer.ByteCount;
		size += 1 * sizeof(ulong);
		NodeSize = size;
	}

	public int? NodeSize { get; }

	public int NodeSizeForObject(ValidStruct obj)
	{
		if (NodeSize is not null) return NodeSize.Value;

		int size = 0;
		size += _floatPropSerializer.ByteCountForValue(obj.FloatProp);
		size += 1 * sizeof(ulong);
		return size;
	}

	public void Serialize(ValidStruct obj, Span<byte> writeBuffer, INodeDataSink dataSink)
	{
		ulong objectPropHash = _objectPropSerializer.SerializeToHash(obj.ObjectProp, dataSink);
		BinaryPrimitives.WriteUInt64LittleEndian(writeBuffer[..sizeof(ulong)], objectPropHash);
		writeBuffer = writeBuffer[sizeof(ulong)..];

		_floatPropSerializer.Serialize(obj.FloatProp, ref writeBuffer);
	}

	public ValidStruct Deserialize(ReadOnlySpan<byte> readBuffer, INodeDataSource dataSource)
	{
		ulong objectPropHash = BinaryPrimitives.ReadUInt64LittleEndian(readBuffer[..sizeof(ulong)]);
		var objectProp = _objectPropSerializer.DeserializeFromHash(objectPropHash, dataSource);
		readBuffer = readBuffer[sizeof(ulong)..];

		var floatProp = _floatPropSerializer.Deserialize(ref readBuffer);

		return new ValidStruct(objectProp, floatProp);
	}
}
