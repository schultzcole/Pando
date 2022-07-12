using System;
using Microsoft.CodeAnalysis;
using Pando.SerializerGenerator.Utils;

namespace Pando.SerializerGenerator;

public class SerializedProp
{
	public ITypeSymbol Type { get; }
	public string Name { get; }
	public string CamelCaseName { get; }
	public string SerializerName { get; }
	public bool IsPrimitive { get; }
	public SerializerType SerializerType { get; } 
	
	public SerializedProp(ITypeSymbol type, string name, bool isPrimitive)
	{
		Type = type;
		Name = name;
		CamelCaseName = name.ToCamelCase();
		SerializerName = $"{CamelCaseName}Serializer";
		IsPrimitive = isPrimitive;
		var typeDisplayString = type.ToDisplayString(CustomSymbolDisplayFormats.NestedTypeName);
		SerializerType = isPrimitive
			? new PrimitiveSerializerType(typeDisplayString)
			: new NodeSerializerType(typeDisplayString);
	}

}

public abstract class SerializerType
{
	public abstract string Name { get; }
	public string GenericName => _nestedName.Value;
	private readonly Lazy<string> _nestedName;
	
	public SerializerType(string innerType)
	{
		_nestedName = new Lazy<string>(() => $"{Name}<{innerType}>");
	}
}

public class NodeSerializerType : SerializerType
{
	public override string Name => "INodeSerializer";
	public NodeSerializerType(string innerType) : base(innerType) { }
}

public class PrimitiveSerializerType : SerializerType
{
	public override string Name => "IPrimitiveSerializer";
	public PrimitiveSerializerType(string innerType) : base(innerType) { }
}
