using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    /// <summary>
    /// Helper for arranging many small sprites into a single larger sheet.
    /// (shamelessly stolen from http://xbox.create.msdn.com/en-US/education/catalog/sample/sprite_sheet)
    /// </summary>
    public class TextureAtlasX
    {
        private BitmapSurface _packedSurface;
        private Rect[] _packedTextureRects;

        /// <summary>
        /// Packs a list of sprites into a single big texture,
        /// recording where each one was stored.
        /// </summary>
        public TextureAtlasX(BitmapSurface[] surfaces)
        {
            if (surfaces.Length == 0)
                throw new ArgumentException();

            _packedTextureRects = new Rect[surfaces.Length];

            // Build up a list of all the sprites needing to be arranged.
            List<ArrangedSprite> sprites = new List<ArrangedSprite>();

            for (int i = 0; i < surfaces.Length; i++)
            {
                ArrangedSprite sprite = new ArrangedSprite();

                // Include a single pixel padding around each sprite, to avoid
                // filtering problems if the sprite is scaled or rotated.
                sprite.Width = surfaces[i].Width + 2;
                sprite.Height = surfaces[i].Height + 2;

                sprite.Index = i;

                sprites.Add(sprite);
            }

            // Sort so the largest sprites get arranged first.
            sprites.Sort(CompareSpriteSizes);

            // Work out how big the output bitmap should be.
            int outputWidth = GuessOutputWidth(sprites);
            int outputHeight = 0;
            int totalSpriteSize = 0;

            // Choose positions for each sprite, one at a time.
            for (int i = 0; i < sprites.Count; i++)
            {
                PositionSprite(sprites, i, outputWidth);

                outputHeight = System.Math.Max(outputHeight, sprites[i].Y + sprites[i].Height);

                totalSpriteSize += sprites[i].Width * sprites[i].Height;
            }

            // Sort the sprites back into index order.
            sprites.Sort(CompareSpriteIndices);

            _packedSurface = CopySpritesToOutput(sprites, surfaces, _packedTextureRects,
                                       outputWidth, outputHeight);
        }

        public BitmapSurface Surface
        {
            get { return _packedSurface; }
        }

        public Rect GetPackedTextureRect(int index)
        {
            return _packedTextureRects[index];
        }

        /// <summary>
        /// Once the arranging is complete, copies the bitmap data for each
        /// sprite to its chosen position in the single larger output bitmap.
        /// </summary>
        static BitmapSurface CopySpritesToOutput(List<ArrangedSprite> sprites,
                                                 BitmapSurface[] sourceSprites,
                                                 Rect[] outputSprites,
                                                 int width, int height)
        {
            BitmapSurface output = new BitmapSurface(width, height, null);

            int counter = 0;
            foreach (ArrangedSprite sprite in sprites)
            {
                BitmapSurface source = sourceSprites[sprite.Index];

                int x = sprite.X;
                int y = sprite.Y;

                int w = source.Width;
                int h = source.Height;

                // Copy the main sprite data to the output sheet.
                BitmapSurface.Copy(x + 1, y + 1, output, 0, 0, w, h, source);

                // Copy a border strip from each edge of the sprite, creating
                // a one pixel padding area to avoid filtering problems if the
                // sprite is scaled or rotated.
                BitmapSurface.Copy(x, y + 1, output, 0, 0, 1, h, source);
                BitmapSurface.Copy(x + w + 1, y + 1, output, w - 1, 0, 1, h, source);
                BitmapSurface.Copy(x + 1, y, output, 0, 0, w, 1, source);
                BitmapSurface.Copy(x + 1, y + h + 1, output, 0, h - 1, w, 1, source);

                // Copy a single pixel from each corner of the sprite,
                // filling in the corners of the one pixel padding area.
                BitmapSurface.Copy(x, y, output, 0, 0, 1, 1, source);
                BitmapSurface.Copy(x + w + 1, y, output, w - 1, 0, 1, 1, source);
                BitmapSurface.Copy(x, y + h + 1, output, 0, h - 1, 1, 1, source);
                BitmapSurface.Copy(x + w + 1, y + h + 1, output, w - 1, h - 1, 1, 1, source);

                // Remember where we placed this sprite.
                outputSprites[counter++] = new Rect(x + 1, y + 1, w, h);
            }

            return output;
        }


        /// <summary>
        /// Internal helper class keeps track of a sprite while it is being arranged.
        /// </summary>
        class ArrangedSprite
        {
            public int Index;

            public int X;
            public int Y;

            public int Width;
            public int Height;
        }


        /// <summary>
        /// Works out where to position a single sprite.
        /// </summary>
        static void PositionSprite(List<ArrangedSprite> sprites,
                                   int index, int outputWidth)
        {
            int x = 0;
            int y = 0;

            while (true)
            {
                // Is this position free for us to use?
                int intersects = FindIntersectingSprite(sprites, index, x, y);

                if (intersects < 0)
                {
                    sprites[index].X = x;
                    sprites[index].Y = y;

                    return;
                }

                // Skip past the existing sprite that we collided with.
                x = sprites[intersects].X + sprites[intersects].Width;

                // If we ran out of room to move to the right,
                // try the next line down instead.
                if (x + sprites[index].Width > outputWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }


        /// <summary>
        /// Checks if a proposed sprite position collides with anything
        /// that we already arranged.
        /// </summary>
        static int FindIntersectingSprite(List<ArrangedSprite> sprites,
                                          int index, int x, int y)
        {
            int w = sprites[index].Width;
            int h = sprites[index].Height;

            for (int i = 0; i < index; i++)
            {
                if (sprites[i].X >= x + w)
                    continue;

                if (sprites[i].X + sprites[i].Width <= x)
                    continue;

                if (sprites[i].Y >= y + h)
                    continue;

                if (sprites[i].Y + sprites[i].Height <= y)
                    continue;

                return i;
            }

            return -1;
        }


        /// <summary>
        /// Comparison function for sorting sprites by size.
        /// </summary>
        static int CompareSpriteSizes(ArrangedSprite a, ArrangedSprite b)
        {
            int aSize = a.Height * 1024 + a.Width;
            int bSize = b.Height * 1024 + b.Width;

            return bSize.CompareTo(aSize);
        }


        /// <summary>
        /// Comparison function for sorting sprites by their original indices.
        /// </summary>
        static int CompareSpriteIndices(ArrangedSprite a, ArrangedSprite b)
        {
            return a.Index.CompareTo(b.Index);
        }


        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        static int GuessOutputWidth(List<ArrangedSprite> sprites)
        {
            // Gather the widths of all our sprites into a temporary list.
            List<int> widths = new List<int>();

            foreach (ArrangedSprite sprite in sprites)
            {
                widths.Add(sprite.Width);
            }

            // Sort the widths into ascending order.
            widths.Sort();

            // Extract the maximum and median widths.
            int maxWidth = widths[widths.Count - 1];
            int medianWidth = widths[widths.Count / 2];

            // Heuristic assumes an NxN grid of median sized sprites.
            int width = medianWidth * (int)System.Math.Round(System.Math.Sqrt(sprites.Count));

            // Make sure we never choose anything smaller than our largest sprite.
            return System.Math.Max(width, maxWidth);
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
            root.AddToBitmap(_packedTexture.Pixels, outputWidth, outputHeight);

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

                    float dw = Rectangle.Width - surface.Width;
                    float dh = Rectangle.Height - surface.Height;

                    if (dw > dh)
                    {
                        Child1.Rectangle = new Rect(Rectangle.X, Rectangle.Y, 
                            surface.Width, Rectangle.Height);
                        Child2.Rectangle = new Rect(Rectangle.X + surface.Width + 1, Rectangle.Y, 
                            Rectangle.Width - surface.Width - 1, Rectangle.Height);
                    }
                    else
                    {
                        Child1.Rectangle = new Rect(Rectangle.X, Rectangle.Y,
                            Rectangle.Width, surface.Height);
                        Child2.Rectangle = new Rect(Rectangle.X, Rectangle.Y + surface.Height + 1,
                            Rectangle.Width, Rectangle.Height - surface.Height - 1);
                    }

                    return Child1.Insert(surface, index);
                }
            }

            public void AddToBitmap(byte[] bitmap, int bitmapWidth, int bitmapHeight)
            {
                if (Surface != null)
                    blit((int)Rectangle.X, (int)Rectangle.Y, Surface.Pixels, Surface.Width, Surface.Height, bitmap, bitmapWidth, bitmapHeight);

                if (Child1 != null) Child1.AddToBitmap(bitmap, bitmapWidth, bitmapHeight);
                if (Child2 != null) Child2.AddToBitmap(bitmap, bitmapWidth, bitmapHeight);
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
                if (surface.Width > Rectangle.Width ||
                    surface.Height > Rectangle.Height)
                    return 1; // too big

                if (surface.Width == Rectangle.Width &&
                    surface.Height == Rectangle.Height)
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
