using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace System.Dors.Drive
{
    public interface IDriveRecorder
    {
        void WriteDrive();
        void ReadDrive();
        bool TryReadDrive();
        void OpenDrive();
        bool TryOpenDrive();
        void CloseDrive();
        IDrive Drive { get; set; }
    }
}
