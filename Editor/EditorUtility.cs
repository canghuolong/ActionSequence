using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ASQ
{
    public static partial class EditorUtility 
    {
        private static readonly List<Type> _typeList = new List<Type>();
        public static List<Type> ActionClipDataTypes => _typeList;
        
        static EditorUtility()
        {
            List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if(type.IsSubclassOf(typeof(AActionClipData)) && !type.IsAbstract){
                        _typeList.Add(type);
                    }
                }
            }
            _typeList.Sort((t1, t2) => (String.CompareOrdinal(t1.FullName, t2.FullName)));
        }
    }
}
