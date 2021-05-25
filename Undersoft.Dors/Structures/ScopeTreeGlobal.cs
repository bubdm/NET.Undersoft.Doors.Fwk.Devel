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
namespace System.Dors
{
    public interface IScopeTreeNode
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


    public class NodeTypeInfo
    {        
        public int BaseIndexOffset { get; set; }        //start index in the global cluster of this type
        public int NodeTypeCounter { get; set; }        //number of elements of given type
        public int NodeClusterSize { get; set; }        // cluster size of the given type of a node      
    }

    public class TreeLevelInfo
    {
        public byte TreeLevel { get; set; }
        public byte Count { get; set; }
        public int BaseOffset { get; set; }
        public List<NodeTypeInfo> NodeTypeInfoList { get; set; }
        public TreeLevelInfo()
        {
            TreeLevel = 0;
            BaseOffset = 0;
            NodeTypeInfoList = null;
        }
    }


    public class ScopeTreeRHGlobal<V>
    {
        private HashList<V> table;
        private ScopeTreeInternalNode root;

        private Dictionary<int, IScopeTreeNode> globalCluster;

        private Dictionary<int, IScopeTreeNode> globalSummaryCluster;

        private List<TreeLevelInfo> treeLevelInfoList;

        public int Count { get; set; }

        public int UniverseSize { get; set; }

        public ScopeTreeRHGlobal()
        {
            Initialize(0);
        }

        public ScopeTreeRHGlobal(int range)
        {
            Initialize(range);
        }


        private void BuildGlobalSummaryOffsetList(int universeSize, byte treeLevel, byte nodeTypeIndex, int nodeTypeCounter, int clusterSize)
        {
            int upperUniverseSizeSquare = ScopeTreeInternalNode.upperSquare(universeSize);

            if (treeLevelInfoList == null)
            {
                treeLevelInfoList = new List<TreeLevelInfo>();
            }
            if (treeLevelInfoList.Count <= treeLevel)
            {
                treeLevelInfoList.Add(new TreeLevelInfo());
            }
            if (treeLevelInfoList[treeLevel].NodeTypeInfoList == null)
            {
                treeLevelInfoList[treeLevel].NodeTypeInfoList = new List<NodeTypeInfo>();
                treeLevelInfoList[treeLevel].NodeTypeInfoList.Add(new NodeTypeInfo());
            }
            else
            {
                treeLevelInfoList[treeLevel].NodeTypeInfoList.Add(new NodeTypeInfo());
            }

            treeLevelInfoList[treeLevel].NodeTypeInfoList[nodeTypeIndex].NodeTypeCounter = nodeTypeCounter;
            treeLevelInfoList[treeLevel].NodeTypeInfoList[nodeTypeIndex].NodeClusterSize = upperUniverseSizeSquare;

            if (upperUniverseSizeSquare > 4)
            {
                // summary
                BuildGlobalSummaryOffsetList(upperUniverseSizeSquare, (byte)(treeLevel + 1), (byte)(2 * nodeTypeIndex), nodeTypeCounter, upperUniverseSizeSquare);
                // cluster
                BuildGlobalSummaryOffsetList(upperUniverseSizeSquare, (byte)(treeLevel + 1), (byte)(2 * nodeTypeIndex + 1), nodeTypeCounter * upperUniverseSizeSquare, upperUniverseSizeSquare);
            }

        }

        private void CreateTreeLevelInfoList(int universeSize)
        {
            if (treeLevelInfoList == null)
            {
                int upperUniverseSizeSquare = ScopeTreeInternalNode.upperSquare(universeSize);
                BuildGlobalSummaryOffsetList(universeSize, 0, 0, 1, upperUniverseSizeSquare);
            }

            int baseOffset = 0;
            for (int i = 1; i < treeLevelInfoList.Count; i++)
            {
                treeLevelInfoList[i].BaseOffset = baseOffset;
                for (int j = 0; j < treeLevelInfoList[i].NodeTypeInfoList.Count - 1; j++)  //the last in each .NodeTypeInfoList[] is not summary node, but refer to regular node
                {
                    treeLevelInfoList[i].NodeTypeInfoList[j].BaseIndexOffset = baseOffset;
                    baseOffset += treeLevelInfoList[i].NodeTypeInfoList[j].NodeTypeCounter * treeLevelInfoList[i].NodeTypeInfoList[j].NodeClusterSize;
                }
            }

        }

        public void Initialize(int range = 0)
        {
            //table = new Dictionary<int, V>(new HashComparer());
            globalCluster = new Dictionary<int, IScopeTreeNode>(new HashComparer());
            globalSummaryCluster = new Dictionary<int, IScopeTreeNode>(new HashComparer());

            if ((range == 0) || (range > int.MaxValue))
            {
                range = int.MaxValue;

                // !!! uwaga cos nie dziala HashList dla range = int.MaxValue
                table = new HashList<V>();
            }
            else
            {
                // !!! uwaga cos nie dziala HashList dla range = int.MaxValue
                table = new HashList<V>(range);
            }
            UniverseSize = range;

            CreateTreeLevelInfoList(range);   //create treeLevelInfoList

            root = new ScopeTreeInternalNode(range, globalCluster, globalSummaryCluster, treeLevelInfoList, 0, 0, 0);

            //budowanie struktury indeksow dla globalSummary
                        
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

        public EntrySeriesGlobal<V> GetEnumerator()
        {
            return new EntrySeriesGlobal<V>(this);
        }

        
        //do testowania samego drzewa
        public bool TestInsert(int key)
        {
            root.Insert(0, 1, 0, key);
            Count++;
            return true;
        }
        public bool TestDelete(int key)
        {
            root.Delete(0, 1, 0, key);
            Count--;
            return true;
        }

        public bool TestContains(int key)
        {
            return root.Contains(0,1,0,key);
        }
    }

    public struct ScopeTreeInternalNode : IScopeTreeNode
    {
        private static int MINIMUM_UNIVERSE_SIZE_U4 = 4;
        public static int NULL_KEY = -1;

        private int universeSize;

        private int upperUniverseSizeSquare;
        private int lowerUniverseSizeSquare;

        private int min;
        private int max;

       
        //individual summaryCluster for each summary node)
        //Dictionary<int, IScopeNodeGlobal> summaryCluster; //zbior cluster dla node summary, ale nie jest global, a klasyczny uzywany przez elementy summary; nie ma specjalnych wyliczen - kazdy node summary ma swoj cluster

        //globalSummaryCluster - nie jest zaimplementowane jeszcze        
        IScopeTreeNode summary;
        private Dictionary<int, IScopeTreeNode> globalSummaryCluster;   //!!!! UWAGA: musi być global ale nie dla wyszystkich obiektów tylko powiązanych z danym SortedTreeRH -> referencja jako argument
        private Dictionary<int, IScopeTreeNode> globalCluster;   //!!!! UWAGA: musi być global ale nie dla wyszystkich obiektów tylko powiązanych z danym SortedTreeRH -> referencja jako argument

        //dla wyliczania key dla globalSummaryCluste - mozna pomyslec o przesunieciu do metod - przekazywac przez metody
        private List<TreeLevelInfo> treeLevelInfoList;        
        private byte NodeTypeIndex;
        private byte TreeLevel;
        private int clusterIndex;
        //<-----

        public ScopeTreeInternalNode(int _universeSize, Dictionary<int, IScopeTreeNode> _globalCluster, Dictionary<int, IScopeTreeNode> _globalSummaryCluster, List<TreeLevelInfo> _treeLevelInfoList, byte treeLevel, byte nodeTypeIndex, int _clusterIndex)
        {
            globalCluster = _globalCluster;
            globalSummaryCluster = _globalSummaryCluster;
            treeLevelInfoList = _treeLevelInfoList;

            this.universeSize = _universeSize;
            upperUniverseSizeSquare = upperSquare(universeSize);
            lowerUniverseSizeSquare = lowerSquare(universeSize);

            this.min = NULL_KEY;
            this.max = NULL_KEY;
            this.summary = null;
            
            //individual summaryCluster for each summary node)
            //this.summaryCluster = null;

            this.NodeTypeIndex = nodeTypeIndex;
            this.TreeLevel = treeLevel;
            this.clusterIndex = _clusterIndex;
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

        //operates only on clusterNodes (not summaryNodes)
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
                    int globalClusterKey = offsetBase + offsetIndex * upperUniverseSizeSquare + high(x);
                    IScopeTreeNode globalClusterItem;
                    if (!globalCluster.TryGetValue(globalClusterKey, out globalClusterItem)) return false;
                    return globalClusterItem.Contains(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + high(x), low(x));
                }
            }
        }

        public int Successor(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (min != NULL_KEY && x < min)
            {
                return min;
            }

            IScopeTreeNode globalClusterItem;
            int x_high = high(x);
            int globalClusterKey;

            globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + x_high;
            
            int maximumLow = NULL_KEY;
            if (globalCluster.TryGetValue(globalClusterKey, out globalClusterItem))
            {
                maximumLow = globalClusterItem.MaximumKey;
            }

            if (maximumLow != NULL_KEY && low(x) < maximumLow)
            {
                int _offset = globalClusterItem.Successor(offsetBase + (offsetFactor * upperUniverseSizeSquare), (offsetFactor * upperUniverseSizeSquare), (offsetIndex * upperUniverseSizeSquare) + x_high, low(x));

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

            globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + successorCluster;

            globalCluster.TryGetValue(globalClusterKey, out globalClusterItem);
            int offset = globalClusterItem.MinimumKey;

            return index(successorCluster, offset);
        }       
        public int Successor(int x)
        {
            if (min != NULL_KEY && x < min) // is nullOrSmallerThanMin
            {
                return min;
            }

            int x_high = high(x);

            //individual summaryCluster for each summary node)
            //if (summaryCluster == null) return NULL_KEY;    //przyspieszenie, zeby nie sprawdzac skoro wiadomo, ze null
            //IScopeNodeGlobal summaryClusterItem;
            
            //--- globalSummaryCluster common for all summary nodes ---->                
            IScopeTreeNode globalSummaryClusterItem;

            // base + clusterSize * x + high_x //x is the number of other nodes of this type (each has a list of cluster = clusterSize), and high_x is the index of the cluster in the given node
            NodeTypeInfo nodeTypeInfo = treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex];
            int globalSummaryClusterKey =
                nodeTypeInfo.BaseIndexOffset    //base offset of this type of nodes
                + nodeTypeInfo.NodeClusterSize // size of clusters of this type nodes
                * clusterIndex        //node index within the same type nodes -- offsetIndex
                + x_high; //clusterOffset in this node
            //<-----summary key --- global

            int maximumLow = NULL_KEY;
            //cluster of this node is not null and x_high is in the cluster
            if (globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem))
            {
                maximumLow = globalSummaryClusterItem.MaximumKey;
            }

            if (maximumLow != NULL_KEY && low(x) < maximumLow)
            {
                int _offset = globalSummaryClusterItem.Successor(low(x));
                return index(x_high, _offset);
            }

            if (summary == null)
            {
                return NULL_KEY;
            }

            //======================//
            //from summary part 
            //======================//

            int successorCluster = summary.Successor(x_high);
            if (successorCluster == NULL_KEY)
            {
                return NULL_KEY;
            }

            //--- globalSummaryCluster common for all summary nodes ----> 
            globalSummaryClusterKey = nodeTypeInfo.BaseIndexOffset + nodeTypeInfo.NodeClusterSize * clusterIndex + successorCluster; //successorCluster -- next clusterOffset in this node            
            //<-----summary key --- global

            globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem);
            int offset = globalSummaryClusterItem.MinimumKey;
            return index(successorCluster, offset);
        }    // used only for summary

        public int Predecessor(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
            if (max != NULL_KEY && x > max)
            {
                return max;
            }

            IScopeTreeNode globalClusterItem;
            int x_high = high(x);
            int globalClusterKey;

            globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + x_high;
            
            int minimumLow = NULL_KEY;
            if (globalCluster.TryGetValue(globalClusterKey, out globalClusterItem))
            {
                minimumLow = globalClusterItem.MinimumKey;
            }

            if (minimumLow != NULL_KEY && low(x) > minimumLow)
            {
                int _offset = globalClusterItem.Predecessor(offsetBase + (offsetFactor * upperUniverseSizeSquare), (offsetFactor * upperUniverseSizeSquare), (offsetIndex * upperUniverseSizeSquare) + x_high, low(x));
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
            globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + predecessorCluster;

            globalCluster.TryGetValue(globalClusterKey, out globalClusterItem);
            int offset = globalClusterItem.MaximumKey;
            return index(predecessorCluster, offset);
        }
        public int Predecessor(int x)
        {

            if (max != NULL_KEY && x > max)
            {
                return max;
            }
            //individual summaryCluster for each summary node)
            //if (summaryCluster == null) return NULL_KEY;
            //IScopeNodeGlobal summaryClusterItem;

            int x_high = high(x);

            //--- globalSummaryCluster common for all summary nodes ---->                
            IScopeTreeNode globalSummaryClusterItem;

            // base + clusterSize * x + high_x //x is the number of other nodes of this type (each has a list of cluster = clusterSize), and high_x is the index of the cluster in the given node
            NodeTypeInfo nodeTypeInfo = treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex];
            int globalSummaryClusterKey =
                nodeTypeInfo.BaseIndexOffset    //base offset of this type of nodes
                + nodeTypeInfo.NodeClusterSize // size of clusters of this type nodes
                * clusterIndex        //node index within the same type nodes -- offsetIndex
                + x_high; //clusterOffset in this node
            //<-----summary key --- global

            int minimumLow = NULL_KEY;
            if (globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem))
            {
                minimumLow = globalSummaryClusterItem.MinimumKey;
            }

            if (minimumLow != NULL_KEY && low(x) > minimumLow)
            {
                int _offset = globalSummaryClusterItem.Predecessor(low(x));
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

            //--- globalSummaryCluster common for all summary nodes ----> 
            globalSummaryClusterKey = nodeTypeInfo.BaseIndexOffset + nodeTypeInfo.NodeClusterSize * clusterIndex + predecessorCluster; //successorCluster -- next clusterOffset in this node            
            //<-----summary key --- global

            globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem);
            int offset = globalSummaryClusterItem.MaximumKey;
            return index(predecessorCluster, offset);
        }

        //!!! Wazne - zasada globalCluster:
        //dla L1: base = 0, factor = 1, index = 0, x1 = x
        //Level 1: keyCluster = high(x1), gdzie x1 to nasz początkowy x wstawiany do boasa
        //Level 2: keyCluster = u1 + high(x1) * u2 + high(x2), gdzie u1 to universum level 1, u2 to uniwersum L2 (do wyliczenia ze wzoru), x2 = low(x1)
        //      idx2 = high(x1)*u2 + high(x2) offset dla L2
        //Level 3: keyCluster = (u1 + u1*u2) + idx2*u3+high(x3), gdzie u3 to universum L3 (wyliczane ze wzoru w boasie), x3 = low(x2)
        // offsetBase = (u1+u1*u2), offsetFactor = u1*u2, offsetIndex = idx, 
        //Insert(offsetBase + offsetFactor * UniverseSize, offsetFactor * UniverseSize, offsetIndex * UniverseSize + high(x), low(x));
        //
        // nowe:
        // - base = base + factor * u'  // u' = upperUniverseSquare na bazie u dla danego poziomu
        // - factor = factor * u'       // ile clustrow przejsc do kolejnej grupy clustrow wkazywanych przez wyzszy poziom - step (dla L1:1; L2: u1'; L3: u1'*u2' tego samego poziomu
        // - index = index * u' + high(x)
        // - x = low(x)
        //
        // zastosowanie (u' obliczane dla aktualnego u na danym poziomie):
        // - key = base + factor * u' + high(x) 
        // - index do wyliczania key
        // - factor do wyliczania base dla kolejnego poziomu 
        //
        // czyli zamiast method(V,x) czyli V - vEB (lub konkretny cluster) oraz nowy x => Method(Cluster[high(x), low(x)
        // mamy bardziej rozbudowana forme method(base, factor, index, x), [base, index] pozwalaja obliczyc Cluster; 
        // factor do wyliczenia base na kolejny poziom
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
                IScopeTreeNode globalClusterItem;
                int x_high = high(x);

                int globalClusterKey = offsetBase + offsetIndex * upperUniverseSizeSquare + x_high;
                
                if (!globalCluster.TryGetValue(globalClusterKey, out globalClusterItem))
                {
                    if (upperUniverseSizeSquare == MINIMUM_UNIVERSE_SIZE_U4)
                    {
                        if (summary == null)    //summary of the current level (u>4, e.g., u=16)
                        {
                            summary = new ScopeTreeLeafNode(-1);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high); //tutaj zrobic else
                        }
                        globalClusterItem = new ScopeTreeLeafNode(-1);
                        globalCluster.Add(globalClusterKey, globalClusterItem);
                        globalClusterItem.FirstInsert(low(x));
                    }
                    else //create new node (add next level)
                    {
                        if (summary == null)
                        {
                            summary = new ScopeTreeInternalNode(upperUniverseSizeSquare, globalCluster, globalSummaryCluster, treeLevelInfoList, (byte)(TreeLevel+1), (byte)(2*NodeTypeIndex), clusterIndex);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high); //tutaj zrobic else
                        }

                        globalClusterItem = new ScopeTreeInternalNode(upperUniverseSizeSquare, globalCluster, globalSummaryCluster, treeLevelInfoList, (byte)(TreeLevel + 1), (byte)(2 * NodeTypeIndex + 1),
                            clusterIndex * treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex].NodeClusterSize + x_high); 
                        globalCluster.Add(globalClusterKey, globalClusterItem);                      
                        globalClusterItem.FirstInsert(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + x_high, low(x));
                    }
                }
                else
                {
                    globalClusterItem.Insert(offsetBase + offsetFactor * upperUniverseSizeSquare, offsetFactor * upperUniverseSizeSquare, offsetIndex * upperUniverseSizeSquare + x_high, low(x));
                }
            }

            if (max < x)
            {
                max = x;
            }
        }       
        //wywolywane jest przez summary, kazdy ma summary, ale summary uzywaja metod innych niz cluster
        // oraz dzialaja na globalSummaryCluster
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
                //summaryCluster for each Node
                //IScopeNodeGlobal summaryClusterItem;

                int x_high = high(x);

                //--- globalSummaryCluster common for all summary nodes ---->                
                IScopeTreeNode globalSummaryClusterItem;

                // base + clusterSize * x + high_x //x is the number of other nodes of this type (each has a list of cluster = clusterSize), and high_x is the index of the cluster in the given node
                int globalSummaryClusterKey = 
                    treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex].BaseIndexOffset    //base offset of this type of nodes
                    + treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex].NodeClusterSize // size of clusters of this type nodes
                    * clusterIndex        //node index within the same type nodes -- offsetIndex
                    + x_high; //clusterOffset in this node
                //<-----summary key --- global
                
                //if (!summaryCluster.TryGetValue(x_high, out summaryClusterItem))  //individual summaryCluster for each summary node
                if (!globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem)) //global summary cluster for all summary nodes
                {
                    if (upperUniverseSizeSquare == MINIMUM_UNIVERSE_SIZE_U4)
                    {
                        if (summary == null)
                        {
                            summary = new ScopeTreeLeafNode(-1);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high);     //nie powinno byc else i first insert to summary??? (insert to zawiera, ale po co?)
                        }

                        //summaryCluster for each Node
                        //summaryClusterItem = new ScopeLeafNode(-1);
                        //summaryCluster.Add(x_high, summaryClusterItem);
                        //summaryClusterItem.FirstInsert(low(x));

                        //--- globalSummaryCluster common for all summary nodes ---->
                        globalSummaryClusterItem = new ScopeTreeLeafNode(-1);
                        globalSummaryCluster.Add(globalSummaryClusterKey, globalSummaryClusterItem);
                        globalSummaryClusterItem.FirstInsert(low(x));
                        //<-----summary key --- global
                    }
                    else //create new branch
                    {
                        if (summary == null)
                        {
                            summary = new ScopeTreeInternalNode(upperUniverseSizeSquare, globalCluster, globalSummaryCluster, treeLevelInfoList, (byte)(TreeLevel + 1), (byte)(2 * NodeTypeIndex), clusterIndex);
                            summary.FirstInsert(x_high);
                        }
                        else
                        {
                            summary.Insert(x_high);
                        }
                        //summaryCluster for each Node
                        //summaryClusterItem = new ScopeInternalNode(upperUniverseSizeSquare, globalCluster, globalSummaryCluster, treeLevelInfoList, (byte)(TreeLevel + 1), (byte)(2 * NodeTypeIndex + 1));
                        //summaryCluster.Add(x_high, summaryClusterItem);
                        //summaryClusterItem.FirstInsert(low(x));


                        //--- globalSummaryCluster common for all summary nodes ---->
                        globalSummaryClusterItem = new ScopeTreeInternalNode(upperUniverseSizeSquare, globalCluster, globalSummaryCluster, treeLevelInfoList, (byte)(TreeLevel + 1), (byte)(2 * NodeTypeIndex + 1),
                            clusterIndex * treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex].NodeClusterSize + x_high);
                        globalSummaryCluster.Add(globalSummaryClusterKey, globalSummaryClusterItem);
                        globalSummaryClusterItem.FirstInsert(low(x));
                        //<-----summary key --- global

                    }
                }
                else
                {
                    //summaryCluster for each Node
                    //summaryClusterItem.Insert(low(x));

                    //--- globalSummaryCluster common for all summary nodes ---->
                    globalSummaryClusterItem.Insert(low(x));
                    //<-----summary key --- global
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

            //summaryCluster for each Node
            //summaryCluster = new Dictionary<int, IScopeNodeGlobal>(new HashComparer());
        }

        public bool Delete(int offsetBase, int offsetFactor, int offsetIndex, int x)
        {
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

            IScopeTreeNode globalClusterItem;
            int x_high;
            int globalClusterKey;

            //at least 2 elements and not leaf node (universum > 4) -> remove element from block or min
            if (min == x)   //delete min and set new min, min is not stored in summaryClusters
            {
                int firstCluster = summary.MinimumKey; //if at least 2 elements, it cannot be null
                
                globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + firstCluster;
                globalCluster.TryGetValue(globalClusterKey, out globalClusterItem);
                x = index(firstCluster, globalClusterItem.MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min

                //x = index(firstCluster, summaryCluster[firstCluster].MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min
                min = x;
                //set the next value after min to be new "min" but it must be removed from summaryCluster (after)
            }

            //Delete x from summaryClusters (x or x, which above becomes new "min"
            x_high = high(x);
            globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + x_high;
            
            if (!globalCluster.TryGetValue(globalClusterKey, out globalClusterItem)) return false;    //no such element to be deleted
            //summaryClusterItem.Delete(low(x));
            globalClusterItem.Delete(offsetBase + (offsetFactor * upperUniverseSizeSquare), (offsetFactor * upperUniverseSizeSquare), (offsetIndex * upperUniverseSizeSquare) + x_high, low(x));

            //being in this place means that deletion process reached the leaf, then deleted min or max (then min=max, and maxkey become min, in other words max for summaryCluster is updated)

            //if (summaryCluster[high(x)].MinimumKey == NULL_KEY)
            if (globalClusterItem.MinimumKey == NULL_KEY)    //if the summaryCluster[high(x)] is empty, but remember there is another summaryCluster that is not empty, since at leat 2 elements are in this node
            {
                //remove elements related with current summaryCluster, which we know is empty
                //summaryCluster.Remove(high(x));   
                
                //!!!! PYTANIE: czy to spowoduje, ze obiekt, ktorego reference znajduje sie w globalCluster(x_high) zostanie usuniety - chyba tak, nie ma innej referencji do obiektu takiego
                globalCluster.Remove(globalClusterKey);
                
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
                        globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + summaryMaximum;
                        globalCluster.TryGetValue(globalClusterKey, out globalClusterItem);

                        int maximumKey = globalClusterItem.MaximumKey;
                        max = index(summaryMaximum, maximumKey);
                    }
                }
            }
            else if (x == max)      //delete max element, but it is not empty, then 
            {
                //int maximumKey = summaryCluster[high(x)].MaximumKey;   //summaryCluster including element (x==max), and there is at least one other element (the element maximumKey was updated properly due to previous recursive call

                globalClusterKey = offsetBase + (offsetIndex * upperUniverseSizeSquare) + high(x); //x is changed (updated) then high(x) can be different than x_high calculated above
                globalCluster.TryGetValue(globalClusterKey, out globalClusterItem);
                int maximumKey = globalClusterItem.MaximumKey;

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
                //summaryCluster = null;                         //!!! problem jezeli skasujemy dla root drzewa wowczas, nie dziala predecesor i successor (summaryCluster == null i nie moze sprawdzic)
                return true;
            }

            //--- globalSummaryCluster common for all summary nodes ---->                            
            IScopeTreeNode globalSummaryClusterItem;
            NodeTypeInfo nodeTypeInfo = treeLevelInfoList[TreeLevel].NodeTypeInfoList[NodeTypeIndex]; ;
            int globalSummaryClusterKey;
            //<-----summary key --- global
            int x_high;

            //at least 2 elements and not based node (universum > 4) -> remove element from block or min

            if (min == x)   //delete min and set new min, min is not stored in summaryClusters
            {
                int firstCluster = summary.MinimumKey; //if at least 2 elements, it cannot be null

                //--- globalSummaryCluster common for all summary nodes ---->                            
                globalSummaryClusterKey = nodeTypeInfo.BaseIndexOffset + nodeTypeInfo.NodeClusterSize * clusterIndex + firstCluster;
                globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem);    //it cannot be null
                x = index(firstCluster, globalSummaryClusterItem.MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min
                //<-----summary key --- global

                //individula summary for each node (summary)
                //x = index(firstCluster, summaryCluster[firstCluster].MinimumKey); //the minimum value in summaryCluster is the next min afeter "min==x"; thus, it becomes new min
                min = x;
                //set it as "min" but remove from summaryCluster
            }

            //Delete x from summaryClusters
            x_high = high(x);
            //--- globalSummaryCluster common for all summary nodes ---->
            globalSummaryClusterKey = nodeTypeInfo.BaseIndexOffset + nodeTypeInfo.NodeClusterSize * clusterIndex + x_high;
            if (!globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem)) return false;    //no such element to be deleted
            globalSummaryClusterItem.Delete(low(x));
            //<-----summary key --- global

            //individula summary for each node (summary)
            //IScopeNodeGlobal summaryClusterItem;
            //if (!summaryCluster.TryGetValue(high(x), out summaryClusterItem)) return true;    //no such element to be deleted
            //summaryClusterItem.Delete(low(x));

            //being in this place means that deletion process reached the leaf, then deleted min or max (then min=max, and maxkey become min, in other words max for summaryCluster is updated)

            //if (summaryCluster[high(x)].MinimumKey == NULL_KEY)
            //if (summaryClusterItem.MinimumKey == NULL_KEY)    //if the summaryCluster[high(x)] is empty, but remember there is another summaryCluster that is not empty, since at leat 2 elements are in this node
            if (globalSummaryClusterItem.MinimumKey == NULL_KEY)    //if the summaryCluster[high(x)] is empty, but remember there is another summaryCluster that is not empty, since at leat 2 elements are in this node
            {
                //remove elements related with current summaryCluster, which we know is empty
                //individula summary for each node (summary)
                //summaryCluster.Remove(high(x));
                //--- globalSummaryCluster common for all summary nodes ---->
                globalSummaryCluster.Remove(globalSummaryClusterKey);
                //<-----summary key --- global

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
                        //individula summary for each node (summary)
                        //int maximumKey = summaryCluster[summaryMaximum].MaximumKey;

                        //--- globalSummaryCluster common for all summary nodes ---->
                        globalSummaryClusterKey = nodeTypeInfo.BaseIndexOffset + nodeTypeInfo.NodeClusterSize * clusterIndex + summaryMaximum;
                        globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem);
                        int maximumKey = globalSummaryClusterItem.MaximumKey;
                        //<-----summary key --- global

                        max = index(summaryMaximum, maximumKey);
                    }
                }
            }
            else if (x == max)      //delete max element, but it is not empty, then 
            {
                //individula summary for each node (summary)
                //int maximumKey = summaryCluster[high(x)].MaximumKey;   //summaryCluster including element (x==max), and there is at least one other element (the element maximumKey was updated properly due to previous recursive call

                //--- globalSummaryCluster common for all summary nodes ---->
                globalSummaryClusterKey = nodeTypeInfo.BaseIndexOffset + nodeTypeInfo.NodeClusterSize * clusterIndex + high(x); //x is changed (updated) then high(x) can be different than x_high calculated above
                globalSummaryCluster.TryGetValue(globalSummaryClusterKey, out globalSummaryClusterItem);
                int maximumKey = globalSummaryClusterItem.MaximumKey;
                //<-----summary key --- global

                max = index(high(x), maximumKey);   //update max = highest value in the summaryCluster previously included x
            }
            return true;
        }     // for summary

        private int high(int x)
        {
            //x=192053244 => float fx = x => 192053248 !!!!! PROBLEM !!!! use double or int only
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

        public static int upperSquare(int number)
        {
            double exponent = Math.Ceiling(Math.Log(number) / Math.Log(2.0) / 2.0);
            return (int)Math.Pow(2.0, exponent);
        }
        public static int lowerSquare(int number)
        {
            double exponent = Math.Floor(Math.Log(number) / Math.Log(2.0) / 2.0);
            return (int)Math.Pow(2.0, exponent);
        }
    }
    
    //tutaj nie ma odwolan do global, a jedyna roznica na tym etapie implementacji to argumenty:
    //  jezeli (offsetBase,....) to wywolywane na rzecz globalCluster
    //  jezeli (x) to na rzecz summaryCluster
    // 
    public struct ScopeTreeLeafNode : IScopeTreeNode
    {
        public int[] xValue;

        public ScopeTreeLeafNode(int null_key)
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
  
    public class EntryPairGlobal<V>
    {
        public int Key;
        public V Value;

        public EntryPairGlobal()
        { }
        public EntryPairGlobal(int key, V value)
        {
            Key = key;
            Value = value;
        }

        public override String ToString()
        {
            return "(" + Key + " -> " + Value.ToString() + ")";
        }
    }

    public class EntrySeriesGlobal<V> : IEnumerator
    {
        private ScopeTreeRHGlobal<V> map;
        private int iterated = 0;
        private int lastReturned;

        public EntrySeriesGlobal(ScopeTreeRHGlobal<V> Map)
        {
            map = Map;
            Entry = new EntryPairGlobal<V>();
        }

        public EntryPairGlobal<V> Entry;

        public int Key { get { return Entry.Key; } }
        public V Value { get { return Entry.Value; } }

        public object Current => Entry;

        public bool MoveNext()
        {
            return Next();
        }

        public void Reset()
        {
            Entry = new EntryPairGlobal<V>();
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
