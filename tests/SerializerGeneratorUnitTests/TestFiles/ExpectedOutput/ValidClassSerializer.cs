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
internal class ValidClassSerializer : INodeSerializer<ValidClass>
{
	private IPrimitiveSerializer<Int32> _propSerializer;
	private INodeSerializer<String> _prop2Serializer;

	public ValidClassSerializer(
		IPrimitiveSerializer<Int32> propSerializer,
		INodeSerializer<String> prop2Serializer
	)
	{
		_propSerializer = propSerializer;
		_prop2Serializer = prop2Serializer;

		NodeSize = 1 * sizeof(ulong) + _propSerializer.ByteCount;
	}

	public int? NodeSize { get; }

	public int NodeSizeForObject(ValidClass obj)
	{
		if (NodeSize is not null) return NodeSize.Value;

		int size = 0;
		size += _propSerializer.ByteCountForValue(obj.Prop);
		size += 1 * sizeof(ulong);

		return size;
	}

	public void Serialize(ValidClass obj, Span<byte> writeBuffer, INodeDataSink dataSink)
	{
		_propSerializer.Serialize(obj.Prop, ref writeBuffer);

		ulong prop2Hash = _prop2Serializer.SerializeToHash(obj.Prop2, dataSink);
		BinaryPrimitives.WriteUInt64LittleEndian(writeBuffer.Slice(0, sizeof(ulong)), prop2Hash);
	}

	public ValidClass Deserialize(ReadOnlySpan<byte> readBuffer, INodeDataSource dataSource)
	{
		var prop = _propSerializer.Deserialize(ref readBuffer);

		ulong prop2Hash = BinaryPrimitives.ReadUInt64LittleEndian(readBuffer.Slice(0, sizeof(ulong)));
		var prop2 = _prop2Serializer.DeserializeFromHash(prop2Hash, dataSource);

		return new ValidClass(prop, prop2);
	}
}
