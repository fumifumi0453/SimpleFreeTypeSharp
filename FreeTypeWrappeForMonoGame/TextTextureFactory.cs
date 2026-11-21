using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleFreeTypeSharp;
using System.Collections.Generic;

namespace FreeTypeWrapperForMonoGame
{
    /// <summary>
    /// TextTextureを作成するためのFactory
    /// </summary>
    public static class TextTextureFactory
    {
        /// <summary>
        /// TextTextureの利用先のGraphicsDevice
        /// </summary>
        public static GraphicsDevice GraphicsDevice { get; set; }

        private static readonly Dictionary<string, ImageFont> _Service = new();

        /// <summary>
        /// フォントデータを読み込みます
        /// フォントデータはfontKeyをキーとしてキャッシュとして保持します
        /// </summary>
        /// <param name="fontKey"></param>
        /// <param name="fontPath"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool LoadFont(string fontKey, string fontPath, float size)
        {
            if (_Service.ContainsKey(fontKey)) return false;

            var font = new ImageFont();
            try
            {
                font.SetFont(fontPath);
                font.SetSize(size);
            }
            catch
            {
                // 読み込みに失敗した場合もFreeTypeのリソースを解放する
                font.Dispose();
                return false;
            }

            _Service.Add(fontKey, font);
            return true;
        }
        /// <summary>
        /// フォントデータを読み込みます
        /// フォントデータはfontKeyをキーとしてキャッシュとして保持します
        /// </summary>
        /// <param name="fontKey"></param>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool LoadFont(string fontKey, byte[] bytes, float size)
        {
            if (_Service.ContainsKey(fontKey)) return false;

            var font = new ImageFont();
            try
            {
                font.SetFont(bytes);
                font.SetSize(size);
            }
            catch
            {
                // 読み込みに失敗した場合もFreeTypeのリソースを解放する
                font.Dispose();
                return false;
            }

            _Service.Add(fontKey, font);
            return true;
        }

        /// <summary>
        /// 読み込んだフォントデータを破棄します
        /// 破棄したフォントを使用している TextTexture は、以降テクスチャを作成できなくなります
        /// </summary>
        /// <param name="fontKey"></param>
        /// <returns>破棄した場合はtrue</returns>
        public static bool UnloadFont(string fontKey)
        {
            if (_Service.Remove(fontKey, out var font) == false) return false;

            font.Dispose();
            return true;
        }
        /// <summary>
        /// 読み込んだすべてのフォントデータを破棄します
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var font in _Service.Values) font.Dispose();
            _Service.Clear();
        }

        /// <summary>
        /// TextTextureを作成します
        /// </summary>
        /// <param name="fontKey"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static TextTexture Create(string fontKey, string text, Color color)
        {
            return new TextTexture(GraphicsDevice, fontKey, _Service[fontKey], text, color);
        }
        /// <summary>
        /// TextTextureを作成します。
        /// </summary>
        /// <param name="fontKey"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static TextTexture Create(string fontKey, Color color)
        {
            return new TextTexture(GraphicsDevice, fontKey, _Service[fontKey], string.Empty, color);
        }
    }
}
