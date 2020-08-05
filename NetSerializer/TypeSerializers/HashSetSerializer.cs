using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NetSerializer.TypeSerializers
{
    sealed class HashSetSerializer : IStaticTypeSerializer
    {
        public bool Handles(Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genTypeDef = type.GetGenericTypeDefinition();

            return genTypeDef == typeof(HashSet<>);
        }

        public IEnumerable<Type> GetSubtypes(Type type)
        {
            var arg = type.GetGenericArguments()[0];

            var serializedType = typeof(Array).MakeGenericType(arg).MakeArrayType();

            return new[] {serializedType};
        }

        public MethodInfo GetStaticWriter(Type type)
        {
            Debug.Assert(type.IsGenericType);

            if (!type.IsGenericType)
                throw new Exception();

            var genTypeDef = type.GetGenericTypeDefinition();
            
            Debug.Assert(genTypeDef == typeof(HashSet<>));

            var containerType = GetType();

            var reader = containerType.GetMethod("WritePrimitive", BindingFlags.Static | BindingFlags.Public);
            
            var genArgs = type.GetGenericArguments();

            reader = reader!.MakeGenericMethod(genArgs);

            return reader;
        }

        public MethodInfo GetStaticReader(Type type)
        {
            Debug.Assert(type.IsGenericType);

            if (!type.IsGenericType)
                throw new Exception();

            var genTypeDef = type.GetGenericTypeDefinition();
            
            Debug.Assert(genTypeDef == typeof(HashSet<>));

            var containerType = GetType();

            var reader = containerType.GetMethod("ReadPrimitive", BindingFlags.Static | BindingFlags.Public);
            
            var genArgs = type.GetGenericArguments();

            reader = reader!.MakeGenericMethod(genArgs);

            return reader;
        }
    }
}
