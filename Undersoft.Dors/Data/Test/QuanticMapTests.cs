/*
    Dors Map Test Examples
    
    Demonstracyjne testy podstawowych funkcji biblioteki Dors
    Sopot 2018-06-19

    wersja 1.07

*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dors.Data;
using System.Dors;


namespace System.Dors.Data.Tests
{
    public class DorsMapTests
    {
        //private IDorsEvent writeEcho;
        //private bool firstRun = true;
        //// init test method
        //public int LevelCount = 0;
        //private DorsMap<object> qmap;

        //// Van Emde Boas Bit Scopes Tree Map - Warp 7 Speed
        //public unsafe void RunBitScopesTest(IDorsEvent WriteEcho, int capacity, DorsMap<object> _qmap)
        //{

        //    qmap = _qmap;
        //    qmap.RRInsert(10);

        //    try
        //    {
        //        writeEcho = WriteEcho;
        //        writeEcho.Execute("Starting Tests");

        //        qmap = _qmap;
        //        LevelCount = levelCount(capacity);
                
        //        object[] keys = new object[] { "klucz1", 345, new Quid((long)893598347) };
        //        object target = "TARGET";
        //        int hashKey = keys.GetShahCode32();
        

        //    }
        //    catch (Exception ex)
        //    {
        //        writeEcho.Execute(ex.ToString());
        //    }
        //}
        
        //private int levelCount(int capacity)
        //{
        //    int capacitytemp = capacity;
        //    int levelcount = 0;
        //    for (int i = 0; i < 4; i++)
        //    {
        //        capacitytemp = capacitytemp / 256;
        //        if (capacitytemp == 0)
        //            return levelcount;
        //        levelcount++;
        //    }
        //    return levelcount;
        //}

        //private void allocLevel(int key, int[] levels)
        //{                     
        //    int levelvalue = key;
        //    for (int i = 0; i < LevelCount + 1; i++)
        //    {                
        //        levels[LevelCount - i] = levelvalue % 256;
        //        levelvalue /= 256;
        //    }
        //}

        ////public unsafe bool Add(int key, object value)
        ////{
        ////    int* intptr = qmap.bitScopes;

        ////    int[] levels = new int[LevelCount + 1];

        ////    allocLevel(key, levels);

        ////    for (int i = 0; i < LevelCount + 1; i++)
        ////    {                          
        ////        if (i == LevelCount)
        ////        {
        ////            int* ptr0 = (intptr + levels[i]);
        ////            if (*ptr0 == 0)
        ////            {
        ////                IntPtr temp = GCHandle.ToIntPtr(GCHandle.Alloc(value, GCHandleType.Normal));
        ////                *ptr0 = temp.ToInt32();
        ////                return true;
        ////            }
        ////            else
        ////                return false;
        ////        }
        ////        else
        ////        {
        ////            int* ptr0 = (intptr + levels[i]);
        ////            if (*ptr0 == 0)
        ////            {
        ////                int[] oo = new int[256];
        ////                IntPtr temp  = GCHandle.Alloc(oo, GCHandleType.Pinned).AddrOfPinnedObject();
        ////                *ptr0 = temp.ToInt32();
        ////                intptr = (int*)temp.ToPointer();

        ////            }
        ////            else
        ////            {
        ////                intptr = (int*)(new IntPtr(*ptr0).ToPointer());
        ////            }
        ////        }
        ////    }
        ////    return false;

        ////}

        ////public unsafe object Get(int key)
        ////{
        ////    int* intptr = qmap.bitScopes;

        ////    int[] levels = new int[LevelCount + 1];

        ////    allocLevel(key, levels);

        ////    for (int i = 0; i < LevelCount + 1; i++)
        ////    {
        ////        if (i == LevelCount)
        ////        {
        ////            int* ptr0 = (intptr + levels[i]);
        ////            if (*ptr0 != 0)
        ////            {
        ////                return GCHandle.FromIntPtr(new IntPtr(*ptr0)).Target;
        ////            }
        ////            else
        ////                return null;
        ////        }
        ////        else
        ////        {
        ////            int* ptr0 = (intptr + levels[i]);
        ////            if (*ptr0 != 0)
        ////            {
        ////                intptr = (int*)(new IntPtr(*ptr0).ToPointer());
        ////            }
        ////            else
        ////                return null;
        ////        }
        ////    }
        ////    return null;

        ////}

        ////public void RunTests(IDorsEvent WriteEcho)
        ////{
        ////    try
        ////    {
        ////        writeEcho = WriteEcho;

        ////        writeEcho.Execute("Starting Tests");

        ////        TestSize();
        ////        writeEcho.Execute("Test Size");
        ////        TestIsEmpty();
        ////        writeEcho.Execute("Test IsEmpty");
        ////        TestMinimumKey();
        ////        writeEcho.Execute("Test MinimumKey");
        ////        TestMaximumKey();
        ////        writeEcho.Execute("Test MaximumKey");
        ////        TestNextKey();
        ////        writeEcho.Execute("Test NextKey");
        ////        TestPreviousKey();
        ////        writeEcho.Execute("Test PreviousKey");
        ////        TestContainsKey();
        ////        writeEcho.Execute("Test ContainsKey");
        ////        TestGet();
        ////        writeEcho.Execute("Test Get");
        ////        TestSet();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestLowerBound();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestUpperBound();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestMapKeySeries();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestTableKeySeries();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestMapKeyValueSeries();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestTableKeyValueSeries();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestRemove();
        ////        writeEcho.Execute("Starting Tests");
        ////        TestClear();
        ////        writeEcho.Execute("Starting Tests");

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        writeEcho.Execute(ex.ToString());
        ////    }
        ////}

        ////public void TestSize()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-10, 10);

        ////    for (int i = -10, size = 0; i <= 10; ++i, ++size)
        ////    {
        ////        Assert.AreEqual(size, tree.Size);
        ////        tree.Set(i, i.ToString());
        ////        Assert.AreEqual(size + 1, tree.Size);
        ////    }
        ////}

        ////public void TestIsEmpty()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-10, 10);
        ////    try
        ////    {
        ////        Assert.IsTrue(tree.IsEmpty);
        ////        tree.Set(0, "0");
        ////        Assert.IsFalse(tree.IsEmpty);
        ////        tree.Set(1, "1");
        ////        Assert.IsFalse(tree.IsEmpty);
        ////        tree.Remove(0);
        ////        Assert.IsFalse(tree.IsEmpty);
        ////        tree.Remove(1);
        ////        Assert.IsTrue(tree.IsEmpty);
        ////    }
        ////    catch(Exception ex)
        ////    {
        ////        writeEcho.Execute("TestIsEmpty - " + ex.ToString());
        ////    }
        ////}

        ////public void TestMinimumKey()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-5, 4);

        ////    Assert.AreEqual(5, tree.MinimumKey);

        ////    for (int i = 4; i >= -5; --i)
        ////    {
        ////        tree.Set(i, i.ToString());
        ////        Assert.AreEqual(i, tree.MinimumKey);
        ////    }

        ////   // tree.Clear();

        ////    Assert.AreEqual(-5, tree.MinimumKey);

        ////    tree = new DorsMap<string>(int.MaxValue - 5,
        ////                                      int.MaxValue);

        ////    Assert.AreEqual(int.MinValue, tree.MinimumKey);
        ////}

        ////public void TestMaximumKey()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-5, 4);

        ////    Assert.AreEqual(-6, tree.MaximumKey);

        ////    for (int i = -5; i <= 4; ++i)
        ////    {
        ////        tree.Set(i, i.ToString());
        ////        Assert.AreEqual(i, tree.MaximumKey);
        ////    }

        ////   // tree.Clear();

        ////    Assert.AreEqual(4, tree.MaximumKey);

        ////    tree = new DorsMap<string>(int.MinValue,
        ////                                       int.MinValue + 5);

        ////    Assert.AreEqual(int.MaxValue, tree.MaximumKey);
        ////}

        ////public void TestNextKey()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-3, 3);

        ////    tree.Set(-3, (-3).ToString());
        ////    tree.Set(-1, (-1).ToString());
        ////    tree.Set(0, (0).ToString());
        ////    tree.Set(2, (2).ToString());

        ////    Assert.AreEqual(-1, tree.NextKey(-3));
        ////    Assert.AreEqual(-1, tree.NextKey(-2));

        ////    Assert.AreEqual(0, tree.NextKey(-1));
        ////    Assert.AreEqual(2, tree.NextKey(0));
        ////    Assert.AreEqual(2, tree.NextKey(1));
        ////    Assert.AreEqual(-4, tree.NextKey(2));
        ////    Assert.AreEqual(-4, tree.NextKey(3));

        ////    tree.Clear();

        ////    for (int i = -3; i <= 3; ++i)
        ////    {
        ////        Assert.AreEqual(-4, tree.NextKey(i));
        ////    }
        ////}

        ////public void TestPreviousKey()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-3, 3);

        ////    tree.Set(-3, (-3).ToString());
        ////    tree.Set(-1, (-1).ToString());
        ////    tree.Set(0, (0).ToString());
        ////    tree.Set(2, (2).ToString());

        ////    Assert.AreEqual(2, tree.PreviousKey(3));

        ////    Assert.AreEqual(0, tree.PreviousKey(2));
        ////    Assert.AreEqual(0, tree.PreviousKey(1));

        ////    Assert.AreEqual(-1, tree.PreviousKey(0));
        ////    Assert.AreEqual(-3, tree.PreviousKey(-1));
        ////    Assert.AreEqual(-3, tree.PreviousKey(-2));
        ////    Assert.AreEqual(4, tree.PreviousKey(-3));

        ////    tree.Clear();

        ////    for (int i = -3; i <= 3; ++i)
        ////    {
        ////        Assert.AreEqual(4, tree.PreviousKey(i));
        ////    }
        ////}

        ////public void TestContainsKey()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-5, -1);

        ////    tree.Set(-5, null);
        ////    tree.Set(-3, (-3).ToString());
        ////    tree.Set(-1, (-1).ToString());

        ////    Assert.IsFalse(tree.ContainsKey(-5));
        ////    Assert.IsTrue(tree.ContainsKey(-3));
        ////    Assert.IsTrue(tree.ContainsKey(-1));

        ////    Assert.IsFalse(tree.ContainsKey(-4));
        ////    Assert.IsFalse(tree.ContainsKey(-2));
        ////}

        ////public void TestGet()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-5, -1);

        ////    tree.Set(-5, null);
        ////    tree.Set(-3, (-13).ToString());
        ////    tree.Set(-1, (-11).ToString());

        ////    Assert.IsNull(tree.Get(-5));
        ////    Assert.AreEqual((-11).ToString(), tree.Get(-1));
        ////    Assert.AreEqual((-13).ToString(), tree.Get(-3));
        ////}

        ////public void TestSet()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(-5, 10);

        ////    for (int i = -2; i <= 4; ++i)
        ////    {
        ////        Assert.IsFalse(tree.ContainsKey(i));
        ////        Assert.IsNull(tree.Set(i, (2 * i).ToString()));
        ////        Assert.IsTrue(tree.ContainsKey(i));
        ////    }

        ////    for (int i = -3; i >= -5; --i)
        ////    {
        ////        Assert.IsFalse(tree.ContainsKey(i));
        ////        Assert.IsNull(tree.Get(i));
        ////    }

        ////    Assert.IsTrue(tree.ContainsKey(0));
        ////    tree.Remove(0);
        ////    Assert.IsFalse(tree.ContainsKey(0));
        ////}

        ////public void TestLowerBound()
        ////{
        ////    DorsMap<String> tree = new DorsMap<string>(-4, 4);

        ////    tree.Set(-4, "-4");
        ////}

        ////public void TestUpperBound()
        ////{
        ////    DorsMap<String> tree = new DorsMap<string>(-4, 4);

        ////    tree.Set(4, "4");
        ////}

        ////public void TestMapKeySeries()
        ////{
        ////    DorsMap<string> tree = new DorsMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree.Set(i, null);
        ////    }

        ////    DorsMap<string>.KeySeries iterator = tree.MapKeyEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextKey());
        ////        Assert.AreEqual(i, iterator.NextKey());
        ////    }

        ////    Assert.AreEqual(9, tree.Size);

        ////    iterator = tree.MapKeyEnumerator();

        ////    Assert.AreEqual(-4, iterator.NextKey());
        ////    iterator.RemoveKey();
        ////    Assert.AreEqual(-3, iterator.NextKey());
        ////    Assert.AreEqual(-2, iterator.NextKey());
        ////    Assert.AreEqual(-1, iterator.NextKey());
        ////    iterator.RemoveKey();

        ////    Assert.IsFalse(tree.ContainsKey(-4));
        ////    Assert.IsTrue(tree.ContainsKey(-3));
        ////    Assert.IsTrue(tree.ContainsKey(-2));
        ////    Assert.IsFalse(tree.ContainsKey(-1));

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestTableKeySeries()
        ////{
        ////    DorsMap<string> tree = new DorsMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree.Set(i, null);
        ////    }

        ////    DorsMap<string>.KeySeries iterator = tree.TableKeyEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextKey());
        ////        Assert.AreEqual(i, iterator.NextKey());
        ////    }

        ////    Assert.AreEqual(9, tree.Size);

        ////    iterator = tree.MapKeyEnumerator();

        ////    Assert.AreEqual(-4, iterator.NextKey());
        ////    iterator.RemoveKey();
        ////    Assert.AreEqual(-3, iterator.NextKey());
        ////    Assert.AreEqual(-2, iterator.NextKey());
        ////    Assert.AreEqual(-1, iterator.NextKey());
        ////    iterator.RemoveKey();

        ////    Assert.IsFalse(tree.ContainsKey(-4));
        ////    Assert.IsTrue(tree.ContainsKey(-3));
        ////    Assert.IsTrue(tree.ContainsKey(-2));
        ////    Assert.IsFalse(tree.ContainsKey(-1));

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestMapKeyValueSeries()
        ////{
        ////    DorsMap<String> tree = new DorsMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree.Set(i, "" + i);
        ////    }

        ////    DorsMap<string>.KeyValueSeries iterator =
        ////            tree.MapKeyValueEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextEntry());
        ////        iterator.NextEntry();
        ////        Assert.AreEqual(i, iterator.Key);
        ////        Assert.AreEqual("" + i, iterator.Value);
        ////    }

        ////    Assert.AreEqual(9, tree.Size);

        ////    iterator = tree.MapKeyValueEnumerator();
        ////    iterator.NextEntry();

        ////    Assert.AreEqual(-4, iterator.Key);
        ////    Assert.AreEqual("-4", iterator.Value);

        ////    iterator.RemoveEntry();
        ////    iterator.NextEntry();
        ////    Assert.AreEqual(-3, iterator.Key);
        ////    Assert.AreEqual("-3", iterator.Value);

        ////    iterator.NextEntry();
        ////    Assert.AreEqual(-2, iterator.Key);
        ////    Assert.AreEqual("-2", iterator.Value);

        ////    iterator.NextEntry();
        ////    Assert.AreEqual(-1, iterator.Key);
        ////    Assert.AreEqual("-1", iterator.Value);
        ////    iterator.RemoveEntry();

        ////    Assert.IsFalse(tree.ContainsKey(-4));
        ////    Assert.IsTrue(tree.ContainsKey(-3));
        ////    Assert.IsTrue(tree.ContainsKey(-2));
        ////    Assert.IsFalse(tree.ContainsKey(-1));

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestTableKeyValueSeries()
        ////{
        ////    DorsMap<String> tree = new DorsMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree.Set(i, "" + i);
        ////    }

        ////    DorsMap<string>.KeyValueSeries iterator =
        ////            tree.TableKeyValueEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextEntry());
        ////        iterator.NextEntry();
        ////        Assert.AreEqual(i, iterator.Entry.Key);
        ////        Assert.AreEqual("" + i, iterator.Entry.Value);
        ////    }

        ////    Assert.AreEqual(9, tree.Size);

        ////    iterator = tree.TableKeyValueEnumerator();
        ////    iterator.NextEntry();

        ////    Assert.AreEqual(-4, iterator.Entry.Key);
        ////    Assert.AreEqual("-4", iterator.Entry.Value);

        ////    iterator.RemoveEntry();
        ////    iterator.NextEntry();
        ////    Assert.AreEqual(-3, iterator.Entry.Key);
        ////    Assert.AreEqual("-3", iterator.Entry.Value);

        ////    iterator.NextEntry();
        ////    Assert.AreEqual(-2, iterator.Key);
        ////    Assert.AreEqual("-2", iterator.Value);

        ////    iterator.NextEntry();
        ////    Assert.AreEqual(-1, iterator.Key);
        ////    Assert.AreEqual("-1", iterator.Value);
        ////    iterator.RemoveEntry();

        ////    Assert.IsFalse(tree.ContainsKey(-4));
        ////    Assert.IsTrue(tree.ContainsKey(-3));
        ////    Assert.IsTrue(tree.ContainsKey(-2));
        ////    Assert.IsFalse(tree.ContainsKey(-1));

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestRemove()
        ////{
        ////    DorsMap<string> tree = new DorsMap<string>(1, 5);

        ////    tree.Set(3, (3).ToString());
        ////    tree.Set(1, (1).ToString());
        ////    tree.Set(5, (5).ToString());

        ////    Assert.IsTrue(tree.ContainsKey(1));
        ////    Assert.IsTrue(tree.ContainsKey(3));
        ////    Assert.IsTrue(tree.ContainsKey(5));

        ////    tree.Set(1, null);

        ////    Assert.IsTrue(tree.ContainsKey(1));

        ////    tree.Remove(1);

        ////    Assert.IsFalse(tree.ContainsKey(1));
        ////}

        ////public void TestClear()
        ////{
        ////    DorsMap<string> tree =
        ////            new DorsMap<string>(3, 10);

        ////    for (int i = 4, sz = 0; i <= 8; ++i, ++sz)
        ////    {
        ////        Assert.AreEqual(sz, tree.Size);
        ////        tree.Set(i, null);
        ////        Assert.AreEqual(sz + 1, tree.Size);
        ////    }

        ////    Assert.AreEqual(5, tree.Size);
        ////    tree.Clear();
        ////    Assert.AreEqual(0, tree.Size);

        ////    for (int i = 3; i <= 10; ++i)
        ////    {
        ////        Assert.IsFalse(tree.ContainsKey(i));
        ////        Assert.IsNull(tree.Get(i));
        ////        Assert.IsNull(tree.Remove(i));
        ////    }
        ////}

    }
}
