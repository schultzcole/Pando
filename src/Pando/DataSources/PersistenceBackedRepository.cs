using System;
using System.Collections.Immutable;
using Pando.DataSources.Utils;

namespace Pando.DataSources;

public class PersistenceBackedRepository : IPandoRepository, IDisposable
{
	private readonly MemoryDataSource _mainDataSource;
	private readonly StreamRepository _persistentRepository;

	public PersistenceBackedRepository(MemoryDataSource mainDataSource, StreamRepository persistentRepository)
	{
		_mainDataSource = mainDataSource;
		_persistentRepository = persistentRepository;
	}

	public ulong AddNode(ReadOnlySpan<byte> bytes)
	{
		var hash = HashUtils.ComputeNodeHash(bytes);

		if (_mainDataSource.HasNode(hash)) return hash;

		_mainDataSource.AddNodeWithHashUnsafe(hash, bytes);
		_persistentRepository.AddNodeWithHashUnsafe(hash, bytes);
		return hash;
	}

	public ulong AddSnapshot(ulong parentHash, ulong rootNodeHash)
	{
		var hash = HashUtils.ComputeSnapshotHash(parentHash, rootNodeHash);

		if (_mainDataSource.HasSnapshot(hash)) return hash;

		_mainDataSource.AddSnapshotWithHashUnsafe(hash, parentHash, rootNodeHash);
		_persistentRepository.AddSnapshotWithHashUnsafe(hash, parentHash, rootNodeHash);
		return hash;
	}

	public bool HasNode(ulong hash) => _mainDataSource.HasNode(hash);
	public bool HasSnapshot(ulong hash) => _mainDataSource.HasSnapshot(hash);
	public int SnapshotCount => _mainDataSource.SnapshotCount;

	public T GetNode<T>(ulong hash, in IPandoNodeDeserializer<T> nodeDeserializer) => _mainDataSource.GetNode(hash, in nodeDeserializer);
	public int GetSizeOfNode(ulong hash) => _mainDataSource.GetSizeOfNode(hash);

	public ulong GetSnapshotParent(ulong hash) => _mainDataSource.GetSnapshotParent(hash);
	public ulong GetSnapshotRootNode(ulong hash) => _mainDataSource.GetSnapshotRootNode(hash);

	public IImmutableSet<ulong> GetLeafSnapshotHashes() => _mainDataSource.GetLeafSnapshotHashes();

	public void Dispose() => _persistentRepository.Dispose();
}
