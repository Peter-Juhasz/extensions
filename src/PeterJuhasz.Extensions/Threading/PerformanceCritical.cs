namespace App.Core.Threading;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
public class PerformanceCriticalAttribute : Attribute
{
}
