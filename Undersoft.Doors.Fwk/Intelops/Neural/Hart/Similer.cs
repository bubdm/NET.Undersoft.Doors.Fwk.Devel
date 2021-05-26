using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Doors.Intelops
{
    public class Similarness
    {
        #region Properties -------------------
        public List<string> FeatureNameList { get; set; }

        public int FeatureItemSize {get; set;}

        public List<FeatureItem> FeatureItemList { get; set; }

        public List<ClusterRR> ClusterList { get; set; }

        public List<HyperClusterRR> HyperClusterList { get; set; }
        
        private Dictionary<long, ClusterRR> ItemToClusterMap;

        private Dictionary<ClusterRR, HyperClusterRR> ClusterToHyperClusterMap;

        #endregion
        
        #region Constants & Parameters -------

        public double bValue = 0.2f;

        public double pValue = 0.6f;

        public double p2Value = 0.3f;

        public const int rangeLimit = 1;
     
        public int IterationLimit = 50;

        private string tempHardFileName = "surveyResults.art";

        #endregion

        #region Constructors -----------------
        public Similarness()
        {
            FeatureNameList = new List<string>();
            FeatureItemList = new List<FeatureItem>();
            ClusterList = new List<ClusterRR>();
            HyperClusterList = new List<HyperClusterRR>();
            ItemToClusterMap = new Dictionary<long, ClusterRR>();
            ClusterToHyperClusterMap = new Dictionary<ClusterRR, HyperClusterRR>();

            LoadFile(tempHardFileName);
            FeatureItemList = NormalizeFeatureItemList(FeatureItemList);
            Create();
        }

        public void Create()
        {
            ClusterList.Clear();
            HyperClusterList.Clear();
            ItemToClusterMap.Clear();
            ClusterToHyperClusterMap.Clear();

            for (int i = 0; i < FeatureItemList.Count; i++)
            {
                AssignCluster(FeatureItemList[i]);
            }

            //Get items assigned to hyperClusters
            for(int i=0; i<HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        public void Create(ICollection<FeatureItem> itemCollection)
        {
            FeatureItemList.AddRange(itemCollection);
            
            ClusterList.Clear();
            HyperClusterList.Clear();
            ItemToClusterMap.Clear();
            ClusterToHyperClusterMap.Clear();

            for (int i = 0; i < FeatureItemList.Count; i++)
            {
                FeatureItemList[i].Id = i;
                AssignCluster(FeatureItemList[i]);
            }

            //Get items assigned to hyperClusters
            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        #endregion

        #region Core methods -----------------
      
        public void Append(ICollection<FeatureItem> itemCollection)
        {
            int currentCount = FeatureItemList.Count;

            FeatureItemList.AddRange(itemCollection);

            //start from 0 (Chin Xi) or from the currently added collection (RR) ? in my opinion it should be from currentCount, otherwise unecessary and redundant calculations
            for (int i = currentCount; i < FeatureItemList.Count; i++)
            {
                FeatureItemList[i].Id = i;          //update Id according to FeatureItemList numeration
                AssignCluster(FeatureItemList[i]);
            }

            //Get items assigned to hyperClusters
            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        //what about Id???
        public void Append(FeatureItem item)
        {
            item.Id = FeatureItemList.Count;    //update Id according to FeatureItemList numeration
            FeatureItemList.Add(item);
            AssignCluster(item);
            
            //Get items assigned to hyperClusters
            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        public void AssignCluster(FeatureItem item)
        {
            int iterationCounter = IterationLimit;  //assign IterationLimit
            bool isAssignementChanged = true;
            double itemVectorMagnitude = CalculateVectorMagnitude(item.FeatureVector);
            
            while (isAssignementChanged && iterationCounter > 0)
            {
                isAssignementChanged = false;

                List<KeyValuePair<ClusterRR, double>> clusterToProximityList = new List<KeyValuePair<ClusterRR, double>>();
                double proximityThreshold = itemVectorMagnitude / (bValue + rangeLimit * FeatureItemSize);  // ||E_i||/(b+1)
                
                //Calculate proximity values for item and clusters
                for(int i=0; i< ClusterList.Count; i++)
                {
                    double clusterVectorMagnitude = CalculateVectorMagnitude(ClusterList[i].ClusterVector);
                    double proximity = CaulculateVectorIntersectionMagnitude(item.FeatureVector, ClusterList[i].ClusterVector) / (bValue + clusterVectorMagnitude); //prox = ||C_j and E_i ||/ (b + ||E_i||) > proxThres
                    if(proximity > proximityThreshold)
                    {
                        clusterToProximityList.Add(new KeyValuePair<ClusterRR, double>(ClusterList[i], proximity));
                    }
                }

                if (clusterToProximityList.Count > 0)        //???? tutaj zobaczyc, czy nie trzeba sprawdzic dodania lub ominiecia dodania
                {
                    clusterToProximityList.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));  //sorting in place in descending order

                    //search from the maximum proximity to smallest
                    for (int i = 0; i < clusterToProximityList.Count; i++)
                    {
                        ClusterRR newCluster = clusterToProximityList[i].Key;
                        double vigilance = CaulculateVectorIntersectionMagnitude(newCluster.ClusterVector, item.FeatureVector) / itemVectorMagnitude;
                        if (vigilance >= pValue) //passed all tests and has max proximity
                        {
                            if (ItemToClusterMap.ContainsKey(item.Id)) //find cluster with this item
                            {
                                ClusterRR previousCluster = ItemToClusterMap[item.Id];
                                if (ReferenceEquals(newCluster, previousCluster)) break;    //if the best is the same, then it will break (not considered others)
                                if (previousCluster.RemoveItemFromCluster(item) == false)      //the cluster is empty
                                {
                                    ClusterList.Remove(previousCluster);
                                }
                            }
                            //Add item to the current cluster
                            newCluster.AddItemToCluster(item);
                            ItemToClusterMap[item.Id] = newCluster;
                            isAssignementChanged = true;
                            break;
                        }
                    }
                }

                if(ItemToClusterMap.ContainsKey(item.Id) == false)
                {
                    ClusterRR newCluster = new ClusterRR(item);
                    ClusterList.Add(newCluster);
                    ItemToClusterMap.Add(item.Id, newCluster);
                    isAssignementChanged = true;
                }

                iterationCounter--;
            }

            AssignHyperCluster();
        }

        public void AssignHyperCluster()
        {
            int iterationCounter = IterationLimit;  //assign IterationLimit
            bool isAssignementChanged = true;
            
            while (isAssignementChanged && iterationCounter > 0)
            {
                isAssignementChanged = false;
                for (int j = 0; j < ClusterList.Count; j++)
                {
                    List<KeyValuePair<HyperClusterRR, double>> hyperClusterToProximityList = new List<KeyValuePair<HyperClusterRR, double>>();
                    ClusterRR cluster = ClusterList[j];
                    double clusterVectorMagnitude = CalculateVectorMagnitude(cluster.ClusterVector);
                    double proximityThreshold = clusterVectorMagnitude / (bValue + rangeLimit * FeatureItemSize);  // ||C_j||/(b+1)
                    
                    //Calculate proximity values for cluster and hyperClusters
                    for (int i = 0; i < HyperClusterList.Count; i++)
                    {
                        double hyperClusterVectorMagnitude = CalculateVectorMagnitude(HyperClusterList[i].HyperClusterVector);
                        double proximity = CaulculateVectorIntersectionMagnitude(cluster.ClusterVector, HyperClusterList[i].HyperClusterVector) / (bValue + hyperClusterVectorMagnitude); //prox = ||HC_i and C_j ||/ (b + ||HC_j||) > proxThres
                        if (proximity > proximityThreshold)
                        {
                            hyperClusterToProximityList.Add(new KeyValuePair<HyperClusterRR, double>(HyperClusterList[i], proximity));
                        }
                    }

                    if (hyperClusterToProximityList.Count > 0)        
                    {
                        hyperClusterToProximityList.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));  //sorting in place in descending order

                        //search from the maximum proximity to smallest
                        for (int i = 0; i < hyperClusterToProximityList.Count; i++)
                        {
                            HyperClusterRR newHyperCluster = hyperClusterToProximityList[i].Key;
                            double vigilance = CaulculateVectorIntersectionMagnitude(newHyperCluster.HyperClusterVector, cluster.ClusterVector) / clusterVectorMagnitude;   //(vig = || HC_i and C_j|| / ||C_j||) >= p
                            if (vigilance >= p2Value) //passed all tests and has max proximity
                            {
                                if (ClusterToHyperClusterMap.ContainsKey(cluster)) //find cluster with this item
                                {
                                    HyperClusterRR previousHyperCluster = ClusterToHyperClusterMap[cluster];
                                    if (ReferenceEquals(newHyperCluster, previousHyperCluster)) break;    //if the best is the same, then it will break (not considered others)                                    
                                    if (previousHyperCluster.RemoveClusterFromHyperCluster(cluster) == false)      //the cluster is empty
                                    {
                                        HyperClusterList.Remove(previousHyperCluster);
                                    }
                                }
                                //Add item to the current hyperCluster
                                newHyperCluster.AddClusterToHyperCluster(cluster);
                                ClusterToHyperClusterMap[cluster] = newHyperCluster;
                                isAssignementChanged = true;

                                break;
                            }
                        }
                    }

                    if (ClusterToHyperClusterMap.ContainsKey(cluster) == false)
                    {
                        HyperClusterRR newHyperCluster = new HyperClusterRR(cluster);
                        HyperClusterList.Add(newHyperCluster);
                        ClusterToHyperClusterMap.Add(cluster, newHyperCluster);
                        isAssignementChanged = true;
                    }
                }

                iterationCounter--;
            }
        }

        public FeatureItem SimilarTo(FeatureItem item)
        {
            StringBuilder outputText = new StringBuilder();
            double tempItemSimilarSum = 0;
            double itemSimilarSum = 0;
            FeatureItem itemSimilar = null;
            ClusterRR cluster = null;

            ItemToClusterMap.TryGetValue(item.Id, out cluster);
            if (cluster == null)
            {
                //tak jest u (Chin Xi) czy aby dodawac? w cluster powinny byc elementy, ktore mamy na FeatureItemList, inaczej moze powstac niespojnosc!!!
                //to tworzy cluster, ale moze nie byc z nim polaczenia poprzez item, gdy ten item nie nalezy do FeatureItemList
                //AssignCluster(item);  //przedyskutowac z Darkiem
            }
            else
            {
                //for each item in cluster find the closest
                List<FeatureItem> clusterItemList = cluster.ClusterItemList;
                for (int i = 0; i < clusterItemList.Count; i++)
                {
                    if (!ReferenceEquals(item, clusterItemList[i]))
                    {
                        tempItemSimilarSum = CaulculateVectorIntersectionMagnitude(item.FeatureVector, clusterItemList[i].FeatureVector) / CalculateVectorMagnitude(clusterItemList[i].FeatureVector);    //||item(Reference) and itemFromCluster||/ ||itemFromcluster|| => max ||
                        if (itemSimilarSum == 0 || itemSimilarSum < tempItemSimilarSum)
                        {
                            itemSimilarSum = tempItemSimilarSum;
                            itemSimilar = clusterItemList[i];
                        }
                    }
                }

                if (itemSimilar != null)
                {
                    outputText.Append(" Most similiar taste have item " + itemSimilar.Name + "\r\n\r\n");
                }
                else
                {
                    outputText.Append(" There is no similiar item " + item.Name + "\r\n\r\n");
                }
            }
            Debug.WriteLine(outputText.ToString());

            return itemSimilar;
        }

        public FeatureItem SimilarInGroupsTo(FeatureItem item)
        {
            StringBuilder outputText = new StringBuilder();
            double tempItemSimilarSum = 0;
            double itemSimilarSum = 0;
            FeatureItem itemSimilar = null;
            ClusterRR cluster = null;

            ItemToClusterMap.TryGetValue(item.Id, out cluster);
            if(cluster == null)
            {
                //czy aby dodawac? w cluster powinny byc elementy, ktore mamy na FeatureItemList, inaczej moze powstac niespojnosc!!!
                //AssignCluster(item);    //przedyskutowac z Darkiem
            }
            else
            {
                HyperClusterRR hyperCluster = ClusterToHyperClusterMap[cluster];
                List<FeatureItem> hyperClusterItemList = hyperCluster.GetHyperClusterItemList();
                for(int i=0; i< hyperClusterItemList.Count; i++)
                {

                    if (!ReferenceEquals(item, hyperClusterItemList[i]))
                    {
                        tempItemSimilarSum = CaulculateVectorIntersectionMagnitude(item.FeatureVector, hyperClusterItemList[i].FeatureVector) / CalculateVectorMagnitude(hyperClusterItemList[i].FeatureVector);    //||item(Reference) and itemFromCluster||/ ||itemFromcluster|| => max ||
                        if(itemSimilarSum == 0 || itemSimilarSum < tempItemSimilarSum)
                        {
                            itemSimilarSum = tempItemSimilarSum;
                            itemSimilar = hyperClusterItemList[i];
                        }
                    }
                }

                if (itemSimilar != null)
                {
                    outputText.Append(" Most similiar taste in hyper cluster have item " + itemSimilar.Name + "\r\n\r\n");
                }
                else
                {
                    outputText.Append(" There is no simiilar item in hyper cluster " + item.Name + "\r\n\r\n");
                }
            }
            Debug.WriteLine(outputText.ToString());

            return itemSimilar;
        }

        public FeatureItem SimilarInOtherGroupsTo(FeatureItem item)
        {
            StringBuilder outputText = new StringBuilder();
            double tempItemSimilarSum = 0;
            double itemSimilarSum = 0;
            FeatureItem itemSimilar = null;
            ClusterRR cluster = null;

            ItemToClusterMap.TryGetValue(item.Id, out cluster);
            if (cluster == null)
            {
                //czy aby dodawac? w cluster powinny byc elementy, ktore mamy na FeatureItemList, inaczej moze powstac niespojnosc!!!
                //AssignCluster(item); //przedyskutowac z Darkiem
            }
            else
            {
                HyperClusterRR hyperCluster = ClusterToHyperClusterMap[cluster];                
                for(int j=0; j< hyperCluster.ClusterList.Count; j++)                
                {
                    if (!ReferenceEquals(cluster, hyperCluster.ClusterList[j]))    //find in clusters different than item
                    {
                        List<FeatureItem> clusterItemList = hyperCluster.ClusterList[j].ClusterItemList;
                        for (int i = 0; i < clusterItemList.Count; i++)
                        {
                            tempItemSimilarSum = CaulculateVectorIntersectionMagnitude(item.FeatureVector, clusterItemList[i].FeatureVector) / CalculateVectorMagnitude(clusterItemList[i].FeatureVector);    //||item(Reference) and itemFromCluster||/ ||itemFromcluster|| => max ||
                            if (itemSimilarSum == 0 || itemSimilarSum < tempItemSimilarSum)
                            {
                                itemSimilarSum = tempItemSimilarSum;
                                itemSimilar = clusterItemList[i];
                            }
                        }
                    }
                }

                if (itemSimilar != null)
                {
                    outputText.Append(" Most similiar taste in hyper cluster (other clusters) have item " + itemSimilar.Name + "\r\n\r\n");
                }
                else
                {
                    outputText.Append(" There is no simiilar item in hyper cluster (other clusters) " + item.Name + "\r\n\r\n");
                }
            }
            Debug.WriteLine(outputText.ToString());

            return itemSimilar;
        }

        #endregion


        #region Static methods ---------------
        //to moze zrobic pozniej przez interfejsy?

        //In general, they can be different for Cluster than for FeatureItem
        public static double[] CalculateFeatureIntersection(List<FeatureItem> input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = input[0].FeatureVector[i];
                for (int j = 1; j < input.Count; j++)
                {
                    output[i] = Math.Min(output[i], input[j].FeatureVector[i]);
                }
            }
            return output;
        }
        public static double[] CalculateFeatureSummary(List<FeatureItem> input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < input.Count; j++)
                {
                    output[i] += input[j].FeatureVector[i];
                }
            }

            return output;
        }

        //it allows to use various update functions dependent on all input feature vectors
        public static double[] UpdateFeatureIntersectionByLast(List<FeatureItem> input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Math.Min(output[i], input[n].FeatureVector[i]);
            }
            return output;
        }
        public static double[] UpdateFeatureSummaryByLast(List<FeatureItem> input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] += input[n].FeatureVector[i];
            }
            return output;
        }

        //In general, they can be different for Cluster than for FeatureItem
        public static double[] CalculateClusterIntersection(List<ClusterRR> input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = input[0].ClusterVector[i];
                for (int j = 1; j < input.Count; j++)
                {
                    output[i] = Math.Min(output[i], input[j].ClusterVector[i]);
                }
            }
            return output;
        }
        public static double[] CalculateClusterSummary(List<ClusterRR> input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < input.Count; j++)
                {
                    output[i] += input[j].ClusterVector[i];
                }
            }

            return output;
        }

        //it allows to use various update functions dependent on all input feature vectors
        public static double[] UpdateClusterIntersectionByLast(List<ClusterRR> input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Math.Min(output[i], input[n].ClusterVector[i]);
            }
            return output;
        }
        public static double[] UpdateClusterSummaryByLast(List<ClusterRR> input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] += input[n].ClusterVector[i];
            }
            return output;
        }

        public static List<FeatureItem> NormalizeFeatureItemList(List<FeatureItem> featureItemList)
        {
            List<FeatureItem> normalizedFeatureItemList = new List<FeatureItem>();

            int length;
            for (int i = 0; i < featureItemList.Count; i++)
            {
                length = featureItemList[0].FeatureVector.Length;
                double[] featureVector = new double[length];
                for (int j = 0; j < length; j++)
                {

                    featureVector[j] = featureItemList[i].FeatureVector[j] / 10.00;
                }
                normalizedFeatureItemList.Add(new FeatureItem(featureItemList[i].Id, featureItemList[i].Name, featureVector));
            }
            return normalizedFeatureItemList;
        }

        static public double CalculateVectorMagnitude(double[] vector)
        {
            double result = 0;
            for (int i = 0; i < vector.Length; ++i)
            {
                result += vector[i];
            }
            return result;
        }

        static public double CaulculateVectorIntersectionMagnitude(double[] vector1, double[] vector2)
        {
            double result = 0;

            for (int i = 0; i < vector1.Length; ++i)
            {
                result += Math.Min(vector1[i], vector2[i]);
            }

            return result;
        }

        #endregion

        #region Auxuliary methods ------------

        public void LoadFile(string fileLocation)
        {
            string line;
            FeatureNameList.Clear();
            FeatureNameList.Add("Name");

            StreamReader file = new StreamReader(fileLocation);

            while ((line = file.ReadLine()) != null)
            {
                if (line == "ItemList")
                {
                    break;
                }
            }

            if (line == null)
            {
                throw new Exception("ART File does not have a section marked ItemList!");
            }
            else
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line == "--")
                    {
                        break;
                    }
                    else
                    {
                        FeatureNameList.Add(line);
                    }
                }
                FeatureItemSize = FeatureNameList.Count - 1;

                //Finished reading itemList, now read all the people
                int featureItemId = 0;
                while ((line = file.ReadLine()) != null)
                {
                    string featureName = line;
                    line = file.ReadLine();
                    double[] featureVector = new double[FeatureItemSize];
                    int i = 0;
                    while ((line != null) && (line != "--"))
                    {
                        featureVector[i] = Int32.Parse(line); /// 10.00; //here read the values, but normalize in other part.
                        ++i;
                        line = file.ReadLine();
                    }

                    if (line == "--")
                    {
                        if (i != FeatureItemSize)
                        {
                            //For those who don't have a fully specified prefList, fill with 0s for rest. 
                            for (int j = i; j < FeatureItemSize; ++j)
                            {
                                featureVector[j] = 0;
                            }
                        }
                        FeatureItemList.Add(new FeatureItem(featureItemId, featureName, featureVector));
                        featureItemId++;
                    }                    
                }
            }

            file.Close();
        }
        
        #endregion

    }


    public class Similer
    {
        private List<SimilerItem> similerItemList;
        public List<SimilerItem> SimilerItemList
        {
            get { return similerItemList; }
            set { similerItemList = value; }
        }

        List<String> itemList;

        /// <summary>
        /// Beta value
        /// </summary>
        public float bValue = 0.2f;

        /// <summary>
        /// pValue for forming clusters
        /// </summary>
        public float pValue = 0.6f;

        /// <summary>
        /// p value for forming hyper clusters
        /// </summary>
        public float p2Value = 0.3f;

        /// <summary>
        /// The list of clusters formed from running ART
        /// </summary>
        LinkedList<Cluster> clusterVectors;

        /// <summary>
        /// The list of hyperclusters formed running ART on the clusters
        /// </summary>
        LinkedList<HyperCluster> hyperClusterVectors;

        Dictionary<string, Cluster> itemToClusterMap;

        /// <summary>
        /// Map of cluster to the hyper cluster they belong to
        /// </summary>
        Dictionary<Cluster, HyperCluster> clusterToHyperClusterMap;

        /// <summary>
        /// This code is designed for surveys with answers ranking from 0 - 10, 0 being never tried, 1 being tried but dislike, 10 being tried and liked a lot
        /// Change this number to 5 for example if you want your survey to only go up to 5
        /// </summary>
        public const int rangeLimit = 1;    //normalized [0, 0.1, ... 1.0]

        /// <summary>
        /// Epsilon value subtracted when 2 proximity values are actually equivalent to prevent ties. 
        /// </summary>
        public const float epsilon = 0.001f;

        public static int itemListSize;

        /// <summary>
        /// If feature vectors keep oscilating back and forth, this is the max number of iterations we do before stopping.
        /// </summary>
        public int iterLimit = 50;

        private string tempHardFileName = "surveyResults.art";

        public Similer()
        {
            similerItemList = new List<SimilerItem>();
            itemList = new List<String>();
            clusterVectors = new LinkedList<Cluster>();
            hyperClusterVectors = new LinkedList<HyperCluster>();
            itemToClusterMap = new Dictionary<string, Cluster>();
            clusterToHyperClusterMap = new Dictionary<Cluster, HyperCluster>();

            p2Value = Math.Max(pValue - 0.3f, 0.0f);

            LoadFile(tempHardFileName);

            Create();
        }

        public void LoadFile(string fileLocation)
        {
            string line;
            itemList.Clear();
            itemList.Clear();
            itemList.Add("Name");


            StreamReader file = new StreamReader(fileLocation);

            while ((line = file.ReadLine()) != null)
            {
                if (line == "ItemList")
                {
                    break;
                }
            }

            if (line == null)
            {
                throw new Exception("ART File does not have a section marked ItemList!");
            }
            else
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line == "--")
                    {
                        break;
                    }
                    else
                    {
                        itemList.Add(line);
                    }
                }
                itemListSize = itemList.Count - 1;

                //Finished reading itemList, now read all the people
                while ((line = file.ReadLine()) != null)
                {
                    string name = line;
                    line = file.ReadLine();
                    double[] prefList = new double[itemListSize];
                    int i = 0;
                    while ((line != null) && (line != "--"))
                    {
                        prefList[i] = Int32.Parse(line) / 10.00;
                        ++i;
                        line = file.ReadLine();
                    }

                    if (line == "--")
                    {
                        if (i != itemListSize)
                        {
                            //For those who don't have a fully specified prefList, fill with 0s for rest. 
                            for (int j = i; j < itemListSize; ++j)
                            {
                                prefList[j] = 0;
                            }
                        }
                        similerItemList.Add(new SimilerItem(name, prefList));
                    }
                }

            }

            file.Close();
        }
       
        public void Create()
        {
            clusterVectors.Clear();
            hyperClusterVectors.Clear();
            itemToClusterMap.Clear();
            clusterToHyperClusterMap.Clear();

            foreach (SimilerItem p in SimilerItemList)
            {
                AssignCluster(p);
            }
        }
        public void Create(ICollection<SimilerItem> item)
        {
            similerItemList.AddRange(item);

            clusterVectors.Clear();
            hyperClusterVectors.Clear();
            itemToClusterMap.Clear();
            clusterToHyperClusterMap.Clear();

            foreach (SimilerItem p in SimilerItemList)
                AssignCluster(p);
        }

        public void Append(ICollection<SimilerItem> item)
        {
            similerItemList.AddRange(item);
            foreach (SimilerItem p in SimilerItemList)
                AssignCluster(p);
        }
        public void Append(SimilerItem item)
        {
            similerItemList.Add(item);
            AssignCluster(item);
        }      

        public void AssignCluster(SimilerItem p)
        {  
            int _iterLimit = iterLimit;
            bool haveChangedThisLoop = true;

            while (haveChangedThisLoop)
            {
                haveChangedThisLoop = false;

                SortedDictionary<double, Cluster> sortedProximityMap = new SortedDictionary<double, Cluster>();
                double exampleVectorMagnitude = vectorMagnitude(p.prefList);
                double proximityThreshold = exampleVectorMagnitude / (bValue + rangeLimit * itemListSize);
                foreach (Cluster c in clusterVectors)
                {
                    double proximity = (vectorIntersectionMagnitude(p.prefList, c.clusterVector) / (bValue + vectorMagnitude(c.clusterVector)));
                    //To prevent the unusual case where 2 clusters are equally far apart from the item,
                    //if the distance is the same, decrease by epsilon until no such conflict
                    while (sortedProximityMap.ContainsKey(proximity))
                    {
                        proximity -= epsilon;
                    }
                    sortedProximityMap.Add(proximity, c);
                }//end for loop for clusters

                foreach (KeyValuePair<double, Cluster> proximityClusterPair in sortedProximityMap.Reverse())
                {
                    //First check against proximity
                    if (proximityClusterPair.Key >= proximityThreshold)
                    {
                        //Now need to check against vigilance test
                        double vigilanceTest = (double)(vectorIntersectionMagnitude(p.prefList, proximityClusterPair.Value.clusterVector)) / (exampleVectorMagnitude);
                        if (vigilanceTest >= pValue)
                        {
                            //Passed the test, now add this item to that cluster

                            //First remove from previous cluster
                            if (itemToClusterMap.ContainsKey(p.name))
                            {
                                Cluster c = itemToClusterMap[p.name];
                                if (ReferenceEquals(c, proximityClusterPair.Value))
                                {
                                    //This item already belongs in this cluster, just move on to the next iteration of the outside loop
                                    break;
                                }
                                if (c.RemoveSimilerItemFromCluster(p) == false)
                                {
                                    //We just removed the last member from that cluster. Time to remove this cluster. 
                                    clusterVectors.Remove(c);
                                }
                            }

                            //Now to add
                            proximityClusterPair.Value.AddSimilerItemToCluster(p);
                            itemToClusterMap[p.name] = proximityClusterPair.Value;
                            haveChangedThisLoop = true;
                            break;
                        }
                    }

                }

                //This item wasn't assigned to a cluster. 
                //Create a new cluster with his preference vector;
                if (!itemToClusterMap.ContainsKey(p.name))
                {
                    Cluster newCluster = new Cluster(p);
                    clusterVectors.AddLast(newCluster);
                    itemToClusterMap.Add(p.name, newCluster);
                    haveChangedThisLoop = true;
                }

                if (--_iterLimit == 0)
                {
                    //Break out of all loops if we have reached the maximum Iteration Limit
                    break;
                }

            }//end while loop for if changed           

            AssignHyperCluster();
        }

        private void AssignHyperCluster()
        {
            int _iterLimit = iterLimit;
            bool haveChangedThisLoop = true;

            while (haveChangedThisLoop)
            {
                haveChangedThisLoop = false;
                foreach (Cluster currentCluster in clusterVectors)
                {
                    SortedDictionary<double, HyperCluster> sortedProximityMap = new SortedDictionary<double, HyperCluster>();
                    double exampleVectorMagnitude = vectorMagnitude(currentCluster.clusterVector);
                    double proximityThreshold = exampleVectorMagnitude / (bValue + rangeLimit * itemListSize);
                    foreach (HyperCluster hc in hyperClusterVectors)
                    {
                        double proximity = (vectorIntersectionMagnitude(currentCluster.clusterVector, hc.hyperclusterVector) / (bValue + vectorMagnitude(hc.hyperclusterVector)));
                        //To prevent the unusual case where 2 clusters are equally far apart from the item,
                        //if the distance is the same, decrease by epsilon until no such conflict
                        while (sortedProximityMap.ContainsKey(proximity))
                        {
                            proximity -= epsilon;
                        }
                        sortedProximityMap.Add(proximity, hc);
                    } //end of for loop for hyperclusters
                    foreach (KeyValuePair<double, HyperCluster> proximityHyperClusterPair in sortedProximityMap.Reverse())
                    {
                        //First check against proximity
                        if (proximityHyperClusterPair.Key >= proximityThreshold)
                        {
                            //Now need to check against vigilance test
                            double vigilanceTest = (double)(vectorIntersectionMagnitude(currentCluster.clusterVector, proximityHyperClusterPair.Value.hyperclusterVector)) / (exampleVectorMagnitude);
                            if (vigilanceTest >= p2Value)
                            {
                                //Passed the test, now add this item to that cluster

                                //First remove from previous cluster
                                if (clusterToHyperClusterMap.ContainsKey(currentCluster))
                                {
                                    HyperCluster hc = clusterToHyperClusterMap[currentCluster];
                                    if (hc == proximityHyperClusterPair.Value)
                                    {
                                        //This cluster is already in the hypercluster, just move on to the next iteration of the outside loop
                                        break;
                                    }
                                    if (hc.RemoveClusterFromList(currentCluster) == false)
                                    {
                                        //We just removed the last cluster from this hypercluster. Time to remove the hypercluster
                                        hyperClusterVectors.Remove(hc);
                                    }
                                }
                                //Now to add
                                proximityHyperClusterPair.Value.AddClusterToList(currentCluster);
                                clusterToHyperClusterMap[currentCluster] = proximityHyperClusterPair.Value;
                                haveChangedThisLoop = true;
                                break;

                            }

                        }
                    }

                    //This cluster was not assigned to  a hyper cluster. 
                    //Create a new hypercluster with this cluster as its initial member
                    if (!clusterToHyperClusterMap.ContainsKey(currentCluster))
                    {
                        HyperCluster newHyperCluster = new HyperCluster(currentCluster);
                        hyperClusterVectors.AddLast(newHyperCluster);
                        clusterToHyperClusterMap.Add(currentCluster, newHyperCluster);
                        haveChangedThisLoop = true;
                    }


                }//end for loop for clusters

                if (--_iterLimit == 0)
                {
                    break;
                }
            }
        }

        public SimilerItem Similate(SimilerItem p)
        {
            StringBuilder output = new StringBuilder();
            double tempProdSimiliarSum = 0;
            double prodSimiliarSum = 0;
            SimilerItem prodSimiliar = null;

            Cluster c = null;
            itemToClusterMap.TryGetValue(p.name, out c);
            if (c == null)
            {
                //czy aby dodawac? w cluster powinny byc elementy, ktore mamy na FeatureItemList, inaczej moze powstac niespojnosc!!!
                //to tworzy cluster, ale moze nie byc z nim polaczenia poprzez item, gdy ten item nie nalezy do FeatureItemList
                AssignCluster(p);
            }
            else
            {
                foreach (SimilerItem pc in c.itemsInCluster)
                {
                    if (!ReferenceEquals(pc, p))
                    {
                        tempProdSimiliarSum = (double)(vectorIntersectionMagnitude(pc.prefList, p.prefList)) / vectorMagnitude(pc.prefList);

                        if (prodSimiliarSum == 0 || prodSimiliarSum < tempProdSimiliarSum)
                        {
                            prodSimiliarSum = tempProdSimiliarSum;
                            prodSimiliar = pc;
                        }
                        tempProdSimiliarSum = 0;

                    }
                }
                if (prodSimiliar != null)
                {
                    output.Append(" Most similiar taste have item " + prodSimiliar.name + "\r\n\r\n");
                }
                else
                {
                    output.Append(" There is no similiar item " + p.name + "\r\n\r\n");
                }
            }
            Debug.WriteLine(output.ToString());

            return prodSimiliar;
        }

        public SimilerItem SimilateInGroups(SimilerItem p)
        {
            StringBuilder output = new StringBuilder();
            double tempHProdSimiliarSum = 0;
            double hProdSimiliarSum = 0;
            SimilerItem hProdSimiliar = null;

            Cluster c = null;
            itemToClusterMap.TryGetValue(p.name, out c);
            if (c == null)
            {
                //czy aby dodawac? w cluster powinny byc elementy, ktore mamy na FeatureItemList, inaczej moze powstac niespojnosc!!!
                //to tworzy cluster, ale moze nie byc z nim polaczenia poprzez item, gdy ten item nie nalezy do FeatureItemList i zostanie zmieniony, usuniety, etc.
                AssignCluster(p);
            }
            else
            {
                HyperCluster hc = clusterToHyperClusterMap[c];

                foreach (SimilerItem pc in hc.memberList)
                {
                    if (!ReferenceEquals(pc, p))
                    {
                        tempHProdSimiliarSum = (double)(vectorIntersectionMagnitude(pc.prefList, p.prefList)) / vectorMagnitude(pc.prefList);

                        if (hProdSimiliarSum == 0 || hProdSimiliarSum < tempHProdSimiliarSum)
                        {
                            hProdSimiliarSum = tempHProdSimiliarSum;
                            hProdSimiliar = pc;
                        }
                        tempHProdSimiliarSum = 0;

                    }
                }

                if (hProdSimiliar != null)
                {
                    output.Append(" Most similiar taste in hyper cluster have item " + hProdSimiliar.name + "\r\n\r\n");
                }
                else
                {
                    output.Append(" There is no simiilar item in hyper cluster " + p.name + "\r\n\r\n");
                }
            }
            Debug.WriteLine(output.ToString());

            return hProdSimiliar;
        }

        static public double vectorMagnitude(double[] vector)
        {
            double result = 0;
            for (int i = 0; i < itemListSize; ++i)
            {
                result += vector[i];
            }
            return result;
        }

        static public double vectorIntersectionMagnitude(double[] input1, double[] input2)
        {
            double result = 0;

            for (int i = 0; i < itemListSize; ++i)
            {
                result += Math.Min(input1[i], input2[i]);
            }

            return result;
        }

        static public double[] vectorIntersection(double[] input1, double[] input2)
        {
            double[] result = new double[itemListSize];

            for (int i = 0; i < itemListSize; ++i)
            {
                result[i] = Math.Min(input1[i], input2[i]);
            }

            return result;
        }

        static public double[] vectorSum(double[] input1, double[] input2)
        {
            double[] result = new double[itemListSize];

            for (int i = 0; i < itemListSize; ++i)
            {
                result[i] = input1[i] + input2[i];
            }

            return result;
        }
    
    }
}
