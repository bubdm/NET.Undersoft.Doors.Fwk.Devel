using System.Linq;
namespace System.Dors
{
    public static class TypeAssemblies
    {
        public static Type GetType(string strFullyQualifiedName, string nameSpace = null)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return type;
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    if(nameSpace == null || type.Namespace == nameSpace)
                        return type;
            }
            return null;
        }
        public static Type GetTypeByAttribute(Type argumentType, object argumentValue, Type attributeType = null, string nameSpace = null)
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in asms)
            {
                Type[] types = nameSpace != null ?
                                    asm.GetTypes().Where(n => n.Namespace == nameSpace).ToArray() :
                                        asm.GetTypes();
                if (attributeType != null)
                {
                    foreach (var type in types)
                        if (type.GetCustomAttributesData().Where(a => a.AttributeType == attributeType).Where(s => s.ConstructorArguments.Where(o => o.ArgumentType == argumentType &&
                                                        o.Value.Equals(argumentValue)).Any()).Any())
                            return type;
                }
                else
                    foreach (var type in types)
                        if (type.GetCustomAttributesData().Where(s => s.ConstructorArguments.Where(o => o.ArgumentType == argumentType &&
                                                        o.Value.Equals(argumentValue)).Any()).Any())
                            return type;
            }
            return null;
        }
    }
}
