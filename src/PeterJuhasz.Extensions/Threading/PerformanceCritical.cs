namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
public class PerformanceCriticalAttribute : Attribute
{
}
