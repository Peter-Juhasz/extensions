namespace System.Diagnostics
{
    public static class ProcessExtensions
    {
        public static bool WaitForExit(this Process process, TimeSpan timeout) => process.WaitForExit((int)timeout.TotalMilliseconds);
    }
}
