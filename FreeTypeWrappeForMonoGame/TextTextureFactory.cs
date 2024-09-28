using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleFreeTypeSharp;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace FreeTypeWrapper
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
                return false;
            }

            _Service.Add(fontKey, font);
            return true;
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
