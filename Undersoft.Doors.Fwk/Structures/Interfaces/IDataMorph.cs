using System.IO;

namespace System.Doors
{
    public interface IDataMorph
    {
        object Emulator(object source, string name = null);
        object Imitator(object source, string name = null);
        object Impactor(object source, string name = null);
        object Locator(string path = null);
    }
}