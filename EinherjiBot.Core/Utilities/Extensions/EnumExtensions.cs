using System.Reflection;

namespace TehGM.EinherjiBot
{
    public static class EnumExtensions
    {
        // generic
        public static string GetDisplayName<TEnum>(this TEnum value) where TEnum : struct, Enum
            => GetDisplayNameInternal(typeof(TEnum), value.ToString());

        public static IReadOnlyDictionary<TEnum, string> GetValuesAndDisplayNames<TEnum>() where TEnum : struct, Enum
        {
            TEnum[] values = Enum.GetValues<TEnum>();
            return values.ToDictionary(v => v, v => v.GetDisplayName());
        }

        public static IReadOnlyDictionary<string, string> GetDisplayNames<TEnum>() where TEnum : struct, Enum
            => GetDisplayNames(typeof(TEnum));

        public static string GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
            => GetDescription(typeof(TEnum), value.ToString());

        // reflection
        public static IReadOnlyDictionary<string, string> GetDisplayNames(Type enumType)
        {
            string[] values = Enum.GetNames(enumType);
            return values.ToDictionary(v => v, v => GetDisplayNameInternal(enumType, v));
        }

        private static string GetDisplayNameInternal(Type enumType, string memberName)
        {
            MemberInfo valueInfo = enumType.GetMember(memberName).First();
            DisplayNameAttribute attribute = valueInfo.GetCustomAttribute<DisplayNameAttribute>(inherit: true);
            return attribute?.Name ?? valueInfo.Name;
        }

        public static string GetDescription(Type enumType, string memberName)
        {
            MemberInfo valueInfo = enumType.GetMember(memberName).First();
            return valueInfo.GetDescription();
        }
    }
}
