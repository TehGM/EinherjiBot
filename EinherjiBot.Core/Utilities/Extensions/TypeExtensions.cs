namespace TehGM.EinherjiBot
{
    public static class TypeExtensions
    {
        public static bool IsDefaultValue(this Type type, object value)
            => object.Equals(value, GetDefaultValue(type));

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) != null)
                return Activator.CreateInstance(type);
            return null;
        }
    }
}
