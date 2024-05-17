namespace SimpleFreeTypeSharp
{
    public class ImageData
    {
        public ImageData(int width, int height)
        {
            Width = width;
            Height = height;

            Datas = new byte[Width * Height];
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Datas { get; private set; }
        
        public byte GetData(int x, int y) 
        { 
            return Datas[GetIndex(x, y)];
        }
        public void SetData(int x, int y, byte data)
        {
            Datas[GetIndex(x, y)] = data;
        }


        private int GetIndex(int x, int y)
        {
            return x + y * Width;
        }
    }
}
