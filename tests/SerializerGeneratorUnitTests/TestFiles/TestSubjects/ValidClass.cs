using Pando.SerializerGenerator.Attributes;

namespace SerializerGeneratorUnitTests.TestFiles.TestSubjects;

[GenerateNodeSerializer]
internal sealed record ValidClass([property: Primitive] int PrimitiveProp, string NodeProp);
