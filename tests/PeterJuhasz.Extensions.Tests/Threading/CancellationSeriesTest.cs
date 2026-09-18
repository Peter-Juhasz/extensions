namespace System.Extensions.Tests.Threading;

[TestClass]
public class CancellationSeriesTest
{
	[TestMethod]
	public void Read()
	{
		var series = new CancellationSeries(default);
		var token = series.GetNext();
		Assert.IsFalse(token.IsCancellationRequested);
		series.Dispose();
	}

	[TestMethod]
	public void Read_Next()
	{
		var series = new CancellationSeries(default);
		var token1 = series.GetNext();
		Assert.IsFalse(token1.IsCancellationRequested);
		var token2 = series.GetNext();
		Assert.IsTrue(token1.IsCancellationRequested);
		Assert.IsFalse(token2.IsCancellationRequested);
		series.Dispose();
		Assert.IsTrue(token2.IsCancellationRequested);
	}

	[TestMethod]
	public void Read_CancelledAlready()
	{
		using var outerCts = new CancellationTokenSource();
		outerCts.Cancel();

		var series = new CancellationSeries(outerCts.Token);
		var token1 = series.GetNext();
		Assert.IsTrue(token1.IsCancellationRequested);
	}

	[TestMethod]
	public void Read_OuterCancelled()
	{
		using var outerCts = new CancellationTokenSource();

		var series = new CancellationSeries(outerCts.Token);
		var token1 = series.GetNext();
		Assert.IsFalse(token1.IsCancellationRequested);
		outerCts.Cancel();
		Assert.IsTrue(token1.IsCancellationRequested);
	}
}