using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * This class implements a sorted map mapping integer keys to values of 
 * arbitrary type.
 * 
 * @author Rodion "rodde" Efremov
 * @version 1.6 (Dec 9, 2017)
 * @param <V> the type of values.
 */
namespace System.Doors
{
    public interface IScopeNode
    {
        int UniverseSize { get; }

        int MinimumKey { get; }

        int MaximumKey { get; }

        int Successor(int offsetBase, int offsetFactor, int offsetIndex, int x);

        int Successor(int x);

        int Predecessor(int offsetBase, int offsetFactor, int offsetIndex, int x);

        int Predecessor(int x);

        bool Contains(int offsetBase, int offsetFactor, int offsetIndex, int x);

        bool Contains(int x);

        void Insert(int offsetBase, int offsetFactor, int offsetIndex, int x);

        void Insert(int x);

        void FirstInsert(int offsetBase, int offsetFactor, int offsetIndex, int x);

        void FirstInsert(int x);

        bool Delete(int offsetBase, int offsetFactor, int offsetIndex, int x);

        bool Delete(int x);        
    }

    public class ScopeTreeRH<V>
    {
        private HashList<V> table;
        //zmienic na private -> public do testow
        public ScopeBranch root;

        private Dictionary<int, IScopeNode> globalCluster;

        public int Count { get; set; }

        public ScopeTreeRH()
        { }
        public ScopeTreeRH(int range)
        {
            Initialize(range);
        }

        public void Initialize(int range = 0)
        {
            //table = new Dictionary<int, V>(new HashComparer());
            globalCluster = new Dictionary<int, IScopeNode>(new HashComparer());

            if ((range == 0) || (range > int.MaxValue))
            {
                range = int.MaxValue;
                table = new HashList<V>();
            }
            else
                table = new HashList<V>(range);

            root = new ScopeBranch(range, globalCluster);
        }

        public int MinimumKey
        {
            get { return root.MinimumKey; }
        }

        public int MaximumKey
        {
            get { return root.MaximumKey; }
        }

        public bool ContainsKey(int key)
        {
            return table.ContainsKey(key);
        }

        //public bool ContainsValue(V obj)
        //{
        //    return table.ContainsValue(obj);
        //}

        public bool Insert(int key, V obj)
        {
            if (table.Add(key, obj))
            {
                root.Insert(0, 1, 0, key);
                Count++;
                return true;
            }
            return false;
        }

        public bool Delete(int key)
        {
            if (table.TryRemove(key))
            {
                root.Delete(0, 1, 0, key);
                Count--;
                return true;
            }
            return false;
        }

        public int NextKey(int key)
        {
            return root.Successor(0, 1, 0, key);
        }

        public int PreviousKey(int key)
        {
            return root.Predecessor(0, 1, 0, key);
        }

        public V Get(int key)
        {
            return table.Get(key);
        }

        public bool Set(int key, V value)
        {
            return Insert(key, value);
        }

        public EntrySeries<V> GetEnumerator()
        {
            return new EntrySeries<V>(this);
        }
    }

    public struct ScopeBranch : IScopeNode
    {
        private static int MINIMUM_UNIVERSE_SIZE_U4 = 4;
        public static int NULL_KEY = -1;

        private int universeSize;

        private int upperUniverseSizeSquare;
        private int lowerUniverseSizeSquare;

        private int min;
        private int max;

        IScopeNode summary;
        Dictionary<int, IScopeNode> summaryCluster; //zbior cluster dla summary, ale nie jest global, a klasyczny uzywany przez elementy summary; nie ma specjalnych wyliczen

        private Dictionary<int, IScopeNode> globalCluster;   //!!!! UWAGA: musi być global ale nie dla wyszystkich obiektów tylko powiązanych z danym SortedTreeRH -> referencja może, jako argument?

        public ScopeBranch(int _universeSize, Dictionary<int, IScopeNode> _globalCluster)
        {
            globalCluster = _globalCluster;
           
            this.universeSize = _universeSize;
            upperUniverseSizeSquare = upperSquare(universeSize);
            lowerUniverseSizeSquare = lowerSquare(universeSize);

            this.min = NULL_KEY;
            this.max = NULL_KEY;
            this.summary = null;
            this.summaryCluster = null;
        }

        public int UniverseSize
        {
            get { return universeSize; }
        }

        public int MinimumKey
        {
            get { return min; }
        }

        public int MaximumKey
        {
            get { return max; }
        }

        public bool Contains(int x)
        {
            return Contains(0, 1, 0, x);
        }
        public bool Contains(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (x == min || x == max)
            {
                return true;
            }
            else
            {
                if ((universeSize == MINIMUM_UNIVERSE_SIZE_U4) || (x < min) || (x > max))
                {
                    return false;
                }
                else
                {
                    int summaryClusterKey = offsetBase + offsetIndex * upperUniverseSizeSquare + high(x);
                    IScopeNode summaryClusterItem;
                    if (!globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem)) return false;
                    return summaryClusterItem.Contains(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + high(x), low(x));
                }
            }
        }

        public int Successor(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (min != NULL_KEY && x < min)
            {
                return min;
            }

            IScopeNode summaryClusterItem;
            int x_high = high(x);
            int summaryClusterKey;

            summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + x_high;
            
            int maximumLow = NULL_KEY;
            if (globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem))
            {
                maximumLow = summaryClusterItem.MaximumKey;
            }

            if (maximumLow != NULL_KEY && low(x) < maximumLow)
            {
                int _offset = summaryClusterItem.Successor(offsetBase + (offsetFactor * upperUniverseSizeSquare), (offsetFactor * upperUniverseSizeSquare), (offsetIndex * upperUniverseSizeSquare) + x_high, low(x));

                return index(x_high, _offset);
            }

            if (summary == null)
            {
                return NULL_KEY;
            }

            //======================//
            //from summary part 
            //======================//
            int successorCluster = summary.Successor(x_high); //return index of the next summaryCluster for this very level

            if (successorCluster == NULL_KEY)
            {
                return NULL_KEY;
            }

            summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + successorCluster;

            globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem);
            int offset = summaryClusterItem.MinimumKey;

            return index(successorCluster, offset);
        }       
        public int Successor(int x)
        {
            if (min != NULL_KEY && x < min) // is nullOrSmallerThanMin
            {
                return min;
            }

            if (summaryCluster == null) return NULL_KEY;
            IScopeNode summaryClusterItem;
            int x_high = high(x);            

            int maximumLow = NULL_KEY;
            if (summaryCluster.TryGetValue(x_high, out summaryClusterItem))
            {
                maximumLow = summaryClusterItem.MaximumKey;
            }

            if (maximumLow != NULL_KEY && low(x) < maximumLow)
            {
                int _offset = summaryClusterItem.Successor(low(x));
                return index(x_high, _offset);
            }

            if (summary == null)
            {
                return NULL_KEY;
            }

            int successorCluster = summary.Successor(x_high);
            if (successorCluster == NULL_KEY)
            {
                return NULL_KEY;
            }

            summaryCluster.TryGetValue(successorCluster, out summaryClusterItem);
            int offset = summaryClusterItem.MinimumKey;
            return index(successorCluster, offset);
        }    // used only for summary

        public int Predecessor(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (max != NULL_KEY && x > max)
            {
                return max;
            }

            IScopeNode summaryClusterItem;
            int x_high = high(x);
            int summaryClusterKey;

            summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + x_high;
            
            int minimumLow = NULL_KEY;
            if (globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem))
            {
                minimumLow = summaryClusterItem.MinimumKey;
            }

            if (minimumLow != NULL_KEY && low(x) > minimumLow)
            {
                int _offset = summaryClusterItem.Predecessor(offsetBase + (offsetFactor * upperUniverseSizeSquare), (offsetFactor * upperUniverseSizeSquare), (offsetIndex * upperUniverseSizeSquare) + x_high, low(x));
                return index(x_high, _offset);
            }

            if (summary == null)
            {
                return NULL_KEY;
            }

            //======================//
            //from summary part 
            //======================//
            int predecessorCluster = summary.Predecessor(x_high);
            if (predecessorCluster == NULL_KEY)     //summary or sumary.Predecesor can be null, but "min" is not stored in summaryCluster, thus, min has to be checked
            {
                if (min != NULL_KEY && x > min) //min is not stored in summaryCluster, thus it has to be checked
                {
                    return min;
                }

                return NULL_KEY;
            }
            summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + predecessorCluster;

            globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem);
            int offset = summaryClusterItem.MaximumKey;
            return index(predecessorCluster, offset);
        }
        public int Predecessor(int x)
        {

            if (max != NULL_KEY && x > max)
            {
                return max;
            }

            if (summaryCluster == null) return NULL_KEY;
            IScopeNode summaryClusterItem;
            int x_high = high(x);            

            int minimumLow = NULL_KEY;
            if (summaryCluster.TryGetValue(x_high, out summaryClusterItem))
            {
                minimumLow = summaryClusterItem.MinimumKey;
            }

            if (minimumLow != NULL_KEY && low(x) > minimumLow)
            {
                int _offset = summaryClusterItem.Predecessor(low(x));
                return index(x_high, _offset);
            }

            if (summary == null)
            {
                return NULL_KEY;
            }

            int predecessorCluster = summary.Predecessor(x_high);
            if (predecessorCluster == NULL_KEY)     //summary or sumary.Predecesor can be null, but "min" is not stored in summaryCluster, thus, min has to be checked
            {
                if (min != NULL_KEY && x > min) //min is not stored in summaryCluster, thus it has to be checked
                {
                    return min;
                }

                return NULL_KEY;
            }

            summaryCluster.TryGetValue(predecessorCluster, out summaryClusterItem);
            int offset = summaryClusterItem.MaximumKey;
            return index(predecessorCluster, offset);
        }

        //L1: keyCluster = high(x1), gdzie x1 to nasz początkowy x wstawiany do boasa
        //L2: keyCluster = u1 + high(x1) * u2 + high(x2), gdzie u1 to universum level 1, u2 to uniwersum L2 (do wyliczenia ze wzoru), x2 = low(x1)
        //      idx2 = high(x1)*u2 + high(x2) offset dla L2
        //L3: keyCluster = (u1 + u1*u2) + idx2*u3+high(x3), gdzie u3 to universum L3 (wyliczane ze wzoru w boasie), x3 = low(x2)
        // offsetBase = (u1+u1*u2), offsetFactor = u1*u2, offsetIndex = idx, 
        //Insert(offsetBase + offsetFactor * UniverseSize, offsetFactor * UniverseSize, offsetIndex * UniverseSize + high(x), low(x));
        //
        public void Insert(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if ((min == x) || (max == x))
            {
                return;
            }

            if (min == NULL_KEY)
            {
                FirstInsert(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + high(x), x);
                return;
            }

            if (x < min)
            {
                int tmp = x;
                x = min;
                min = tmp;
            }

            if (universeSize != MINIMUM_UNIVERSE_SIZE_U4)
            {
                IScopeNode summaryClusterItem;
                int x_high = high(x);

                int summaryClusterKey = offsetBase + offsetIndex * upperUniverseSizeSquare + x_high;

                if (!globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem))
                {
                    if (upperUniverseSizeSquare == MINIMUM_UNIVERSE_SIZE_U4)
                    {
                        if (summary == null)    //summary of the current level (u>4, e.g., u=16)
                        {
                            summary = new ScopeLeaf(-1);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high); //tutaj zrobic else
                        }
                        summaryClusterItem = new ScopeLeaf(-1);
                        globalCluster.Add(summaryClusterKey, summaryClusterItem);
                        summaryClusterItem.FirstInsert(low(x));
                    }
                    else //create new branch
                    {
                        if (summary == null)
                        {
                            summary = new ScopeBranch(upperUniverseSizeSquare, globalCluster);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high); //tutaj zrobic else
                        }

                        summaryClusterItem = new ScopeBranch(upperUniverseSizeSquare, globalCluster);
                        globalCluster.Add(summaryClusterKey, summaryClusterItem);
                        summaryClusterItem.FirstInsert(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + x_high, low(x));
                    }
                }
                else
                {
                    summaryClusterItem.Insert(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + x_high, low(x));
                }
            }

            if (max < x)
            {
                max = x;
            }
        }       
        public void Insert(int x)
        {
            if ((min == x) || (max == x))
            {
                return;
            }

            //if (min == NULL_KEY)
            //{
            //    FirstInsert(x); //nie wykonuje się nigdy
            //    return;
            //}

            if (x < min)
            {
                int tmp = x;
                x = min;
                min = tmp;                
            }

            if (universeSize != MINIMUM_UNIVERSE_SIZE_U4)
            {
                IScopeNode summaryClusterItem;
                int x_high = high(x);

                if (!summaryCluster.TryGetValue(x_high, out summaryClusterItem))
                {
                    if (upperUniverseSizeSquare == MINIMUM_UNIVERSE_SIZE_U4)
                    {
                        if (summary == null)
                        {
                            summary = new ScopeLeaf(-1);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high);     //nie powinno byc else i first insert to summary??? (insert to zawiera, ale po co?)
                        }
                        summaryClusterItem = new ScopeLeaf(-1);
                        summaryCluster.Add(x_high, summaryClusterItem);
                        summaryClusterItem.FirstInsert(low(x));
                    }
                    else //create new branch
                    {
                        if (summary == null)
                        {
                            summary = new ScopeBranch(upperUniverseSizeSquare, globalCluster);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high);
                        }
                        summaryClusterItem = new ScopeBranch(upperUniverseSizeSquare, globalCluster);
                        summaryCluster.Add(x_high, summaryClusterItem);
                        summaryClusterItem.FirstInsert(low(x));
                    }
                }
                else
                {
                    summaryClusterItem.Insert(low(x));
                }
            }

            if (max < x)
            {
                max = x;
            }
        }    //used only by summary

        public void FirstInsert(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            min = x;
            max = x;
        }      
        public void FirstInsert(int x)
        {
            min = x;
            max = x;

            summaryCluster = new Dictionary<int, IScopeNode>(new HashComparer());
        }    // for summary

        public bool Delete(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            IScopeNode summaryClusterItem;
            int x_high;
            int summaryClusterKey;


            if (min == max) //one element only, after deletion empty set
            {
                if (min != x) return false;   // if delete (x), which do not belong to tree
                min = NULL_KEY;
                max = NULL_KEY;
                summary = null;
                //summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + high(x);
                //globalCluster.Remove(summaryClusterKey);                         //there should be no summaryCluster inside of this, because, there were Removed 

                //kasowanie aktualnego summaryCluster odbedzie sie w dalszej czesci jak zwroci z wywolania i bedzie min == NULL_KEY
                return true;
            }

            //at least 2 elements and not leaf node (universum > 4) -> remove element from block or min
            if (min == x)   //delete min and set new min, min is not stored in summaryClusters
            {
                int firstCluster = summary.MinimumKey; //if at least 2 elements, it cannot be null
                
                summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + firstCluster;
                globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem);
                x = index(firstCluster, summaryClusterItem.MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min

                //x = index(firstCluster, summaryCluster[firstCluster].MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min
                min = x;
                //set the next value after min to be new "min" but it must be removed from summaryCluster (after)
            }

            //Delete x from summaryClusters (x or x, which above becomes new "min"
            x_high = high(x);
            summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + x_high;
            
            if (!globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem)) return false;    //no such element to be deleted
            //summaryClusterItem.Delete(low(x));
            summaryClusterItem.Delete(offsetBase + (offsetFactor * upperUniverseSizeSquare), (offsetFactor * upperUniverseSizeSquare), (offsetIndex * upperUniverseSizeSquare) + x_high, low(x));

            //being in this place means that deletion process reached the leaf, then deleted min or max (then min=max, and maxkey become min, in other words max for summaryCluster is updated)

            //if (summaryCluster[high(x)].MinimumKey == NULL_KEY)
            if (summaryClusterItem.MinimumKey == NULL_KEY)    //if the summaryCluster[high(x)] is empty, but remember there is another summaryCluster that is not empty, since at leat 2 elements are in this node
            {
                //remove elements related with current summaryCluster, which we know is empty
                //summaryCluster.Remove(high(x));   
                
                //!!!! PYTANIE: czy to spowoduje, ze obiekt, ktorego reference znajduje sie w globalCluster(x_high) zostanie usuniety - chyba tak, nie ma innej referencji do obiektu takiego
                globalCluster.Remove(summaryClusterKey);
                
                summary.Delete(high(x));    //if [high(x)] is empty, then delete index of this block from summary, i.e., delete [high(x)] in summary, it is handeled by "elese if(x==max)" deleting max element from node (which can be a summary summaryCluster)

                if (x == max)   //if max element is deleted, then update summary.max for this node
                {
                    int summaryMaximum = summary.MaximumKey; //previously we called summary.Delete(high(x)), which properly updated summary.MaximumKey

                    if (summaryMaximum == NULL_KEY) //if the only element is min, which is not stored in summaryCluster, but in "min"
                    {
                        max = min;
                        summary = null; //the only element is min
                    }
                    else  //there is other element (not only min), thus, max is the maximum element in the first non-empty block (summaryCluster) summaryMaximum is not null, it has a value
                    {
                        //int maximumKey = summaryCluster[summaryMaximum].MaximumKey;
                        summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + summaryMaximum;
                        globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem);

                        int maximumKey = summaryClusterItem.MaximumKey;
                        max = index(summaryMaximum, maximumKey);
                    }
                }
            }
            else if (x == max)      //delete max element, but it is not empty, then 
            {
                //int maximumKey = summaryCluster[high(x)].MaximumKey;   //summaryCluster including element (x==max), and there is at least one other element (the element maximumKey was updated properly due to previous recursive call

                summaryClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + high(x);
                globalCluster.TryGetValue(summaryClusterKey, out summaryClusterItem);
                int maximumKey = summaryClusterItem.MaximumKey;

                max = index(high(x), maximumKey);   //update max = highest value in the summaryCluster previously included x
            }
            return true;
        }      
        public bool Delete(int x)
        {
            if (min == max) //one element only, after deletion empty set
            {
                if (min != x) return true;   // if delete (x), which do not belong to tree
                min = NULL_KEY;
                max = NULL_KEY;
                summary = null;
                summaryCluster = null;                         //!!! problem jezeli skasujemy dla root drzewa wowczas, nie dziala predecesor i successor (summaryCluster == null i nie moze sprawdzic)
                return true;
            }

            //at least 2 elements and not based node (universum > 4) -> remove element from block or min

            if (min == x)   //delete min and set new min, min is not stored in summaryClusters
            {
                int firstCluster = summary.MinimumKey; //if at least 2 elements, it cannot be null
                x = index(firstCluster, summaryCluster[firstCluster].MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min
                min = x;
                //set it as "min" but remove from summaryCluster
            }

            //Delete x from summaryClusters
            IScopeNode summaryClusterItem;
            if (!summaryCluster.TryGetValue(high(x), out summaryClusterItem)) return true;    //no such element to be deleted
            summaryClusterItem.Delete(low(x));

            //being in this place means that deletion process reached the leaf, then deleted min or max (then min=max, and maxkey become min, in other words max for summaryCluster is updated)

            //if (summaryCluster[high(x)].MinimumKey == NULL_KEY)
            if (summaryClusterItem.MinimumKey == NULL_KEY)    //if the summaryCluster[high(x)] is empty, but remember there is another summaryCluster that is not empty, since at leat 2 elements are in this node
            {
                //remove elements related with current summaryCluster, which we know is empty
                summaryCluster.Remove(high(x));

                summary.Delete(high(x));    //if [high(x)] is empty, then delete index of this block from summary, i.e., delete [high(x)] in summary, it is handeled by "elese if(x==max)" deleting max element from node (which can be a summary summaryCluster)

                if (x == max)   //if max element is deleted, then update summary.max for this node
                {
                    int summaryMaximum = summary.MaximumKey; //previously we called summary.Delete(high(x)), which properly updated summary.MaximumKey

                    if (summaryMaximum == NULL_KEY) //if the only element is min, which is not stored in summaryCluster, but in "min"
                    {
                        max = min;
                        summary = null; //the only element is min
                    }
                    else  //there is other element (not only min), thus, max is the maximum element in the first non-empty block (summaryCluster) summaryMaximum is not null, it has a value
                    {
                        int maximumKey =
                                summaryCluster[summaryMaximum].MaximumKey;
                        max = index(summaryMaximum, maximumKey);
                    }
                }
            }
            else if (x == max)      //delete max element, but it is not empty, then 
            {
                int maximumKey = summaryCluster[high(x)].MaximumKey;   //summaryCluster including element (x==max), and there is at least one other element (the element maximumKey was updated properly due to previous recursive call
                max = index(high(x), maximumKey);   //update max = highest value in the summaryCluster previously included x
            }
            return true;
        }     // for summary

        private int high(int x)
        {
            //x=192053244 => float fx = x => 192053248 !!!!!
            //return (int)((double)((double)(x) / (double)(lowerUniverseSizeSquare)));
            return x / lowerUniverseSizeSquare;
        }

        private int low(int x)
        {
            //equivalent to: x % (lowerUniverseSizeSquare)
            return x & (lowerUniverseSizeSquare - 1);            
        }

        private int index(int x, int y)
        {
            return (x * lowerUniverseSizeSquare + y);
        }

        private static int upperSquare(int number)
        {

            double exponent = Math.Ceiling(Math.Log(number) / Math.Log(2.0) / 2.0);
            return (int)Math.Pow(2.0, exponent);
        }
        private static int lowerSquare(int number)
        {
            double exponent = Math.Floor(Math.Log(number) / Math.Log(2.0) / 2.0);
            return (int)Math.Pow(2.0, exponent);
        }
    }
    
    public struct ScopeLeaf : IScopeNode
    {
        public int[] xValue;

        public ScopeLeaf(int null_key)
        {
            xValue = new int[] { null_key, null_key, null_key, null_key };
        }

        public int UniverseSize
        {
            get { return 4; }
        }

        public int MinimumKey
        {
            get { return xValue[0]; }
        }

        public int MaximumKey
        {
            get { return xValue[3]; }
        }

        public bool Contains(int x)
        {
            if (xValue[x] == x)
            {
                return true;
            }

            return false;
        }
        public bool Contains(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (xValue[x] == x)
            {
                return true;
            }

            return false;
        }

        public int Successor(int offsetBase, int offsetFactor, int offsetIndex, int x)        
        {                        
//            if (x >= xValue[3]) return -1; //x == max or max == null 
//            if (xValue[x + 1] > x) return xValue[x + 1];
//            if (xValue[x + 2] > x) return xValue[x + 2];           
//            return xValue[3];

            if (x >= xValue[3]) return -1; //x == max or max == null 
            if (xValue[x + 1] != -1) return xValue[x + 1];    //0, 1, 2
            if (xValue[x + 2] != -1) return xValue[x + 2];    //0, 1
            return xValue[3]; //1


        }
        public int Successor(int x)
        {
            //dziala
            //if (x >= xValue[3]) return -1; //x == max or max == null 
            //if (xValue[x + 1] > x) return xValue[x + 1];
            //if (xValue[x + 2] > x) return xValue[x + 2];

            if (x >= xValue[3]) return -1; //x == max or max == null 
            if (xValue[x + 1] != -1) return xValue[x + 1];    //0, 1, 2
            if (xValue[x + 2] != -1) return xValue[x + 2];    //0, 1
            return xValue[3]; //1
        }

        public int Predecessor(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (x <= xValue[0]) return -1; //x == min or min == null
            if (xValue[x - 1] != -1) return xValue[x - 1];    //1, 2, 3
            if (xValue[x - 2] != -1) return xValue[x - 2];    //2, 3
            return xValue[0];   //3
        }
        public int Predecessor(int x)
        {
            if (x <= xValue[0]) return -1; //x == min or min == null
            if (xValue[x - 1] != -1) return xValue[x - 1];
            if (xValue[x - 2] != -1) return xValue[x - 2];            
            return xValue[0];
        }

        //if leaf is summary u=2 => //summary can have min=max=null; min=max=0; min=max=1; and min=0, max=1;   
        //if leaf is a summaryCluster item, it is always: min=max {null or 1}
        // min  | 0 | 1 | 2 | 3 | 0 | 0 | .. | 0 | .. | 0 |
        // mid0 | - | 1 | - | - | 1 | - | .. | - | .. | 1 |
        // mid1 | - | - | 2 | - | - | 2 | .. | 2 | .. | 2 |
        // max  | 0 | 1 | 2 | 3 | 1 | 2 | .. | 3 | .. | 3 |
        //
        public void Insert(int x)
        {
            if (xValue[x] == x)
            {
                return;
            }

            xValue[x] = x;
            if (x > xValue[3])
            {
                xValue[3] = x;
                return;
            }

            if (x < xValue[0])
            {
                xValue[0] = x;
                return;
            }
        }
        public void Insert(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (xValue[x] == x)
            {
                return;
            }

            xValue[x] = x;
            if (x > xValue[3])
            {
                xValue[3] = x;
                return;

            }
            if (x < xValue[0])
            {
                xValue[0] = x;
                return;
            }
        }

        public void FirstInsert(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            xValue[0] = x;
            xValue[3] = x;
            xValue[x] = x;
        }
        public void FirstInsert(int x)  //first insert to leaf means that min = -1
        {
            xValue[0] = x;
            xValue[3] = x;
            xValue[x] = x;
        }

        public bool Delete(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (xValue[x] != x) return false; //(27 case)

            //update if x is min
            if (xValue[0] == x)
            {
                xValue[x] = -1;
                if (xValue[1] != -1) { xValue[0] = xValue[1]; return true; }
                if (xValue[2] != -1) { xValue[0] = xValue[2]; return true; }
                if (xValue[3] != -1 && xValue[3] != x) { xValue[0] = xValue[3]; return true; } //more often than min==max
                xValue[0] = -1;
                xValue[3] = -1;
                return true;
            }
            //update if x is max
            if (xValue[3] == x)
            {
                xValue[x] = -1;
                if (xValue[2] != -1) { xValue[3] = xValue[2]; return true; }
                if (xValue[1] != -1) { xValue[3] = xValue[1]; return true; }
                //knowing that xValue[0] != x and xValue[0] != -1, then
                xValue[3] = xValue[0];
                return true;
            }
            xValue[x] = -1;
            return true;    
        }       
        public bool Delete(int x)
        {
            if (xValue[x] != x) return false; //(27 case)

            //x is stored
            //if (xValue[0] == xValue[3]) { xValue[0] = -1; xValue[3] = -1; return true; }
           
            //update if x is min
            if (xValue[0] == x)
            {
                xValue[x] = -1;
                if (xValue[1] != -1) { xValue[0] = xValue[1]; return true; }
                if (xValue[2] != -1) { xValue[0] = xValue[2]; return true; }
                if (xValue[3] != -1 && xValue[3]!=x) { xValue[0] = xValue[3]; return true; } //more often than min==max
                xValue[0] = -1;
                xValue[3] = -1;
                return true;
            }
            //update if x is max
            if (xValue[3] == x)
            {
                xValue[x] = -1;
                if (xValue[2] != -1) { xValue[3] = xValue[2]; return true; }
                if (xValue[1] != -1) { xValue[3] = xValue[1]; return true; }
                //knowing that xValue[0] != x and xValue[0] != -1, then
                xValue[3] = xValue[0];
                return true;
            }
            xValue[x] = -1;
            return true;
        }    //update: min & max (4); min (3); max (3); min & middle (3+4); max & middle (3+4); update middle (4); 

    }   //For ScopeLeaf universeSize = 4 if insert to leaf as summary u=4 => x \in {0, 1, 2, 3} xValue[0] => min, xValue[3] => max
  
    public class EntryPair<V>
    {
        public int Key;
        public V Value;

        public EntryPair()
        { }
        public EntryPair(int key, V value)
        {
            Key = key;
            Value = value;
        }

        public override String ToString()
        {
            return "(" + Key + " -> " + Value.ToString() + ")";
        }
    }

    public class EntrySeries<V> : IEnumerator
    {
        private ScopeTreeRH<V> map;
        private int iterated = 0;
        private int lastReturned;

        public EntrySeries(ScopeTreeRH<V> Map)
        {
            map = Map;
            Entry = new EntryPair<V>();
        }

        public EntryPair<V> Entry;

        public int Key { get { return Entry.Key; } }
        public V Value { get { return Entry.Value; } }

        public object Current => Entry;

        public bool MoveNext()
        {
            return Next();
        }

        public void Reset()
        {
            Entry = new EntryPair<V>();
            iterated = 0;
        }

        public bool HasNext()
        {
            return iterated < map.Count;
        }

        public bool Next()
        {
            if (!HasNext())
            {
                return false;
            }

            if (iterated == 0)
            {
                lastReturned = map.MinimumKey;
                iterated++;
                Entry.Key = lastReturned;
                Entry.Value = map.Get(lastReturned);
            }
            else
            {
                lastReturned = map.NextKey(lastReturned); ;
                iterated++;
                Entry.Key = lastReturned;
                Entry.Value = map.Get(lastReturned);
            }
            return true;
        }
    }

}
