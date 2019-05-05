using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace TehGM.EinherjiBot.Extensions
{
    public static class AssemblyExtensions
    {
        public static Type[] FindDerivedTypes(this Assembly assembly, Type baseType, bool includeAbstract = false, bool includeCompilerGenerated = false)
        {
            Type[] allTypesInAsm = assembly.GetTypes();
            List<Type> derivedTypes = new List<Type>(allTypesInAsm.Length);
            for (int i = 0; i < allTypesInAsm.Length; i++)
            {
                Type t = allTypesInAsm[i];
                if (derivedTypes.Contains(t))
                    continue;
                // we can't create abstract class
                if (!includeAbstract && t.IsAbstract)
                    continue;
                if (!includeCompilerGenerated && Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)))
                    continue;
                if (baseType.IsAssignableFrom(t))
                    derivedTypes.Add(t);
            }
            return derivedTypes.ToArray();
        }
    }
}
