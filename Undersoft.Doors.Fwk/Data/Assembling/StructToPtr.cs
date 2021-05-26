using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;

namespace System.Doors.Data
{
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    //public unsafe struct DynamicStructure
    //{

    //    public double Index;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    //    public string Name;
    //    public int Id;

    //    public int GetByteSize()
    //    {
    //        return Marshal.SizeOf(typeof(DynamicStructure));
    //    }

    //    public unsafe byte* DynamicStructureToPtr()
    //    {                                  
    //        //structure ptr
    //        TypedReference tr = __makeref(this);           
    //        byte* bstruct = (byte*)(*((IntPtr*)&tr)).ToPointer();

    //        int size = Marshal.SizeOf(typeof(DynamicStructure));
    //        byte* pserial = (byte*)Marshal.AllocHGlobal(size);

    //        FieldInfo[] fields = this.GetType().GetFields();

    //        int offset = 0;
    //        int n = fields.Count();
    //        int fieldSize;

    //        if (n == 0)
    //        {
    //            throw new System.ArgumentException("Empty structure");
    //        }
           
    //        for (int i = 1; i < n; i++)
    //        {
    //            //copy field[i-1]
    //            FieldInfo field = fields[i];
    //            int fieldOffset = (int)Marshal.OffsetOf(this.GetType(), field.Name);                
    //            fieldSize = fieldOffset - offset;
    //            MemoryCopier.Copy(bstruct, pserial + offset, offset, fieldSize);
    //            offset += fieldSize;
    //        }

    //        //last field
    //        fieldSize = size - offset;
    //        MemoryCopier.Copy(bstruct, pserial + offset, offset, fieldSize);
    //        return pserial;
    //    }
    //    public unsafe void PtrToDynamicStructure(byte * pserial)
    //    {
    //        //structure ptr
    //        TypedReference tr = __makeref(this);
    //        TypedReference* ptr = &tr;
    //        IntPtr rstruct = *((IntPtr*)ptr);
    //        byte* bstruct = (byte*)rstruct.ToPointer();

    //        int size = Marshal.SizeOf(typeof(DynamicStructure));
            
    //        FieldInfo[] fields = this.GetType().GetFields();

    //        int offset = 0;
    //        int n = fields.Count();
    //        int fieldSize;

    //        if (n == 0 || pserial == null)
    //        {
    //            throw new System.ArgumentException("Empty structure or null input");
    //        }

    //        for (int i = 1; i < n; i++)
    //        {
    //            //copy field[i-1]
    //            FieldInfo field = fields[i];
    //            int fieldOffset = (int)Marshal.OffsetOf(this.GetType(), field.Name);
    //            fieldSize = fieldOffset - offset;
    //            MemoryCopier.Copy(pserial, bstruct + offset, offset, fieldSize);
    //            offset += fieldSize;
    //        }

    //        //last field
    //        fieldSize = size - offset;
    //        MemoryCopier.Copy(pserial, bstruct + offset, offset, fieldSize);            
    //    }
    //}
    
    //class Program
    //{
    //    public unsafe static void Main(string[] args)
    //    {
    //        DynamicStructure dsA = new DynamicStructure() { Id = 10, Index = 5, Name = "abc" };
    //        DynamicStructure dsB = new DynamicStructure() { Id = 2, Index = 4, Name = "xy" };
    //        byte* ptrdsA;
    //        byte* ptrdsB;
    //        int sizeA = dsA.GetByteSize();
    //        ptrdsA = dsA.DynamicStructureToPtr();

    //        for(int i = 0; i<sizeA; i++)
    //        {
    //            Debug.WriteLine(ptrdsA[i]);
    //        }

    //        dsB.PtrToDynamicStructure(ptrdsA);            
    //    }
    //}

    ////Darka
    //public static class MemoryCopier
    //{
    //    static readonly IMemoryCopier _copier;

    //    private static AssemblyName _asmName = new AssemblyName() { Name = "MemoryCopier" };
    //    private static ModuleBuilder _modBuilder;
    //    private static AssemblyBuilder _asmBuilder;

    //    static MemoryCopier()
    //    {
    //        _asmBuilder = AssemblyBuilder.DefineDynamicAssembly(_asmName, AssemblyBuilderAccess.RunAndCollect);
    //        _modBuilder = _asmBuilder.DefineDynamicModule(_asmName.Name + ".dll");

    //        var typeBuilder = _modBuilder.DefineType("MemoryCopier",
    //                   TypeAttributes.Public
    //                   | TypeAttributes.AutoClass
    //                   | TypeAttributes.AnsiClass
    //                   | TypeAttributes.Class
    //                   | TypeAttributes.Serializable
    //                   | TypeAttributes.BeforeFieldInit);
    //        typeBuilder.AddInterfaceImplementation(typeof(IMemoryCopier));

    //        CreateCopyPointersInt(typeBuilder);

    //        var copierType = typeBuilder.CreateTypeInfo();
    //        _copier = (IMemoryCopier)Activator.CreateInstance(copierType);

    //    }

    //    private static void CreateCopyPointersInt(TypeBuilder tb)
    //    {
    //        var copyMethod = tb.DefineMethod("Copy",
    //                                         MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
    //                                         typeof(void),
    //                                         new Type[] { typeof(byte*), typeof(byte*), typeof(int), typeof(int) });
    //        var code = copyMethod.GetILGenerator();

    //        //updated by Darek
    //        code.Emit(OpCodes.Ldarg_2);
    //        code.Emit(OpCodes.Ldarg_1);
    //        code.Emit(OpCodes.Ldarg_3);
    //        code.Emit(OpCodes.Add);
    //        code.Emit(OpCodes.Ldarg, 4);
    //        code.Emit(OpCodes.Conv_U);
    //        code.Emit(OpCodes.Cpblk);
    //        code.Emit(OpCodes.Ret);

    //        tb.DefineMethodOverride(copyMethod, typeof(IMemoryCopier).GetMethod("Copy", new Type[] { typeof(byte*), typeof(byte*), typeof(int), typeof(int) }));
    //    }

    //    public static unsafe void Copy(byte* source, byte* dest, int offset, int count)
    //    {
    //        _copier.Copy(source, dest, offset, count);
    //    }
    //}

    //public interface IMemoryCopier
    //{       
    //    unsafe void Copy(byte* source, byte* dest, int offset, int count);
    //}
      
}
