﻿using System;
using System.Collections.Generic;
using System.Linq;
using UISleuth.Messages;

namespace UISleuth.Reflection
{
    internal class UIMessageFinder : IUIMessageFinder
    {
        private static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>();
        private static List<Type> _messageTypes;


        public Type Find(string typeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    return null;
                }

                if (Types.ContainsKey(typeName))
                {
                    return Types[typeName];
                }

                var assemblyType = Type.GetType(typeName, false);

                if (assemblyType != null)
                {
                    if (!assemblyType.IsSubclassOf(typeof(UIMessage)))
                    {
                        assemblyType = null;
                    }

                    Types[typeName] = assemblyType;
                    return assemblyType;
                }

                if (_messageTypes == null)
                {
                    var assemblies = AppDomain
                        .CurrentDomain
                        .GetAssemblies();

                    var sleuthAssemblies = new List<System.Reflection.Assembly>();
                    foreach (var assembly in assemblies)
                    {
                        try
                        {
                            if (ReflectionMethods.IsUISleuthDiscoverable(assembly))
                            {
                                sleuthAssemblies.Add(assembly);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                    //var assembly = assemblies
                    //    .Where(ReflectionMethods.IsUISleuthDiscoverable);
                    _messageTypes = sleuthAssemblies
                        .SelectMany(a => a.GetTypes())
                        .Where(t => t.IsSubclassOf(typeof(UIMessage)))
                        .ToList();
                }

                var found = _messageTypes.FirstOrDefault(t => t.Name.Equals(typeName));

                if (found == null)
                {
                    Types[typeName] = null;
                    return null;
                }

                Types[typeName] = found;

                return found;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
