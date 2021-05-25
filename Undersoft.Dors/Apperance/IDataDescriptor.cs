using System.ComponentModel;

namespace System.Dors
{  
    public interface IDataDescriptor
    {
        PropertyDescriptorCollection CreateDescriptors(string name, string _dataId, PropertyDescriptorCollection _descriptors = null);
    }  
}