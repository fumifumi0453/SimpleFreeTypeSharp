using FreeTypeSharp;
using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    public unsafe class ImageFont 
    {
        private static readonly FreeTypeLibrary FTL_Library;
        private static readonly Dictionary<byte[], FontFileContent> SL_FontDatas;
        private const int INT_FontSize = 12;

        private FreeTypeFaceFacade _FaceFacade;
        private float _FontSize;

        static ImageFont()
        {
            FTL_Library = new();
            SL_FontDatas = new();
        }

        public ImageFont()
        {
            _FontSize = INT_FontSize;
        }

        public string FontName { get { return _FaceFacade.MarshalFamilyName(); } }

        public FreeTypeFaceFacade FaceFacade { get { return _FaceFacade; } }

        public float Size { get { return _FontSize; } }

        public void SetFont(string fontpath)
        {
            FT_FaceRec_* face;

            var error = FT.FT_New_Face(FTL_Library.Native, (byte*)Marshal.StringToHGlobalAnsi(fontpath), IntPtr.Zero, &face);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            SetFont(face);
        }
        public void SetFont(byte[] data)
        {
            FT_FaceRec_* face;

            if (SL_FontDatas.ContainsKey(data) == false) SL_FontDatas.Add(data, new FontFileContent(data));  

            var error = FT.FT_New_Memory_Face(FTL_Library.Native, (byte*)SL_FontDatas[data].FontMemory, (IntPtr)data.Length, IntPtr.Zero, &face);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            SetFont(face);
        }
        public void SetFont(FT_FaceRec_* face)
        {
            _FaceFacade = new FreeTypeFaceFacade(FTL_Library, face);
            SetSize(_FontSize);
        }

        public void SetSize(float size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            _FontSize = size;
            if (_FaceFacade != null)
            {
                var error = FT.FT_Set_Char_Size(_FaceFacade.FaceRec, IntPtr.Zero, new IntPtr((int)(_FontSize * 64)), 0, 96);
                
                if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);
            }
        }

        public StringImageData Render(string text)
        {
            if (string.IsNullOrEmpty(text)) return StringImageData.Empty;

            var charList = text.ReplaceLineEndings("\n").ToCharArray();
            var cidList = new List<List<CharImageData>>();
            cidList.Add(new List<CharImageData>());

            int baseHeight = (int)_FaceFacade.FaceRec->size->metrics.height >> 6;
            int fontHeight = baseHeight;
            int fontWidth = (int)_FaceFacade.FaceRec->size->metrics.max_advance >> 6;
            foreach (var c in charList)
            {
                switch (c)
                {
                    case '\r':
                    case '\n':
                        cidList.Add(new List<CharImageData>());
                        break;

                    case '\t':
                        var tab = new CharImageData(fontWidth * 2, 0, 0, 0, 0);
                        cidList.Last().Add(tab);
                        break;

                    default:
                        var cid = CreateCharImageData(c);
                        var tmpHeight = baseHeight - cid.BearingY + cid.Height;
                        if (fontHeight < tmpHeight) fontHeight = tmpHeight;
                        cidList.Last().Add(cid);
                        break;
                }
            }

            return new StringImageData(text, baseHeight, fontHeight, fontWidth, cidList);
        }
        private CharImageData CreateCharImageData(char c)
        {
            var griphIndex = _FaceFacade.GetCharIndex(c);
            var error = FT.FT_Load_Glyph(_FaceFacade.FaceRec, griphIndex, FT_LOAD.FT_LOAD_DEFAULT);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            var advanceX = (int)_FaceFacade.GlyphSlot->metrics.horiAdvance >> 6;
            var bealingX = (int)_FaceFacade.GlyphSlot->metrics.horiBearingX >> 6;
            var bearingY = (int)_FaceFacade.GlyphSlot->metrics.horiBearingY >> 6;

            error = FT.FT_Render_Glyph(_FaceFacade.GlyphSlot, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            var bitmap = _FaceFacade.GlyphSlot->bitmap;
            var cid = new CharImageData(advanceX, bealingX, bearingY, (int)bitmap.width, (int)bitmap.rows);
            for (int i = 0; i < bitmap.rows; i++)
            {
                var tmp = i * bitmap.pitch;
                for (int j = 0; j < bitmap.width; j++)
                {
                    cid.SetData(j, i, bitmap.buffer[j + tmp]);
                }
            }

            return cid;
        }
    }
}
