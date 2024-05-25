namespace SimpleFreeTypeSharp
{
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

        public int AdvanceX { get; set; }
        public int BearingX { get; set; }
        public int BearingY { get; set; }
        public int Width
        {
            get { return Data.Width; }
        }
        public int Height
        {
            get { return Data.Height; }
        }

        public ImageData Data { get; set; }

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
