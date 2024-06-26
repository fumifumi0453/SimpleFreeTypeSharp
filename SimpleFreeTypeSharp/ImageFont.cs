﻿using FreeTypeSharp;
using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    public unsafe class ImageFont : IDisposable
    {
        private static readonly FreeTypeLibrary FTL_Library;
        private const int INT_FontSize = 12;

        private FreeTypeFaceFacade _FaceFacade;
        private IntPtr _FontMemory = IntPtr.Zero;
        private float _FontSize;

        static ImageFont()
        {
            FTL_Library = new FreeTypeLibrary();
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

            _FontMemory = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, _FontMemory, data.Length);

            var error = FT.FT_New_Memory_Face(FTL_Library.Native, (byte*)_FontMemory, (IntPtr)data.Length, IntPtr.Zero, &face);

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
            if (size <= 0) throw new ArgumentOutOfRangeException("size");

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

            var charList = text.ToCharArray();
            var resList = new StringImageData(text)
            {
                FontWidth = (int)_FaceFacade.FaceRec->size->metrics.max_advance >> 6,
                FontHeight = (int)_FaceFacade.FaceRec->size->metrics.height >> 6,
            };

            int index = 0;
            int baseHeight = resList.FontHeight;
            foreach (var c in charList)
            {
                switch (c)
                {
                    case '\r':
                    case '\n':
                        break;

                    case '\t':
                        var tab = new CharImageData(resList.FontWidth * 2, 0, 0, 0, 0);
                        resList.Datas[index] = tab;
                        break;

                    default:
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

                        var tmpHeight = baseHeight - cid.BearingY + cid.Height;
                        if (resList.FontHeight < tmpHeight) resList.FontHeight = tmpHeight;
                        resList.Datas[index] = cid;
                        break;
                }
                index++;
            }

            resList.BaseLine = baseHeight;
            return resList;
        }

        #region IDisposable

        public void Dispose()
        {
            if (_FontMemory != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_FontMemory);
                _FontMemory = IntPtr.Zero;
            }
        }

        #endregion

    }
}
