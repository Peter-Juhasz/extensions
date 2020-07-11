namespace System.Reflection
{
    public static class ReflectionExtensions
    {
        public static T GetValue<T>(this PropertyInfo property, object instance) => (T)property.GetValue(instance);

        public static T GetValue<T>(this FieldInfo property, object instance) => (T)property.GetValue(instance);


        public static object GetStaticValue(this PropertyInfo property) => property.GetValue(null);
        public static T GetStaticValue<T>(this PropertyInfo property) => (T)property.GetValue(null);

        public static object GetStaticValue(this FieldInfo property) => property.GetValue(null);
        public static T GetStaticValue<T>(this FieldInfo property) => (T)property.GetValue(null);
    }
}
