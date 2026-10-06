using System.Threading.Channels;

namespace System.Extensions.Tests;

[TestClass]
public class MultiContributorWrapperTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task Add_WhileUpdateInProgress_LastContributorObservesUpdate()
	{
		var cancellationToken = TestContext.CancellationToken;
		var wrapper = new MultiContributorWrapper<int>(0, requiredContributors: 2);
		using var updating = new ManualResetEventSlim();
		using var release = new ManualResetEventSlim();

		var applying = Task.Run(() => wrapper.Apply(x =>
		{
			updating.Set();
			release.Wait(cancellationToken);
			return x + 1;
		}), cancellationToken);
		updating.Wait(cancellationToken);

		var adding = Task.Run(() => (isLast: wrapper.Add(), value: wrapper.Value), cancellationToken);

		// give the other contributor a chance to complete while the update is still in progress
		await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
		release.Set();

		Assert.IsFalse(await applying);
		var (isLast, value) = await adding;
		Assert.IsTrue(isLast);
		Assert.AreEqual(1, value);
	}

	[TestMethod]
	public void Apply_AfterRequiredContributorsReached_ThrowsWithoutUpdating()
	{
		var wrapper = new MultiContributorWrapper<int>(0, requiredContributors: 1);
		Assert.IsTrue(wrapper.Apply(x => x + 1));

		Assert.ThrowsExactly<InvalidOperationException>(() => wrapper.Apply(x => x + 1));
		Assert.AreEqual(1, wrapper.Value);
	}
}
