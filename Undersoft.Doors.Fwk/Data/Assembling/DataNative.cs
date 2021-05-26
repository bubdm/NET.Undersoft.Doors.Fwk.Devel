using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Doors;

namespace System.Doors.Data
{

    public unsafe class DataNative
    {
        public Type ModelType;
        public object ModelObject;
        public Type ObjectType;
        public int ModelSize;
        private ConstructorInfo dataMemberCtor;
        private PropertyInfo[] dataMemberProps;
        private ConstructorInfo intPtrCtor;
        private ConstructorInfo marshalMemberCtor;
        private MethodInfo marshalCopy;
        private MethodInfo marshalSizeOf;
        private MethodInfo marshalPtrToStructure;
        private MethodInfo marshalStructureToPtr;

        private FieldInfo[] marshalStrProps;
        private FieldInfo[] marshalAryProps;
        public FieldInfo[] ModelFields;

        public string ClassName;

        public DataNative(DataPylons pylons, string className)
        {
            ClassName = className;
            ModelType = CompileModelType(pylons, className);
            ModelObject = Activator.CreateInstance(ModelType);
            ObjectType = ModelObject.GetType();
            ModelSize = Marshal.SizeOf(ObjectType);            
            pylons.PylonFields = ModelFields;
            pylons.Select((p, y) => p.PylonField = ModelFields[y]).ToArray();
            pylons.Select((p, y) => p.LineOffset = (int)Marshal.OffsetOf(ObjectType, p.PylonField.Name)).ToArray();
            pylons.Skip(1).Select((p, y) => pylons[y].PylonSize = p.LineOffset - pylons[y].LineOffset).ToArray();
            pylons.Last().PylonSize = Marshal.SizeOf(ObjectType) - pylons.Last().LineOffset;
        }      

        public Type CompileModelType(DataPylons pylons, string className)
        {           

            CreateInternalIdentifiers(pylons);

            TypeBuilder tb = GetTypeBuilder(className);

            dataMemberCtor = typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes);
            dataMemberProps = new[] { typeof(DataMemberAttribute).GetProperty("Order"), typeof(DataMemberAttribute).GetProperty("Name") };
            marshalMemberCtor = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            marshalStrProps = new[] { typeof(MarshalAsAttribute).GetField("SizeConst") };
            marshalAryProps = new[] { typeof(MarshalAsAttribute).GetField("SizeConst"),
                                      typeof(MarshalAsAttribute).GetField("ArraySubType") };
            marshalCopy = typeof(Marshal).GetMethod("Copy", new[] { typeof(IntPtr), typeof(byte[]), typeof(int), typeof(int) });
            marshalSizeOf = typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) });
            marshalPtrToStructure = typeof(Marshal).GetMethod("PtrToStructure", new[] { typeof(IntPtr), typeof(object) });
            marshalStructureToPtr = typeof(Marshal).GetMethod("StructureToPtr", new[] { typeof(object), typeof(IntPtr), typeof(bool) });

            intPtrCtor = typeof(IntPtr).GetConstructor(new[] { typeof(void*) });

            FieldBuilder[] fields = CreateProperty(tb, ref pylons);

            CreatePrimeArrayProperty(tb, fields, pylons);

            CreateItemByIntProperty(tb, fields, pylons);

            CreateItemByStringProperty(tb, fields, pylons);

            //CreateByteArrayProperty(tb, fields, pylons);

            //CreateGetCellGenericMethod(tb, fields, pylons);

            Type objectType = tb.CreateTypeInfo();

            ModelFields = fields;

            return objectType;
        }

        private TypeBuilder GetTypeBuilder(string className)
        {
            string typeSignature = (className != null && className != "") ? className : "DataNative" + DateTime.Now.ToString("yyyyMMddHHmmss");
            AssemblyName an = new AssemblyName(typeSignature);
            
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(typeSignature + "Module");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Serializable | 
                                                                     TypeAttributes.AnsiClass | TypeAttributes.SequentialLayout, typeof(ValueType));            
            /// above implement Hybrid Structure Class, fully referenced with behavior like structure for marshal, gc pointers, serialization.
            /// it's not value type, not copying data. below comment show basic struct attributes implementation - need to add ValueType to typebuilder. 
            /// tb.SetCustomAttribute(new CustomAttributeBuilder(typeof(StructLayoutAttribute).GetConstructor(new[] { typeof(LayoutKind) }), new object[1] { LayoutKind.Sequential },
            ///                                                                                              new[] { typeof(StructLayoutAttribute).GetField("CharSet") }, new object[] { CharSet.Ansi }));
            tb.SetCustomAttribute(new CustomAttributeBuilder(typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
            tb.AddInterfaceImplementation(typeof(IDataNative));
            
            return tb;
        }

        private FieldBuilder[] CreateProperty(TypeBuilder tb, ref DataPylons pylons)
        {
            ModelSize = 0;
            FieldBuilder[] fieldBuilder = new FieldBuilder[pylons.Count];
            for (int i = 0; i < fieldBuilder.Length; i++)
            {
                if (pylons[i].DataType != null)
                {
                    FieldBuilder field = tb.DefineField("_" + pylons[i].PylonName, pylons[i].DataType, FieldAttributes.Public);
                    fieldBuilder[i] = field;
                    PropertyBuilder prop = tb.DefineProperty(pylons[i].PylonName, PropertyAttributes.HasDefault, 
                                                             pylons[i].DataType, new Type[] { pylons[i].DataType });

                    MethodBuilder getter = tb.DefineMethod("get_" + pylons[i].PylonName, MethodAttributes.Public | 
                                                                    MethodAttributes.HideBySig, pylons[i].DataType, 
                                                                    Type.EmptyTypes);
                    prop.SetGetMethod(getter);
                    ILGenerator il = getter.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, field); // foo load
                    il.Emit(OpCodes.Ret); // return

                    MethodBuilder setter = tb.DefineMethod("set_" + pylons[i].PylonName, MethodAttributes.Public | 
                                                                    MethodAttributes.HideBySig, typeof(void), 
                                                                    new Type[] { pylons[i].DataType });
                    prop.SetSetMethod(setter);
                    il = setter.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldarg_1); // value
                    il.Emit(OpCodes.Stfld, field); // foo store
                    il.Emit(OpCodes.Ret);

                    prop.SetCustomAttribute(new CustomAttributeBuilder(
                        dataMemberCtor, new object[0],
                        dataMemberProps, new object[2] { i + 1, pylons[i].PylonName }));                  

                    if (pylons[i].DataType == typeof(string))
                    {

                        field.SetCustomAttribute(new CustomAttributeBuilder(
                          marshalMemberCtor, new object[] { UnmanagedType.ByValTStr },
                          marshalStrProps, new object[] { pylons[i].PylonSize }));
                    }
                    else if (pylons[i].DataType.IsArray)
                    { 

                        field.SetCustomAttribute(new CustomAttributeBuilder(
                            marshalMemberCtor, new object[] { UnmanagedType.ByValArray },
                            marshalAryProps, new object[] { pylons[i].PylonSize, UnmanagedType.U1 }));
                    }

                }
            }

            return fieldBuilder;
        }

        private void CreatePrimeArrayProperty(TypeBuilder tb, FieldBuilder[] fields, DataPylons pylons)
        {
            PropertyInfo prop = typeof(IDataNative).GetProperty("PrimeArray");

            MethodInfo accessor = prop.GetGetMethod();

            ParameterInfo[] args = accessor.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(accessor.Name, accessor.Attributes & ~MethodAttributes.Abstract,
                                                          accessor.CallingConvention, accessor.ReturnType, argTypes);
            tb.DefineMethodOverride(method, accessor);

            ILGenerator il = method.GetILGenerator();
            il.DeclareLocal(typeof(object[]));

            il.Emit(OpCodes.Ldc_I4, fields.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_0);

            for (int i = 0; i < fields.Length; i++)
            {
                il.Emit(OpCodes.Ldloc_0); // this
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldfld, fields[i]); // foo load
                if (pylons[i].DataType.IsValueType)
                {
                    il.Emit(OpCodes.Box, pylons[i].DataType); // box
                }
                il.Emit(OpCodes.Stelem, typeof(object)); // this
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret); // return

            MethodInfo mutator = prop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            method = tb.DefineMethod(mutator.Name, mutator.Attributes & ~MethodAttributes.Abstract,
                                               mutator.CallingConvention, mutator.ReturnType, argTypes);
            tb.DefineMethodOverride(method, mutator);
            il = method.GetILGenerator();
            il.DeclareLocal(typeof(object[]));
           
            il.Emit(OpCodes.Ldarg_1); // value
            il.Emit(OpCodes.Stloc_0);
            for (int i = 0; i < fields.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldloc_0); // this
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem, typeof(object));
                il.Emit(pylons[i].DataType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, pylons[i].DataType); // type
                il.Emit(OpCodes.Stfld, fields[i]); // 
            }
            il.Emit(OpCodes.Ret);
        }

        private void CreateItemByIntProperty(TypeBuilder tb, FieldBuilder[] fields, DataPylons pylons)
        {

            foreach (PropertyInfo prop in typeof(IDataNative).GetProperties())
            {
                MethodInfo accessor = prop.GetGetMethod();
                if (accessor != null)
                {
                    ParameterInfo[] args = accessor.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (args.Length == 1 && argTypes[0] == typeof(int))
                    {
                        MethodBuilder method = tb.DefineMethod(accessor.Name, accessor.Attributes & ~MethodAttributes.Abstract,
                                                          accessor.CallingConvention, accessor.ReturnType, argTypes);
                        tb.DefineMethodOverride(method, accessor);
                        ILGenerator il = method.GetILGenerator();

                        Label[] branches = new Label[fields.Length];
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].DataType != null)
                                branches[i] = il.DefineLabel();
                        }
                        il.Emit(OpCodes.Ldarg_1); // key

                        il.Emit(OpCodes.Switch, branches); // switch
                                                           // default:
                        il.ThrowException(typeof(ArgumentOutOfRangeException));
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].DataType != null)
                            {
                                il.MarkLabel(branches[i]);
                                il.Emit(OpCodes.Ldarg_0); // this
                                il.Emit(OpCodes.Ldfld, fields[i]); // foo load
                                if (pylons[i].DataType.IsValueType)
                                {
                                    il.Emit(OpCodes.Box, pylons[i].DataType); // box
                                }
                                il.Emit(OpCodes.Ret); // end
                            }
                        }
                    }                   
                }


                MethodInfo mutator = prop.GetSetMethod();
                if (mutator != null)
                {
                    ParameterInfo[] args = mutator.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (args.Length == 2 && argTypes[0] == typeof(int) && argTypes[1] == typeof(object))
                    {
                        MethodBuilder method = tb.DefineMethod(mutator.Name, mutator.Attributes & ~MethodAttributes.Abstract,
                                                           mutator.CallingConvention, mutator.ReturnType, argTypes);
                        tb.DefineMethodOverride(method, mutator);
                        ILGenerator il = method.GetILGenerator();                       

                        Label[] branches = new Label[fields.Length];
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].DataType != null)
                                branches[i] = il.DefineLabel();
                        }
                        il.Emit(OpCodes.Ldarg_1); // key

                        il.Emit(OpCodes.Switch, branches); // switch
                                                           // default:
                        il.ThrowException(typeof(ArgumentOutOfRangeException));
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].DataType != null)
                            {
                                il.MarkLabel(branches[i]);
                                il.Emit(OpCodes.Ldarg_0); // this
                                il.Emit(OpCodes.Ldarg_2); // value
                                il.Emit(pylons[i].DataType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, pylons[i].DataType); // type
                                il.Emit(OpCodes.Stfld, fields[i]); // 
                                il.Emit(OpCodes.Ret); // end
                            }
                        }
                    }                    
                }
                
            }                          
        }

        private void CreateItemByStringProperty(TypeBuilder tb, FieldBuilder[] fields, DataPylons pylons)
        {

            foreach (PropertyInfo prop in typeof(IDataNative).GetProperties())
            {
                MethodInfo accessor = prop.GetGetMethod();
                if (accessor != null)
                {
                    ParameterInfo[] args = accessor.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);
                    
                    if (args.Length == 1 && argTypes[0] == typeof(string))
                    {
                        MethodBuilder method = tb.DefineMethod(accessor.Name, accessor.Attributes & ~MethodAttributes.Abstract,
                                                           accessor.CallingConvention, accessor.ReturnType, argTypes);
                        tb.DefineMethodOverride(method, accessor);
                        ILGenerator il = method.GetILGenerator();

                        il.DeclareLocal(typeof(string));

                        Label[] branches = new Label[fields.Length];

                        for (int i = 0; i < fields.Length; i++)
                        {
                            branches[i] = il.DefineLabel();
                        }

                        il.Emit(OpCodes.Ldarg_1); // key
                        il.Emit(OpCodes.Stloc_0);

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].PylonName != null)
                            {
                                il.Emit(OpCodes.Ldloc_0);
                                il.Emit(OpCodes.Ldstr, pylons[i].PylonName);
                                il.EmitCall(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }), null);
                                //il.Emit(OpCodes.Conv_I4);
                                il.Emit(OpCodes.Brtrue, branches[i]);
                            }
                        }

                        il.Emit(OpCodes.Ldnull); // this
                        il.Emit(OpCodes.Ret);

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].PylonName != null)
                            {
                                il.MarkLabel(branches[i]);
                                il.Emit(OpCodes.Ldarg_0); // this
                                il.Emit(OpCodes.Ldfld, fields[i]); // foo load
                                if (pylons[i].DataType.IsValueType)
                                {
                                    il.Emit(OpCodes.Box, pylons[i].DataType); // box
                                }
                                il.Emit(OpCodes.Ret);
                            }
                        }
                    }
                }


                MethodInfo mutator = prop.GetSetMethod();
                if (mutator != null)
                {
                    ParameterInfo[] args = mutator.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (args.Length == 2 && argTypes[0] == typeof(string) && argTypes[1] == typeof(object))
                    {
                        MethodBuilder method = tb.DefineMethod(mutator.Name, mutator.Attributes & ~MethodAttributes.Abstract,
                                                           mutator.CallingConvention, mutator.ReturnType, argTypes);
                        tb.DefineMethodOverride(method, mutator);
                        ILGenerator il = method.GetILGenerator();

                        il.DeclareLocal(typeof(string));

                        Label[] branches = new Label[fields.Length];
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].PylonName != null)
                                branches[i] = il.DefineLabel();
                        }

                        il.Emit(OpCodes.Ldarg_1); // key
                        il.Emit(OpCodes.Stloc_0);

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].PylonName != null)
                            {
                                il.Emit(OpCodes.Ldloc_0);
                                il.Emit(OpCodes.Ldstr, pylons[i].PylonName);
                                il.EmitCall(OpCodes.Call, typeof(string).GetMethod("op_Equality", new[] { typeof(string), typeof(string) }), null);
                                //il.Emit(OpCodes.Conv_I4);
                                il.Emit(OpCodes.Brtrue, branches[i]);
                            }
                        }

                        il.Emit(OpCodes.Ret);

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (pylons[i].PylonName != null)
                            {
                                il.MarkLabel(branches[i]);
                                il.Emit(OpCodes.Ldarg_0); // this
                                il.Emit(OpCodes.Ldarg_2); // value
                                il.Emit(pylons[i].DataType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, pylons[i].DataType); // type
                                il.Emit(OpCodes.Stfld, fields[i]); // 
                                il.Emit(OpCodes.Ret);
                            }
                        }
                    }
                }

            }
        }

        private void CreateGetCellGenericMethod(TypeBuilder tb, FieldBuilder[] fields, DataPylons pylons)
        {
            MethodInfo accessor = typeof(IDataNative).GetMethod("Cell");
            MethodInfo generic = accessor.GetGenericMethodDefinition();
            Type[] genTypes = generic.GetGenericArguments();
            ParameterInfo[] args = generic.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);
            GenericTypeParameterBuilder[] gtb = tb.DefineGenericParameters(new string[] { "T" });
            
            MethodBuilder method =  tb.DefineMethod(generic.Name, generic.Attributes & ~MethodAttributes.Abstract,
                                                          generic.CallingConvention, generic.ReturnType, argTypes);
            tb.DefineMethodOverride(method, generic);

            ILGenerator il = method.GetILGenerator();
            Label[] branches = new Label[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                if (pylons[i].DataType != null)
                    branches[i] = il.DefineLabel();
            }
            il.Emit(OpCodes.Ldarg_1); // key

            il.Emit(OpCodes.Switch, branches); // switch
                                               // default:
            il.ThrowException(typeof(ArgumentOutOfRangeException));
            for (int i = 0; i < fields.Length; i++)
            {
                if (pylons[i].DataType != null)
                {
                    il.MarkLabel(branches[i]);
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, fields[i]); // foo load
                    il.Emit(OpCodes.Ret); // end
                }
            }

        }

        private void CreateByteArrayProperty(TypeBuilder tb, FieldBuilder[] fields, DataPylons pylons)
        {

            MethodInfo mi = typeof(IntPtr).GetMethods().Where(p => p.ReturnType == typeof(void*)).Where(p=>p.GetParameters().Select(a => a.GetType() == typeof(IntPtr)).Any()).FirstOrDefault();

            PropertyInfo prop = typeof(IDataNative).GetProperty("ByteArray");

            MethodInfo accessor = prop.GetGetMethod();

            ParameterInfo[] args = accessor.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(accessor.Name, accessor.Attributes & ~MethodAttributes.Abstract,
                                                          accessor.CallingConvention, accessor.ReturnType, argTypes);
            tb.DefineMethodOverride(method, accessor);

            ILGenerator il = method.GetILGenerator();
            il.DeclareLocal(typeof(byte[]).MakePointerType(), true);
            il.DeclareLocal(typeof(TypedReference));
            il.DeclareLocal(typeof(byte*));
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(byte[]));


            Type t1 = tb.UnderlyingSystemType;
            Type t2 = tb.ReflectedType;
            Type t3 = tb.Module.GetType(ClassName);
            Type t4 = tb.GetTypeInfo();

            il.Emit(OpCodes.Ldarg_0); // this
            il.EmitCall(OpCodes.Call, marshalSizeOf, null); // this
            il.Emit(OpCodes.Stloc_3);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Newarr, typeof(byte));
            il.Emit(OpCodes.Stloc, 4);
            il.Emit(OpCodes.Ldloc, 4);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Mkrefany, t3); // this
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloca, 1);
            il.Emit(OpCodes.Conv_U);           
            il.EmitCall(OpCodes.Call, typeof(IntPtr).GetMethod("ToPointer"), null);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Cpblk);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret); // return

            MethodInfo mutator = prop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            method = tb.DefineMethod(mutator.Name, mutator.Attributes & ~MethodAttributes.Abstract,
                                               mutator.CallingConvention, mutator.ReturnType, argTypes);
            tb.DefineMethodOverride(method, mutator);
            il = method.GetILGenerator();
            il.DeclareLocal(typeof(byte[]).MakePointerType(), true);
            il.DeclareLocal(typeof(TypedReference));
            il.DeclareLocal(typeof(byte[]).MakePointerType());

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Mkrefany, typeof(object)); // this
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloca, 1);
            il.Emit(OpCodes.Conv_U);
            il.EmitCall(OpCodes.Call, typeof(IntPtr).GetMethod("ToPointer"), null);
            il.Emit(OpCodes.Stloc_2);          
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_0); // this
            il.EmitCall(OpCodes.Call, marshalSizeOf, null); // this
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Cpblk);
            il.Emit(OpCodes.Ret); // return
        }

        private void CreateSerialArrayProperty(TypeBuilder tb, FieldBuilder[] fields, DataPylons pylons)
        {

            MethodInfo mi = typeof(IntPtr).GetMethods().Where(p => p.ReturnType == typeof(void*)).Where(p => p.GetParameters().Select(a => a.GetType() == typeof(IntPtr)).Any()).FirstOrDefault();

            PropertyInfo prop = typeof(IDataNative).GetProperty("ByteArray");

            MethodInfo accessor = prop.GetGetMethod();

            ParameterInfo[] args = accessor.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(accessor.Name, accessor.Attributes & ~MethodAttributes.Abstract,
                                                          accessor.CallingConvention, accessor.ReturnType, argTypes);
            tb.DefineMethodOverride(method, accessor);

            ILGenerator il = method.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(byte[]));
            il.DeclareLocal(typeof(IntPtr));
            il.DeclareLocal(typeof(byte[]).MakePointerType(), true);
                     

            il.Emit(OpCodes.Ldarg_0); // this
            il.EmitCall(OpCodes.Call, marshalSizeOf, null); // this
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Newarr, typeof(byte));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Stloc_3);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Newobj, typeof(IntPtr).GetConstructor(new Type[] { typeof(void*) })); // this
            il.Emit(OpCodes.Ldc_I4_0);
            il.EmitCall(OpCodes.Call, marshalStructureToPtr, null);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_3);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret); // return

            MethodInfo mutator = prop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            method = tb.DefineMethod(mutator.Name, mutator.Attributes & ~MethodAttributes.Abstract,
                                               mutator.CallingConvention, mutator.ReturnType, argTypes);
            tb.DefineMethodOverride(method, mutator);
            il = method.GetILGenerator();
            il.DeclareLocal(typeof(IntPtr));
            il.DeclareLocal(typeof(byte[]).MakePointerType(), true);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Newobj, typeof(IntPtr).GetConstructor(new Type[] { typeof(void*) })); // this
            il.Emit(OpCodes.Ldarg_0); // this
            il.EmitCall(OpCodes.Call, marshalPtrToStructure, null);
            il.Emit(OpCodes.Ret); // return
        }

        private void CreateInternalIdentifiers(DataPylons pylons)
        {
            if (!pylons.Have("NOID"))
                pylons.Add(new DataPylon(typeof(Noid), pylons[0].Trell, "NOID", "NOID")
                {
                    PylonSize = 24,
                    isNoid = true,
                    isKey = false,
                    isIdentity = true,
                    Visible = false
                });
        }

        public unsafe byte* DynamicStructureToPtr()
        {
            //structure ptr
            object o = this;
            TypedReference tr = __makeref(o);
            byte* bstruct = (byte*)(*((IntPtr*)&tr)).ToPointer();

            int size = Marshal.SizeOf(ModelObject);
            byte* pserial = (byte*)Marshal.AllocHGlobal(size);

            FieldInfo[] fields = ModelObject.GetType().GetFields();

            int offset = 0;
            int n = fields.Count();
            int fieldSize;

            if (n == 0)
            {
                throw new System.ArgumentException("Empty structure");
            }

            for (int i = 1; i < n; i++)
            {
                //copy field[i-1]
                FieldInfo field = fields[i];
                int fieldOffset = (int)Marshal.OffsetOf(ModelObject.GetType(), field.Name);
                fieldSize = fieldOffset - offset;
                MemoryNative.Copy(bstruct + offset, pserial + offset, (uint)fieldSize);
                offset += fieldSize;
            }

            //last field
            fieldSize = size - offset;
            MemoryNative.Copy(bstruct + offset, pserial + offset, (uint)fieldSize);
            return pserial;
        }
        public unsafe void PtrToDynamicStructure(byte* pserial)
        {
            //structure ptr
            TypedReference tr = __makeref(ModelObject);
            TypedReference* ptr = &tr;
            IntPtr rstruct = *((IntPtr*)ptr);
            byte* bstruct = (byte*)rstruct.ToPointer();

            int size = Marshal.SizeOf(ModelObject);

            FieldInfo[] fields = ModelObject.GetType().GetFields();

            int offset = 0;
            int n = fields.Count();
            int fieldSize;

            if (n == 0 || pserial == null)
            {
                throw new System.ArgumentException("Empty structure or null input");
            }

            for (int i = 1; i < n; i++)
            {
                //copy field[i-1]
                FieldInfo field = fields[i];
                int fieldOffset = (int)Marshal.OffsetOf(ModelObject.GetType(), field.Name);
                fieldSize = fieldOffset - offset;
                MemoryNative.Copy(pserial + offset, bstruct + offset, (uint)fieldSize);
                offset += fieldSize;
            }

            //last field
            fieldSize = size - offset;
            MemoryNative.Copy(pserial + offset, bstruct + offset, (uint)fieldSize);
        }

    }

    public static class NType
    {
        public static Queue<object> New(this Queue<object> queue, Type type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                queue.Enqueue(Activator.CreateInstance(type));
            }
            return queue;
        }

        public static object New(this Type type)
        {
            return Activator.CreateInstance(type);
        }
        public static object[] New(this Type[] types)
        {
            object[] models = new object[types.Length];
            for (int i = 0; i < types.Length; i++)
                models[i] = Activator.CreateInstance(types[i]);
            return models;
        }
    }

    public interface IPropertyAfector { }
    public static class PropertyAfect
    {
        public static void SetProperty(object self, string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(self)[propertyName];
            property.SetValue(self, value);
        }
        public static void SetProperty(object obj, DataTier tier)
        {
            PropertyDescriptorCollection propertys = TypeDescriptor.GetProperties(obj);
            foreach (DataPylon col in tier.Trell.Pylons)
            {
                PropertyDescriptor property = propertys[col.PylonName];
                if (property != null && tier[col.Ordinal] != DBNull.Value)
                    property.SetValue(obj, tier[col.Ordinal]);
            }
        }
        public static object SetProperty(object obj, PropertyDescriptor[] props, DataTier tier)
        {
            object val = obj;
            for (int i = 0; i < props.Length; i++)
                if (props[i] != null && tier[i] != DBNull.Value)
                    props[i].SetValue(val, tier[i]);
            return val;
        }

        public static object GetProperty(object self, string propertyName)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(self)[propertyName];
            return property.GetValue(self);
        }
        public static List<object> GetProperty(object obj, DataTier tier)
        {

            PropertyDescriptorCollection propertys = TypeDescriptor.GetProperties(obj);
            List<object> val = new List<object>();
            foreach (DataPylon col in tier.Trell.Pylons)
            {
                PropertyDescriptor property = propertys[col.PylonName];
                if (property != null)
                    val.Add(property.GetValue(obj));
            }
            return val;
        }
        public static object[] GetProperty(object obj, PropertyDescriptor[] props)
        {
            object[] val = new object[props.Length];
            for (int i = 0; i < props.Length; i++)
                if (props[i] != null)
                    val[i] = props[i].GetValue(obj);
            return val;
        }
    }

    public static class ExtExpandObject
    {
        public static ExpandoObject convertToExpando(object obj)
        {
            //Get Properties Using Reflections
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] properties = obj.GetType().GetProperties(flags);

            //Add Them to a new Expando
            ExpandoObject expando = new ExpandoObject();
            foreach (PropertyInfo property in properties)
            {
                AddProperty(expando, property.Name, property.GetValue(obj, null));
            }

            return expando;
        }

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            //Take use of the NDictionary implementation
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }


    //foreach (MethodInfo function in typeof(IDataNative).GetMethods().Where(f => f.Name == "CopyTo"))
    //{

    //    ParameterInfo[] args = function.GetParameters();
    //    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

    //    if (args.Length == 1 && argTypes[0] == typeof(IntPtr))
    //    {
    //        MethodBuilder method = tb.DefineMethod(function.Name, function.Attributes & ~MethodAttributes.Abstract,
    //                                        function.CallingConvention, typeof(void), argTypes);
    //        tb.DefineMethodOverride(method, function);
    //        ILGenerator il = method.GetILGenerator();

    //        il.Emit(OpCodes.Ldarg_1);
    //        il.Emit(OpCodes.Ldarga_S, (byte)0);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Newobj, intPtrCtor);
    //        il.Emit(OpCodes.Ldc_I4, SizeInBytes);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Cpblk);
    //        il.Emit(OpCodes.Ret);
    //    }
    //    else if (args.Length == 1 && argTypes[0] == typeof(byte[]))
    //    {
    //        MethodBuilder method = tb.DefineMethod(function.Name, function.Attributes & ~MethodAttributes.Abstract,
    //                                        function.CallingConvention, typeof(void), argTypes);
    //        tb.DefineMethodOverride(method, function);
    //        ILGenerator il = method.GetILGenerator();
    //        il.DeclareLocal(typeof(byte).MakePointerType(), pinned: true);
    //        il.DeclareLocal(typeof(byte[]));

    //        il.Emit(OpCodes.Ldarg_1);
    //        il.Emit(OpCodes.Ldc_I4_0);
    //        il.Emit(OpCodes.Ldelema, typeof(byte));
    //        il.Emit(OpCodes.Stloc_0);
    //        il.Emit(OpCodes.Ldloc_0);
    //        il.Emit(OpCodes.Conv_I);
    //        il.Emit(OpCodes.Ldarga_S, (byte)0);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Newobj, intPtrCtor);
    //        il.Emit(OpCodes.Ldc_I4, SizeInBytes);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Cpblk);
    //        il.Emit(OpCodes.Ret);
    //    }
    //}

    //foreach (MethodInfo function in typeof(IDataNative).GetMethods().Where(f => f.Name == "CopyFrom" || f.Name == "ToStructure"))
    //{
    //    ParameterInfo[] args = function.GetParameters();
    //    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

    //    if (args.Length == 1 && argTypes[0] == typeof(IntPtr))
    //    {
    //        MethodBuilder method = tb.DefineMethod(function.Name, function.Attributes & ~MethodAttributes.Abstract,
    //                                        function.CallingConvention, typeof(void), argTypes);
    //        tb.DefineMethodOverride(method, function);
    //        ILGenerator il = method.GetILGenerator();

    //        il.Emit(OpCodes.Ldarga_S, (byte)0);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Newobj, intPtrCtor);
    //        il.Emit(OpCodes.Ldarg_1);
    //        il.Emit(OpCodes.Ldc_I4, SizeInBytes);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Cpblk);
    //        il.Emit(OpCodes.Ret);
    //    }
    //    else if (args.Length == 1 && argTypes[0] == typeof(byte[]))
    //    {
    //        MethodBuilder method = tb.DefineMethod(function.Name, function.Attributes & ~MethodAttributes.Abstract,
    //                                        function.CallingConvention, typeof(void), argTypes);
    //        tb.DefineMethodOverride(method, function);
    //        ILGenerator il = method.GetILGenerator();

    //        il.DeclareLocal(typeof(byte).MakePointerType(), pinned: true);
    //        il.DeclareLocal(typeof(byte[]));

    //        il.Emit(OpCodes.Ldarga_S, (byte)0);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Newobj, intPtrCtor);
    //        il.Emit(OpCodes.Ldarg_1);
    //        il.Emit(OpCodes.Ldc_I4_0);
    //        il.Emit(OpCodes.Ldelema, typeof(byte));
    //        il.Emit(OpCodes.Stloc_0);
    //        il.Emit(OpCodes.Ldloc_0);
    //        il.Emit(OpCodes.Ldc_I4, SizeInBytes);
    //        il.Emit(OpCodes.Conv_U);
    //        il.Emit(OpCodes.Cpblk);
    //        il.Emit(OpCodes.Ret);
    //    }
    //    else if (args.Length == 2 && argTypes[0] == typeof(byte[]) && argTypes[1] == typeof(Type))
    //    {
    //        MethodBuilder method = tb.DefineMethod(function.Name, function.Attributes & ~MethodAttributes.Abstract,
    //                                        function.CallingConvention, function.ReturnType, argTypes);
    //        tb.DefineMethodOverride(method, function);
    //        ILGenerator il = method.GetILGenerator();

    //        il.DeclareLocal(typeof(byte).MakePointerType(), pinned: true);
    //        il.DeclareLocal(argTypes[1]);

    //        //fixed (byte* pData = &data[0])
    //        il.Emit(OpCodes.Ldarg_1);
    //        il.Emit(OpCodes.Ldc_I4_0);
    //        il.Emit(OpCodes.Ldelema, typeof(byte));
    //        il.Emit(OpCodes.Stloc_0);

    //        // return *(T*)pData;
    //        il.Emit(OpCodes.Ldloc_0);
    //        il.Emit(OpCodes.Conv_I);
    //        il.Emit(OpCodes.Ldobj, argTypes[1]);
    //        il.Emit(OpCodes.Ret);
    //    }

    //}

    //else if (args.Length == 2 && argTypes[0] == typeof(long) && argTypes[1] == typeof(byte[]))

    //{
    //    MethodBuilder method = tb.DefineMethod(mutator.Name + "_SetBytes", mutator.Attributes & ~MethodAttributes.Abstract,
    //                                                            mutator.CallingConvention, mutator.ReturnType, argTypes);
    //    tb.DefineMethodOverride(method, mutator);
    //    ILGenerator il = method.GetILGenerator();
    //    //il.DeclareLocal(typeof(byte).MakePointerType(), pinned: true);
    //    //il.DeclareLocal(typeof(byte[]));

    //    Label[] branches = new Label[fields.Length];

    //    for (int i = 0; i < fields.Length; i++)
    //    {
    //        if (pylons[i].DataType != null)
    //            branches[i] = il.DefineLabel();
    //    }
    //    il.Emit(OpCodes.Ldarg_1); // key

    //    il.Emit(OpCodes.Switch, branches); // switch
    //                                       // default:
    //    il.ThrowException(typeof(ArgumentOutOfRangeException));
    //    for (int i = 0; i < fields.Length; i++)
    //    {
    //        if (pylons[i].PylonSize > 1)
    //        {
    //            il.MarkLabel(branches[i]);

    //            il.Emit(OpCodes.Ldarg_0);
    //            il.Emit(OpCodes.Ldc_I4, pylons[i].LineOffset);
    //            il.Emit(OpCodes.Add);
    //            il.Emit(OpCodes.Ldarg_2);
    //            il.Emit(OpCodes.Ldc_I4_0);
    //            il.Emit(OpCodes.Ldelema, typeof(byte));
    //            il.Emit(OpCodes.Stloc_0);
    //            il.Emit(OpCodes.Ldloc_0);
    //            il.Emit(OpCodes.Conv_I);
    //            il.Emit(OpCodes.Ldc_I4, pylons[i].PylonSize);
    //            il.Emit(OpCodes.Conv_U);
    //            il.Emit(OpCodes.Cpblk);
    //            il.Emit(OpCodes.Ret);

    //        }
    //    }
    //}
    //else if (args.Length == 1 && argTypes[0] == typeof(long))
    //{
    //    MethodBuilder method = tb.DefineMethod(accessor.Name + "_SetBytes", accessor.Attributes & ~MethodAttributes.Abstract,
    //                                     accessor.CallingConvention, accessor.ReturnType, argTypes);
    //    tb.DefineMethodOverride(method, accessor);
    //    ILGenerator il = method.GetILGenerator();

    //    //il.DeclareLocal(typeof(byte[]));

    //    Label[] branches = new Label[fields.Length];
    //    for (int i = 0; i < fields.Length; i++)
    //    {
    //        if (pylons[i].DataType != null)
    //            branches[i] = il.DefineLabel();
    //    }
    //    il.Emit(OpCodes.Ldarg_1); // key

    //    il.Emit(OpCodes.Switch, branches); // switch
    //                                       // default:
    //    il.ThrowException(typeof(ArgumentOutOfRangeException));
    //    for (int i = 0; i < fields.Length; i++)
    //    {
    //        if (pylons[i].PylonSize > 1)
    //        {

    //            il.MarkLabel(branches[i]);
    //            OpCode ldcStructSize = pylons[i].PylonSize < sbyte.MaxValue ? OpCodes.Ldc_I4_S : OpCodes.Ldc_I4;
    //            il.Emit(ldcStructSize, pylons[i].PylonSize);
    //            il.Emit(OpCodes.Newarr, typeof(byte));
    //            il.Emit(OpCodes.Stloc_0);
    //            il.Emit(OpCodes.Ldloc_0);
    //            il.Emit(OpCodes.Ldarg_0);
    //            il.Emit(OpCodes.Ldc_I4, pylons[i].LineOffset);
    //            il.Emit(OpCodes.Add);
    //            il.Emit(OpCodes.Ldc_I4, pylons[i].PylonSize);
    //            il.Emit(OpCodes.Conv_U);
    //            il.Emit(OpCodes.Cpblk);
    //            il.Emit(OpCodes.Ldloc_0);
    //            il.Emit(OpCodes.Ret);

    //        }
    //    }
    //}


}
