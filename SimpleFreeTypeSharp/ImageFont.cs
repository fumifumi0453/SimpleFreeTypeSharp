using FreeTypeSharp;
using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    public unsafe class ImageFont : IDisposable
    {
        private FreeTypeLibrary _Library;
        private FreeTypeFaceFacade _FaceFacade;
        private float _Size;
        private bool disposedValue;

        public ImageFont()
        {
            _Library = new FreeTypeLibrary();
            _Size = 12;
        }

        public FreeTypeFaceFacade FaceFacade { get { return _FaceFacade; } }

        public float Size { get { return _Size; } }

        public void SetFont(string fontpath)
        {
            FT_FaceRec_* face;

            var error = FT.FT_New_Face(_Library.Native, (byte*)Marshal.StringToHGlobalAnsi(fontpath), IntPtr.Zero, &face);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            SetFont(face);
        }
        public void SetFont(FT_FaceRec_* face)
        {
            _FaceFacade = new FreeTypeFaceFacade(_Library, face);
            SetSize(_Size);
        }

        public void SetSize(float size)
        {
            _Size = size;
            if (_FaceFacade != null)
            {
                var error = FT.FT_Set_Char_Size(_FaceFacade.FaceRec, IntPtr.Zero, new IntPtr((int)(_Size * 64)), 96, 96);
                
                if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);
            }
        }

        public StringImageData Render(string text)
        {
            if (string.IsNullOrEmpty(text)) return StringImageData.Empty;

            var charList = text.ToCharArray();
            var resList = new StringImageData(text)
            {
                FontWidth = (int)_FaceFacade.FaceRec->size->metrics.max_advance / 64,
                FontHeight = (int)_FaceFacade.FaceRec->size->metrics.height / 64,
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

                        var advanceX = (int)_FaceFacade.GlyphSlot->metrics.horiAdvance / 64;
                        var bealingX = (int)_FaceFacade.GlyphSlot->metrics.horiBearingX / 64;
                        var bearingY = (int)_FaceFacade.GlyphSlot->metrics.horiBearingY / 64;

                        error = FT.FT_Render_Glyph(_FaceFacade.GlyphSlot, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);

                        if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

                        var bitmap = _FaceFacade.GlyphSlot->bitmap;
                        var cid = new CharImageData(advanceX, bealingX, bearingY, (int)bitmap.width, (int)bitmap.rows + 1);
                        for (int i = 0; i < bitmap.width; i++)
                        {
                            for (int j = 0; j < bitmap.rows; j++)
                            {
                                cid.SetData(i, j, bitmap.buffer[i + j * bitmap.pitch]);
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
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    _Library.Dispose();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                _FaceFacade = null;

                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ImageFont()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
