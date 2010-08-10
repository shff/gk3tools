using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public class TextureAtlas
    {
        private UpdatableTexture _packedTexture;
        private Rect[] _packedTextureRects;

        public TextureAtlas(TextureResource[] textures)
        {
            // heavily based on
            // http://www.blackpawn.com/texts/lightmaps/default.html
            // basically it uses a kd-tree to pack the lightmaps

            AtlasNode root = new AtlasNode();
            root.Rectangle = new Rect(0, 0, 512, 512);

            for (int i = 0; i < textures.Length; i++)
            {
                root.Insert(textures[i], i);
            }

            // TODO: this shoudln't be hardcoded!!
            int outputWidth = 512;
            int outputHeight = 512;
            
            byte[] textureData = new byte[outputWidth * outputHeight * 4];
            root.AddToBitmap(textureData, outputWidth, outputHeight);
            
            _packedTexture = RendererManager.CurrentRenderer.CreateUpdatableTexture("blah", 512, 512);
            _packedTexture.Update(textureData);

            // create the packed rectangles
            _packedTextureRects = new Rect[textures.Length];
            root.UpdateRectList(_packedTextureRects);

            // now we have the rects, but they need to be converted from pixel coords
            for (int i = 0; i < _packedTextureRects.Length; i++)
            {
                Rect r = _packedTextureRects[i];
                r.X /= outputWidth;
                r.Y /= outputHeight;
                r.Width /= outputWidth;
                r.Height /= outputHeight;

                _packedTextureRects[i] = r;
            }

            Utils.WriteTga("lightmap.tga", textureData, 512, 512);
        }

        public TextureResource PackedTexture
        {
            get { return _packedTexture; }
        }

        public Rect GetPackedTextureRect(int index)
        {
            return _packedTextureRects[index];
        }

        private class AtlasNode
        {
            public AtlasNode Child1;
            public AtlasNode Child2;

            public Rect Rectangle;
            public int Index;
            public TextureResource Texture;

            public AtlasNode Insert(TextureResource texture, int index) 
            {
                AtlasNode newNode = null;

                if (Child1 != null && Child2 != null)
                {
                    // not on a leaf...
                    newNode = Child1.Insert(texture, index);
                    if (newNode != null) return newNode;

                    // no room? try the other child...
                    return Child2.Insert(texture, index);
                }
                else
                {
                    // we're on a leaf!
                    if (Texture != null)
                        return null;

                    int fit = testFit(texture);
                    if (fit > 0) return null; // too big
                    if (fit == 0)
                    {
                        Texture = texture;
                        Index = index;
                        return this;
                    }

                    // guess we need to split this node
                    Child1 = new AtlasNode();
                    Child2 = new AtlasNode();

                    float dw = Rectangle.Width - texture.Width;
                    float dh = Rectangle.Height - texture.Height;

                    if (dw > dh)
                    {
                        Child1.Rectangle = new Rect(Rectangle.X, Rectangle.Y, 
                            texture.Width, Rectangle.Height);
                        Child2.Rectangle = new Rect(Rectangle.X + texture.Width + 1, Rectangle.Y, 
                            Rectangle.Width - texture.Width - 1, Rectangle.Height);
                    }
                    else
                    {
                        Child1.Rectangle = new Rect(Rectangle.X, Rectangle.Y,
                            Rectangle.Width, texture.Height);
                        Child2.Rectangle = new Rect(Rectangle.X, Rectangle.Y + texture.Height + 1,
                            Rectangle.Width, Rectangle.Height - texture.Height - 1);
                    }

                    return Child1.Insert(texture, index);
                }
            }

            public void AddToBitmap(byte[] bitmap, int bitmapWidth, int bitmapHeight)
            {
                if (Texture != null)
                    blit((int)Rectangle.X, (int)Rectangle.Y, Texture.Pixels, Texture.Width, Texture.Height, bitmap, bitmapWidth, bitmapHeight);

                if (Child1 != null) Child1.AddToBitmap(bitmap, bitmapWidth, bitmapHeight);
                if (Child2 != null) Child2.AddToBitmap(bitmap, bitmapWidth, bitmapHeight);
            }

            public void UpdateRectList(Rect[] packedRects)
            {
                if (Texture != null)
                {
                    // HACK: this problably only improves the direct3d9 renderer,
                    // with its weird texturing coordinate nonsense...
                    Rect r = Rectangle;
                    r.X += 0.5f;
                    r.Y += 0.5f;
                    r.Width -= 1.0f;
                    r.Height -= 1.0f;
                    packedRects[Index] = r;
                }

                if (Child1 != null) Child1.UpdateRectList(packedRects);
                if (Child2 != null) Child2.UpdateRectList(packedRects);
            }

            private int testFit(TextureResource texture)
            {
                if (texture.Width > Rectangle.Width ||
                    texture.Height > Rectangle.Height)
                    return 1; // too big

                if (texture.Width == Rectangle.Width &&
                    texture.Height == Rectangle.Height)
                    return 0; // just right

                return -1; // texture fits with space left over
            }

            private void blit(int destX, int destY, byte[] source, int sourceWidth, int sourceHeight,
                byte[] destination, int destinationWidth, int destinationHeight)
            {
                for (int y = 0; y < sourceHeight; y++)
                {
                    int destinationIndex = ((destY + y) * destinationWidth + destX) * 4;
                    Array.Copy(source, y * sourceWidth * 4, destination,
                        destinationIndex, sourceWidth * 4);
                }
            }
        }
    }
}
