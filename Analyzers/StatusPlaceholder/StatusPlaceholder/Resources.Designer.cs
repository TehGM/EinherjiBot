﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TehGM.Analyzers.PlaceholdersEngine {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TehGM.Analyzers.PlaceholdersEngine.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Placeholder with [Placeholder] attribute cannot be abstract.
        /// </summary>
        internal static string IsAbstract_AnalyzerDescription {
            get {
                return ResourceManager.GetString("IsAbstract_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder &apos;{0}&apos; is abstract. If it&apos;s intended to be used as a base class, remove the [Placeholder] attribute..
        /// </summary>
        internal static string IsAbstract_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("IsAbstract_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder cannot be abstract.
        /// </summary>
        internal static string IsAbstract_AnalyzerTitle {
            get {
                return ResourceManager.GetString("IsAbstract_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder needs to be a class.
        /// </summary>
        internal static string IsClass_AnalyzerDescription {
            get {
                return ResourceManager.GetString("IsClass_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder needs to be a class. Struct and interface placeholders aren&apos;t supported..
        /// </summary>
        internal static string IsClass_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("IsClass_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder needs to be a class.
        /// </summary>
        internal static string IsClass_AnalyzerTitle {
            get {
                return ResourceManager.GetString("IsClass_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generic placeholders with [Placeholder] attribute are not supported.
        /// </summary>
        internal static string IsGeneric_AnalyzerDescription {
            get {
                return ResourceManager.GetString("IsGeneric_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder &apos;{0}&apos; is generic which is not supported. Please note that abstract base classes can be generic..
        /// </summary>
        internal static string IsGeneric_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("IsGeneric_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generic placeholders are not supported.
        /// </summary>
        internal static string IsGeneric_AnalyzerTitle {
            get {
                return ResourceManager.GetString("IsGeneric_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-abstract placeholder should be decorated with [Placeholder] attribute.
        /// </summary>
        internal static string MissingAttribute_AnalyzerDescription {
            get {
                return ResourceManager.GetString("MissingAttribute_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-abstract placeholder &apos;{0}&apos; doesn&apos;t have [Placeholder] attribute. This attribute is required for placeholder to be picked up by the engine..
        /// </summary>
        internal static string MissingAttribute_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("MissingAttribute_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-abstract placeholder should be decorated with [Placeholder] attribute.
        /// </summary>
        internal static string MissingAttribute_AnalyzerTitle {
            get {
                return ResourceManager.GetString("MissingAttribute_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder needs to implement IPlaceholder interface.
        /// </summary>
        internal static string MissingInterface_AnalyzerDescription {
            get {
                return ResourceManager.GetString("MissingInterface_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder &apos;{0}&apos; doesn&apos;t implement IPlaceholder. Implementation of this interface is required for placeholder to run..
        /// </summary>
        internal static string MissingInterface_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("MissingInterface_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Placeholder needs to implement IPlaceholder interface.
        /// </summary>
        internal static string MissingInterface_AnalyzerTitle {
            get {
                return ResourceManager.GetString("MissingInterface_AnalyzerTitle", resourceCulture);
            }
        }
    }
}
