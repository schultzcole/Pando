using System;
using FluentAssertions;
using Pando.Serialization.PrimitiveSerializers;
using Xunit;

// Rider doesn't detect subclasses of BaseSerializerTest as being used, because they inherit their test methods
// ReSharper disable UnusedType.Global

namespace PandoTests.Tests.Serialization.PrimitiveSerializers;

public class TimeSerializerTests
{
	public class DateTimeUnixSerializerTests : BaseSerializerTest<DateTime>, ISerializerTestData<DateTime>
	{
		protected override IPrimitiveSerializer<DateTime> Serializer => new DateTimeUnixSerializer(new SimpleLongSerializer());

		public static TheoryData<DateTime, byte[]> SerializationTestData => new()
		{
			{ new DateTime(1415, 10, 25, 12, 34, 56, 789, DateTimeKind.Utc), new byte[] { 0x46, 0x32, 0x2F, 0x8B, 0x6E, 0xBC, 0x3C, 0x50 } }
		};

		public static TheoryData<int?> ByteCountTestData => new() { sizeof(long) };

		/// By default, date time comparison doesn't compare the Kind property,
		/// but we want to make sure that the Kind property is serialized/deserialized properly so we manually check it
		protected override void AssertDeserializedValuesEquivalent(DateTime actualValue, DateTime expectedValue)
		{
			base.AssertDeserializedValuesEquivalent(actualValue, expectedValue);
			actualValue.Kind.Should().Be(expectedValue.Kind);
		}
	}
}
