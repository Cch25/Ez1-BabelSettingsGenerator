using System;
using System.Reflection;
using System.Collections.Generic;

namespace Generator
{
    public static class Extensions
    {
        public static Dictionary<string, Type> GetNestedProperties(this PropertyInfo[] properties, Dictionary<string, Type> dictionary, string current)
        {
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.IsArray)
                {
                    continue;
                }
                else if (property.PropertyType.IsClass && 
                    property.PropertyType != typeof(string))
                {
                    GetNestedProperties(property.PropertyType.GetProperties(), dictionary, $"{current}{property.Name}.");
                    continue;
                }
                dictionary.Add($"{current}{property.Name}", property.PropertyType);
            }
            return dictionary;
        }
    }
}
