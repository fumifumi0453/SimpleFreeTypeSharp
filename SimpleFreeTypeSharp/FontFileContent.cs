using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// フォントバイナリデータの重複防止とアドレスを固定するためのクラス
    /// </summary>
    public class FontFileContent : IDisposable
    {
        public IntPtr _FontMemory;
        public byte[] _Content;
        private bool disposedValue;

        public FontFileContent(byte[] content)
        {
            _Content = content;

            _FontMemory = Marshal.AllocHGlobal(_Content.Length);
            Marshal.Copy(_Content, 0, _FontMemory, _Content.Length);
        }

        /// <summary>
        /// フォントバイナリデータの固定されたアドレス
        /// </summary>
        public IntPtr FontMemory
        {
            get { return _FontMemory; }
        }
        /// <summary>
        /// フォントバイナリデータ
        /// </summary>
        public byte[] Content
        {
            get { return _Content; }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    Marshal.FreeHGlobal(_FontMemory);
                    _FontMemory = IntPtr.Zero;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~FontFileContent()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
