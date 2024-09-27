namespace SimpleFreeTypeSharp
{
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

        public string Value { get; private set; } 
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Value); }
        }

        public int FontHeight { get; set; }
        public int FontWidth { get; set; }
        public int BaseLine { get; set; }
        public int LineDistance { get; set; } = INT_LineDistance;

        public ImageData ToImageData()
        {
            return ToImageData(Value.Length);
        }
        public ImageData ToImageData(int length)
        {
            var image = new ImageData(_MaxWidth, (FontHeight + LineDistance) * _Datas.Length);

            int startX = 0;
            int startY = 0;
            int index = 0;
            foreach (var line in _Datas)
            {
                foreach (var data in line)
                {
                    if (startX != 0 || data.BearingX > 0) startX += data.BearingX;
                    var tempY = startY + BaseLine - data.BearingY;
                    for (int i = 0; i < data.Width; i++)
                    {
                        var tempX = startX + i;
                        for (int j = 0; j < data.Height; j++)
                        {
                            image.SetData(tempX, tempY + j, data.GetData(i, j));
                        }
                    }
                    startX += data.AdvanceX - data.BearingX;

                    index++;
                    if (index == length) return image;
                }
                startY += FontHeight + LineDistance;
                startX = 0;
            }

            return image;
        }

    }
}
