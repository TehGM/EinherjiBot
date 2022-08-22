using System.Reflection;

namespace TehGM.EinherjiBot
{
    public static class AttributeProviderExtensions
    {
        public static string GetDescription(this ParameterInfo parameter)
            => parameter.GetCustomAttribute<DescriptionAttribute>()?.Description;
        public static string GetDescription(this MemberInfo member)
            => member.GetCustomAttribute<DescriptionAttribute>()?.Description;

        public static string GetDescription(this ICustomAttributeProvider provider, bool inherit = true)
        {
            object attr = provider.GetCustomAttributes(typeof(DescriptionAttribute), inherit).FirstOrDefault();
            return (attr as DescriptionAttribute)?.Description;
        }
    }
}
