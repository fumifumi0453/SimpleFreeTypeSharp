namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// 文字の画像データ
    /// </summary>
    public class CharImageData
    {

        public static ImageData Merge(CharImageData[] lst)
        {
            int width = 0;
            int height = int.MinValue;
            foreach(var tid in lst)
            {
                width += tid.Width + 1;
                if (height < tid.Height) height = tid.Height;
            }

            var res = new ImageData(width, height);
            int startX = 0;
            foreach (var tid in lst)
            {
                for (int i = 0; i < tid.Width; i++)
                {
                    for (int j = 0; j < tid.Height; j++)
                    {
                        res.SetData(startX + i, j, tid.GetData(i, j));
                    }
                }
                startX += tid.Width;
            }

            return res;
        }

        public static readonly CharImageData Empty = new(0, 0, 0, 0, 0);

        public CharImageData(int width, int height)
        {
            Data = new ImageData(width, height);
        }
        public CharImageData(int advanceX, int bearingX, int bearingY, int width, int height) : this(width, height)
        {
            AdvanceX = advanceX;
            BearingX = bearingX;
            BearingY = bearingY;
        }

        /// <summary>
        /// 描画するときの適正幅サイズ
        /// </summary>
        public int AdvanceX { get; set; }
        /// <summary>
        /// 描画開始点から最適なX方向のオフセット値
        /// </summary>
        public int BearingX { get; set; }
        /// <summary>
        /// 描画開始点から最適なY方向のオフセット値
        /// </summary>
        public int BearingY { get; set; }
        /// <summary>
        /// 文字幅
        /// </summary>
        public int Width
        {
            get { return Data.Width; }
        }
        /// <summary>
        /// 文字高さ
        /// </summary>
        public int Height
        {
            get { return Data.Height; }
        }

        /// <summary>
        /// 文字データ本体
        /// </summary>
        public ImageData Data { get; private set; }


        public void SetData(int x, int y, byte data)
        {
            Data.SetData(x, y, data);
        }
        public byte GetData(int x, int y)
        {
            return Data.GetData(x, y);
        }
    }
}
