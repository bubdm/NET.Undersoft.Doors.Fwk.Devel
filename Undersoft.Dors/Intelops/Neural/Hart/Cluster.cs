//-----------------------------------------------------------------------
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors.Intelops
{

    //RR: pozniej zamienie nazwe na wlasciwa
    public class ClusterRR
    {
        //----- remove for debugging and tests only
        public double tempClusterVectorMagnitude { get; set; }  // remove for debugging and tests only
        public double tempClusterVectorSummaryMagnitude { get; set; }  // remove for debugging and tests only
                                                                       //----- remove for debugging and tests only


        /// <summary>
        /// The non-negative integral vector that represents a cluster. We use int instead of unsigned for simplicity.
        /// </summary>
        public double[] ClusterVector { get; set; }

        /// <summary>
        /// The list of people that are included in this cluster.
        /// </summary>
        public List<FeatureItem> ClusterItemList { get; set; }

        /// <summary>
        /// The summar vector that is associated with this cluster
        /// </summary>
        public double[] ClusterVectorSummary { get; set; }

        /// <summary>
        /// Constructor. Cluster vector is set to the initial feature vector. 
        /// </summary>
        /// <param name="item">The item used to construct this cluster</param>
        public ClusterRR(FeatureItem item)
        {
            //resulting values should be copied
            ClusterVector = new double[item.FeatureVector.Length];
            Array.Copy(item.FeatureVector, ClusterVector, item.FeatureVector.Length);
            ClusterVectorSummary = new double[item.FeatureVector.Length];
            Array.Copy(item.FeatureVector, ClusterVectorSummary, item.FeatureVector.Length);
            ClusterItemList = new List<FeatureItem>();
            ClusterItemList.Add(item);

            //----- remove for debugging and tests only
            tempClusterVectorMagnitude = Similarness.CalculateVectorMagnitude(ClusterVector);   //remove for debugging and tests only
            tempClusterVectorSummaryMagnitude = Similarness.CalculateVectorMagnitude(ClusterVectorSummary); //remove for debugging and tests only                                                                                                                
                                                                                                            
        }

        /// <summary>
        /// Remove a item from this cluster and update cluster Vector accordingly (basd on other assigned items)
        /// Return false if ItemList is empty
        /// </summary>
        /// <param name="item">The item to be removed</param>
        public bool RemoveItemFromCluster(FeatureItem item)
        {
            if (ClusterItemList.Remove(item) == true)
            {
                if (ClusterItemList.Count > 0)   //recalculate vector summary only if not empty
                {
                    Similarness.CalculateFeatureIntersection(ClusterItemList, ClusterVector);
                    Similarness.CalculateFeatureSummary(ClusterItemList, ClusterVectorSummary);

                    //----- remove for debugging and tests only
                    tempClusterVectorMagnitude = Similarness.CalculateVectorMagnitude(ClusterVector);   //remove for debugging and tests only
                    tempClusterVectorSummaryMagnitude = Similarness.CalculateVectorMagnitude(ClusterVectorSummary); //remove for debugging and tests only                                                                                                                
                    

                }
            }
            return ClusterItemList.Count > 0;
        }

        /// <summary>
        /// Add a item to this cluster and adjust the cluster vector, itemship list and sum vector accordingly.
        /// </summary>
        /// <param name="item">The item to be added.</param>        
        public void AddItemToCluster(FeatureItem item)
        {
            if (!ClusterItemList.Contains(item))
            {
                ClusterItemList.Add(item);
                Similarness.UpdateFeatureIntersectionByLast(ClusterItemList, ClusterVector);
                Similarness.UpdateFeatureSummaryByLast(ClusterItemList, ClusterVectorSummary);

                //----- remove for debugging and tests only
                tempClusterVectorMagnitude = Similarness.CalculateVectorMagnitude(ClusterVector);   //remove for debugging and tests only
                tempClusterVectorSummaryMagnitude = Similarness.CalculateVectorMagnitude(ClusterVectorSummary); //remove for debugging and tests only                                                                                                                
                
            }
        }

        //Move here(?) Calculate/Update VectorItersection & VectorSummary
        //Individual for class Cluster(?) Calculate/Update VectorItersection & VectorSummary for Cluster & HyperCluster?
        // ale to zamyka przyszly polimorfizm

    }

    public class Cluster
    {
        /// <summary>
        /// The non-negative integral vector that represents a cluster. We use int instead of unsigned for simplicity.
        /// </summary>
        public double[] clusterVector{get; set;}

        /// <summary>
        /// The list of people that are included in this cluster.
        /// </summary>
        public LinkedList<SimilerItem> itemsInCluster{get; set;}

        /// <summary>
        /// The sum vector that is associated with this cluster
        /// </summary>
        public double[] sumVector {get; set;}

        /// <summary>
        /// Constructor. Cluster vector is set to the initial member's preference list. 
        /// </summary>
        /// <param name="initialMember">The item we use to construct this cluster</param>
        public Cluster(SimilerItem initialMember) 
        {
            clusterVector = initialMember.prefList;
            sumVector = initialMember.prefList;
            itemsInCluster = new LinkedList<SimilerItem> ();
            itemsInCluster.AddFirst(initialMember);

        }

        /// <summary>
        /// Remove a item from this cluster and adjust the cluster vector, itemship list and sum vector accordingly.
        /// </summary>
        /// <param name="p">The item to be removed.</param>
        public bool RemoveSimilerItemFromCluster(SimilerItem p) 
        {
            if (itemsInCluster.Remove(p) == true)
            {
                if (itemsInCluster.Count > 0) 
                { 
                    //Get first element in LinkedList first
                    LinkedList<SimilerItem>.Enumerator itemsEnumerator = itemsInCluster.GetEnumerator();
                    itemsEnumerator.MoveNext();

                    clusterVector = itemsEnumerator.Current.prefList;
                    sumVector = itemsEnumerator.Current.prefList;

                    while (itemsEnumerator.MoveNext() != false) 
                    {
                        
                        clusterVector = Similer.vectorIntersection(clusterVector, itemsEnumerator.Current.prefList);
                        //RR: takie podejscie nie jest do konca elastyczne - nie mozna zrobic np. sredniej zamiast sumy 
                        //zaleznej od wszystkimich elementów (item) w danym Cluster
                        sumVector = Similer.vectorSum(sumVector, itemsEnumerator.Current.prefList);
                    }
                }
            }
            return itemsInCluster.Count > 0;
        }

        /// <summary>
        /// Add a item to this cluster and adjust the cluster vector, itemship list and sum vector accordingly.
        /// </summary>
        /// <param name="p">The item to be added.</param>
        public void AddSimilerItemToCluster(SimilerItem p) 
        {
            if (!itemsInCluster.Contains(p))
            { 
                clusterVector = Similer.vectorIntersection(clusterVector, p.prefList);
                sumVector = Similer.vectorSum(sumVector, p.prefList);
                itemsInCluster.AddLast(p);
            }
        }


    }
}
