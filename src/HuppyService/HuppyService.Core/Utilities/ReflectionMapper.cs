﻿using HuppyService.Core.Abstraction;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
        public static T Map<T>(object input) where T : class, new()
        {
            T result = new();

            var tProps = result.GetType().GetProperties();
            var inputProps = input.GetType().GetProperties().ToDictionary(e => e.Name, e => e);

            foreach (var prop in tProps)
            {
                inputProps.TryGetValue(prop.Name, out PropertyInfo? matchingProp);

                if (matchingProp is null) continue;

                var inputPropInstance = matchingProp.GetValue(input, null);

                prop.SetValue(result, GetMappedValue(prop.PropertyType, inputPropInstance));
            }

            return result;
        }

        public static ICollection<T>? Map<T>(object[] input) where T  :class, new()
        {
            var result = typeof(List<>).MakeGenericType(typeof(T));

            var tProps = result.GetType().GetProperties();
            var inputProps = input.GetType().GetProperties().ToDictionary(e => e.Name, e => e);

            foreach (var inputObject in input)
            {
                T objectToAdd = new();
                foreach (var prop in tProps)
                {
                    inputProps.TryGetValue(prop.Name, out PropertyInfo? matchingProp);
                    if (matchingProp is null) continue;
                    var inputPropInstance = matchingProp.GetValue(inputObject, null);

                    prop.SetValue(objectToAdd, GetMappedValue(prop.PropertyType, inputPropInstance));
                }

                ((ICollection<T>)result).Add(objectToAdd);
            }

            return result as ICollection<T>;
        }

        public static object? GetMappedValue(Type newValue, object? inputValue)
        {
            if (inputValue is null) return default;

            switch (inputValue)
            {
                case DateTime:
                    if (newValue == typeof(ulong))
                        return Miscellaneous.DateTimeToUnixTimeStamp((DateTime)inputValue);
                    break;
                case ulong:
                    if (newValue == typeof(DateTime))
                        return Miscellaneous.UnixTimeStampToUtcDateTime((ulong)inputValue);
                    break;
                default:
                    return inputValue;
            };

            return inputValue;
        }
    }
}
