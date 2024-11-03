namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// 文字列の画像データ
    /// </summary>
    public class StringImageData
    {
        public static readonly StringImageData Empty = new (string.Empty, 0, 0, 0, null);

        private const int INT_LineDistance = 2;

        private CharImageData[][] _Datas;
        private int _MaxWidth;

        public StringImageData(string value, int baseLine, int fontHeight, int fontWidth, List<List<CharImageData>> datas) 
        {
            Value = value;
            BaseLine = baseLine;
            FontHeight = fontHeight;
            FontWidth = fontWidth;

            _MaxWidth = 0;

            if (datas is not null)
            {
                _Datas = new CharImageData[datas.Count][];
                for (int i = 0; i < datas.Count; i++)
                {
                    _Datas[i] = datas[i].ToArray();
                    var w = 1;
                    foreach (var cid in datas[i]) w += cid.AdvanceX;
                    if (_MaxWidth < w) _MaxWidth = w;
                }
            }
            else
            {
                _Datas = Array.Empty<CharImageData[]>();
            }
        }

        /// <summary>
        /// 文字列
        /// </summary>
        public string Value { get; private set; } 
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Value); }
        }

        /// <summary>
        /// 標準的な文字の高さ
        /// </summary>
        public int FontHeight { get; private set; }
        /// <summary>
        /// 標準的な文字の幅
        /// </summary>
        public int FontWidth { get; private set; }
        /// <summary>
        /// 高さ方向の
        /// </summary>
        public int BaseLine { get; private set; }
        /// <summary>
        /// 行間のサイズ
        /// </summary>
        public int LineDistance { get; set; } = INT_LineDistance;

        /// <summary>
        /// 文字ごとの画像データを連結して１つの画像データを返す
        /// </summary>
        /// <returns></returns>
        public ImageData ToImageData()
        {
            return ToImageData(Value.Length);
        }
        /// <summary>
        /// 文字ごとの画像データを指定した文字数分連結して１つの画像データを返す
        /// </summary>
        /// <param name="length">指定する文字数</param>
        /// <returns></returns>
        public ImageData ToImageData(int length)
        {
            var image = new ImageData(_MaxWidth, (FontHeight + LineDistance) * _Datas.Length);

            int startX = 0;
            int startY = 0;
            int index = 0;
            foreach (var line in _Datas)
            {
                if (line is not null) {
                    foreach (var data in line)
                    {
                        // 先頭の文字の描画開始点がマイナス位置の場合、オフセットする
                        if (startX != 0 || data.BearingX > 0) startX += data.BearingX;

                        var tempY = startY + BaseLine - data.BearingY;
                        //for (int i = 0; i < data.Width; i++)
                        //{
                        //    var tempX = startX + i;
                        //    for (int j = 0; j < data.Height; j++)
                        //        image.SetData(tempX, tempY + j, data.GetData(i, j));
                        //}
                        for (int i = 0; i < data.Height; i++) 
                            image.SetData(data.Data.Datas, data.Data.GetIndex(0, i), image.GetIndex(startX, tempY + i), data.Width);

                        startX += data.AdvanceX - data.BearingX;

                        index++;
                        if (index == length) return image;
                    }
                }
                startY += FontHeight + LineDistance;
                startX = 0;
            }

            return image;
        }

    }
}
