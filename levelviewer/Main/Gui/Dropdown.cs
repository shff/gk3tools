﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    class Dropdown : IButtonContainer
    {
        private bool _down;
        private FontInstance _font;
        private Button _downArrow;
        private Graphics.TextureResource _top;
        private Graphics.TextureResource _side;
        private Graphics.TextureResource _blCorner, _brCorner, _tlCorner, _trCorner;
        private Graphics.TextureResource _dropdownBackground;
        private IButtonContainer _container;
        private List<KeyValuePair<string, string>> _items = new List<KeyValuePair<string,string>>();
        private int _selectedIndex;
        private Unit _x, _y;
        int _width;
        private int _screenX, _screenY;
        private Graphics.Rect _boxRect;
        private EventHandler _onSelectedItemChanged;
        private const int _itemVerticalPadding = 4;

        public Dropdown(IButtonContainer container, Resource.ResourceManager globalContent, int width, 
            string arrowDownSprite, string arrowHoverSprite, string arrowUpSprite)
        {
            _container = container;
            _downArrow = new Button(this, globalContent, arrowDownSprite, arrowHoverSprite, arrowUpSprite, null, null);
            _font = Gk3Main.Gui.Font.Load(globalContent.Load<FontSpec>("F_ARIAL_T8"));
            _top = globalContent.Load<Graphics.TextureResource>("RC_BOX_TOP");
            _side = globalContent.Load<Graphics.TextureResource>("RC_BOX_SIDE");
            _blCorner = globalContent.Load<Graphics.TextureResource>("RC_BOX_CORNER_BL");
            _brCorner = globalContent.Load<Graphics.TextureResource>("RC_BOX_CORNER_BR");
            _tlCorner = globalContent.Load<Graphics.TextureResource>("RC_BOX_CORNER_TL");
            _trCorner = globalContent.Load<Graphics.TextureResource>("RC_BOX_CORNER_TR");
            _dropdownBackground = globalContent.Load<Graphics.TextureResource>("OPTSC");

            _downArrow.OnClick += new EventHandler(_downArrow_OnClick);

            _width = width;
        }

        public void Render(Graphics.SpriteBatch sb, int tickCount)
        {
            const int padding = 4;
            int left = _screenX - _width + _downArrow.Width + padding;
            int top = _screenY + padding;

            _downArrow.Render(sb, tickCount);

            if (_selectedIndex >= 0)
                Gk3Main.Gui.Font.Print(sb, _font, left, top, _items[_selectedIndex].Value);

            if (_down)
            {
                // draw the top
                sb.Draw(_top, new Gk3Main.Graphics.Rect(_boxRect.X - 1, _boxRect.Y - 1, _boxRect.Width + 2, _top.Height), null, 0);

                // draw the bottom
                sb.Draw(_top, new Gk3Main.Graphics.Rect(_boxRect.X - 1, _boxRect.Y + _boxRect.Height - 1, _boxRect.Width + 2, _top.Height), null, 0);
            
                // draw the left
                sb.Draw(_side, new Gk3Main.Graphics.Rect(_boxRect.X - 1, _boxRect.Y - 1, _side.Width, _boxRect.Height + 2), null, 0);

                // draw the right
                sb.Draw(_side, new Gk3Main.Graphics.Rect(_boxRect.X + _boxRect.Width - 1, _boxRect.Y - 1, _side.Width, _boxRect.Height + 2), null, 0);

                // fill
                sb.Draw(_dropdownBackground, _boxRect, new Gk3Main.Graphics.Rect(3, 3, 1, 1), 0);

                // render the item text
                int texty = _screenY + _downArrow.Height;
                foreach (KeyValuePair<string, string> item in _items)
                {
                    Gk3Main.Gui.Font.Print(sb, _font, (int)_boxRect.X + 1, texty + _itemVerticalPadding, item.Value);

                    texty += _font.Font.LineHeight + _itemVerticalPadding;
                }
            }
        }

        public Unit X
        {
            get { return _x; }
            set { _x = value; CalculateScreenCoordinates(); }
        }

        public Unit Y
        {
            get { return _y; }
            set { _y = value; CalculateScreenCoordinates(); }
        }

        public List<KeyValuePair<string, string>> Items { get { return _items; } }

        public int SelectedIndex { get { return _selectedIndex; } }

        public string SelectedValue
        {
            get
            {
                if (_selectedIndex < 0) return null;

                return _items[_selectedIndex].Value;
            }
            set
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i].Key.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        _selectedIndex = i;
                        return;
                    }
                }

                // doh, that value doesn't exist!
                _selectedIndex = -1;
            }
        }

        public int ScreenX { get { return _screenX; } }
        public int ScreenY { get { return _screenY; } }

        public void InsertAndSelect(string key, string value, bool sorted)
        {
            SelectedValue = key;
            if (_selectedIndex == -1)
            {
                if (sorted)
                {
                    for (int i = 0; i < _items.Count; i++)
                    {
                        if (string.Compare(_items[i].Value, value) > 0)
                        {
                            _items.Insert(i, new KeyValuePair<string, string>(key, value));
                            _selectedIndex = i;
                            return;
                        }
                    }
                }

                // still here? either they don't care if this is sorted, or
                // the best sorted way to insert is at the end.
                _items.Add(new KeyValuePair<string, string>(key, value));
                _selectedIndex = Items.Count - 1;
            }
        }

        internal void CalculateScreenCoordinates()
        {
            Graphics.Viewport vp = Graphics.RendererManager.CurrentRenderer.Viewport;

            if (_container != null)
            {
                _screenX = (int)(vp.X + _x.Scale * vp.Width + _x.Offset) + _container.ScreenX;
                _screenY = (int)(vp.Y + _y.Scale * vp.Height + _y.Offset) + _container.ScreenY;
            }
            else
            {
                _screenX = (int)(vp.X + _x.Scale * vp.Width + _x.Offset);
                _screenY = (int)(vp.Y + _y.Scale * vp.Height + _y.Offset);
            }

            _downArrow.CalculateScreenCoordinates();

            calculateBoxRect();
        }

        public void OnMouseMove(int tickCount, int mouseX, int mouseY)
        {
            _downArrow.OnMouseMove(tickCount, mouseX, mouseY);
        }

        public void OnMouseUp(int button, int mx, int my)
        {
            _downArrow.OnMouseUp(button);

            // check if the user selected an item
            if (_down)
            {
                int item = getItemMouseIsOver(mx, my);

                if (item >= 0)
                {
                    int oldSelectedIndex = _selectedIndex;
                    _selectedIndex = item;
                    _down = false;

                    if (oldSelectedIndex != _selectedIndex &&
                        _onSelectedItemChanged != null)
                        _onSelectedItemChanged(this, EventArgs.Empty);
                }
            }
        }

        public void OnMouseDown(int button)
        {
            _downArrow.OnMouseDown(button);
        }

        public event EventHandler OnSelectedItemChanged
        {
            add { _onSelectedItemChanged += value; }
            remove { _onSelectedItemChanged -= value; }
        }

        private void calculateBoxRect()
        {
            float width = _width;
            foreach (KeyValuePair<string, string> item in _items)
            {
                float itemWidth = Gk3Main.Gui.Font.MeasureString(_font, item.Value).X;
                if (itemWidth > width)
                    width = itemWidth;
            }

            float boxLeft = _screenX - width + _downArrow.Width;
            int height = _items.Count * (_font.Font.LineHeight + _itemVerticalPadding);

            _boxRect = new Gk3Main.Graphics.Rect(boxLeft + 1, _screenY + _downArrow.Height + 1, width - 2, height - 2);
        }

        private int getItemMouseIsOver(int mx, int my)
        {
            // transform mouse coords relative to the box
            int rmx = mx - (int)_boxRect.X;
            int rmy = my - (int)_boxRect.Y;

            if (rmx >= 0 && rmx < _boxRect.Width &&
                rmy >= 0 && rmy < _boxRect.Height)
            {
                return rmy / (_font.Font.LineHeight + _itemVerticalPadding);
            }

            return -1;
        }

        private void _downArrow_OnClick(object sender, EventArgs e)
        {
            _down = !_down;
        }
    }
}
