using System.Doors;

namespace System.Doors
{
    public interface IDataTier
    {
        IDataTrellis iTrell { get; }
        IDataGrating Grating
        { get; set; }      
        IDataNative iN
        {
            get;
        }

        DataState State { get; }

        object N
        { get; set; }

        object this[int cell]
        { get; set; }
        object this[string cellname]
        { get; set; }

        bool IsPrime
        { get; }
        bool IsCube
        { get; }
        bool Edited
        { get; set; }
        bool Synced
        { get; set; }
        bool Deleted
        { get; set; }
        bool Added
        { get; set; }

        int Index
        { get; set; }
        int ViewIndex
        { get; }
        int PrimeIndex
        { get; }
        int DevIndex
        { get; }
        int NoidOrdinal
        { get; }
        int[] KeyId
        { get; }

        int IndexOf(object value);

        bool Contains(object value);
        bool HasCell(string CellName);
        bool HasPylon(string PylonName);
        bool AnyChild(string ChildTrellName);
        bool AnyParent(string ParentTrellName);

        bool IsEditable(int id);
        bool IsEditable(string name);

        object Find(object value);

        object[] Collect(string TrellName, RelaySite Site);

        //Noid GetShah();
    }
}