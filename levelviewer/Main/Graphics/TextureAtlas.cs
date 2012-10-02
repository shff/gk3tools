using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public class TextureAtlas
    {
        private BitmapSurface _packedTexture;
        private Rect[] _packedTextureRects;

        public TextureAtlas(BitmapSurface[] surfaces)
        {
            AtlasNode root = new AtlasNode();
            root.Rectangle = new Rect(0, 0, 512, 512);
            _packedTextureRects = new Rect[surfaces.Length];

            for (int i = 0; i < surfaces.Length; i++)
            {
                root.Insert(surfaces[i], i);
            }

            load(root);
        }

        public TextureAtlas(TextureResource[] textures)
        {
            // heavily based on
            // http://www.blackpawn.com/texts/lightmaps/default.html
            // basically it uses a kd-tree to pack the lightmaps

            AtlasNode root = new AtlasNode();
            root.Rectangle = new Rect(0, 0, 512, 512);
            _packedTextureRects = new Rect[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                root.Insert(new BitmapSurface(textures[i]), i);
            }

            load(root);
        }

        public BitmapSurface Surface
        {
            get { return _packedTexture; }
        }

        public Rect GetPackedTextureRect(int index)
        {
            return _packedTextureRects[index];
        }

        private void load(AtlasNode root)
        {
            // heavily based on
            // http://www.blackpawn.com/texts/lightmaps/default.html
            // basically it uses a kd-tree to pack the lightmaps

            // TODO: this shoudln't be hardcoded!!
            int outputWidth = 512;
            int outputHeight = 512;

            _packedTexture = new BitmapSurface(512, 512, null);
            root.AddToBitmap(_packedTexture);

            // create the packed rectangles
            root.UpdateRectList(_packedTextureRects);

            // now we have the rects, but they need to be converted from pixel coords
           /* for (int i = 0; i < _packedTextureRects.Length; i++)
            {
                Rect r = _packedTextureRects[i];
                r.X /= outputWidth;
                r.Y /= outputHeight;
                r.Width /= outputWidth;
                r.Height /= outputHeight;

                _packedTextureRects[i] = r;
            }*/

            Utils.WriteTga("lightmap.tga", _packedTexture.Pixels, 512, 512);
        }

        private class AtlasNode
        {
            public AtlasNode Child1;
            public AtlasNode Child2;

            public Rect Rectangle;
            public int Index;
            public BitmapSurface Surface;

            private const int Padding = 1;

            public AtlasNode Insert(BitmapSurface surface, int index) 
            {
                AtlasNode newNode = null;

                if (Child1 != null && Child2 != null)
                {
                    // not on a leaf...
                    newNode = Child1.Insert(surface, index);
                    if (newNode != null) return newNode;

                    // no room? try the other child...
                    return Child2.Insert(surface, index);
                }
                else
                {
                    // we're on a leaf!
                    if (Surface != null)
                        return null;

                    int fit = testFit(surface);
                    if (fit > 0) return null; // too big
                    if (fit == 0)
                    {
                        Surface = surface;
                        Index = index;
                        return this;
                    }

                    // guess we need to split this node
                    Child1 = new AtlasNode();
                    Child2 = new AtlasNode();

                    int paddedWidth = surface.Width + Padding * 2;
                    int paddedHeight = surface.Height + Padding * 2;

                    float dw = Rectangle.Width - paddedWidth;
                    float dh = Rectangle.Height - paddedHeight;

                    if (dw > dh)
                    {
                        Child1.Rectangle = new Rect(Rectangle.X, Rectangle.Y, 
                            paddedWidth, Rectangle.Height);
                        Child2.Rectangle = new Rect(Rectangle.X + paddedWidth + 1, Rectangle.Y, 
                            Rectangle.Width - paddedWidth - 1, Rectangle.Height);
                    }
                    else
                    {
                        Child1.Rectangle = new Rect(Rectangle.X, Rectangle.Y,
                            Rectangle.Width, paddedHeight);
                        Child2.Rectangle = new Rect(Rectangle.X, Rectangle.Y + paddedHeight + 1,
                            Rectangle.Width, Rectangle.Height - paddedHeight - 1);
                    }

                    return Child1.Insert(surface, index);
                }
            }

            public void AddToBitmap(BitmapSurface bitmap)
            {
                if (Surface != null)
                {
                    // top
                    BitmapSurface.Copy((int)Rectangle.X + 1, (int)Rectangle.Y, bitmap, 0, 0, Surface.Width, 1, Surface); 

                    // bottom
                    BitmapSurface.Copy((int)Rectangle.X + 1, (int)(Rectangle.Y + Rectangle.Height) - 1, bitmap, 0, Surface.Height - 1, Surface.Width, 1, Surface);

                    //left
                    BitmapSurface.Copy((int)Rectangle.X, (int)Rectangle.Y + 1, bitmap, 0, 0, 1, Surface.Height, Surface); 

                    // right
                    BitmapSurface.Copy((int)(Rectangle.X + Rectangle.Width) - 1, (int)Rectangle.Y + 1, bitmap, Surface.Width - 1, 0, 1, Surface.Height, Surface); 

                    // center
                    BitmapSurface.Copy((int)Rectangle.X + 1, (int)Rectangle.Y + 1, bitmap, 0, 0, Surface.Width, Surface.Height, Surface);

                    // upper left
                    BitmapSurface.Copy((int)Rectangle.X, (int)Rectangle.Y, bitmap, 0, 0, 1, 1, Surface); 

                    // upper right
                    BitmapSurface.Copy((int)(Rectangle.X + Rectangle.Width) - 1, (int)Rectangle.Y, bitmap, Surface.Width - 1, 0, 1, 1, Surface); 

                    // lower left
                    BitmapSurface.Copy((int)Rectangle.X, (int)(Rectangle.Y + Rectangle.Height) - 1, bitmap, 0, Surface.Height - 1, 1, 1, Surface); 

                    // lower right
                    BitmapSurface.Copy((int)(Rectangle.X + Rectangle.Width) - 1, (int)(Rectangle.Y + Rectangle.Height) - 1, bitmap, Surface.Width - 1, Surface.Height - 1, 1, 1, Surface); 

                }

                if (Child1 != null) Child1.AddToBitmap(bitmap);
                if (Child2 != null) Child2.AddToBitmap(bitmap);
            }

            public void UpdateRectList(Rect[] packedRects)
            {
                if (Surface != null)
                {
                    // HACK: this problably only improves the direct3d9 renderer,
                    // with its weird texturing coordinate nonsense...
                    Rect r = Rectangle;
                   /*r.X += 0.5f;
                    r.Y += 0.5f;
                    r.Width -= 1.0f;
                    r.Height -= 1.0f;*/
                    packedRects[Index] = r;
                }

                if (Child1 != null) Child1.UpdateRectList(packedRects);
                if (Child2 != null) Child2.UpdateRectList(packedRects);
            }

            private int testFit(BitmapSurface surface)
            {
                int width = surface.Width + Padding * 2;
                int height = surface.Height + Padding * 2;

                if (width > Rectangle.Width ||
                    height > Rectangle.Height)
                    return 1; // too big

                if (width == Rectangle.Width &&
                    height == Rectangle.Height)
                    return 0; // just right

                return -1; // texture fits with space left over
            }

            private void blit(int destX, int destY, byte[] source, int sourceWidth, int sourceHeight,
                byte[] destination, int destinationWidth)
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
