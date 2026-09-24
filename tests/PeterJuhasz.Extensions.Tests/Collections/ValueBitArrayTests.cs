using System.Collections;

namespace System.Extensions.Tests.Collections;

[TestClass]
public class ValueBitArrayTests
{
	[TestMethod]
	public void GetRequiredBucketCount_DependsOnBucketSize()
	{
		Assert.AreEqual(0, ValueBitArray<byte>.GetRequiredBucketCount(0));
		Assert.AreEqual(2, ValueBitArray<byte>.GetRequiredBucketCount(9));
		Assert.AreEqual(1, ValueBitArray<ulong>.GetRequiredBucketCount(64));
		Assert.AreEqual(2, ValueBitArray<ulong>.GetRequiredBucketCount(65));
	}

	[TestMethod]
	public void Indexer_SetAndClear_Byte()
	{
		Span<byte> buffer = stackalloc byte[2];
		var bits = new ValueBitArray<byte>(buffer);

		Assert.AreEqual(16, bits.Capacity);
		Assert.IsTrue(bits.IsEmpty());

		bits[9] = true;
		Assert.IsTrue(bits[9]);
		Assert.IsFalse(bits[8]);
		Assert.AreEqual((byte)0b10, buffer[1]);

		bits[9] = false;
		Assert.IsFalse(bits[9]);
		Assert.IsTrue(bits.IsEmpty());
	}

	[TestMethod]
	public void Indexer_SignBit_Long()
	{
		Span<long> buffer = stackalloc long[1];
		var bits = new ValueBitArray<long>(buffer);

		bits[63] = true;

		Assert.IsTrue(bits[63]);
		Assert.IsTrue(bits.Any());
		Assert.AreEqual(long.MinValue, buffer[0]);
	}

	[TestMethod]
	public void All_WhenEveryBitSet()
	{
		Span<int> buffer = stackalloc int[2];
		var bits = new ValueBitArray<int>(buffer);

		for (var i = 0; i < bits.Capacity - 1; i++)
		{
			bits[i] = true;
		}
		Assert.IsFalse(bits.All());

		bits[bits.Capacity - 1] = true;
		Assert.IsTrue(bits.All());

		bits.Clear();
		Assert.IsTrue(bits.IsEmpty());
	}
}
