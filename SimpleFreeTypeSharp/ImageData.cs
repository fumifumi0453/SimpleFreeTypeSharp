using System.Runtime.CompilerServices;

namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// 画像データ
    /// </summary>
    public class ImageData
    {
        public ImageData(int width, int height)
        {
            Width = width;
            Height = height;

            Datas = new byte[Width * Height];
        }

        /// <summary>
        /// 画像の幅
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// 画像の高さ
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// 画像データ配列
        /// </summary>
        public byte[] Datas { get; private set; }
        
        /// <summary>
        /// 画像の該当ドットのバイナリデータを取得します
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte GetData(int x, int y) 
        { 
            return Datas[GetIndex(x, y)];
        }
        /// <summary>
        /// 画像の該当ドットのバイナリデータを設定します
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        public void SetData(int x, int y, byte data)
        {
            Datas[GetIndex(x, y)] = data;
        }
        /// <summary>
        /// 画像の該当ドットのバイナリデータを設定します
        /// </summary>
        /// <param name="srcData"></param>
        /// <param name="srcIndex"></param>
        /// <param name="destIndex"></param>
        /// <param name="length"></param>
        public void SetData(byte[] srcData, int srcIndex, int destIndex, int length)
        {
            Array.Copy(srcData, srcIndex, Datas, destIndex, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y)
        {
            return x + y * Width;
        }
    }
}
