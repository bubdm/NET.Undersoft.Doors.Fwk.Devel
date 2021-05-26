using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace System.Doors.Data
{
    public static class StructureFormater
    {
        static readonly ConstructorInfo IntPtrCtor = typeof(IntPtr).GetConstructor(new[] { typeof(void*) });
        static readonly MethodInfo MarshalCopy = typeof(Marshal).GetMethod("Copy", new[] { typeof(IntPtr), typeof(byte[]), typeof(int), typeof(int) });
        static readonly MethodInfo MarshalSizeOf = typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) });
        static readonly MethodInfo MarshalPtrToStructure = typeof(Marshal).GetMethod("PtrToStructure", new[] { typeof(IntPtr), typeof(Type) });
        private static class DelegateHolder
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            // ReSharper disable StaticMemberInGenericType
            public static readonly Type TypeOfType = null;
            public static readonly int SizeInBytes = Marshal.SizeOf(TypeOfType);

            public static readonly Func<object,byte[]> Serialize = CreateSerializationDelegate();
            public static readonly Func<byte[], Type, object> Deserialize = CreateDeserializationDelegate();


            //public static byte[] Serialize(Type value)
            //{
            //    IntPtr p = new IntPtr(&value);
            //    byte[] result = new byte[sizeof(Type)];
            //    Marshal.Copy(p, result, 0, result.Length);
            //    return result;
            //}
            private static Func<object, byte[]> CreateSerializationDelegate()
            {
                DynamicMethod dm = new DynamicMethod("SerializeStructure",
                    typeof(byte[]),
                    new[] { typeof(object) },
                    Assembly.GetExecutingAssembly().ManifestModule);
                var generator = dm.GetILGenerator();
                generator.DeclareLocal(typeof(byte[]));

                //IntPtr p = new IntPtr(&value);
                generator.Emit(OpCodes.Ldarga_S, (byte)0);                
                generator.Emit(OpCodes.Conv_U);
                generator.Emit(OpCodes.Newobj, IntPtrCtor);

                //byte[] result = new byte[sizeof(Type)]; 
                //OpCode ldcStructSize = SizeInBytes < sbyte.MaxValue ? OpCodes.Ldc_I4_S : OpCodes.Ldc_I4;
                //generator.Emit(ldcStructSize, SizeInBytes);

                generator.Emit(OpCodes.Ldarg_0);
                generator.EmitCall(OpCodes.Call, MarshalSizeOf, null);
                generator.Emit(OpCodes.Newarr, typeof(byte));

                //Marshal.Copy(p, result, 0, result.Length);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldlen);
                generator.Emit(OpCodes.Conv_I4);
                generator.EmitCall(OpCodes.Call, MarshalCopy, null);

                //return result
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);



                return (Func<object, byte[]>)dm.CreateDelegate(typeof(Func<object, byte[]>));
            }

            //public static obect Deserialize(byte[] data)
            //{
            //    fixed (byte* pData = &data[0])
            //    {
            //        return *(Type*)pData;
            //    }
            //}
            private static Func<byte[], Type, object> CreateDeserializationDelegate()
            {
                DynamicMethod dm = new DynamicMethod("DeserializeStructure",
                                            typeof(object),
                                            new[] { typeof(byte[]), typeof(Type) },
                                            Assembly.GetExecutingAssembly().ManifestModule);
                var generator = dm.GetILGenerator();
                generator.DeclareLocal(typeof(byte).MakePointerType(), pinned: true);

               //fixed (byte* pData = &data[0])
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ldelema, typeof(byte));
                generator.Emit(OpCodes.Stloc_0);

                //return *(Type*)pData;
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Conv_U);
                generator.Emit(OpCodes.Newobj, IntPtrCtor);
                generator.Emit(OpCodes.Ldarg_1);
                generator.EmitCall(OpCodes.Call, MarshalPtrToStructure, null);
                generator.Emit(OpCodes.Ret);
                return (Func<byte[], Type, object>)dm.CreateDelegate(typeof(Func<byte[], Type, object>));
            }

            //private static Func<Type, byte[]> CreateSerializationDelegate()
            //{
            //    var dm = new DynamicMethod("Serialize" + TypeOfType.Name,
            //        typeof(byte[]),
            //        new[] { TypeOfType },
            //        Assembly.GetExecutingAssembly().ManifestModule);
            //    dm.DefineParameter(1, ParameterAttributes.None, "value");

            //    var generator = dm.GetILGenerator();
            //    generator.DeclareLocal(typeof(byte[]));

            //    GenerateIntPtr(generator);
            //    GenerateResultArray(generator);
            //    GenerateCopy(generator);
            //    GenerateReturn(generator);

            //    return (Func<Type, byte[]>)dm.CreateDelegate(typeof(Func<Type, byte[]>));
            //}

            private static void GenerateReturn(ILGenerator generator)
            {
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);
            }

            private static void GenerateCopy(ILGenerator generator)
            {
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldlen);
                generator.Emit(OpCodes.Conv_I4);
                generator.EmitCall(OpCodes.Call, MarshalCopy, null);
            }

            private static void GenerateResultArray(ILGenerator generator)
            {
                generator.Emit(OpCodes.Ldloc_1);
                generator.EmitCall(OpCodes.Call, MarshalSizeOf, null);
                generator.Emit(OpCodes.Newarr, typeof(byte));
            }

            private static void GenerateIntPtr(ILGenerator generator)
            {
                generator.Emit(OpCodes.Ldarga_S, (byte)0);
                generator.Emit(OpCodes.Conv_U);
                generator.Emit(OpCodes.Newobj, IntPtrCtor);
            }
        }


        /// <summary>
        /// Do not check array bounds, possible buffer overflow
        /// </summary>
        public static object Deserialize(byte[] data, Type t)
        {
            return DelegateHolder.Deserialize(data, t);
        }

        /// <summary>
        /// Check array bounds
        /// </summary>
        public static object DeserializeSafe(byte[] data, Type t)
        {
            if (DelegateHolder.SizeInBytes != data.Length)
                throw new ArgumentException(string.Format("Struct size is {0} bytes but array is {1} bytes length", DelegateHolder.SizeInBytes, data.Length));
            return DelegateHolder.Deserialize(data, t);
        }

        /// <summary>
        /// Marshal struct in byte array without any type information
        /// </summary>
        public static byte[] Serialize(this object value)
        {
            return DelegateHolder.Serialize(value);
        }
    }
}