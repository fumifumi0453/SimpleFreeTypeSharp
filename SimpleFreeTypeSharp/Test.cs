using FreeTypeSharp;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    public unsafe class Test
    {

        public static CharImageData TestChar(string path, char c)
        {
            var library = new FreeTypeLibrary();
            FT_FaceRec_* face;

            var error = FT.FT_New_Face(library.Native, (byte*)Marshal.StringToHGlobalAnsi(path), IntPtr.Zero, &face);

            error = FT.FT_Set_Char_Size(face, IntPtr.Zero, new IntPtr(24 * 64), 300, 300);

            var glyphIndex = FT.FT_Get_Char_Index(face, new UIntPtr(c));
            error = FT.FT_Load_Glyph(face, glyphIndex, FT_LOAD.FT_LOAD_DEFAULT);
            error = FT.FT_Render_Glyph(face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);

            var bitmap = face->glyph->bitmap;

            var tid = new CharImageData((int)bitmap.width, (int)bitmap.rows + 1);
            for (int i = 0; i < bitmap.width; i++)
            {
                for (int j = 0; j < bitmap.rows; j++)
                {
                    tid.SetData(i, j, bitmap.buffer[i + j * bitmap.pitch]);
                }
            }
            return tid;
        }

        public static CharImageData[] TestString(string path, string s)
        {
            var library = new FreeTypeLibrary();
            FT_FaceRec_* face;
            var lst = new List<CharImageData>(s.Length);
            
            var error = FT.FT_New_Face(library.Native, (byte*)Marshal.StringToHGlobalAnsi(path), IntPtr.Zero, &face);

            error = FT.FT_Set_Char_Size(face, IntPtr.Zero, new IntPtr(24 * 64), 300, 300);

            foreach (var c in s.ToCharArray())
            {
                var glyphIndex = FT.FT_Get_Char_Index(face, new UIntPtr(c));
                error = FT.FT_Load_Glyph(face, glyphIndex, FT_LOAD.FT_LOAD_DEFAULT);
                error = FT.FT_Render_Glyph(face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);

                var bitmap = face->glyph->bitmap;

                var tid = new CharImageData((int)bitmap.width, (int)bitmap.rows + 1);
                for (int i = 0; i < bitmap.width; i++)
                {
                    for (int j = 0; j < bitmap.rows; j++)
                    {
                        tid.SetData(i, j, bitmap.buffer[i + j * bitmap.pitch]);
                    }
                }
                lst.Add(tid);
            }
            return lst.ToArray();
        }


    }
}
