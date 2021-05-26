using System.ComponentModel;

namespace System.Doors
{  
    public interface IDataDescriptor
    {
        PropertyDescriptorCollection CreateDescriptors(string name, string _dataId, PropertyDescriptorCollection _descriptors = null);
    }  
}