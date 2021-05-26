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
using System.Doors.Data;
using System.Doors;
using System.Diagnostics;


namespace System.Doors.Data.Tests
{
    public class HyperMapTests
    {
        private IDorsEvent writeEcho;
        private bool firstRun = true;
        // init test method

     
        int testValue = 0;
        int successor_value;
        int predecessor_value;
        bool boolTest = false;


        public unsafe void ScopeTreeRHGlobalTest(IDorsEvent WriteEcho)
        {

            HashList<float> quid = new HashList<float>();

            int counter = 0;
            int sizeN = 200000;

            //ScopeTreeRHGlobal<int> ScopeTreeRHGlobal = new ScopeTreeRHGlobal<int>(Int32.MaxValue);
            ScopeTreeRHGlobal<int> ScopeTreeRHGlobal = new ScopeTreeRHGlobal<int>();


            int[] keyTab = new int[sizeN];
            for (int i = 0; i < sizeN; i++)
            {

                int hst = (((i + "VEB" + i).GetHashCode()) << 1);
                hst = hst >> 1;
                keyTab[i] = (int)((uint)((int)(i + "VEB" + i).GetHashCode()) >> 1);
            }





            List<int> sortedKeyList = keyTab.OrderBy(x=> x).ToList();

            int k = 1;
            int sizeK = sizeN;
            for(; ; )
            {
                if(sortedKeyList[k-1] == sortedKeyList[k])
                {
                    sortedKeyList.RemoveAt(k-1);
                    sizeK--;
                }
                else
                {
                    k++;
                }
                if (k >= sizeK) break;
            }



            for (int i = 0; i < sizeN; i++)
            {
                if (!ScopeTreeRHGlobal.ContainsKey(keyTab[i]))
                {
                    //scopeTree.Insert(keyTab[i], keyTab[i]);
                    ScopeTreeRHGlobal.Insert(keyTab[i], keyTab[i]);

                }
            }


            ScopeTreeRH<int> scopeTree = new ScopeTreeRH<int>(0);

            for (int i = 0; i < sizeN; i++)
            {
                if (!scopeTree.ContainsKey(keyTab[i]))
                {
                    int ktab = keyTab[i];
                    if (ktab == 192053244 || ktab == 192044310 || ktab == 192054846)
                    {
                        Debug.Write("aaa");
                    }
                    scopeTree.Insert(keyTab[i], keyTab[i]);
                    //ScopeTreeRHGlobal.Insert(keyTab[i], keyTab[i]);
                    
                }
            }




            ScopeTreeRHGlobal.ContainsKey(keyTab[0]);


            

            for (int i = 0; i < sizeN; i++)
            {
                if (!scopeTree.ContainsKey(keyTab[i]))
                {
                    //scopeTree.Insert(keyTab[i], keyTab[i]);
                }
            }

            for (int i=0; i < sizeK-1; i++)
            {

                if(i == 17932)
                {
                    Debug.Write("aaa");
                }

                int current = sortedKeyList[i];
                int nextInList = sortedKeyList[i + 1];
                int nextscope = scopeTree.NextKey(current);
                int nextkey = ScopeTreeRHGlobal.NextKey(current);
                
                
                if (nextkey != nextInList)
                {
                    boolTest = ScopeTreeRHGlobal.ContainsKey(current);
                    boolTest = ScopeTreeRHGlobal.TestContains(current);
                    nextscope = scopeTree.NextKey(current);
                    boolTest = ScopeTreeRHGlobal.TestContains(nextInList);

                    scopeTree.root.Contains(0, 1, 0, current);
                    scopeTree.root.Contains(0, 1, 0, nextInList);
                    nextscope = scopeTree.NextKey(current);
                    Debug.WriteLine("Aaaa");
                }
            }


            

            

            scopeTree.ContainsKey(keyTab[0]);

            //ScopeTreeRHGlobal.TestInsert(2);
            //ScopeTreeRHGlobal.TestInsert(7);
            //ScopeTreeRHGlobal.TestInsert(14);
            //ScopeTreeRHGlobal.TestInsert(15);
            //ScopeTreeRHGlobal.TestInsert(20);
            //ScopeTreeRHGlobal.TestInsert(40);
            //ScopeTreeRHGlobal.TestInsert(50);
            //ScopeTreeRHGlobal.TestInsert(60);
            //ScopeTreeRHGlobal.TestInsert(61);
            //ScopeTreeRHGlobal.TestInsert(120);
            //ScopeTreeRHGlobal.TestInsert(124);
            //ScopeTreeRHGlobal.TestInsert(200);
            //ScopeTreeRHGlobal.TestInsert(205);
            //ScopeTreeRHGlobal.TestInsert(245);


            //ScopeTreeRHGlobal.TestDelete(2);
            //ScopeTreeRHGlobal.TestDelete(7);
            //ScopeTreeRHGlobal.TestDelete(14);
            //ScopeTreeRHGlobal.TestDelete(15);
            //ScopeTreeRHGlobal.TestDelete(20);
            //ScopeTreeRHGlobal.TestDelete(40);
            //ScopeTreeRHGlobal.TestDelete(50);
            //ScopeTreeRHGlobal.TestDelete(60);
            //ScopeTreeRHGlobal.TestDelete(61);
            //ScopeTreeRHGlobal.TestDelete(120);
            //ScopeTreeRHGlobal.TestDelete(124);
            //ScopeTreeRHGlobal.TestDelete(200);
            //ScopeTreeRHGlobal.TestDelete(205);
            //ScopeTreeRHGlobal.TestDelete(245);


            //testValue = ScopeTreeRHGlobal.NextKey(12);
            //testValue = ScopeTreeRHGlobal.NextKey(13);
            //testValue = ScopeTreeRHGlobal.NextKey(14);
            //testValue = ScopeTreeRHGlobal.NextKey(20);
            //testValue = ScopeTreeRHGlobal.NextKey(21);
            //testValue = ScopeTreeRHGlobal.NextKey(40);
            //testValue = ScopeTreeRHGlobal.NextKey(41);
            //testValue = ScopeTreeRHGlobal.NextKey(50);
            //testValue = ScopeTreeRHGlobal.NextKey(52);
            //testValue = ScopeTreeRHGlobal.NextKey(60);
            //testValue = ScopeTreeRHGlobal.NextKey(61);
            //testValue = ScopeTreeRHGlobal.NextKey(62);
            //testValue = ScopeTreeRHGlobal.NextKey(120);
            //testValue = ScopeTreeRHGlobal.NextKey(121);
            //testValue = ScopeTreeRHGlobal.NextKey(124);
            //testValue = ScopeTreeRHGlobal.NextKey(125);
            //testValue = ScopeTreeRHGlobal.NextKey(200);
            //testValue = ScopeTreeRHGlobal.NextKey(201);
            //testValue = ScopeTreeRHGlobal.NextKey(205);
            //testValue = ScopeTreeRHGlobal.NextKey(206);
            //testValue = ScopeTreeRHGlobal.NextKey(208);
            //testValue = ScopeTreeRHGlobal.NextKey(245);
            //testValue = ScopeTreeRHGlobal.NextKey(246);




        }


        // Van Emde Boas Bit Scopes Tree Map - Warp 7 Speed
        public unsafe void RunBitScopesTest(IDorsEvent WriteEcho)
        {
            try
            {
                writeEcho = WriteEcho;
                writeEcho.Execute("Starting Tests");
                long elapsed = 0;                
                int z = 0;
                Tuple<long, string>[] vea = new Tuple<long, string>[200000];
                Tuple<int, string>[] veb = new Tuple<int, string>[200000];
                //Article<string>[] vear = new Article<string>[200000];
                for (int i = 0; i < 200000; i++)
                {
                    string _keys = i.ToString() + "_" + DateTime.Now.Ticks.ToString();
                    long l = _keys.GetShahCode64();
                    int I = Math.Abs(_keys.GetShahCode32() / 21500);
                    vea[i] = new Tuple<long, string>(l, i + "_TARGET_" + l);
                    veb[i] = new Tuple<int, string>(I, I + "_TARGET_" + I);
                }

                //for(int i = 0; i < 50; i ++)
                //{
                //    veb[i] = new Tuple<int, string>((100 % (i + 1)) + i, i + "_TARGET_" + i);
                //}

                Stopwatch sw = new Stopwatch();

                SortedDictionary<int, string> sd = new SortedDictionary<int, string>();
                //SortedList<int, string>[] sl = new SortedList<int, string>[100000];
                //sl.Select((s, y) => sl[y] = new SortedList<int, string>()).ToArray();
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 0; x < 200000; x++)
                    {
                        //for (int i = 0; i < 20; i++)
                        //{
                        sd.TryAdd(veb[x].Item1, veb[x].Item2);
                        //}
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedDictionary Insert 200k: " + elapsed.ToString() + " ms");
                string[] vew = new string[200000];
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 199999; x > -1; x--)
                    {
                        //for (int i = 0; i < 10; i++)
                        sd.TryGetValue(veb[x].Item1, out vew[x]);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedDictionary Get 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 199999; x > -1; x--)
                    {
                        //for (int i = 0; i < 10; i++)
                        sd.ContainsKey(veb[x].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedDictionary Contains 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    z = 0;
                    sw.Reset();
                    sw.Start();
                    //for (int x = 99999; x > -1; x--)
                    //{
                    foreach (KeyValuePair<int, string> bag in sd)
                    {
                        vew[z++] = bag.Value;
                    }
                    //   }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedDictionary Iterate 200k: " + elapsed.ToString() + " ms");

                SortedList<int, string> sl = new SortedList<int, string>();
                //SortedList<int, string>[] sl = new SortedList<int, string>[100000];
                //sl.Select((s, y) => sl[y] = new SortedList<int, string>()).ToArray();
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 0; x < 200000; x++)
                    {
                        //for (int i = 0; i < 20; i++)
                        //{
                        if (!sl.ContainsKey(veb[x].Item1))
                            sl.Add(veb[x].Item1, veb[x].Item2);
                        //}
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedList Insert 200k: " + elapsed.ToString() + " ms");
                string[] vel = new string[200000];
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 199999; x > -1; x--)
                    {
                        //for (int i = 0; i < 10; i++)
                        vel[x] = sl.Get(veb[x].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedList Get 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 199999; x > -1; x--)
                    {
                        //for (int i = 0; i < 10; i++)
                        sl.ContainsKey(veb[x].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedList Contains 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    z = 0;
                    sw.Reset();
                    sw.Start();
                    //for (int x = 99999; x > -1; x--)
                    //{
                    foreach (KeyValuePair<int, string> bag in sl)
                    {
                        vel[z++] = bag.Value;
                    }
                    //   }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedList Iterate 200k: " + elapsed.ToString() + " ms");



                ScopeTreeRH<string> st = new ScopeTreeRH<string>(200000);
                st.Initialize();
                //ScopeTreeRH<string>[] st = new ScopeTreeRH<string>[10000];
                //st.Select((s,y) => st[y] = new ScopeTreeRH<string>(100)).ToArray();
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 0; x < 200000; x++)
                    {
                        //for(int i = 0; i < 20; i++)
                            st.Insert(veb[x].Item1, veb[x].Item2);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedTree Insert 200k: " + elapsed.ToString() + " ms");
                string[] vet = new string[200000];
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 199999; x > -1; x--)
                    {
                        //for (int i = 0; i < 10; i++)
                            vet[x] = st.Get(veb[x].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedTree Get 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int x = 199999; x > -1; x--)
                    {
                        //for (int i = 0; i < 10; i++)
                            st.ContainsKey(veb[x].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedTree Contains 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    z = 0;
                    sw.Reset();
                    sw.Start();
                    //for (int x = 99999; x > -1; x--)
                    //{
                        foreach (EntryPair<string> bag in st)
                        {
                            vet[z++] = bag.Value;
                        }
                 //   }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("SortedTree Iterate 200k: " + elapsed.ToString() + " ms");

                HashList<string> vt = null;
                vt = new HashList<string>();             
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int i = 0; i < 200000; i++)
                    {
                        vt.Add(vea[i].Item1, vea[i].Item2);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("HashList Insert 200k: " + elapsed.ToString() + " ms");              

                string[] ver = new string[200000];
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int i = 199999; i > -1; i--)
                    {
                        ver[i] = vt.Get(vea[i].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("HashList Get 200k: " + elapsed.ToString() + " ms");
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int i = 199999; i > -1; i--)
                    {
                        vt.ContainsKey(vea[i].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("HashList Contains 200k: " + elapsed.ToString() + " ms");
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    z = 0;
                    sw.Reset();
                    sw.Start();
                    foreach (Vessel<string> bag in vt)
                    {
                        ver[z] = bag.Value;
                        z++;
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("HashList Iterate 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {

                    sw.Reset();
                    sw.Start();
                    for (int i = 199999; i > -1; i--)
                    {
                        vt.Remove(vea[i].Item1);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("HashList Remove 200k: " + elapsed.ToString() + " ms");
                writeEcho.Execute("HashList Count 200k: " + vt.Count);

                Dictionary<string, string> test = null;
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    test = new Dictionary<string, string>();
                    sw.Reset();
                    sw.Start();
                    for (int i = 0; i < 200000; i++)
                    {
                        test.TryAdd(vea[i].Item2, vea[i].Item2);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("Dictionary Insert 200k: " + elapsed.ToString() + " ms");
                z = 0;
                Tuple<string, string>[] ved = new Tuple<string, string>[200000];
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    sw.Reset();
                    sw.Start();
                    for (int i = 199999; i > -1; i--)
                    {
                        string value = null;
                        test.TryGetValue(vea[i].Item2, out value);
                        ved[i] = new Tuple<string, string>(vea[i].Item2, value);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("Dictionary Get 200k: " + elapsed.ToString() + " ms");

                Tuple<string, string>[] vearesult = new Tuple<string, string>[200000];

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    int x = 0;
                    sw.Reset();
                    sw.Start();
                    foreach (KeyValuePair<string, string> v in test)
                    {
                        vearesult[x] = new Tuple<string, string>(v.Key, v.Value);
                        x++;
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("Dictionary Iterate 200k: " + elapsed.ToString() + " ms");

                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    sw.Reset();
                    sw.Start();
                    for (int i = 199999; i > -1; i--)
                    {
                        string value = null;
                        test.TryRemove(vea[i].Item2, out value);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("Dictionary Remove 200k: " + elapsed.ToString() + " ms");

                writeEcho.Execute("Dictionary Count 200k: " + test.Count);

                List<string> list = null;
                elapsed = 0;
                for (int t = 0; t < 1; t++)
                {
                    list = new List<string>();
                    sw.Reset();
                    sw.Start();
                    for (int i = 0; i < 200000; i++)
                    {
                        list.Add(vea[i].Item2);
                    }
                    sw.Stop();
                    elapsed += sw.ElapsedMilliseconds;
                }
                writeEcho.Execute("List Insert 200k: " + elapsed.ToString() + " ms");

                //z = 0;
                //Tuple<string, string>[] vlist = new Tuple<string, string>[200000];
                //elapsed = 0;
                //for (int t = 0; t < 1; t++)
                //{
                //    sw.Reset();
                //    sw.Start();
                //    for (int f = 10; f > -1; f--)
                //    {
                //        int x = list.IndexOf(vear[f].Value);
                //        vlist[f] = new Tuple<string, string>(vear[f].Value, list[f]);
                //    }
                //    sw.Stop();
                //    elapsed += sw.ElapsedMilliseconds;
                //}
                //writeEcho.Execute("List Get 200k: " + elapsed.ToString() + " ms");

            }
            catch (Exception ex)
            {
                writeEcho.Execute(ex.ToString());
            }
        }

        static bool FindTest(string str)
        {
            return str.Equals(str);
        }


        //public void RunRHTreeTest(IDorsEvent WriteEcho)
        //{

        //    writeEcho = WriteEcho;
        //    writeEcho.Execute("Starting Boas Tests");           

        //    Dictionary<int, float> testTable = new Dictionary<int, float>();
        //    HashList<float> quid = new HashList<float>();

        //    int counter = 0;
        //    int sizeN = 100000;
        //    int[] keyTab = new int[sizeN];
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        int hst = (((i + "VEB" + i).GetHashCode()) << 1);
        //        hst = hst >> 1;
        //        keyTab[i] = (int)((uint)((int)(i + "VEB" + i).GetHashCode()) >> 1);
        //    }

        //    RRroot = new PeakMap<string>(int.MaxValue);
        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        RRroot.Insert(keyTab[i], i.ToString());
        //    }

        //    counter = 0;
        
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        testTable.TryAdd(keyTab[i], i);
        //    }

        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        quid.Add(keyTab[i], i);
        //    }


        //    boolTest = true;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        RRroot.Contains(keyTab[i]);
        //    }

        //    boolTest = true;
          
        //    boolTest = true;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        testTable.ContainsKey(keyTab[i]);
        //    }

        //    boolTest = true;

        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        quid.ContainsKey(keyTab[i]);
        //    }

        //    boolTest = true;

        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {
        //        RRroot.Delete(keyTab[i]);
        //    }


        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {
        //        testTable.Remove(keyTab[i]);
        //    }

        //    boolTest = false;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        boolTest = boolTest | RRroot.Contains(keyTab[i]);
        //    }

        //    int xxx = 0;
        //    xxx = 5;
        //}

        //public void RunBoasTreeTest(IDorsEvent WriteEcho)
        //{

        //    writeEcho = WriteEcho;
        //    writeEcho.Execute("Starting Boas Tests");

        //    RRVirtualBranch RRvirtualBranch = new RRVirtualBranch(16);

        //    //RRvirtualBranch.Insert(2);
        //    //RRvirtualBranch.Insert(3);
        //    //RRvirtualBranch.Insert(4);
        //    //RRvirtualBranch.Insert(5);
        //    //RRvirtualBranch.Insert(7);
        //    //RRvirtualBranch.Insert(14);
        //    //RRvirtualBranch.Insert(15);

        //    //RRroot = new RRBoasTree(16);


        //    //RRroot.Insert(2);
        //    //RRroot.Insert(3);
        //    //RRroot.Insert(4);
        //    //RRroot.Insert(5);
        //    //RRroot.Insert(7);
        //    //RRroot.Insert(14);
        //    //RRroot.Insert(15);


        //    ////RRroot.Insert(5);
        //    ////RRroot.Insert(6);
        //    //RRroot.Insert(10);
        //    //testValue = RRroot.Successor(3);
        //    //testValue = RRroot.Predecessor(3);



        //    //predecessor_value = RRroot.Predecessor(1);
        //    //predecessor_value = RRroot.Predecessor(2);
        //    //predecessor_value = RRroot.Predecessor(10);
        //    //successor_value = RRroot.Successor(1);
        //    //RRroot.Insert(2);
        //    //predecessor_value = RRroot.Predecessor(2);
        //    //successor_value = RRroot.Successor(1);
        //    //predecessor_value = RRroot.Predecessor(2);
        //    //predecessor_value = RRroot.Predecessor(10);
        //    //RRroot.Insert(3);
        //    //successor_value = RRroot.Successor(4);
        //    //RRroot.Insert(4);
        //    //RRroot.Insert(5);
        //    //RRroot.Insert(7);
        //    //RRroot.Insert(14);
        //    //RRroot.Insert(15);



        //    //RRroot.Delete(2);
        //    //boolTest = RRroot.Contains(2);
        //    //RRroot.Delete(3);
        //    //boolTest = RRroot.Contains(3);
        //    //RRroot.Delete(4);
        //    //boolTest = RRroot.Contains(4);
        //    //RRroot.Delete(5);
        //    //boolTest = RRroot.Contains(5);
        //    //RRroot.Delete(7);
        //    //boolTest = RRroot.Contains(6);
        //    //RRroot.Delete(14);
        //    //boolTest = RRroot.Contains(14);
        //    ////RRroot.Delete(15);                //!!!! uwaga problem, jezeli delete ponowny i 
        //    //boolTest = RRroot.Contains(15);
        //    //boolTest = RRroot.Contains(2);
        //    //predecessor_value = RRroot.Predecessor(2);

        //    ////Delete usunie cluster z root'a i nie moze wywolac predecesora dla zadnego clustera
        //    //predecessor_value = RRroot.Predecessor(16);

        //    //successor_value = RRroot.Successor(2);
        //    //successor_value = RRroot.Successor(3);
        //    //successor_value = RRroot.Successor(4);
        //    //successor_value = RRroot.Successor(5);
        //    //successor_value = RRroot.Successor(6);
        //    //successor_value = RRroot.Successor(7);
        //    //successor_value = RRroot.Successor(10);
        //    //successor_value = RRroot.Successor(15);


        //    ////TODO: negative values //




        //    ////RRroot.Delete(2);
        //    //boolTest = RRroot.Contains(2);
        //    //boolTest = RRroot.Contains(3);
        //    //boolTest = RRroot.Contains(4);
        //    //boolTest = RRroot.Contains(5);
        //    //boolTest = RRroot.Contains(6);
        //    //boolTest = RRroot.Contains(14);
        //    //RRroot.Delete(15);
        //    //boolTest = RRroot.Contains(15);
        //    //boolTest = RRroot.Contains(1);
        //    //boolTest = RRroot.Contains(0);
        //    //boolTest = RRroot.Contains(5);

        //    //boolTest = RRroot.Contains(16);

        //    Dictionary<int, float> testTable = new Dictionary<int, float>();


           


        //    int counter = 0;
        //    int sizeN = 32;
        //    int[] keyTab = new int[sizeN];
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        int hst = (((i + "VEB" + i).GetHashCode()) << 1);
        //        hst = hst >> 1;
        //        keyTab[i] = (int)((uint)((int)(i + "VEB" + i).GetHashCode()) >> 1);
        //    }

        //    RRvirtualBranch = new RRVirtualBranch(int.MaxValue);
        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        //RRroot.Insert(((i + "VEB" + i).GetHashCode() << 1) >> 1);
        //        //if (RRroot.Contains(keyTab[i]))
        //        //{
        //        //    counter++;
        //        //}
        //        RRvirtualBranch.Insert(keyTab[i]);
        //    }

        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        //RRroot.Insert(((i + "VEB" + i).GetHashCode() << 1) >> 1);
        //        //if (RRroot.Contains(keyTab[i]))
        //        //{
        //        //    counter++;
        //        //}
        //        RRroot.Insert(keyTab[i]);
        //    }


        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        testTable.TryAdd(keyTab[i], i);
        //    }


        //    boolTest = true;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        boolTest = boolTest & RRvirtualBranch.Contains(keyTab[i]);
        //    }

        //    boolTest = true;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        boolTest = boolTest & RRroot.Contains(keyTab[i]);
        //    }

        //    boolTest = true;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        boolTest = boolTest & testTable.ContainsKey(keyTab[i]);
        //    }


        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {
        //        RRroot.Delete(keyTab[i]);
        //    }


        //    counter = 0;
        //    for (int i = 0; i < sizeN; i++)
        //    {
        //        testTable.Remove(keyTab[i]);
        //    }

        //    boolTest = false;
        //    for (int i = 0; i < sizeN; i++)
        //    {

        //        boolTest = boolTest | RRroot.Contains(keyTab[i]);
        //    }

        //    int xxx = 0;
        //    xxx = 5;
        //}

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
        ////        writeEcho.Execute(ex.ToString();
        ////    }
        ////}

        ////public void TestSize()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-10, 10);

        ////    for (int i = -10, size = 0; i <= 10; ++i, ++size)
        ////    {
        ////        Assert.AreEqual(size, tree.Size);
        ////        tree[i, i.ToString();
        ////        Assert.AreEqual(size + 1, tree.Size);
        ////    }
        ////}

        ////public void TestIsEmpty()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-10, 10);
        ////    try
        ////    {
        ////        Assert.IsTrue(tree.IsEmpty);
        ////        tree[0, "0");
        ////        Assert.IsFalse(tree.IsEmpty);
        ////        tree[1, "1");
        ////        Assert.IsFalse(tree.IsEmpty);
        ////        tree.Remove(0);
        ////        Assert.IsFalse(tree.IsEmpty);
        ////        tree.Remove(1);
        ////        Assert.IsTrue(tree.IsEmpty);
        ////    }
        ////    catch(Exception ex)
        ////    {
        ////        writeEcho.Execute("TestIsEmpty - " + ex.ToString();
        ////    }
        ////}

        ////public void TestMinimumKey()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-5, 4);

        ////    Assert.AreEqual(5, tree.MinimumKey);

        ////    for (int i = 4; i >= -5; --i)
        ////    {
        ////        tree[i, i.ToString();
        ////        Assert.AreEqual(i, tree.MinimumKey);
        ////    }

        ////   // tree.Clear();

        ////    Assert.AreEqual(-5, tree.MinimumKey);

        ////    tree = new HyperMap<string>(int.MaxValue - 5,
        ////                                      int.MaxValue);

        ////    Assert.AreEqual(int.MinValue, tree.MinimumKey);
        ////}

        ////public void TestMaximumKey()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-5, 4);

        ////    Assert.AreEqual(-6, tree.MaximumKey);

        ////    for (int i = -5; i <= 4; ++i)
        ////    {
        ////        tree[i, i.ToString();
        ////        Assert.AreEqual(i, tree.MaximumKey);
        ////    }

        ////   // tree.Clear();

        ////    Assert.AreEqual(4, tree.MaximumKey);

        ////    tree = new HyperMap<string>(int.MinValue,
        ////                                       int.MinValue + 5);

        ////    Assert.AreEqual(int.MaxValue, tree.MaximumKey);
        ////}

        ////public void TestNextKey()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-3, 3);

        ////    tree[-3, (-3).ToString();
        ////    tree[-1, (-1).ToString();
        ////    tree[0, (0).ToString();
        ////    tree[2, (2).ToString();

        ////    Assert.AreEqual(-1, tree.NextKey(-3]);
        ////    Assert.AreEqual(-1, tree.NextKey(-2]);

        ////    Assert.AreEqual(0, tree.NextKey(-1]);
        ////    Assert.AreEqual(2, tree.NextKey(0]);
        ////    Assert.AreEqual(2, tree.NextKey(1]);
        ////    Assert.AreEqual(-4, tree.NextKey(2]);
        ////    Assert.AreEqual(-4, tree.NextKey(3]);

        ////    tree.Clear();

        ////    for (int i = -3; i <= 3; ++i)
        ////    {
        ////        Assert.AreEqual(-4, tree.NextKey(i]);
        ////    }
        ////}

        ////public void TestPreviousKey()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-3, 3);

        ////    tree[-3, (-3).ToString();
        ////    tree[-1, (-1).ToString();
        ////    tree[0, (0).ToString();
        ////    tree[2, (2).ToString();

        ////    Assert.AreEqual(2, tree.PreviousKey(3]);

        ////    Assert.AreEqual(0, tree.PreviousKey(2]);
        ////    Assert.AreEqual(0, tree.PreviousKey(1]);

        ////    Assert.AreEqual(-1, tree.PreviousKey(0]);
        ////    Assert.AreEqual(-3, tree.PreviousKey(-1]);
        ////    Assert.AreEqual(-3, tree.PreviousKey(-2]);
        ////    Assert.AreEqual(4, tree.PreviousKey(-3]);

        ////    tree.Clear();

        ////    for (int i = -3; i <= 3; ++i)
        ////    {
        ////        Assert.AreEqual(4, tree.PreviousKey(i]);
        ////    }
        ////}

        ////public void TestContainsKey()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-5, -1);

        ////    tree[-5] = null;
        ////    tree[-3] = (-3).ToString();
        ////    tree[-1] = (-1).ToString();

        ////    Assert.IsFalse(tree.ContainsKey(-5));
        ////    Assert.IsTrue(tree.ContainsKey(-3));
        ////    Assert.IsTrue(tree.ContainsKey(-1));

        ////    Assert.IsFalse(tree.ContainsKey(-4));
        ////    Assert.IsFalse(tree.ContainsKey(-2));
        ////}

        ////public void TestGet()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-5, -1);

        ////    tree[-5] = null;
        ////    tree[-3] = (-13).ToString();
        ////    tree[-1] = (-11).ToString();

        ////    Assert.IsNull(tree[-5]);
        ////    Assert.AreEqual((-11).ToString(), tree[-1]);
        ////    Assert.AreEqual((-13).ToString(), tree[-3]);
        ////}

        ////public void TestSet()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(-5, 10);

        ////    for (int i = -2; i <= 4; ++i)
        ////    {
        ////        Assert.IsFalse(tree.ContainsKey(i]);
        ////        Assert.IsNull(tree[i, (2 * i).ToString()]);
        ////        Assert.IsTrue(tree.ContainsKey(i]);
        ////    }

        ////    for (int i = -3; i >= -5; --i)
        ////    {
        ////        Assert.IsFalse(tree.ContainsKey(i]);
        ////        Assert.IsNull(tree[i]);
        ////    }

        ////    Assert.IsTrue(tree.ContainsKey(0]);
        ////    tree.Remove(0);
        ////    Assert.IsFalse(tree.ContainsKey(0]);
        ////}

        ////public void TestLowerBound()
        ////{
        ////    HyperMap<String> tree = new HyperMap<string>(-4, 4);

        ////    tree[-4, "-4");
        ////}

        ////public void TestUpperBound()
        ////{
        ////    HyperMap<String> tree = new HyperMap<string>(-4, 4);

        ////    tree[4, "4");
        ////}

        ////public void TestMapKeySeries()
        ////{
        ////    HyperMap<string> tree = new HyperMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree[i, null);
        ////    }

        ////    HyperMap<string>.KeySeries iterator = tree.MapKeyEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextKey();
        ////        Assert.AreEqual(i, iterator.NextKey();
        ////    }

        ////    Assert.AreEqual(9, tree.Size);

        ////    iterator = tree.MapKeyEnumerator();

        ////    Assert.AreEqual(-4, iterator.NextKey();
        ////    iterator.RemoveKey();
        ////    Assert.AreEqual(-3, iterator.NextKey();
        ////    Assert.AreEqual(-2, iterator.NextKey();
        ////    Assert.AreEqual(-1, iterator.NextKey();
        ////    iterator.RemoveKey();

        ////    Assert.IsFalse(tree.ContainsKey(-4]);
        ////    Assert.IsTrue(tree.ContainsKey(-3]);
        ////    Assert.IsTrue(tree.ContainsKey(-2]);
        ////    Assert.IsFalse(tree.ContainsKey(-1]);

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestTableKeySeries()
        ////{
        ////    HyperMap<string> tree = new HyperMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree[i, null);
        ////    }

        ////    HyperMap<string>.KeySeries iterator = tree.TableKeyEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextKey();
        ////        Assert.AreEqual(i, iterator.NextKey();
        ////    }

        ////    Assert.AreEqual(9, tree.Size);

        ////    iterator = tree.MapKeyEnumerator();

        ////    Assert.AreEqual(-4, iterator.NextKey();
        ////    iterator.RemoveKey();
        ////    Assert.AreEqual(-3, iterator.NextKey();
        ////    Assert.AreEqual(-2, iterator.NextKey();
        ////    Assert.AreEqual(-1, iterator.NextKey();
        ////    iterator.RemoveKey();

        ////    Assert.IsFalse(tree.ContainsKey(-4]);
        ////    Assert.IsTrue(tree.ContainsKey(-3]);
        ////    Assert.IsTrue(tree.ContainsKey(-2]);
        ////    Assert.IsFalse(tree.ContainsKey(-1]);

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestMapKeyValueSeries()
        ////{
        ////    HyperMap<String> tree = new HyperMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree[i, "" + i);
        ////    }

        ////    HyperMap<string>.KeyValueSeries iterator =
        ////            tree.MapKeyValueEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextEntry();
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

        ////    Assert.IsFalse(tree.ContainsKey(-4]);
        ////    Assert.IsTrue(tree.ContainsKey(-3]);
        ////    Assert.IsTrue(tree.ContainsKey(-2]);
        ////    Assert.IsFalse(tree.ContainsKey(-1]);

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestTableKeyValueSeries()
        ////{
        ////    HyperMap<String> tree = new HyperMap<string>(-4, 4);

        ////    for (int i = 4; i >= -4; --i)
        ////    {
        ////        tree[i, "" + i);
        ////    }

        ////    HyperMap<string>.KeyValueSeries iterator =
        ////            tree.TableKeyValueEnumerator();

        ////    for (int i = -4; i <= 4; ++i)
        ////    {
        ////        Assert.IsTrue(iterator.HasNextEntry();
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

        ////    Assert.IsFalse(tree.ContainsKey(-4]);
        ////    Assert.IsTrue(tree.ContainsKey(-3]);
        ////    Assert.IsTrue(tree.ContainsKey(-2]);
        ////    Assert.IsFalse(tree.ContainsKey(-1]);

        ////    Assert.AreEqual(7, tree.Size);
        ////}

        ////public void TestRemove()
        ////{
        ////    HyperMap<string> tree = new HyperMap<string>(1, 5);

        ////    tree[3, (3).ToString();
        ////    tree[1, (1).ToString();
        ////    tree[5, (5).ToString();

        ////    Assert.IsTrue(tree.ContainsKey(1]);
        ////    Assert.IsTrue(tree.ContainsKey(3]);
        ////    Assert.IsTrue(tree.ContainsKey(5]);

        ////    tree[1, null);

        ////    Assert.IsTrue(tree.ContainsKey(1]);

        ////    tree.Remove(1);

        ////    Assert.IsFalse(tree.ContainsKey(1]);
        ////}

        ////public void TestClear()
        ////{
        ////    HyperMap<string> tree =
        ////            new HyperMap<string>(3, 10);

        ////    for (int i = 4, sz = 0; i <= 8; ++i, ++sz)
        ////    {
        ////        Assert.AreEqual(sz, tree.Size);
        ////        tree[i, null);
        ////        Assert.AreEqual(sz + 1, tree.Size);
        ////    }

        ////    Assert.AreEqual(5, tree.Size);
        ////    tree.Clear();
        ////    Assert.AreEqual(0, tree.Size);

        ////    for (int i = 3; i <= 10; ++i)
        ////    {
        ////        Assert.IsFalse(tree.ContainsKey(i]);
        ////        Assert.IsNull(tree[i]);
        ////        Assert.IsNull(tree.Remove(i]);
        ////    }
        ////}

    }
}
