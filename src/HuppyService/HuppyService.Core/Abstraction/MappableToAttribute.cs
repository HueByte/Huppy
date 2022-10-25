using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuppyService.Core.Abstraction
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MappableToAttribute : Attribute
    {
        public Type MappableTo;
        public string AlternativeName;
        public MappableToAttribute(Type type, string mappingName)
        {
            MappableTo = type;
            AlternativeName = mappingName;
        }
    }
}
