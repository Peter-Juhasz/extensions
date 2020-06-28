namespace System
{
    public static class GuidExtensions
    {
        public static Guid? TreatEmptyAsNull(this Guid guid) => guid == Guid.Empty ? null as Guid? : guid;
    }
}
