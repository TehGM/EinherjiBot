﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TehGM.Analyzers.StatusPlaceholder {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class CodeFixResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CodeFixResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TehGM.Analyzers.StatusPlaceholder.CodeFixResources", typeof(CodeFixResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove abstract keyword.
        /// </summary>
        internal static string IsAbstract_RemoveAbstractKeywordTitle {
            get {
                return ResourceManager.GetString("IsAbstract_RemoveAbstractKeywordTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove [StatusPlaceholder] attribute.
        /// </summary>
        internal static string IsAbstract_RemoveAttributeTitle {
            get {
                return ResourceManager.GetString("IsAbstract_RemoveAttributeTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Convert to class.
        /// </summary>
        internal static string IsClass_ChangeToClassTitle {
            get {
                return ResourceManager.GetString("IsClass_ChangeToClassTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Convert to abstract base class.
        /// </summary>
        internal static string IsGeneric_MakeAbstractTitle {
            get {
                return ResourceManager.GetString("IsGeneric_MakeAbstractTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove generic arguments.
        /// </summary>
        internal static string IsGeneric_RemoveGenericTitle {
            get {
                return ResourceManager.GetString("IsGeneric_RemoveGenericTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Make class abstract.
        /// </summary>
        internal static string MissingAttribute_AddAbstractKeywordTitle {
            get {
                return ResourceManager.GetString("MissingAttribute_AddAbstractKeywordTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add [StatusPlaceholder] attribute.
        /// </summary>
        internal static string MissingAttribute_AddAttributeTitle {
            get {
                return ResourceManager.GetString("MissingAttribute_AddAttributeTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add IStatusPlaceholder interface.
        /// </summary>
        internal static string MissingInterface_AddInterfaceTitle {
            get {
                return ResourceManager.GetString("MissingInterface_AddInterfaceTitle", resourceCulture);
            }
        }
    }
}