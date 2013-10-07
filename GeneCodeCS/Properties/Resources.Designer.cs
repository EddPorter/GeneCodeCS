﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1008
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GeneCodeCS.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GeneCodeCS.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to A bot fitness evaluation function must be provided..
        /// </summary>
        internal static string BotFitnessFunctionRequired {
            get {
                return ResourceManager.GetString("BotFitnessFunctionRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The index of a bot must be non-negative..
        /// </summary>
        internal static string BotIndexValidRange {
            get {
                return ResourceManager.GetString("BotIndexValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}_Gen{1}_Bot{2}.
        /// </summary>
        internal static string BotNameStringFormat {
            get {
                return ResourceManager.GetString("BotNameStringFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A bot&apos;s name cannot be blank..
        /// </summary>
        internal static string BotNameValidRange {
            get {
                return ResourceManager.GetString("BotNameValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of bots per generation must be greater than zero..
        /// </summary>
        internal static string BotsPerGenerationValidRange {
            get {
                return ResourceManager.GetString("BotsPerGenerationValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chromosome optimisation returned null..
        /// </summary>
        internal static string ChromosomeOptimisationReturnedNull {
            get {
                return ResourceManager.GetString("ChromosomeOptimisationReturnedNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid chromose tree must be provided..
        /// </summary>
        internal static string ChromosomeRequired {
            get {
                return ResourceManager.GetString("ChromosomeRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The elite percentage must be between 0 and 100 inclusive..
        /// </summary>
        internal static string ElitePercentageValidRange {
            get {
                return ResourceManager.GetString("ElitePercentageValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The generation number must be non-negative..
        /// </summary>
        internal static string GenerationNumberValidRange {
            get {
                return ResourceManager.GetString("GenerationNumberValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The maximum tree depth must be non-negative..
        /// </summary>
        internal static string MaxTreeDepthValidRange {
            get {
                return ResourceManager.GetString("MaxTreeDepthValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The branching method must return a bool..
        /// </summary>
        internal static string MethodMustReturnBool {
            get {
                return ResourceManager.GetString("MethodMustReturnBool", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The branching method&apos;s parameters must derive from IParameter&lt;&gt;..
        /// </summary>
        internal static string MethodParametersMustDeriveFromIParameter {
            get {
                return ResourceManager.GetString("MethodParametersMustDeriveFromIParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The mutation rate must be between 0 and 100 inclusive..
        /// </summary>
        internal static string MutationRateValidRange {
            get {
                return ResourceManager.GetString("MutationRateValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A non-null log class must be provided..
        /// </summary>
        internal static string NonNullLogClassRequired {
            get {
                return ResourceManager.GetString("NonNullLogClassRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The reports for the prevoius generation must be provided when breeding subsequent generations..
        /// </summary>
        internal static string PreviousGenerationReportsRequired {
            get {
                return ResourceManager.GetString("PreviousGenerationReportsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The random bot percentage must be between 0 and 100 inclusive..
        /// </summary>
        internal static string RandomBotPercentageValidRange {
            get {
                return ResourceManager.GetString("RandomBotPercentageValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starter bots must contain valid fitness reports..
        /// </summary>
        internal static string StarterBotsFitnessReportMustNotBeNull {
            get {
                return ResourceManager.GetString("StarterBotsFitnessReportMustNotBeNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to At least one candidate terminal method must be provided by the TBot class..
        /// </summary>
        internal static string TerminalMethodsValidRange {
            get {
                return ResourceManager.GetString("TerminalMethodsValidRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid collection of bots is required..
        /// </summary>
        internal static string ValidBotCollectionRequired {
            get {
                return ResourceManager.GetString("ValidBotCollectionRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid bot must be provided..
        /// </summary>
        internal static string ValidBotRequired {
            get {
                return ResourceManager.GetString("ValidBotRequired", resourceCulture);
            }
        }
    }
}
