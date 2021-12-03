using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Pando.Repositories;
using Pando.Repositories.Utils;
using PandoTests.Utils;

namespace PandoTests.Repositories.InMemoryRepositoryTests;

public class ReconstitutionTests
{
	[Test]
	public void Should_have_node_after_being_reconstituted()
	{
		// Test Data
		var nodeHash = 123UL;
		var nodeIndex = ArrayX.Concat(ByteConverter.GetBytes(nodeHash), new byte[8]);

		// Arrange/Act
		var nodeIndexStream = new MemoryStream(nodeIndex.CreateCopy());
		var repository = new InMemoryRepository(Stream.Null, nodeIndexStream, Stream.Null);

		// Assert
		repository.HasNode(nodeHash).Should().BeTrue();
	}

	[Test]
	public void Should_return_correct_node_data_after_being_reconstituted()
	{
		// Test Data
		var hash1 = 0123UL;
		var hash2 = 4567UL;
		var nodeData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
		var nodeIndexEntry = ArrayX.Concat(
			ByteConverter.GetBytes(hash1),
			ByteConverter.GetBytes(0),
			ByteConverter.GetBytes(4),
			ByteConverter.GetBytes(hash2),
			ByteConverter.GetBytes(4),
			ByteConverter.GetBytes(4)
		);

		// Arrange/Act
		var nodeIndexStream = new MemoryStream(nodeIndexEntry.CreateCopy());
		var nodeDataStream = new MemoryStream(nodeData.CreateCopy());
		var repository = new InMemoryRepository(Stream.Null, nodeIndexStream, nodeDataStream);

		// Assert
		var result1 = repository.GetNode(hash1, bytes => bytes.ToArray());
		var result2 = repository.GetNode(hash2, bytes => bytes.ToArray());
		result1.Should().Equal(nodeData[..4]);
		result2.Should().Equal(nodeData[4..8]);
	}

	[Test]
	public void Should_have_snapshot_after_being_reconstituted()
	{
		// Test Data
		var hash = 123UL;
		var snapshotIndex = ArrayX.Concat(ByteConverter.GetBytes(hash), new byte[16]);

		// Arrange/Act
		var snapshotIndexStream = new MemoryStream(snapshotIndex);
		var repository = new InMemoryRepository(snapshotIndexStream, Stream.Null, Stream.Null);

		// Assert
		repository.HasSnapshot(hash).Should().BeTrue();
	}

	[Test]
	public void Should_return_correct_snapshot_after_being_reconstituted()
	{
		// Test Data
		var hash = 123UL;
		var parentHash = 5UL;
		var rootNodeHash = 42UL;
		var snapshotIndexEntry = ArrayX.Concat(
			ByteConverter.GetBytes(hash),
			ByteConverter.GetBytes(parentHash),
			ByteConverter.GetBytes(rootNodeHash)
		);

		// Arrange/Act
		var snapshotIndexStream = new MemoryStream(snapshotIndexEntry.CreateCopy());
		var repository = new InMemoryRepository(snapshotIndexStream, Stream.Null, Stream.Null);

		// Assert
		var actualParentHash = repository.GetSnapshotParent(hash);
		var actualRootNodeHash = repository.GetSnapshotRootNode(hash);
		actualParentHash.Should().Be(parentHash);
		actualRootNodeHash.Should().Be(rootNodeHash);
	}

	[Test]
	public void Should_not_overallocate_node_data_list()
	{
		var nodeDataStream = new MemoryStream(8);
		nodeDataStream.Write(new byte[] { 1, 2, 3 });
		var nodeDataList = new SpannableList<byte>();
		var _ = new InMemoryRepository(
			Stream.Null, Stream.Null, nodeDataStream,
			nodeData: nodeDataList
		);

		AssertionExtensions.Should((int)nodeDataList.Count).Be(3);
	}

	[Test]
	public void Should_set_latest_snapshot_when_reconstituted()
	{
		var hash1 = 1UL;
		var hash2 = 2UL;
		var snapshotIndex = ArrayX.Concat(
			ByteConverter.GetBytes(hash1),
			new byte[16],
			ByteConverter.GetBytes(hash2),
			new byte[16]
		);

		var snapshotIndexStream = new MemoryStream(snapshotIndex.CreateCopy());
		var repository = new InMemoryRepository(snapshotIndexStream, Stream.Null, Stream.Null);

		repository.LatestSnapshot.Should().Be(hash2);
	}
}
