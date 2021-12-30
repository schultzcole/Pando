﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Pando.DataSources;
using Pando.Exceptions;
using Pando.Repositories.Utils;
using Pando.Serialization;

namespace Pando.Repositories;

public class PandoRepository<T> : IRepository<T>
{
	private readonly IDataSource _dataSource;
	private readonly INodeSerializer<T> _serializer;

	private ulong? _rootSnapshot;
	private readonly Dictionary<ulong, SmallSet<ulong>> _snapshotTreeElements = new();

	public PandoRepository(IDataSource dataSource, INodeSerializer<T> serializer)
	{
		_dataSource = dataSource;
		_serializer = serializer;

		var snapshotCount = _dataSource.SnapshotCount;
		if (snapshotCount > 0)
		{
			_snapshotTreeElements = new Dictionary<ulong, SmallSet<ulong>>(snapshotCount);
			InitializeSnapshotTree(_dataSource.GetLeafSnapshotHashes());
		}
	}

	public ulong SaveRootSnapshot(T tree)
	{
		if (_dataSource.SnapshotCount > 0) throw new AlreadyHasRootSnapshotException();

		var nodeHash = _serializer.Serialize(tree, _dataSource);
		var snapshotHash = _dataSource.AddSnapshot(0UL, nodeHash);
		AddToSnapshotTree(snapshotHash);
		return snapshotHash;
	}

	public ulong SaveSnapshot(T tree, ulong parentHash)
	{
		var nodeHash = _serializer.Serialize(tree, _dataSource);
		var snapshotHash = _dataSource.AddSnapshot(parentHash, nodeHash);
		AddToSnapshotTree(snapshotHash, parentHash);
		return snapshotHash;
	}

	public T GetSnapshot(ulong hash)
	{
		var nodeHash = _dataSource.GetSnapshotRootNode(hash);
		return _dataSource.GetNode(nodeHash, _serializer);
	}

	public SnapshotTree GetSnapshotTree()
	{
		if (_rootSnapshot is null) throw new NoRootSnapshotException();

		return GetSnapshotTreeInternal(_rootSnapshot.Value);
	}

	private SnapshotTree GetSnapshotTreeInternal(ulong hash)
	{
		if (!_snapshotTreeElements.ContainsKey(hash)) throw new HashNotFoundException($"Could not find a snapshot with hash {hash}");

		var children = _snapshotTreeElements[hash];
		var childrenCount = children.Count;
		switch (childrenCount)
		{
			case 0: return new SnapshotTree(hash);
			case 1:
				var list = ImmutableArray.Create(GetSnapshotTreeInternal(children.Single));
				return new SnapshotTree(hash, list);
			default:
				var treeChildren = ImmutableArray.CreateBuilder<SnapshotTree>(childrenCount);
				foreach (var childHash in children.All)
				{
					treeChildren.Add(GetSnapshotTreeInternal(childHash));
				}

				return new SnapshotTree(hash, treeChildren.MoveToImmutable());
		}
	}

	private void InitializeSnapshotTree(IImmutableSet<ulong> leaves)
	{
		foreach (var leaf in leaves)
		{
			_snapshotTreeElements[leaf] = new SmallSet<ulong>();

			var currentHash = leaf;
			var parentHash = _dataSource.GetSnapshotParent(currentHash);
			while (parentHash != 0UL)
			{
				if (_snapshotTreeElements.TryGetValue(parentHash, out var set))
				{
					set.Add(currentHash);
					_snapshotTreeElements[parentHash] = set;
					break; // We've run into an existing hash; that means we've already explored everything above this.
				}

				_snapshotTreeElements.Add(parentHash, new SmallSet<ulong>(currentHash));

				currentHash = parentHash;
				parentHash = _dataSource.GetSnapshotParent(currentHash);
			}

			if (parentHash == 0UL) _rootSnapshot = currentHash;
		}
	}

	private void AddToSnapshotTree(ulong hash)
	{
		_rootSnapshot = hash;
		_snapshotTreeElements[hash] = default;
	}

	private void AddToSnapshotTree(ulong hash, ulong parentHash)
	{
		if (!_snapshotTreeElements.ContainsKey(parentHash)) throw new HashNotFoundException($"Could not find a snapshot with hash {hash}");

		_snapshotTreeElements[hash] = default;
		var children = _snapshotTreeElements[parentHash];
		children.Add(hash);
		_snapshotTreeElements[parentHash] = children;
	}
}
