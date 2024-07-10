using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    public class FontFileContent : IDisposable
    {
        public IntPtr _FontMemory;
        public byte[] _Content;

        public FontFileContent(byte[] content)
        {
            _Content = content;

            _FontMemory = Marshal.AllocHGlobal(_Content.Length);
            Marshal.Copy(_Content, 0, _FontMemory, _Content.Length);
        }

        public IntPtr FontMemory
        {
            get { return _FontMemory; }
        }
        public byte[] Content
        {
            get { return _Content; }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_FontMemory);
            _FontMemory = IntPtr.Zero;
        }
    }
}
