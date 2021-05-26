using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace System.Doors
{
    #region Memory

    //------------------------> fast copy
    public static class MemoryCopier
    {
        static readonly IMemoryCopier _copier;

        private static AssemblyName _asmName = new AssemblyName() { Name = "MemoryCopier" };
        private static ModuleBuilder _modBuilder;
        private static AssemblyBuilder _asmBuilder;

        static MemoryCopier()
        {
            _asmBuilder = AssemblyBuilder.DefineDynamicAssembly(_asmName, AssemblyBuilderAccess.RunAndCollect);
            //_asmBuilder = Thread.GetDomain().DefineDynamicAssembly(_asmName, AssemblyBuilderAccess.RunAndSave);
            _modBuilder = _asmBuilder.DefineDynamicModule(_asmName.Name + ".dll");
            //_modBuilder = _asmBuilder.DefineDynamicModule(_asmName.Name, _asmName.Name + ".dll", true);

            var typeBuilder = _modBuilder.DefineType("MemoryCopier",
                       TypeAttributes.Public
                       | TypeAttributes.AutoClass
                       | TypeAttributes.AnsiClass
                       | TypeAttributes.Class
                       | TypeAttributes.Serializable
                       | TypeAttributes.BeforeFieldInit);
            typeBuilder.AddInterfaceImplementation(typeof(IMemoryCopier));

            CreateCopyByteArraysUInt(typeBuilder);
            CreateCopyPointersUInt(typeBuilder);
           
            CreateCopyByteArraysULong(typeBuilder);
            CreateCopyPointersULong(typeBuilder);

            //CreateStructureToArray(typeBuilder);
            CreateArrayToStructure(typeBuilder);

            CreateStructureToPtr(typeBuilder);
            CreatePtrToStructure(typeBuilder);

            //RR
            CreateStructureToPtrBlkRR(typeBuilder);
            //RR
            CreatePtrToStructureBlkRR(typeBuilder);

            //RR
            CreateStructureToArrayBlkRR(typeBuilder);
            //DH
            CreateArrayToStructureBlkDH(typeBuilder);
            
            //RR
            CreateArrayToStructureBlkRR(typeBuilder);

            TypeInfo copierType = typeBuilder.CreateTypeInfo();
            _copier = (IMemoryCopier)Activator.CreateInstance(copierType);          
        }

        private static void CreateCopyByteArraysUInt(TypeBuilder tb)
        {
            var copyMethod = tb.DefineMethod("Copy",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte[]), typeof(byte[]), typeof(uint), typeof(uint) });
            var code = copyMethod.GetILGenerator();

            code.DeclareLocal(typeof(byte[]).MakePointerType(), pinned: true);
            code.DeclareLocal(typeof(byte[]).MakePointerType(), pinned: true);

            //updated by Darek
            code.Emit(OpCodes.Ldarg_2);
            code.Emit(OpCodes.Ldc_I4_0);
            code.Emit(OpCodes.Ldelema, typeof(byte));   //managed pointer, a powinno byc unmanaged
            code.Emit(OpCodes.Stloc_0);
            code.Emit(OpCodes.Ldloc_0);
            code.Emit(OpCodes.Ldarg_1);
            code.Emit(OpCodes.Ldarg_3);
            code.Emit(OpCodes.Ldelema, typeof(byte));
            code.Emit(OpCodes.Stloc_1);
            code.Emit(OpCodes.Ldloc_1);
            code.Emit(OpCodes.Ldarg, 4);
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);
            code.Emit(OpCodes.Ret);

            tb.DefineMethodOverride(copyMethod, typeof(IMemoryCopier).GetMethod("Copy", new Type[] { typeof(byte[]), typeof(byte[]), typeof(uint), typeof(uint) }));
        }     
        private static void CreateCopyPointersUInt(TypeBuilder tb)
        {
            var copyMethod = tb.DefineMethod("Copy",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte*), typeof(byte*), typeof(uint), typeof(uint) });
            var code = copyMethod.GetILGenerator();

            //updated by Darek
            code.Emit(OpCodes.Ldarg_2);
            code.Emit(OpCodes.Ldarg_1);
            code.Emit(OpCodes.Ldarg_3);
            code.Emit(OpCodes.Add);
            code.Emit(OpCodes.Ldarg, 4);
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);
            code.Emit(OpCodes.Ret);


            ////org from source
            //code.Emit(OpCodes.Ldarg_2);
            //code.Emit(OpCodes.Ldc_I4_0);
            //code.Emit(OpCodes.Ldelema, typeof(byte));
            //code.Emit(OpCodes.Ldarg_1);
            //code.Emit(OpCodes.Ldarg_3);
            //code.Emit(OpCodes.Ldelema, typeof(byte));
            //code.Emit(OpCodes.Ldarg, 4);
            //code.Emit(OpCodes.Cpblk);
            //code.Emit(OpCodes.Ret);
            ////-----
            ///

            tb.DefineMethodOverride(copyMethod, typeof(IMemoryCopier).GetMethod("Copy", new Type[] { typeof(byte*), typeof(byte*), typeof(uint), typeof(uint) }));
        }

        private static void CreateCopyByteArraysULong(TypeBuilder tb)
        {
            var copyMethod = tb.DefineMethod("Copy",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte[]), typeof(byte[]), typeof(ulong), typeof(ulong) });
            var code = copyMethod.GetILGenerator();

            code.DeclareLocal(typeof(byte[]).MakePointerType(), pinned: true);
            code.DeclareLocal(typeof(byte[]).MakePointerType(), pinned: true);

            //updated by Darek
            code.Emit(OpCodes.Ldarg_2);
            code.Emit(OpCodes.Ldc_I4_0);
            code.Emit(OpCodes.Ldelema, typeof(byte));   //managed pointer, a powinno byc unmanaged
            code.Emit(OpCodes.Stloc_0);
            code.Emit(OpCodes.Ldloc_0);
            code.Emit(OpCodes.Ldarg_1);
            code.Emit(OpCodes.Ldarg_3);
            code.Emit(OpCodes.Ldelema, typeof(byte));
            code.Emit(OpCodes.Stloc_1);
            code.Emit(OpCodes.Ldloc_1);
            code.Emit(OpCodes.Ldarg, 4);
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);
            code.Emit(OpCodes.Ret);


            ////org from source
            //code.Emit(OpCodes.Ldarg_2);
            //code.Emit(OpCodes.Ldc_I4_0);
            //code.Emit(OpCodes.Ldelema, typeof(byte));
            //code.Emit(OpCodes.Ldarg_1);
            //code.Emit(OpCodes.Ldarg_3);
            //code.Emit(OpCodes.Ldelema, typeof(byte));
            //code.Emit(OpCodes.Ldarg, 4);
            //code.Emit(OpCodes.Cpblk);
            //code.Emit(OpCodes.Ret);
            ////-----
            ///

            tb.DefineMethodOverride(copyMethod, typeof(IMemoryCopier).GetMethod("Copy", new Type[] { typeof(byte[]), typeof(byte[]), typeof(ulong), typeof(ulong) }));
        }
        private static void CreateCopyPointersULong(TypeBuilder tb)
        {
            var copyMethod = tb.DefineMethod("Copy",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte*), typeof(byte*), typeof(ulong), typeof(ulong) });
            var code = copyMethod.GetILGenerator();

            //updated by Darek
            code.Emit(OpCodes.Ldarg_2);
            code.Emit(OpCodes.Ldarg_1);
            code.Emit(OpCodes.Ldarg_3);
            code.Emit(OpCodes.Add);
            code.Emit(OpCodes.Ldarg, 4);
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);
            code.Emit(OpCodes.Ret);


            ////org from source
            //code.Emit(OpCodes.Ldarg_2);
            //code.Emit(OpCodes.Ldc_I4_0);
            //code.Emit(OpCodes.Ldelema, typeof(byte));
            //code.Emit(OpCodes.Ldarg_1);
            //code.Emit(OpCodes.Ldarg_3);
            //code.Emit(OpCodes.Ldelema, typeof(byte));
            //code.Emit(OpCodes.Ldarg, 4);
            //code.Emit(OpCodes.Cpblk);
            //code.Emit(OpCodes.Ret);
            ////-----
            ///

            tb.DefineMethodOverride(copyMethod, typeof(IMemoryCopier).GetMethod("Copy", new Type[] { typeof(byte*), typeof(byte*), typeof(ulong), typeof(ulong) } ));
        }

        //private static void CreateStructureToArray(TypeBuilder tb)
        //{
        //    var strToPtr = tb.DefineMethod("StructToPtr",
        //                                     MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
        //                                     typeof(byte[]),
        //                                     new Type[] { typeof(object) });

        //    ILGenerator il = strToPtr.GetILGenerator();
        //    il.DeclareLocal(typeof(int));
        //    il.DeclareLocal(typeof(byte[]));
        //    il.DeclareLocal(typeof(IntPtr));
        //    il.DeclareLocal(typeof(byte[]).MakePointerType(), true);

        //    il.Emit(OpCodes.Ldarg_1); // this
        //    il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) }), null); // this
        //    il.Emit(OpCodes.Stloc_0);
        //    il.Emit(OpCodes.Ldloc_0);
        //    il.Emit(OpCodes.Newarr, typeof(byte));
        //    il.Emit(OpCodes.Stloc_1);
        //    il.Emit(OpCodes.Ldloc_1);
        //    il.Emit(OpCodes.Ldc_I4_0);
        //    il.Emit(OpCodes.Ldelema, typeof(byte));
        //    il.Emit(OpCodes.Stloc_3);
        //    il.Emit(OpCodes.Ldloc_3);
        //    il.Emit(OpCodes.Conv_U);
        //    il.Emit(OpCodes.Stloc_2);
        //    il.Emit(OpCodes.Ldarg_1);
        //    il.Emit(OpCodes.Ldloc_2);
        //    il.Emit(OpCodes.Newobj, typeof(IntPtr).GetConstructor(new Type[] { typeof(void*) })); // this
        //    il.Emit(OpCodes.Ldc_I4_0);
        //    il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("StructureToPtr", new[] { typeof(object), typeof(IntPtr), typeof(bool) }), null);
        //    il.Emit(OpCodes.Ldc_I4_0);
        //    il.Emit(OpCodes.Conv_U);
        //    il.Emit(OpCodes.Stloc_3);
        //    il.Emit(OpCodes.Ldloc_1);
        //    il.Emit(OpCodes.Ret); // return          

        //    tb.DefineMethodOverride(strToPtr, typeof(IMemoryCopier).GetMethod("StructToPtr", new Type[] { typeof(object) }));
        //}
        private static void CreateArrayToStructure(TypeBuilder tb)
        {
            var ptrToStruct = tb.DefineMethod("PtrToStruct",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(object),
                                             new Type[] { typeof(byte[]), typeof(Type) });

            ILGenerator il = ptrToStruct.GetILGenerator();
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
            il.Emit(OpCodes.Ldarg_2); // this
            il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("PtrToStructure", new[] { typeof(IntPtr), typeof(Type) }), null);
            il.Emit(OpCodes.Ret); // return

            tb.DefineMethodOverride(ptrToStruct, typeof(IMemoryCopier).GetMethod("PtrToStruct", new Type[] { typeof(byte[]), typeof(Type) }));
        }

        private static void CreateStructureToPtr(TypeBuilder tb)
        {
            var strToPtr = tb.DefineMethod("StructToPtr",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(object), typeof(byte*) });

            ILGenerator il = strToPtr.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(byte[]));
            il.DeclareLocal(typeof(IntPtr));
            il.DeclareLocal(typeof(byte[]).MakePointerType(), true);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Newobj, typeof(IntPtr).GetConstructor(new Type[] { typeof(void*) })); // this
            il.Emit(OpCodes.Ldc_I4_0);
            il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("StructureToPtr", new[] { typeof(object), typeof(IntPtr), typeof(bool) }), null);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_3);
            il.Emit(OpCodes.Ret); // return          

            tb.DefineMethodOverride(strToPtr, typeof(IMemoryCopier).GetMethod("StructToPtr", new Type[] { typeof(object), typeof(byte*) }));
        }
        private static void CreatePtrToStructure(TypeBuilder tb)
        {
            var ptrToStruct = tb.DefineMethod("PtrToStruct",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(object),
                                             new Type[] { typeof(byte*), typeof(Type) });

            ILGenerator il = ptrToStruct.GetILGenerator();
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
            il.Emit(OpCodes.Ldarg_2); // this
            il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("PtrToStructure", new[] { typeof(IntPtr), typeof(Type) }), null);
            il.Emit(OpCodes.Ret); // return

            tb.DefineMethodOverride(ptrToStruct, typeof(IMemoryCopier).GetMethod("PtrToStruct", new Type[] { typeof(byte*), typeof(Type) }));
        }

        /*private static void CreateStructureToPtrBlk(TypeBuilder tb)
        {
            var strToPtr = tb.DefineMethod("StructToPtr",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(object), typeof(byte*).MakeByRefType() });

            ILGenerator il = strToPtr.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(byte[]));
            il.DeclareLocal(typeof(IntPtr));
            il.DeclareLocal(typeof(byte[]).MakePointerType(), true);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Newobj, typeof(IntPtr).GetConstructor(new Type[] { typeof(void*) })); // this
            il.Emit(OpCodes.Ldc_I4_0);
            il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("StructureToPtr", new[] { typeof(object), typeof(IntPtr), typeof(bool) }), null);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stloc_3);
            il.Emit(OpCodes.Ret); // return          

            tb.DefineMethodOverride(strToPtr, typeof(IMemoryCopier).GetMethod("StructToPtr", new Type[] { typeof(object), typeof(byte*).MakeByRefType() }));
        }
        private static void CreatePtrToStructureBlk(TypeBuilder tb)
        {
            var ptrToStruct = tb.DefineMethod("PtrToStruct",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(object),
                                             new Type[] { typeof(byte*).MakeByRefType(), typeof(object).MakeByRefType() });

            ILGenerator il = ptrToStruct.GetILGenerator();
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
            il.Emit(OpCodes.Ldarg_2); // this
            il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("PtrToStructure", new[] { typeof(IntPtr), typeof(Type) }), null);
            il.Emit(OpCodes.Ret); // return

            tb.DefineMethodOverride(ptrToStruct, typeof(IMemoryCopier).GetMethod("PtrToStruct", new Type[] { typeof(byte*).MakeByRefType(), typeof(Type).MakeByRefType() }));
        }*/

        //RR
        private static void CreateStructureToArrayBlkRR(TypeBuilder tb)
        {
            var structToPtrMethod = tb.DefineMethod("StructToPtr",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(byte[]),
                                             new Type[] { typeof(object) });
            var code = structToPtrMethod.GetILGenerator();


            //TypedReference tr = __makeref(structure);
            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            //byte* pserial = (byte*)Marshal.AllocHGlobal(size);
            //MemoryCopier.Copy(bstruct, pserial, 0, size); <- CpBlk
            //ptr = pserial;

            code.DeclareLocal(typeof(int));             //[0] size of structure
            code.DeclareLocal(typeof(TypedReference));  //[1] reference to structure (tr)            
            code.DeclareLocal(typeof(byte*));           //[2] [uint8*] //pstructure
            code.DeclareLocal(typeof(byte[]));           //[3] [uint8*] //pserial
            code.DeclareLocal(typeof(byte[]).MakePointerType(), true);           //[3] [uint8*] //pserial


            //size of structure [Marshal.SizeOf(structure)
            code.Emit(OpCodes.Ldarg_1); // ld ValueType structure
            code.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) }), null); // size of structure
            code.Emit(OpCodes.Stloc_0); //size

            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            code.Emit(OpCodes.Ldarga, 1);   //&structure;
            code.Emit(OpCodes.Mkrefany, typeof(object)); //TypedReference tr = __makeref(structure);
            code.Emit(OpCodes.Stloc_1); //TypedReference
            code.Emit(OpCodes.Ldloca, 1); // tr => &tr
            code.Emit(OpCodes.Conv_U); // (&tr) => (IntPtr)
            code.Emit(OpCodes.Ldind_I); // (IntPtr) => IntPtr*
            //--->

            MethodInfo method_IntPtr_op_Explicit = typeof(IntPtr).GetMethods().Where(m => m.Name == "op_Explicit").Where(m => m.ReturnType == typeof(void*)).FirstOrDefault();
            code.EmitCall(OpCodes.Call, method_IntPtr_op_Explicit, null); // (IntPtr*) => *(IntPtr*)
            code.Emit(OpCodes.Stloc, 2);    //*(IntPtr) => byte* pstructure

            //code.Emit(OpCodes.Ldarg_2); //ptr_arg
            code.Emit(OpCodes.Ldloc_0); //size
            code.Emit(OpCodes.Newarr, typeof(byte));           
            code.Emit(OpCodes.Stloc_3);

            //MemoryCopier
            code.Emit(OpCodes.Ldloc_3); //dest: pserial
            code.Emit(OpCodes.Ldc_I4_0);
            code.Emit(OpCodes.Ldelema, typeof(byte));
            code.Emit(OpCodes.Stloc, 4);
            code.Emit(OpCodes.Ldloc, 4);
            code.Emit(OpCodes.Ldloc_2); //source: pstructure
            code.Emit(OpCodes.Ldloc_0);  //size: 
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);

            // A address
            // V value            
            //Stind: A <- V (A stores the pointer to V) 
            //ref ptr = pserial
            //code.Emit(OpCodes.Ldarg_2); // ptr 
            //code.Emit(OpCodes.Ldloc_3); // pserial
            //code.Emit(OpCodes.Stind_I); // into (ref byte* ptr) => address of pserial, ptr refer now to new allocation pserial

            code.Emit(OpCodes.Ldloc_3); // pserial
            code.Emit(OpCodes.Ret); // return   

            tb.DefineMethodOverride(structToPtrMethod, typeof(IMemoryCopier).GetMethod("StructToPtr", new Type[] { typeof(object) }));
        }

        //RR 
        private static void CreateStructureToPtrBlkRR(TypeBuilder tb)
        {
            var structToPtrMethod = tb.DefineMethod("StructToPtr",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(object), typeof(byte*).MakeByRefType() });
            var code = structToPtrMethod.GetILGenerator();


            //TypedReference tr = __makeref(structure);
            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            //byte* pserial = (byte*)Marshal.AllocHGlobal(size);
            //MemoryCopier.Copy(bstruct, pserial, 0, size); <- CpBlk
            //ptr = pserial;

            code.DeclareLocal(typeof(int));             //[0] size of structure
            code.DeclareLocal(typeof(TypedReference));  //[1] reference to structure (tr)            
            code.DeclareLocal(typeof(byte*));           //[2] [uint8*] //pstructure
            code.DeclareLocal(typeof(byte*));           //[3] [uint8*] //pserial


            //size of structure [Marshal.SizeOf(structure)
            code.Emit(OpCodes.Ldarg_1); // ld ValueType structure
            code.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) }), null); // size of structure
            code.Emit(OpCodes.Stloc_0); //size

            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            code.Emit(OpCodes.Ldarga, 1);   //&structure;
            code.Emit(OpCodes.Mkrefany, typeof(object)); //TypedReference tr = __makeref(structure);
            code.Emit(OpCodes.Stloc_1); //TypedReference
            code.Emit(OpCodes.Ldloca, 1); // tr => &tr
            code.Emit(OpCodes.Conv_U); // (&tr) => (IntPtr)
            code.Emit(OpCodes.Ldind_I); // (IntPtr) => IntPtr*
            //--->

            MethodInfo method_IntPtr_op_Explicit = typeof(IntPtr).GetMethods().Where(m => m.Name == "op_Explicit").Where(m => m.ReturnType == typeof(void*)).FirstOrDefault();
            code.EmitCall(OpCodes.Call, method_IntPtr_op_Explicit, null); // (IntPtr*) => *(IntPtr*)
            code.Emit(OpCodes.Stloc, 2);    //*(IntPtr) => byte* pstructure

            //code.Emit(OpCodes.Ldarg_2); //ptr_arg
            code.Emit(OpCodes.Ldloc_0); //size
            code.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("AllocHGlobal", new Type[] { typeof(int) }), null); //return allocate IntPtr
            code.EmitCall(OpCodes.Call, method_IntPtr_op_Explicit, null);   //IntPtr (Alloc) => void*
            code.Emit(OpCodes.Stloc_3);

            //MemoryCopier
            code.Emit(OpCodes.Ldloc_3); //dest: pserial
            code.Emit(OpCodes.Ldloc_2); //source: pstructure
            code.Emit(OpCodes.Ldloc_0);  //size: 
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);

            // A address
            // V value            
            //Stind: A <- V (A stores the pointer to V) 
            //ref ptr = pserial
            code.Emit(OpCodes.Ldarg_2); // ptr 
            code.Emit(OpCodes.Ldloc_3); // pserial
            code.Emit(OpCodes.Stind_I); // into (ref byte* ptr) => address of pserial, ptr refer now to new allocation pserial


            code.Emit(OpCodes.Ret); // return   

            tb.DefineMethodOverride(structToPtrMethod, typeof(IMemoryCopier).GetMethod("StructToPtr", new Type[] { typeof(object), typeof(byte*).MakeByRefType() }));
        }

        //RR
        private static void CreatePtrToStructureBlkRR(TypeBuilder tb)
        {
            var ptrToStructMethod = tb.DefineMethod("PtrToStruct",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte*).MakeByRefType(), typeof(object).MakeByRefType() });
            var code = ptrToStructMethod.GetILGenerator();

            //int size = Marshal.SizeOf(structure.GetType());
            //TypedReference tr = __makeref(structure);
            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            //MemoryCopier.Copy(pserial, bstruct, 0, size);

            code.DeclareLocal(typeof(int));             //[0] size of structure
            code.DeclareLocal(typeof(TypedReference));  //[1] reference to structure (tr)            
            code.DeclareLocal(typeof(byte*));           //[2] [uint8*] //pstructure

            //size of structure [Marshal.SizeOf(structure)
            code.Emit(OpCodes.Ldarg_2); // ld ValueType structure
            code.Emit(OpCodes.Ldind_Ref);
            code.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) }), null); // size of structure
            code.Emit(OpCodes.Stloc_0); //size

            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            code.Emit(OpCodes.Ldarg, 2);   //&structure;
            code.Emit(OpCodes.Mkrefany, typeof(object)); //TypedReference tr = __makeref(structure);
            code.Emit(OpCodes.Stloc_1); //TypedReference
            code.Emit(OpCodes.Ldloca, 1); // tr => &tr
            code.Emit(OpCodes.Conv_U); // (&tr) => (IntPtr)
            code.Emit(OpCodes.Ldind_I); // (IntPtr) => IntPtr*

            MethodInfo method_IntPtr_op_Explicit = typeof(IntPtr).GetMethods().Where(m => m.Name == "op_Explicit").Where(m => m.ReturnType == typeof(void*)).FirstOrDefault();
            code.EmitCall(OpCodes.Call, method_IntPtr_op_Explicit, null); // (IntPtr*) => *(IntPtr*)
            code.Emit(OpCodes.Stloc, 2);    //*(IntPtr) => byte* pstructure

            //CpyBlk
            code.Emit(OpCodes.Ldloc_2); //dest: pstructure
            code.Emit(OpCodes.Ldarg_1); //source: ld ptr
            code.Emit(OpCodes.Ldind_I); //source: ld to stack addr of ptr
            code.Emit(OpCodes.Ldloc_0);  //size: 
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);

            code.Emit(OpCodes.Ret); // return   

            tb.DefineMethodOverride(ptrToStructMethod, typeof(IMemoryCopier).GetMethod("PtrToStruct", 
                                                                                          new Type[] 
                                                                                          {
                                                                                              typeof(byte*).MakeByRefType(),
                                                                                              typeof(object).MakeByRefType()
                                                                                          }));
        }

        //DH
        private static void CreateArrayToStructureBlkDH(TypeBuilder tb)
        {
            var ptrToStructMethod = tb.DefineMethod("PtrToStruct",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte[]).MakeByRefType(), typeof(object).MakeByRefType() });
            var code = ptrToStructMethod.GetILGenerator();

            //int size = Marshal.SizeOf(structure.GetType());
            //TypedReference tr = __makeref(structure);
            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            //MemoryCopier.Copy(pserial, bstruct, 0, size);

            code.DeclareLocal(typeof(int));             //[0] size of structure
            code.DeclareLocal(typeof(TypedReference));  //[1] reference to structure (tr)            
            code.DeclareLocal(typeof(byte*));           //[2] [uint8*] //pstructure
            code.DeclareLocal(typeof(byte[]).MakePointerType(), true);

            //size of structure [Marshal.SizeOf(structure)
            code.Emit(OpCodes.Ldarg_2); // ld ValueType structure
            code.Emit(OpCodes.Ldind_Ref);
            code.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("SizeOf", new[] { typeof(object) }), null); // size of structure
            code.Emit(OpCodes.Stloc_0); //size

            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            code.Emit(OpCodes.Ldarg, 2);   //&structure;
            code.Emit(OpCodes.Mkrefany, typeof(object)); //TypedReference tr = __makeref(structure);
            code.Emit(OpCodes.Stloc_1); //TypedReference
            code.Emit(OpCodes.Ldloca, 1); // tr => &tr
            code.Emit(OpCodes.Conv_U); // (&tr) => (IntPtr)
            code.Emit(OpCodes.Ldind_I); // (IntPtr) => IntPtr*

            MethodInfo method_IntPtr_op_Explicit = typeof(IntPtr).GetMethods().Where(m => m.Name == "op_Explicit").Where(m => m.ReturnType == typeof(void*)).FirstOrDefault();
            code.EmitCall(OpCodes.Call, method_IntPtr_op_Explicit, null); // (IntPtr*) => *(IntPtr*)
            code.Emit(OpCodes.Stloc, 2);    //*(IntPtr) => byte* pstructure

            //CpyBlk
            code.Emit(OpCodes.Ldloc_2); //dest: pstructure
            code.Emit(OpCodes.Ldarg_1); //source: ld ptr
            code.Emit(OpCodes.Ldind_I); //source: ld to stack addr of ptr
            code.Emit(OpCodes.Ldc_I4_0);
            code.Emit(OpCodes.Ldelema, typeof(byte));
            code.Emit(OpCodes.Stloc_3);
            code.Emit(OpCodes.Ldloc_3);
          
            code.Emit(OpCodes.Ldloc_0);  //size: 
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);

            code.Emit(OpCodes.Ret); // return   

            tb.DefineMethodOverride(ptrToStructMethod, typeof(IMemoryCopier).GetMethod("PtrToStruct", 
                                                                                        new Type[] 
                                                                                        {
                                                                                            typeof(byte[]).MakeByRefType(),
                                                                                            typeof(object).MakeByRefType()
                                                                                        }));
        }


        private static void CreateArrayToStructureBlkRR(TypeBuilder tb)
        {
            var ptrToStructMethod = tb.DefineMethod("ArrayToStruct",
                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                             typeof(void),
                                             new Type[] { typeof(byte[]), typeof(ValueType).MakeByRefType() });
            var code = ptrToStructMethod.GetILGenerator();

            //int size = Marshal.SizeOf(structure.GetType());
            //TypedReference tr = __makeref(structure);
            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            //fixed(byte* pserial = &barray[0] {
            //MemoryCopier.Copy(pserial, bstruct, 0, size); }
            //

            code.DeclareLocal(typeof(int));             //[0] size of structure
            code.DeclareLocal(typeof(TypedReference));  //[1] reference to structure (tr)            
            code.DeclareLocal(typeof(byte*));           //[2] [uint8*] //pstructure
            code.DeclareLocal(typeof(byte*));                           //[3] [uint8*] //pserial 
            code.DeclareLocal(typeof(byte[]).MakePointerType(), true);  //[4] [uint8& pinned] // pinned &barray[0]

            //size of structure [Marshal.SizeOf(structure)
            code.Emit(OpCodes.Ldarg_2); // ld ValueType structure
            code.Emit(OpCodes.Ldind_Ref);
            code.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("SizeOf", new[] { typeof(ValueType) }), null); // size of structure
            code.Emit(OpCodes.Stloc_0); //size

            //byte* bstruct = (byte*)*((IntPtr*)&tr);
            code.Emit(OpCodes.Ldarg, 2);   //&structure;
            code.Emit(OpCodes.Mkrefany, typeof(ValueType)); //TypedReference tr = __makeref(structure);
            code.Emit(OpCodes.Stloc_1); //TypedReference
            code.Emit(OpCodes.Ldloca, 1); // tr => &tr
            code.Emit(OpCodes.Conv_U); // (&tr) => (IntPtr)
            code.Emit(OpCodes.Ldind_I); // (IntPtr) => IntPtr*

            MethodInfo method_IntPtr_op_Explicit = typeof(IntPtr).GetMethods().Where(m => m.Name == "op_Explicit").Where(m => m.ReturnType == typeof(void*)).FirstOrDefault();
            code.EmitCall(OpCodes.Call, method_IntPtr_op_Explicit, null); // (IntPtr*) => *(IntPtr*)
            code.Emit(OpCodes.Stloc, 2);    //*(IntPtr) => byte* pstructure


            //fixed (byte* parray = &array[0])

            code.Emit(OpCodes.Ldarg_1); //load barray, which stores the address to array            
            code.Emit(OpCodes.Ldc_I4_0);  //x=0
            code.Emit(OpCodes.Ldelema, typeof(byte)); //get address of the x element of array, i.e., &(barray[x=0])
            code.Emit(OpCodes.Stloc, 4); //pinned (&array[0]), i.e., a pointer-address as a pinned array

            //convert pinned (&barray[x=0) => *parray
            code.Emit(OpCodes.Ldloc, 4); // 
            code.Emit(OpCodes.Conv_U); //
            code.Emit(OpCodes.Stloc_3); // parray = (&array[x=0])

            //Copy Blk
            code.Emit(OpCodes.Ldloc_2); //dest: *pstructure
            code.Emit(OpCodes.Ldloc_3); //source: *parray
            code.Emit(OpCodes.Ldloc_0); //size: 
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Cpblk);

            //clear pinned?
            code.Emit(OpCodes.Ldc_I4_0);
            code.Emit(OpCodes.Conv_U);
            code.Emit(OpCodes.Stloc, 4);
            code.Emit(OpCodes.Ret);

            tb.DefineMethodOverride(ptrToStructMethod, typeof(IMemoryCopier).GetMethod("ArrayToStruct", new Type[] { typeof(byte[]), typeof(ValueType).MakeByRefType() }));
        }



        public static void Copy(byte[] source, byte[] dest, uint offset, uint count)
        {
            _copier.Copy(source, dest, offset, count);
        }         
        public static unsafe void Copy(byte* source, byte* dest, uint offset, uint count)
        {
            _copier.Copy(source, dest, offset, count);
        }

        public static void Copy(byte[] source, byte[] dest, ulong offset, ulong count)
        {
            _copier.Copy(source, dest, offset, count);
        }
        public static unsafe void Copy(byte* source, byte* dest, ulong offset, ulong count)
        {
            _copier.Copy(source, dest, offset, count);
        }

        public static byte[] StructToPtr(this object structure)
        {
            return _copier.StructToPtr(structure);
        }
        //public unsafe static void StructToPtr(this object structure, byte* ptr)
        //{
        //    _copier.StructToPtr(structure, ptr);
        //}

        public static object PtrToStruct(this byte[] binary, Type structure)
        {
            //return _copier.PtrToStruct(binary, structure);
            object o = Activator.CreateInstance(structure);
            PtrToStruct(ref binary, ref o);
            return o;
        }
        public static object PtrToStruct(this Type structure, byte[] binary)
        {
            //return _copier.PtrToStruct(binary, structure);

            object o = Activator.CreateInstance(structure);
            PtrToStruct(ref binary, ref o);
            return o;
        }
        public unsafe static object PtrToStruct(this Type structure, byte* binary)
        {
            // return _copier.PtrToStruct(binary, structure);

            object o = Activator.CreateInstance(structure);
            PtrToStruct(ref binary, ref o);
            return o;
        }
        public unsafe static void PtrToStruct(this byte[] binary, object structure)
        {
            // return _copier.PtrToStruct(binary, structure);

            PtrToStruct(ref binary, ref structure);
        }
        public unsafe static void PtrToStruct(this object structure, byte* binary)
        {
            // return _copier.PtrToStruct(binary, structure);

            PtrToStruct(ref binary, ref structure);
        }
        public unsafe static void PtrToStruct(this object structure, byte[] binary)
        {
            // return _copier.PtrToStruct(binary, structure);

            PtrToStruct(ref binary, ref structure);
        }

        //RR
        public unsafe static void StructToPtr(this object structure, ref byte* ptr)
        {
            _copier.StructToPtr(structure, ref ptr);
        }      

        //RR
        public unsafe static void PtrToStruct(ref byte* ptr, ref object structure)
        {
            _copier.PtrToStruct(ref ptr, ref structure);
        }
        //RR
        public unsafe static void PtrToStruct(ref byte[] ptr, ref object structure)
        {
            _copier.PtrToStruct(ref ptr, ref structure);
        }

        //RR
        public unsafe static void ArrayToStruct(byte[] array, ref ValueType structure)
        {
            _copier.ArrayToStruct(array, ref structure);
        }

    }

    public interface IMemoryCopier
    {
        void Copy(byte[] source, byte[] dest, ulong offset, ulong count);
        unsafe void Copy(byte* source, byte* dest, ulong offset, ulong count);

        void Copy(byte[] source, byte[] dest, uint offset, uint count);
        unsafe void Copy(byte* source, byte* dest, uint offset, uint count);

        byte[] StructToPtr(object structure);
        unsafe void StructToPtr(object structure, byte* ptr);

        object PtrToStruct(byte[] binary, Type structure);
        unsafe object PtrToStruct(byte* ptr, Type structure);

        /*unsafe void StructToPtr(object structure, ref byte* ptr);

        unsafe void PtrToStruct(ref byte* ptr, ref object structure);*/
        
        //RR
        unsafe void StructToPtr(object structure, ref byte* ptr);

        //RR
        unsafe void PtrToStruct(ref byte* ptr, ref object structure);
        //RR
        unsafe void PtrToStruct(ref byte[] ptr, ref object structure);

        //RR
        unsafe void ArrayToStruct(byte[] array, ref ValueType structure);        

    }

    //<----------------------------

    public static class MemoryNative
    {
        static MemoryNative()
        { }
      
        public static unsafe void Copy(byte[] dest, byte[] src, uint count)
        {
            MemoryCopier.Copy(src, dest, 0, count);        
        }
        public static unsafe void Copy(IntPtr dest, IntPtr src, uint count)
        {
            MemoryCopier.Copy((byte*)src.ToPointer(), (byte*)dest.ToPointer(), 0, count);
        }
        public static unsafe void Copy(byte* dest, byte* src, uint count)
        {
            MemoryCopier.Copy(src, dest, 0, count);           
        }
        public static unsafe void Copy(void* dest, void* src, uint count)
        {
            MemoryCopier.Copy((byte*)src, (byte*)dest, 0, count);
        }

        public static unsafe void Copy(byte[] dest, byte[] src, ulong count)
        {
            MemoryCopier.Copy(src, dest, 0, count);      
        }
        public static unsafe void Copy(IntPtr dest, IntPtr src, ulong count)
        {
             MemoryCopier.Copy((byte*)src.ToPointer(), (byte*)dest.ToPointer(), 0, count);
        }
        public static unsafe void Copy(byte* dest, byte* src, ulong count)
        {
            MemoryCopier.Copy(src, dest, 0, count);

            //byte* destptr = dest;
            //byte* srcptr = src;
            //ulong lenght64 = count / 8;
            //ulong lenght8 = count % 8;
            //for (uint i = 0; i < lenght64; i++)
            //{
            //    *((long*)destptr) = *((long*)srcptr);
            //    destptr += 8;
            //    srcptr += 8;
            //}
            //for (uint i = 0; i < lenght8; i++)
            //{
            //    *destptr = *srcptr;
            //    destptr++;
            //    srcptr++;
            //}
        }
        public static unsafe void Copy(void* dest, void* src, ulong count)
        {
            MemoryCopier.Copy((byte*)src, (byte*)dest, 0, count);

            //byte* destptr = (byte*)dest;
            //byte* srcptr = (byte*)src;
            //ulong lenght64 = count / 8;
            //ulong lenght8 = count % 8;
            //for (uint i = 0; i < lenght64; i++)
            //{
            //    *((long*)destptr) = *((long*)srcptr);
            //    destptr += 8;
            //    srcptr += 8;
            //}
            //for (uint i = 0; i < lenght8; i++)
            //{
            //    *destptr = *srcptr;
            //    destptr++;
            //    srcptr++;
            //}
        }

        //public static unsafe byte[] StructToBytes(object structure)
        //{

        //    int count = Marshal.SizeOf(structure);
        //    byte[] barray = new byte[count];
        //    fixed (byte* dest = &barray[0])
        //    {
        //        int lenght64 = count / 8;
        //        int lenght8 = count % 8;
        //        byte* destptr = dest;

        //        GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //        byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //        for (uint i = 0; i < lenght64; i++)
        //        {
        //            *((long*)destptr) = *((long*)srcptr);
        //            destptr += 8;
        //            srcptr += 8;
        //        }
        //        for (uint i = 0; i < lenght8; i++)
        //        {
        //            *destptr = *srcptr;
        //            destptr++;
        //            srcptr++;
        //        }
        //        handle.Free();
        //    }
        //    return barray;
        //}
        //public static unsafe byte[] StructToBytes(object[] structures)
        //{
        //    if (structures.Length > 0)
        //    {
        //        int count = Marshal.SizeOf(structures[0]);
        //        byte[] barray = new byte[count * structures.Length];
        //        fixed (byte* dest = &barray[0])
        //        {
        //            int lenght64 = count / 8;
        //            int lenght8 = count % 8;
        //            byte* destptr = dest;
        //            foreach (object structure in structures)
        //            {
        //                GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //                byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //                for (uint i = 0; i < lenght64; i++)
        //                {
        //                    *((long*)destptr) = *((long*)srcptr);
        //                    destptr += 8;
        //                    srcptr += 8;
        //                }
        //                for (uint i = 0; i < lenght8; i++)
        //                {
        //                    *destptr = *srcptr;
        //                    destptr++;
        //                    srcptr++;
        //                }
        //                handle.Free();
        //            }
        //        }
        //        return barray;
        //    }
        //    return null;
        //}
        //public static unsafe void StructToBytes(object structure, ref byte[] barray, int offset = 0)
        //{

        //    int count = Marshal.SizeOf(structure);
        //    fixed (byte* dest = &barray[offset])
        //    {
        //        int lenght64 = count / 8;
        //        int lenght8 = count % 8;
        //        byte* destptr = dest;

        //        GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //        byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //        for (uint i = 0; i < lenght64; i++)
        //        {
        //            *((long*)destptr) = *((long*)srcptr);
        //            destptr += 8;
        //            srcptr += 8;
        //        }
        //        for (uint i = 0; i < lenght8; i++)
        //        {
        //            *destptr = *srcptr;
        //            destptr++;
        //            srcptr++;
        //        }
        //        handle.Free();
        //    }
        //}
        //public static unsafe void StructToBytes(object[] structures, ref byte[] barray, int offset = 0)
        //{
        //    if (structures.Length > 0)
        //    {
        //        int count = Marshal.SizeOf(structures[0]);

        //        fixed (byte* dest = &barray[offset])
        //        {
        //            int lenght64 = count / 8;
        //            int lenght8 = count % 8;
        //            byte* destptr = dest;
        //            foreach (object structure in structures)
        //            {
        //                GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //                byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //                for (uint i = 0; i < lenght64; i++)
        //                {
        //                    *((long*)destptr) = *((long*)srcptr);
        //                    destptr += 8;
        //                    srcptr += 8;
        //                }
        //                for (uint i = 0; i < lenght8; i++)
        //                {
        //                    *destptr = *srcptr;
        //                    destptr++;
        //                    srcptr++;
        //                }
        //                handle.Free();
        //            }
        //        }
        //    }
        //}
    }

    //public static class MemoryNative
    //{
    //    static MemoryNative()
    //    { }

    //    public static unsafe void MemCopy(byte* dest, byte* src, Point[] counts)
    //    {
    //        byte* destptr = dest;
    //        for (int i = 0; i < counts.Length; i++)
    //        {
    //            byte* srcptr = src + (uint)counts[i].Y;
    //            int count = counts[i].X;
    //            for (uint x = 0; i < count; x++)
    //            {
    //                *destptr = *srcptr;
    //                destptr++;
    //                srcptr++;
    //            }
    //        }
    //    }

    //    public static unsafe void Copy(IntPtr dest, IntPtr src, uint count)
    //    {
    //        byte* destptr = (byte*)dest.ToPointer();
    //        byte* srcptr = (byte*)src.ToPointer();
    //        uint lenght64 = count / 8;
    //        uint lenght8 = count % 8;
    //        for (uint i = 0; i < lenght64; i++)
    //        {
    //            *((long*)destptr) = *((long*)srcptr);
    //            destptr += 8;
    //            srcptr += 8;
    //        }
    //        for (uint i = 0; i < lenght8; i++)
    //        {
    //            *destptr = *srcptr;
    //            destptr++;
    //            srcptr++;
    //        }
    //    }
    //    public static unsafe void Copy(byte* dest, byte* src, uint count)
    //    {
    //        byte* destptr = dest;
    //        byte* srcptr = src;
    //        uint lenght64 = count / 8;
    //        uint lenght8 = count % 8;
    //        for (uint i = 0; i < lenght64; i++)
    //        {
    //            *((long*)destptr) = *((long*)srcptr);
    //            destptr += 8;
    //            srcptr += 8;
    //        }
    //        for (uint i = 0; i < lenght8; i++)
    //        {
    //            *destptr = *srcptr;
    //            destptr++;
    //            srcptr++;
    //        }
    //    }
    //    public static unsafe void Copy(void* dest, void* src, uint count)
    //    {
    //        byte* destptr = (byte*)dest;
    //        byte* srcptr = (byte*)src;
    //        uint lenght64 = count / 8;
    //        uint lenght8 = count % 8;
    //        for (uint i = 0; i < lenght64; i++)
    //        {
    //            *((long*)destptr) = *((long*)srcptr);
    //            destptr += 8;
    //            srcptr += 8;
    //        }
    //        for (uint i = 0; i < lenght8; i++)
    //        {
    //            *destptr = *srcptr;
    //            destptr++;
    //            srcptr++;
    //        }
    //    }

    //    public static unsafe void Copy(IntPtr dest, IntPtr src, ulong count)
    //    {
    //        byte* destptr = (byte*)dest.ToPointer();
    //        byte* srcptr = (byte*)src.ToPointer();
    //        ulong lenght64 = count / 8;
    //        ulong lenght8 = count % 8;
    //        for (uint i = 0; i < lenght64; i++)
    //        {
    //            *((long*)destptr) = *((long*)srcptr);
    //            destptr += 8;
    //            srcptr += 8;
    //        }
    //        for (uint i = 0; i < lenght8; i++)
    //        {
    //            *destptr = *srcptr;
    //            destptr++;
    //            srcptr++;
    //        }
    //    }
    //    public static unsafe void Copy(byte* dest, byte* src, ulong count)
    //    {
    //        byte* destptr = dest;
    //        byte* srcptr = src;
    //        ulong lenght64 = count / 8;
    //        ulong lenght8 = count % 8;
    //        for (uint i = 0; i < lenght64; i++)
    //        {
    //            *((long*)destptr) = *((long*)srcptr);
    //            destptr += 8;
    //            srcptr += 8;
    //        }
    //        for (uint i = 0; i < lenght8; i++)
    //        {
    //            *destptr = *srcptr;
    //            destptr++;
    //            srcptr++;
    //        }
    //    }
    //    public static unsafe void Copy(void* dest, void* src, ulong count)
    //    {
    //        byte* destptr = (byte*)dest;
    //        byte* srcptr = (byte*)src;
    //        ulong lenght64 = count / 8;
    //        ulong lenght8 = count % 8;
    //        for (uint i = 0; i < lenght64; i++)
    //        {
    //            *((long*)destptr) = *((long*)srcptr);
    //            destptr += 8;
    //            srcptr += 8;
    //        }
    //        for (uint i = 0; i < lenght8; i++)
    //        {
    //            *destptr = *srcptr;
    //            destptr++;
    //            srcptr++;
    //        }
    //    }


    //    public static unsafe byte[] StructToBytes(object structure)
    //    {

    //        int count = Marshal.SizeOf(structure);
    //        byte[] barray = new byte[count];
    //        fixed (byte* dest = &barray[0])
    //        {
    //            int lenght64 = count / 8;
    //            int lenght8 = count % 8;
    //            byte* destptr = dest;

    //            GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
    //            byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

    //            for (uint i = 0; i < lenght64; i++)
    //            {
    //                *((long*)destptr) = *((long*)srcptr);
    //                destptr += 8;
    //                srcptr += 8;
    //            }
    //            for (uint i = 0; i < lenght8; i++)
    //            {
    //                *destptr = *srcptr;
    //                destptr++;
    //                srcptr++;
    //            }
    //            handle.Free();
    //        }
    //        return barray;
    //    }
    //    public static unsafe byte[] StructToBytes(object[] structures)
    //    {
    //        if (structures.Length > 0)
    //        {
    //            int count = Marshal.SizeOf(structures[0]);
    //            byte[] barray = new byte[count * structures.Length];
    //            fixed (byte* dest = &barray[0])
    //            {
    //                int lenght64 = count / 8;
    //                int lenght8 = count % 8;
    //                byte* destptr = dest;
    //                foreach (object structure in structures)
    //                {
    //                    GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
    //                    byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

    //                    for (uint i = 0; i < lenght64; i++)
    //                    {
    //                        *((long*)destptr) = *((long*)srcptr);
    //                        destptr += 8;
    //                        srcptr += 8;
    //                    }
    //                    for (uint i = 0; i < lenght8; i++)
    //                    {
    //                        *destptr = *srcptr;
    //                        destptr++;
    //                        srcptr++;
    //                    }
    //                    handle.Free();
    //                }
    //            }
    //            return barray;
    //        }
    //        return null;
    //    }
    //    public static unsafe void StructToBytes(object structure, ref byte[] barray, int offset = 0)
    //    {

    //        int count = Marshal.SizeOf(structure);
    //        fixed (byte* dest = &barray[offset])
    //        {
    //            int lenght64 = count / 8;
    //            int lenght8 = count % 8;
    //            byte* destptr = dest;

    //            GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
    //            byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

    //            for (uint i = 0; i < lenght64; i++)
    //            {
    //                *((long*)destptr) = *((long*)srcptr);
    //                destptr += 8;
    //                srcptr += 8;
    //            }
    //            for (uint i = 0; i < lenght8; i++)
    //            {
    //                *destptr = *srcptr;
    //                destptr++;
    //                srcptr++;
    //            }
    //            handle.Free();
    //        }
    //    }
    //    public static unsafe void StructToBytes(object[] structures, ref byte[] barray, int offset = 0)
    //    {
    //        if (structures.Length > 0)
    //        {
    //            int count = Marshal.SizeOf(structures[0]);

    //            fixed (byte* dest = &barray[offset])
    //            {
    //                int lenght64 = count / 8;
    //                int lenght8 = count % 8;
    //                byte* destptr = dest;
    //                foreach (object structure in structures)
    //                {
    //                    GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
    //                    byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

    //                    for (uint i = 0; i < lenght64; i++)
    //                    {
    //                        *((long*)destptr) = *((long*)srcptr);
    //                        destptr += 8;
    //                        srcptr += 8;
    //                    }
    //                    for (uint i = 0; i < lenght8; i++)
    //                    {
    //                        *destptr = *srcptr;
    //                        destptr++;
    //                        srcptr++;
    //                    }
    //                    handle.Free();
    //                }
    //            }
    //        }
    //    }
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public MemoryProtection AllocationProtect;
        public IntPtr RegionSize;
        public MemoryState State;
        public MemoryProtection Protect;
        public MemoryType Type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnicodeString : IComparable<UnicodeString>, IEquatable<UnicodeString>, IDisposable
    {
        public UnicodeString(string str)
        {
            if (str != null)
            {
                UnicodeString newString;

                if (!Win32.RtlCreateUnicodeString(out newString, str))
                    throw new OutOfMemoryException();

                this = newString;
            }
            else
            {
                this.Length = 0;
                this.MaximumLength = 0;
                this.Buffer = IntPtr.Zero;
            }
        }

        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;

        public int CompareTo(UnicodeString unicodeString, bool caseInsensitive)
        {
            return Win32.RtlCompareUnicodeString(ref this, ref unicodeString, caseInsensitive);
        }

        public int CompareTo(UnicodeString unicodeString)
        {
            return this.CompareTo(unicodeString, false);
        }

        public void Dispose()
        {
            if (this.Buffer == IntPtr.Zero)
                return;

            Win32.RtlFreeUnicodeString(ref this);
            this.Buffer = IntPtr.Zero;
        }

        /// <summary>
        /// Copies the string to a newly allocated string.
        /// </summary>
        public UnicodeString Duplicate()
        {
            NtStatus status;
            UnicodeString newString;

            if ((status = Win32.RtlDuplicateUnicodeString(
                RtlDuplicateUnicodeStringFlags.AllocateNullString |
                RtlDuplicateUnicodeStringFlags.NullTerminate,
                ref this, out newString)) >= NtStatus.Error)
                Win32.Throw(status);

            return newString;
        }

        public bool Equals(UnicodeString unicodeString, bool caseInsensitive)
        {
            return Win32.RtlEqualUnicodeString(ref this, ref unicodeString, caseInsensitive);
        }

        public bool Equals(UnicodeString unicodeString)
        {
            return this.Equals(unicodeString, false);
        }

        //public override int GetHashCode()
        //{
        //    return this.Hash();
        //}

        //public int Hash(HashStringAlgorithm algorithm, bool caseInsensitive)
        //{
        //    NtStatus status;
        //    int hash;

        //    if ((status = Win32.RtlHashUnicodeString(ref this,
        //        caseInsensitive, algorithm, out hash)) >= NtStatus.Error)
        //        Win32.Throw(status);

        //    return hash;
        //}

        //public int Hash(HashStringAlgorithm algorithm)
        //{
        //    return this.Hash(algorithm, false);
        //}

        //public int Hash()
        //{
        //    return this.Hash(HashStringAlgorithm.Default);
        //}

        public string Read()
        {
            if (this.Length == 0)
                return "";

            return Marshal.PtrToStringUni(this.Buffer, this.Length / 2);
        }

        //public string Read(ProcessHandle processHandle)
        //{
        //    if (this.Length == 0)
        //        return "";

        //    byte[] strData = processHandle.ReadMemory(this.Buffer, this.Length);
        //    GCHandle strDataHandle = GCHandle.Alloc(strData, GCHandleType.Pinned);

        //    try
        //    {
        //        return Marshal.PtrToStringUni(strDataHandle.AddrOfPinnedObject(), this.Length / 2);
        //    }
        //    finally
        //    {
        //        strDataHandle.Free();
        //    }
        //}

        public bool StartsWith(UnicodeString unicodeString, bool caseInsensitive)
        {
            return Win32.RtlPrefixUnicodeString(ref this, ref unicodeString, caseInsensitive);
        }

        public bool StartsWith(UnicodeString unicodeString)
        {
            return this.StartsWith(unicodeString, false);
        }

        public AnsiString ToAnsiString()
        {
            NtStatus status;
            AnsiString ansiStr = new AnsiString();

            if ((status = Win32.RtlUnicodeStringToAnsiString(ref ansiStr, ref this, true)) >= NtStatus.Error)
                Win32.Throw(status);

            return ansiStr;
        }

        public override string ToString()
        {
            return this.Read();
        }

        public AnsiString ToUpperAnsiString()
        {
            NtStatus status;
            AnsiString ansiStr = new AnsiString();

            if ((status = Win32.RtlUpcaseUnicodeStringToAnsiString(ref ansiStr, ref this, true)) >= NtStatus.Error)
                Win32.Throw(status);

            return ansiStr;
        }
    }

    [Flags]
    public enum AllocFlags : uint
    {
        LHnd = 0x42,
        LMemFixed = 0x0,
        LMemMoveable = 0x2,
        LMemZeroInit = 0x40,
        LPtr = 0x40,
        NonZeroLHnd = LMemMoveable,
        NonZeroLPtr = LMemFixed
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        AccessDenied = 0x0,
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        Guard = 0x100,
        NoCache = 0x200,
        WriteCombine = 0x400,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08
    }

    public enum MemoryMapType : int
    {
        Process = 1,
        System = 2
    }

    public enum MemoryInformationClass : int
    {
        MemoryBasicInformation,
        MemoryWorkingSetInformation,
        MemoryMappedFilenameInformation,
        MemoryRegionInformation,
        MemoryWorkingSetExInformation
    }

    [Flags]
    public enum MemoryState : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,

        /// <summary>
        /// Decommits memory, putting it into the reserved state.
        /// </summary>
        Decommit = 0x4000,

        /// <summary>
        /// Frees memory, putting it into the freed state.
        /// </summary>
        Release = 0x8000,
        Free = 0x10000,
        Reset = 0x80000,
        Physical = 0x400000,
        LargePages = 0x20000000
    }

    public enum MemoryType : int
    {
        Image = 0x1000000,
        Mapped = 0x40000,
        Private = 0x20000
    }

    #endregion

    #region NtStatus
    public enum NtStatus : uint
    {
        // Success
        Success = 0x00000000,
        Wait0 = 0x00000000,
        Wait1 = 0x00000001,
        Wait2 = 0x00000002,
        Wait3 = 0x00000003,
        Wait63 = 0x0000003f,
        Abandoned = 0x00000080,
        AbandonedWait0 = 0x00000080,
        AbandonedWait1 = 0x00000081,
        AbandonedWait2 = 0x00000082,
        AbandonedWait3 = 0x00000083,
        AbandonedWait63 = 0x000000bf,
        UserApc = 0x000000c0,
        KernelApc = 0x00000100,
        Alerted = 0x00000101,
        Timeout = 0x00000102,
        Pending = 0x00000103,
        Reparse = 0x00000104,
        MoreEntries = 0x00000105,
        NotAllAssigned = 0x00000106,
        SomeNotMapped = 0x00000107,
        OpLockBreakInProgress = 0x00000108,
        VolumeMounted = 0x00000109,
        RxActCommitted = 0x0000010a,
        NotifyCleanup = 0x0000010b,
        NotifyEnumDir = 0x0000010c,
        NoQuotasForAccount = 0x0000010d,
        PrimaryTransportConnectFailed = 0x0000010e,
        PageFaultTransition = 0x00000110,
        PageFaultDemandZero = 0x00000111,
        PageFaultCopyOnWrite = 0x00000112,
        PageFaultGuardPage = 0x00000113,
        PageFaultPagingFile = 0x00000114,
        CrashDump = 0x00000116,
        ReparseObject = 0x00000118,
        NothingToTerminate = 0x00000122,
        ProcessNotInJob = 0x00000123,
        ProcessInJob = 0x00000124,
        ProcessCloned = 0x00000129,
        FileLockedWithOnlyReaders = 0x0000012a,
        FileLockedWithWriters = 0x0000012b,

        // Informational
        Informational = 0x40000000,
        ObjectNameExists = 0x40000000,
        ThreadWasSuspended = 0x40000001,
        WorkingSetLimitRange = 0x40000002,
        ImageNotAtBase = 0x40000003,
        RegistryRecovered = 0x40000009,

        // Warning
        Warning = 0x80000000,
        GuardPageViolation = 0x80000001,
        DatatypeMisalignment = 0x80000002,
        Breakpoint = 0x80000003,
        SingleStep = 0x80000004,
        BufferOverflow = 0x80000005,
        NoMoreFiles = 0x80000006,
        HandlesClosed = 0x8000000a,
        PartialCopy = 0x8000000d,
        DeviceBusy = 0x80000011,
        InvalidEaName = 0x80000013,
        EaListInconsistent = 0x80000014,
        NoMoreEntries = 0x8000001a,
        LongJump = 0x80000026,
        DllMightBeInsecure = 0x8000002b,

        // Error
        Error = 0xc0000000,
        Unsuccessful = 0xc0000001,
        NotImplemented = 0xc0000002,
        InvalidInfoClass = 0xc0000003,
        InfoLengthMismatch = 0xc0000004,
        AccessViolation = 0xc0000005,
        InPageError = 0xc0000006,
        PagefileQuota = 0xc0000007,
        InvalidHandle = 0xc0000008,
        BadInitialStack = 0xc0000009,
        BadInitialPc = 0xc000000a,
        InvalidCid = 0xc000000b,
        TimerNotCanceled = 0xc000000c,
        InvalidParameter = 0xc000000d,
        NoSuchDevice = 0xc000000e,
        NoSuchFile = 0xc000000f,
        InvalidDeviceRequest = 0xc0000010,
        EndOfFile = 0xc0000011,
        WrongVolume = 0xc0000012,
        NoMediaInDevice = 0xc0000013,
        NoMemory = 0xc0000017,
        NotMappedView = 0xc0000019,
        UnableToFreeVm = 0xc000001a,
        UnableToDeleteSection = 0xc000001b,
        IllegalInstruction = 0xc000001d,
        AlreadyCommitted = 0xc0000021,
        AccessDenied = 0xc0000022,
        BufferTooSmall = 0xc0000023,
        ObjectTypeMismatch = 0xc0000024,
        NonContinuableException = 0xc0000025,
        BadStack = 0xc0000028,
        NotLocked = 0xc000002a,
        NotCommitted = 0xc000002d,
        InvalidParameterMix = 0xc0000030,
        ObjectNameInvalid = 0xc0000033,
        ObjectNameNotFound = 0xc0000034,
        ObjectNameCollision = 0xc0000035,
        ObjectPathInvalid = 0xc0000039,
        ObjectPathNotFound = 0xc000003a,
        ObjectPathSyntaxBad = 0xc000003b,
        DataOverrun = 0xc000003c,
        DataLate = 0xc000003d,
        DataError = 0xc000003e,
        CrcError = 0xc000003f,
        SectionTooBig = 0xc0000040,
        PortConnectionRefused = 0xc0000041,
        InvalidPortHandle = 0xc0000042,
        SharingViolation = 0xc0000043,
        QuotaExceeded = 0xc0000044,
        InvalidPageProtection = 0xc0000045,
        MutantNotOwned = 0xc0000046,
        SemaphoreLimitExceeded = 0xc0000047,
        PortAlreadySet = 0xc0000048,
        SectionNotImage = 0xc0000049,
        SuspendCountExceeded = 0xc000004a,
        ThreadIsTerminating = 0xc000004b,
        BadWorkingSetLimit = 0xc000004c,
        IncompatibleFileMap = 0xc000004d,
        SectionProtection = 0xc000004e,
        EasNotSupported = 0xc000004f,
        EaTooLarge = 0xc0000050,
        NonExistentEaEntry = 0xc0000051,
        NoEasOnFile = 0xc0000052,
        EaCorruptError = 0xc0000053,
        FileLockConflict = 0xc0000054,
        LockNotGranted = 0xc0000055,
        DeletePending = 0xc0000056,
        CtlFileNotSupported = 0xc0000057,
        UnknownRevision = 0xc0000058,
        RevisionMismatch = 0xc0000059,
        InvalidOwner = 0xc000005a,
        InvalidPrimaryGroup = 0xc000005b,
        NoImpersonationToken = 0xc000005c,
        CantDisableMandatory = 0xc000005d,
        NoLogonServers = 0xc000005e,
        NoSuchLogonSession = 0xc000005f,
        NoSuchPrivilege = 0xc0000060,
        PrivilegeNotHeld = 0xc0000061,
        InvalidAccountName = 0xc0000062,
        UserExists = 0xc0000063,
        NoSuchUser = 0xc0000064,
        GroupExists = 0xc0000065,
        NoSuchGroup = 0xc0000066,
        MemberInGroup = 0xc0000067,
        MemberNotInGroup = 0xc0000068,
        LastAdmin = 0xc0000069,
        WrongPassword = 0xc000006a,
        IllFormedPassword = 0xc000006b,
        PasswordRestriction = 0xc000006c,
        LogonFailure = 0xc000006d,
        AccountRestriction = 0xc000006e,
        InvalidLogonHours = 0xc000006f,
        InvalidWorkstation = 0xc0000070,
        PasswordExpired = 0xc0000071,
        AccountDisabled = 0xc0000072,
        NoneMapped = 0xc0000073,
        TooManyLuidsRequested = 0xc0000074,
        LuidsExhausted = 0xc0000075,
        InvalidSubAuthority = 0xc0000076,
        InvalidAcl = 0xc0000077,
        InvalidSid = 0xc0000078,
        InvalidSecurityDescr = 0xc0000079,
        ProcedureNotFound = 0xc000007a,
        InvalidImageFormat = 0xc000007b,
        NoToken = 0xc000007c,
        BadInheritanceAcl = 0xc000007d,
        RangeNotLocked = 0xc000007e,
        DiskFull = 0xc000007f,
        ServerDisabled = 0xc0000080,
        ServerNotDisabled = 0xc0000081,
        TooManyGuidsRequested = 0xc0000082,
        GuidsExhausted = 0xc0000083,
        InvalidIdAuthority = 0xc0000084,
        AgentsExhausted = 0xc0000085,
        InvalidVolumeLabel = 0xc0000086,
        SectionNotExtended = 0xc0000087,
        NotMappedData = 0xc0000088,
        ResourceDataNotFound = 0xc0000089,
        ResourceTypeNotFound = 0xc000008a,
        ResourceNameNotFound = 0xc000008b,
        ArrayBoundsExceeded = 0xc000008c,
        FloatDenormalOperand = 0xc000008d,
        FloatDivideByZero = 0xc000008e,
        FloatInexactResult = 0xc000008f,
        FloatInvalidOperation = 0xc0000090,
        FloatOverflow = 0xc0000091,
        FloatStackCheck = 0xc0000092,
        FloatUnderflow = 0xc0000093,
        IntegerDivideByZero = 0xc0000094,
        IntegerOverflow = 0xc0000095,
        PrivilegedInstruction = 0xc0000096,
        TooManyPagingFiles = 0xc0000097,
        FileInvalid = 0xc0000098,
        InstanceNotAvailable = 0xc00000ab,
        PipeNotAvailable = 0xc00000ac,
        InvalidPipeState = 0xc00000ad,
        PipeBusy = 0xc00000ae,
        IllegalFunction = 0xc00000af,
        PipeDisconnected = 0xc00000b0,
        PipeClosing = 0xc00000b1,
        PipeConnected = 0xc00000b2,
        PipeListening = 0xc00000b3,
        InvalidReadMode = 0xc00000b4,
        IoTimeout = 0xc00000b5,
        FileForcedClosed = 0xc00000b6,
        ProfilingNotStarted = 0xc00000b7,
        ProfilingNotStopped = 0xc00000b8,
        NotSameDevice = 0xc00000d4,
        FileRenamed = 0xc00000d5,
        CantWait = 0xc00000d8,
        PipeEmpty = 0xc00000d9,
        CantTerminateSelf = 0xc00000db,
        InternalError = 0xc00000e5,
        InvalidParameter1 = 0xc00000ef,
        InvalidParameter2 = 0xc00000f0,
        InvalidParameter3 = 0xc00000f1,
        InvalidParameter4 = 0xc00000f2,
        InvalidParameter5 = 0xc00000f3,
        InvalidParameter6 = 0xc00000f4,
        InvalidParameter7 = 0xc00000f5,
        InvalidParameter8 = 0xc00000f6,
        InvalidParameter9 = 0xc00000f7,
        InvalidParameter10 = 0xc00000f8,
        InvalidParameter11 = 0xc00000f9,
        InvalidParameter12 = 0xc00000fa,
        MappedFileSizeZero = 0xc000011e,
        TooManyOpenedFiles = 0xc000011f,
        Cancelled = 0xc0000120,
        CannotDelete = 0xc0000121,
        InvalidComputerName = 0xc0000122,
        FileDeleted = 0xc0000123,
        SpecialAccount = 0xc0000124,
        SpecialGroup = 0xc0000125,
        SpecialUser = 0xc0000126,
        MembersPrimaryGroup = 0xc0000127,
        FileClosed = 0xc0000128,
        TooManyThreads = 0xc0000129,
        ThreadNotInProcess = 0xc000012a,
        TokenAlreadyInUse = 0xc000012b,
        PagefileQuotaExceeded = 0xc000012c,
        CommitmentLimit = 0xc000012d,
        InvalidImageLeFormat = 0xc000012e,
        InvalidImageNotMz = 0xc000012f,
        InvalidImageProtect = 0xc0000130,
        InvalidImageWin16 = 0xc0000131,
        LogonServer = 0xc0000132,
        DifferenceAtDc = 0xc0000133,
        SynchronizationRequired = 0xc0000134,
        DllNotFound = 0xc0000135,
        IoPrivilegeFailed = 0xc0000137,
        OrdinalNotFound = 0xc0000138,
        EntryPointNotFound = 0xc0000139,
        ControlCExit = 0xc000013a,
        PortNotSet = 0xc0000353,
        DebuggerInactive = 0xc0000354,
        CallbackBypass = 0xc0000503,
        PortClosed = 0xc0000700,
        MessageLost = 0xc0000701,
        InvalidMessage = 0xc0000702,
        RequestCanceled = 0xc0000703,
        RecursiveDispatch = 0xc0000704,
        LpcReceiveBufferExpected = 0xc0000705,
        LpcInvalidConnectionUsage = 0xc0000706,
        LpcRequestsNotAllowed = 0xc0000707,
        ResourceInUse = 0xc0000708,
        ProcessIsProtected = 0xc0000712,
        VolumeDirty = 0xc0000806,
        FileCheckedOut = 0xc0000901,
        CheckOutRequired = 0xc0000902,
        BadFileType = 0xc0000903,
        FileTooLarge = 0xc0000904,
        FormsAuthRequired = 0xc0000905,
        VirusInfected = 0xc0000906,
        VirusDeleted = 0xc0000907,
        TransactionalConflict = 0xc0190001,
        InvalidTransaction = 0xc0190002,
        TransactionNotActive = 0xc0190003,
        TmInitializationFailed = 0xc0190004,
        RmNotActive = 0xc0190005,
        RmMetadataCorrupt = 0xc0190006,
        TransactionNotJoined = 0xc0190007,
        DirectoryNotRm = 0xc0190008,
        CouldNotResizeLog = 0xc0190009,
        TransactionsUnsupportedRemote = 0xc019000a,
        LogResizeInvalidSize = 0xc019000b,
        RemoteFileVersionMismatch = 0xc019000c,
        CrmProtocolAlreadyExists = 0xc019000f,
        TransactionPropagationFailed = 0xc0190010,
        CrmProtocolNotFound = 0xc0190011,
        TransactionSuperiorExists = 0xc0190012,
        TransactionRequestNotValid = 0xc0190013,
        TransactionNotRequested = 0xc0190014,
        TransactionAlreadyAborted = 0xc0190015,
        TransactionAlreadyCommitted = 0xc0190016,
        TransactionInvalidMarshallBuffer = 0xc0190017,
        CurrentTransactionNotValid = 0xc0190018,
        LogGrowthFailed = 0xc0190019,
        ObjectNoLongerExists = 0xc0190021,
        StreamMiniversionNotFound = 0xc0190022,
        StreamMiniversionNotValid = 0xc0190023,
        MiniversionInaccessibleFromSpecifiedTransaction = 0xc0190024,
        CantOpenMiniversionWithModifyIntent = 0xc0190025,
        CantCreateMoreStreamMiniversions = 0xc0190026,
        HandleNoLongerValid = 0xc0190028,
        NoTxfMetadata = 0xc0190029,
        LogCorruptionDetected = 0xc0190030,
        CantRecoverWithHandleOpen = 0xc0190031,
        RmDisconnected = 0xc0190032,
        EnlistmentNotSuperior = 0xc0190033,
        RecoveryNotNeeded = 0xc0190034,
        RmAlreadyStarted = 0xc0190035,
        FileIdentityNotPersistent = 0xc0190036,
        CantBreakTransactionalDependency = 0xc0190037,
        CantCrossRmBoundary = 0xc0190038,
        TxfDirNotEmpty = 0xc0190039,
        IndoubtTransactionsExist = 0xc019003a,
        TmVolatile = 0xc019003b,
        RollbackTimerExpired = 0xc019003c,
        TxfAttributeCorrupt = 0xc019003d,
        EfsNotAllowedInTransaction = 0xc019003e,
        TransactionalOpenNotAllowed = 0xc019003f,
        TransactedMappingUnsupportedRemote = 0xc0190040,
        TxfMetadataAlreadyPresent = 0xc0190041,
        TransactionCubeCallbacksNotSet = 0xc0190042,
        TransactionRequiredPromotion = 0xc0190043,
        CannotExecuteFileInTransaction = 0xc0190044,
        TransactionsNotFrozen = 0xc0190045,

        MaximumNtStatus = 0xffffffff
    }
    #endregion
}
