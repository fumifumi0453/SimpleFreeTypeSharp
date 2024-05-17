namespace SimpleFreeTypeSharp
{
    public class StringImageData
    {
        public static readonly StringImageData Empty = new StringImageData(string.Empty);

        public StringImageData(string value) 
        {
            Value = value;
            Datas = new CharImageData[Value.Length];
        }

        public string Value { get; private set; } 
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Value); }
        }

        public int FontHeight { get; set; }
        public int FontWidth { get; set; }
        public int BaseLine { get; set; }
        public CharImageData[] Datas { get; private set; }
        
        public ImageData ToImageData()
        {
            return ToImageData(0, Value.Length);
        }
        public ImageData ToImageData(int start, int length)
        {
            var lst = CatDatas(start, length);
            var image = new ImageData(GetTotalWidth(lst), FontHeight);

            int startX = 0;
            int startY;
            foreach (var data in lst)
            {
                if (startX != 0 || data.BearingX > 0) startX += data.BearingX;
                startY = BaseLine - data.BearingY;
                for (int i = 0; i < data.Width; i++)
                {
                    for (int j = 0; j < data.Height; j++)
                    {
                        image.SetData(startX + i, startY + j, data.GetData(i, j));
                    }
                }
                startX += data.AdvanceX - data.BearingX;
            }

            return image;
        }

        private CharImageData[] CatDatas(int start, int length)
        {
            if (start == 0 && length == Value.Length) return Datas;

            var res = new CharImageData[length];
            Array.Copy(Datas, start, res, 0, length);
            return res;
        }
        private int GetTotalWidth(CharImageData[] datas)
        {
            int width = 0;
            foreach (var data in datas)
            {
                width += data.AdvanceX;
            }

            return width;
        }
    }
}
