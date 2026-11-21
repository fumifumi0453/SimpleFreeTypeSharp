using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// フォントバイナリデータの重複防止とアドレスを固定するためのクラス
    /// 同じ内容のフォントデータは参照カウントで共有し、誰も使わなくなった時点で解放します
    /// </summary>
    public sealed class FontFileContent : IDisposable
    {
        private static readonly Dictionary<byte[], FontFileContent> SL_Cache = new(ByteArrayContentComparer.Instance);
        private static readonly object SL_Lock = new();

        private IntPtr _FontMemory;
        private readonly byte[] _Content;
        private int _RefCount;
        private bool disposedValue;

        private FontFileContent(byte[] content)
        {
            // 呼び出し元で配列が書き換えられても影響を受けないようにコピーを保持する
            _Content = (byte[])content.Clone();

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
        /// フォントバイナリデータのサイズ
        /// </summary>
        public int Length
        {
            get { return _Content.Length; }
        }
        /// <summary>
        /// フォントバイナリデータ
        /// </summary>
        public ReadOnlySpan<byte> Content
        {
            get { return _Content; }
        }

        /// <summary>
        /// フォントバイナリデータを取得します
        /// 同じ内容のデータがすでに読み込まれている場合はそれを共有します
        /// 使い終わったら必ず <see cref="Release(FontFileContent)"/> を呼んでください
        /// </summary>
        /// <param name="data">フォントファイルのバイナリデータ</param>
        /// <returns></returns>
        public static FontFileContent Acquire(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data.Length == 0) throw new ArgumentException("フォントデータが空です", nameof(data));

            lock (SL_Lock)
            {
                if (SL_Cache.TryGetValue(data, out var content) == false)
                {
                    content = new FontFileContent(data);
                    SL_Cache.Add(content._Content, content);
                }
                content._RefCount++;
                return content;
            }
        }
        /// <summary>
        /// フォントバイナリデータの利用を終了します
        /// 誰も利用しなくなった場合はメモリを解放します
        /// </summary>
        /// <param name="content"></param>
        public static void Release(FontFileContent content)
        {
            ArgumentNullException.ThrowIfNull(content);

            lock (SL_Lock)
            {
                if (content._RefCount <= 0) return;

                content._RefCount--;
                if (content._RefCount == 0)
                {
                    SL_Cache.Remove(content._Content);
                    content.Dispose();
                }
            }
        }

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // アンマネージドメモリはファイナライザからでも解放する
                if (_FontMemory != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_FontMemory);
                    _FontMemory = IntPtr.Zero;
                }

                disposedValue = true;
            }
        }

        ~FontFileContent()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// メモリを解放します
        /// 通常は <see cref="Release(FontFileContent)"/> を使用してください
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// byte[] を内容で比較するための Comparer
        /// </summary>
        private sealed class ByteArrayContentComparer : IEqualityComparer<byte[]>
        {
            public static readonly ByteArrayContentComparer Instance = new();

            public bool Equals(byte[]? x, byte[]? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.AsSpan().SequenceEqual(y);
            }

            public int GetHashCode(byte[] obj)
            {
                var hash = new HashCode();
                hash.AddBytes(obj);
                return hash.ToHashCode();
            }
        }
    }
}
