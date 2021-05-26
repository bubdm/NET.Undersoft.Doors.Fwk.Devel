//-----------------------------------------------------------------------
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Doors.Intelops
{

    public class HyperClusterRR
    {
        private bool ValidHyperClusterItemList;

        public double[] HyperClusterVector { get; set; }

        public List<ClusterRR> ClusterList { get; set; }

        public List<FeatureItem> HyperClusterItemList { get; set; } //redundant - analyse to delete it // should be private to have access by Get

        public double[] HyperClusterVectorSummary { get; set; }

        public HyperClusterRR(ClusterRR cluster)
        {
            //resulting values (tables) should be copied
            HyperClusterVector = new double[cluster.ClusterVector.Length];
            Array.Copy(cluster.ClusterVector, HyperClusterVector, cluster.ClusterVector.Length);
            HyperClusterVectorSummary = new double[cluster.ClusterVectorSummary.Length];
            Array.Copy(cluster.ClusterVectorSummary, HyperClusterVectorSummary, cluster.ClusterVectorSummary.Length);
            ClusterList = new List<ClusterRR>();
            ClusterList.Add(cluster);

            //ItemList in HyperCluster = ClusterList[i].ItemList[j], i=0,..., ; j = 0, ...
            //redundant - try reorganize code to remove it
            //HyperClusterItemList = new List<FeatureItem>();
            //for(int i=0; i< cluster.ClusterItemList.Count; i++)
            //{
            //    HyperClusterItemList.Add(cluster.ClusterItemList[i]);
            //}
            //ValidHyperClusterItemList = true;
        }

        public bool RemoveClusterFromHyperCluster(ClusterRR cluster)
        {
            if(ClusterList.Remove(cluster) == true)
            {
                if(ClusterList.Count > 0)
                {
                    Similarness.CalculateClusterIntersection(ClusterList, HyperClusterVector);
                    Similarness.CalculateClusterSummary(ClusterList, HyperClusterVectorSummary);

                    //TODO: redundant analyse to remove it
                    // nie ma senzu sa kazdym razem tworzyc listy, tylko wtedy gdy bedzie do niej potrzebny dostep
                    //HyperClusterItemList.Clear();
                    //for(int i=0; i< ClusterList.Count; i++)
                    //{
                    //    for(int j = 0; j < ClusterList[i].ClusterItemList.Count; j++)
                    //    {
                    //        HyperClusterItemList.Add(ClusterList[i].ClusterItemList[j]);
                    //    }
                    //}
                }
                ValidHyperClusterItemList = false;
            }
            return ClusterList.Count > 0;
        }

        public void AddClusterToHyperCluster(ClusterRR cluster)
        {
            ClusterList.Add(cluster);
            Similarness.UpdateClusterIntersectionByLast(ClusterList, HyperClusterVector);
            Similarness.UpdateClusterSummaryByLast(ClusterList, HyperClusterVectorSummary);

            ValidHyperClusterItemList = false;
            //TODO: rendutant analyse to remove it            
            //for (int i = 0; i < cluster.ClusterItemList.Count; i++)
            //{                
            //    HyperClusterItemList.Add(cluster.ClusterItemList[i]);
            //}
        }


        public List<FeatureItem> GetHyperClusterItemList()
        {
            //TODO: valid nie ma sensu, jezeli zewnetrzna metoda dodala do cluster, ktory juz jest wewnatrz hypercluster
            //chyba, zeby dac zmienna obiektu, ktora przy jakiejkolwiek zamienie o tym informuje
            //pytanie tylko czy to ma sens
            //if (ValidHyperClusterItemList == false)
            //{
                List<FeatureItem> updatedItemList = new List<FeatureItem>();

                for (int i = 0; i < ClusterList.Count; i++)
                {
                    for (int j = 0; j < ClusterList[i].ClusterItemList.Count; j++)
                    {
                        updatedItemList.Add(ClusterList[i].ClusterItemList[j]);
                    }
                }
                HyperClusterItemList = updatedItemList;
                //ValidHyperClusterItemList = true;
            //}

            return HyperClusterItemList;
        }

    }

    class HyperCluster
    {
        /// <summary>
        /// The non-negative integral vector that represents a hypercluster. We use int instead of unsigned for simplicity.
        /// </summary>
        public double[] hyperclusterVector { get; set; }

        /// <summary>
        /// The list of clusters that are included in this hypercluster.
        /// </summary>
        public LinkedList<Cluster> clusterList { get; set; }

        /// <summary>
        /// The list of people that are included in this hypercluster.
        /// </summary>
        public LinkedList<SimilerItem> memberList { get; set; }

        /// <summary>
        /// The sum vector that is associated with this hypercluster
        /// </summary>
        public double[] sumVector { get; set; }

        /// <summary>
        /// Constructor. Hypercluster vector is set to the initial member's cluster vector. 
        /// </summary>
        /// <param name="initialCluster">The cluster we use to construct this hypercluster</param>
        public HyperCluster(Cluster initialCluster) 
        {
            hyperclusterVector = initialCluster.clusterVector;
            sumVector = initialCluster.sumVector;
            memberList = new LinkedList<SimilerItem>();
            foreach (SimilerItem p in initialCluster.itemsInCluster) 
            {
                memberList.AddLast(p);
            }
            clusterList = new LinkedList<Cluster>();
            clusterList.AddLast(initialCluster);
            

        }

        /// <summary>
        /// Remove a cluster from this hypercluster and adjust the hypercluster vector, itemship lists and sum vector accordingly.
        /// </summary>
        /// <param name="formerMember">The cluster to be removed.</param>
        public bool RemoveClusterFromList(Cluster formerMember) 
        {
            if (clusterList.Remove(formerMember) == true)
            {
                if (clusterList.Count > 0)
                {
                    //Get first element in LinkedList first
                    LinkedList<Cluster>.Enumerator clusterEnumerator = clusterList.GetEnumerator();
                    clusterEnumerator.MoveNext();

                    hyperclusterVector = clusterEnumerator.Current.clusterVector;
                    sumVector = clusterEnumerator.Current.sumVector;
                    memberList.Clear();
                    foreach (SimilerItem p in clusterEnumerator.Current.itemsInCluster) 
                    {
                        memberList.AddLast(p);
                    }

                    while (clusterEnumerator.MoveNext() != false) 
                    {
                        hyperclusterVector = Similer.vectorIntersection(hyperclusterVector, clusterEnumerator.Current.clusterVector);
                        sumVector = Similer.vectorSum(sumVector, clusterEnumerator.Current.sumVector);
                        foreach (SimilerItem p in clusterEnumerator.Current.itemsInCluster)
                        {
                            memberList.AddLast(p);
                        }
                    }
                }
            }

            return clusterList.Count > 0;
        }

        /// <summary>
        /// Add a cluster to this hypercluster and adjust the hypercluster vector, itemship lists and sum vector accordingly.
        /// </summary>
        /// <param name="c">The cluster to be added.</param>
        public void AddClusterToList(Cluster c) 
        {
            hyperclusterVector = Similer.vectorIntersection(hyperclusterVector, c.clusterVector);
            sumVector = Similer.vectorSum(sumVector, c.sumVector);
            foreach (SimilerItem p in c.itemsInCluster) //RR: nie oblicza prawdziwej wartosci elementow w powiazanych clusters, tylko pierwsze wystapienie, przy dodawaniu, jezeli nie ma przesuniecie (przeliczenia ponownego) to nieprawdziwa wartosc 
            {
                memberList.AddLast(p);
            }
            clusterList.AddLast(c);
        }
    }
}

