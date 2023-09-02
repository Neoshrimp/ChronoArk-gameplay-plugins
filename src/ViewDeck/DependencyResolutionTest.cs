#if false

using ChronoArkMod;
using ChronoArkMod.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ViewDeck
{
    internal static class DependencyResolutionTest
    {
        public static void ParseBadAss()
        {
            var assDef = Mono.Cecil.AssemblyDefinition.ReadAssembly("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Chrono Ark\\x64\\Master\\BepInEx\\ViewDeck.dll");

            var typeNames = new List<string>();
            
            //var assDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(fileInfo.FullName);

            // unlike assembly.GetTypes() reading types from AssemblyDefinition doesn't trigger dependency resolution
            foreach (var td in assDef.MainModule.GetTypes())
            {
                try
                {
                    if (td.IsSubtypeOf(typeof(ChronoArkPlugin)))
                    {
                        typeNames.Add(td.FullName);
                    }
                }
                // resolution exception needs to be caught 
                catch (Mono.Cecil.AssemblyResolutionException ex)
                {
                }
            }

/*            Assembly pluginAssembly = Assembly.LoadFrom(fileInfo.FullName);
            foreach (var tn in typeNames) 
            { 
                var type = pluginAssembly.GetType(tn); 
                // do the type instantiation and so on..
            } */
        }

        public static bool IsSubtypeOf(this Mono.Cecil.TypeDefinition self, Type td)
        {
            if (self.FullName == td.FullName)
                return true;
            return self.FullName != "System.Object" && (self.BaseType?.Resolve()?.IsSubtypeOf(td) ?? false);
        }


    }
}
#endif