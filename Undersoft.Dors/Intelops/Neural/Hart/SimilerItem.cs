//-----------------------------------------------------------------------
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors.Intelops
{
    public  class FeatureItem
    {
        /// <summary>
        /// The name of the feature item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the feature item for fast search
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The non-negative integral vector that represents the feature vector of this item
        /// </summary>
        public double[] FeatureVector { get; set; }

        /// <summary>
        /// Constructor. A item is defined as a name and a preference list
        /// </summary>
        /// /// <param name="id">The id of the item</param>
        /// <param name="name">The name of the item</param>
        /// <param name="featureVector">The non-negative integral value vector that is this item's preference list. Int used instead of unsigned for simplicity</param>
        public FeatureItem(long id, string name, double[] featureVector)
        {
            Id = id;
            Name = name;
            
            //TODO: consider pointer or a copy of featureVector?
            //FeatureVector = new double[featureVector.Length];
            //Array.Copy(featureVector, FeatureVector, featureVector.Length);
            FeatureVector = featureVector;            
        }

        public FeatureItem(FeatureItem featureItem)
        {
            Id = featureItem.Id;
            Name = featureItem.Name;
            //TODO: consider pointer or a copy of featureVector?
            //FeatureVector = new double[featureItem.FeatureVector.Length];
            //Array.Copy(featureItem.FeatureVector, FeatureVector, featureItem.FeatureVector.Length);
            FeatureVector = featureItem.FeatureVector;
        }
    }

    public class SimilerItem
    {
        /// <summary>
        /// The name of the item
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The non-negative integral vector that represents the preference list of this item.
        /// </summary>
        public double[] prefList { get; set; }

        /// <summary>
        /// Constructor. A item is defined as a name and a preference list
        /// </summary>
        /// <param name="myName">The name of the item</param>
        /// <param name="preferenceList">The non-negative integral value vector that is this item's preference list. Int used instead of unsigned for simplicity</param>
        public SimilerItem(string myName, double[] preferenceList) 
        {
            name = myName;
            prefList = preferenceList;
        }
           
    }
}
