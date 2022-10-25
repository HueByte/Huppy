using HuppyService.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuppyService.Core.Utilities
{
    public static class ReflectionMapper
    {
        public static D? Map<T, D>(T input) 
            where T : class
            where D : class, new()
        {
            //var type1 = typeof(T);
            //var type2 = typeof(D);

            // create instance of D
            D result = new();

            // get props of D
            var propsOfD = typeof(D).GetProperties();

            // get props of T
            var propsOfT = input.GetType().GetProperties();

            // map props to each other via names
            foreach (var tProp in propsOfT)
            {
                MappableToAttribute? attributeInfo = tProp.GetCustomAttribute(typeof(MappableToAttribute)) as MappableToAttribute;
                //string searchName = attributeInfo is not null ? attributeInfo.AlternativeName : tProp.Name;

                // get instance of D prop 
                PropertyInfo propInfo;
                if (attributeInfo != null)
                    propInfo = propsOfD.First(prop => prop.Name == tProp.Name || prop.Name == attributeInfo.AlternativeName);
                else
                    propInfo = propsOfD.First(prop => prop.Name == tProp.Name);

                //PropertyInfo propInfo = propsOfD.First(prop => prop.Name == searchName);
                var propInstance = propInfo.GetValue(result, null);

                propInfo.SetValue(result, GetFinalProperty(attributeInfo, propInstance));
            }

            return result;
        }

        public static object? GetFinalProperty(MappableToAttribute? attributeInfo, object? instance)
        {
            // get Mappable attribute
            //MappableToAttribute? attributeInfo = propInfo.GetCustomAttribute(typeof(MappableToAttribute)) as MappableToAttribute;

            // get instance of value
            //var value = propInfo.GetValue(instance, null);
            
            // perform custom mappings
            if (attributeInfo is not null)
            {
                if (instance is DateTime dateValue && attributeInfo.MappableTo == typeof(ulong))
                {
                    return Miscellaneous.DateTimeToUnixTimeStamp(dateValue);
                }
                else if (instance is ulong ulongValue && attributeInfo.MappableTo == typeof(DateTime))
                {
                    return Miscellaneous.UnixTimeStampToUtcDateTime(ulongValue);
                }

                return instance;
            }

            return instance;
        }
    }
}
