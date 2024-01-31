using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using nxtlvlOS.Windowing.Elements.Shapes;
using nxtlvlOS.Windowing.Fonts;
using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace nxtlvlOS.Windowing.Elements {
    public class TextField : BufferedElement {
        private uint backgroundColor = ColorUtils.Primary500;
        public uint BackgroundColor {
            get => backgroundColor;
            set {
                backgroundColor = value;
                frame.SetBackgroundColor(value); // The frame also gets the background color so we dont have to perform costly PixelByPixel operations
                this.SetDirty(true);
            }
        }

        private uint pressedColor = ColorUtils.Primary100;
        public uint PressedColor {
            get => pressedColor;
            set {
                pressedColor = value;
                this.SetDirty(true);
            }
        }

        public string Text {
            get => frame.Text;
            set => frame.SetText(value);
        }

        public string Placeholder {
            get => frame.Placeholder;
            set => frame.SetPlaceholder(value);
        }
        public uint TextColor {
            get => frame.TextColor;
            set => frame.SetTextColor(value);
        }

        public uint PlaceholderColor {
            get => frame.PlaceholderColor;
            set => frame.SetPlaceholderColor(value);
        }
        public Font Font {
            get => frame.Font;
            set => frame.SetFont(value);
        }
        public int ScrollX {
            get => frame.ScrollX;
            set => frame.SetScrollX(value);
        }
        public int ScrollY {
            get => frame.ScrollY;
            set => frame.SetScrollY(value);
        }
                                            
        public int CursorPos = 0;           
                                            
        public bool IsMouseDown { get; private set; } = false;
        public bool IsMouseHovering { get; private set; } = false;
        public bool EnterIsConfirm { get; set; } = false;

        public Event Confirmed { get; set; } = new Event();
                                            
        private ScrollableTextFrame frame = new() {
            CustomId = "TextFieldFrame"     
        };                                  
                                            
        private Rect cursor = new() {       
            SizeX = 2,                      
            SizeY = 16,                     
            CustomId = "CursorRect"         
        };                                  
                                            
        public TextField() {                
            DrawMode = BufferDrawMode.RawCopy;
                                            
            cursor.BackgroundColor = 0xFF000000;
            cursor.Visible = false;         
                                            
            frame.SetBackgroundColor(backgroundColor);
            UpdateFrameSizing();

            SizeChanged += () => {
                UpdateFrameSizing();
            };           
                                            
            AddChild(frame);                
            AddChild(cursor);               
        }                                   
                                            
        public override void Draw() {       
            SetDirty(false);                
                                            
            if (IsMouseDown) {              
                DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
            } else {                        
                if (!IsMouseHovering) {     
                    DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor);
                } else {                    
                    DrawRect(0, 0, SizeX, SizeY, 0xFF000000);
                    DrawRectFilled(1, 1, SizeX - 1, SizeY - 1, backgroundColor);
                }                           
            }                               
        }                                   
                                            
        public override void Update() {     
            if(WindowManager.FocusedElement == this && RTC.Second % 2 == 0) {
                cursor.Visible = true;      
                                            
                var textPos = GetTextPosition(CursorPos);
                var relPos = GetRelativePositionFromTextPosition(textPos.x, textPos.y);
                                            
                cursor.RelativePosX = relPos.x + 3;
                cursor.RelativePosY = relPos.y + 3;
            } else {                        
                cursor.Visible = false;     
            }                               
                                            
            base.Update();                  
        }                                   
                                            
        private void UpdateFrameSizing() {  
            frame.SizeX = SizeX - 8;        
            frame.SizeY = SizeY - 8;        
            frame.RelativePosX = 4;         
            frame.RelativePosY = 4;         
            frame.SetDirty(true);           
        }                                   
                                            
        private (int x, int y) GetTextPosition(int offset) {
            int x = 0;                      
            int y = 0;                      
                                            
            string[] lines = Text.Split('\n');

            for(int i = 0; i < offset; i++) {
                if (lines[y].Length <= x) {
                    x = 0;
                    y++;
                }else {
                    x++;
                }
            }

            return (x, y);
        }

        private (int x, int y) GetRelativePositionFromTextPosition(int textX, int textY) {
            int x, y;
            if (Font.Width == 0) {
                // This is prob a ttf font, so its variable. This makes shit a bit hard... Lets just take the size for now
                if(Font is TTFFont ttf) {
                    x = textX * ttf.Size / 2;
                    y = textY * ttf.Size;
                } else { // Fallback
                    x = textX * 8;
                    y = textY * 16;
                }
            } else {
                x = textX * Font.Width;
                y = textY * Font.Height;
            }

            return (x - ScrollX, y - ScrollY);
        }

        private (int x, int y) GetTextPositionFromRelativePosition(int relativeX, int relativeY) {
            int actualFontWidth, actualFontHeight;

            if (Font.Width == 0) {
                // also prob a ttf font
                if (Font is TTFFont ttf) {
                    actualFontWidth = ttf.Size / 2;
                    actualFontHeight = ttf.Size;
                } else { // Fallback
                    actualFontWidth = 8;
                    actualFontHeight = 16;
                }
            } else {
                actualFontWidth = Font.Width;
                actualFontHeight = Font.Height;
            }

            int textX = (relativeX + ScrollX) / actualFontWidth;
            int textY = (relativeY + ScrollY) / actualFontHeight;

            return (textX, textY);
        }

        /*private int GetOffsetFromTextPosition(int textX, int textY) {
            int offset = 0;
            string[] lines = Text.Split('\n');

            for (int y = 0; y < textY; y++) {
                if (y >= lines.Length)
                    break;

                offset += lines[y].Length + '\n'.Length;
            }

            offset += textX;
            return offset;
        }*/

        private int GetOffsetFromTextPosition(int textX, int textY) {
            int offset = 0;
            string[] lines = Text.Split('\n');

            for (int y = 0; y < textY; y++) {
                if (y >= lines.Length)
                    break;

                offset += lines[y].Length + 1;
            }

            if (textY < lines.Length)
                offset += textX;
            else if (textY == lines.Length && textX <= lines[textY - 1].Length)
                offset += textX;
            else
                offset = Text.Length;

            if (offset < 0) offset = 0;
            if (offset > Text.Length) offset = Text.Length;

            return offset;
        }

        public override void OnKey(KeyEvent ev) {
            if(char.IsControl(ev.KeyChar)) {
                switch(ev.Key) {
                    case ConsoleKeyEx.Backspace:
                        if (CursorPos > 0 && Text.Length > 0) {
                            Text = (Text.Remove(CursorPos - 1, 1));
                            CursorPos--;
                        }
                        break;
                    case ConsoleKeyEx.LeftArrow:
                        CursorPos--;
                        if (CursorPos < 0) CursorPos = 0;
                        break;
                    case ConsoleKeyEx.RightArrow:
                        CursorPos++;
                        if (CursorPos > Text.Length) CursorPos = Text.Length;
                        break;
                    case ConsoleKeyEx.Enter:
                        if(EnterIsConfirm) {
                            Confirmed.Invoke();
                            return;
                        }

                        Text = (Text.Insert(CursorPos, "\n"));
                        CursorPos++;
                        break;
                    case ConsoleKeyEx.UpArrow:
                        var textPos = GetTextPosition(CursorPos);
                        textPos.y--;
                        if (textPos.y < 0) textPos.y = 0;
                        CursorPos = GetOffsetFromTextPosition(textPos.x, textPos.y);
                        break;
                    case ConsoleKeyEx.DownArrow:
                        textPos = GetTextPosition(CursorPos);
                        textPos.y++;
                        CursorPos = GetOffsetFromTextPosition(textPos.x, textPos.y);
                        break;
                }
            }else {
                Text = (Text.Insert(CursorPos, ev.KeyChar.ToString()));
                CursorPos++;
            }
        }

        public override void OnHoverStart() {
            IsMouseHovering = true;
            this.SetDirty(true);
        }

        public override void OnHoverEnd() {
            IsMouseHovering = false;
            this.SetDirty(true);
        }

        public override void OnMouseDown(MouseState state) {
            base.OnMouseDown(state);

            IsMouseDown = true;
            this.SetDirty(true);
        }

        public override void OnMouseUp(MouseState state, MouseState prev, bool mouseIsOver) {
            base.OnMouseUp(state, prev, mouseIsOver);

            var relativeMouse = GetRelativeFromAbsolutePosition((int)MouseManager.X, (int)MouseManager.Y);
            var textPos = GetTextPositionFromRelativePosition(relativeMouse.x, relativeMouse.y);

            CursorPos = GetOffsetFromTextPosition(textPos.x, textPos.y);
            IsMouseDown = false;
            this.SetDirty(true);
        }

        public class ScrollableTextFrame : BufferedElement {
            private string text = "";
            public string Text => text;
            private string placeholder = "Placeholder";
            public string Placeholder => placeholder;

            private Font font = WindowManager.DefaultFont;
            public Font Font => font;

            private uint textColor = 0xFFFFFFFF;
            public uint TextColor => textColor;

            private uint placeholderColor = 0xFFCCCCCC;
            public uint PlaceholderColor => placeholderColor;

            private uint backgroundColor = 0xFF444466;
            private uint BackgroundColor => backgroundColor;

            private int scrollX = 0;
            private int scrollY = 0;

            public int ScrollX => scrollX;
            public int ScrollY => scrollY;

            private string textCache = "";
            private bool overflowX = false, overflowY = false;


            public bool IsMouseDown { get; private set; } = false;


            public ScrollableTextFrame() {
                DrawMode = BufferDrawMode.RawCopy;
                MousePassThrough = true;
                SizeChanged += () => {
                    Kernel.Instance.Logger.Log(LogLevel.Info, "Size changed for ScrollableTextFrame with text " + text);
                };
            }

            public override void Update() {
                if(text != textCache) {
                    textCache = text;

                    (overflowX, overflowY) = GetOverflowAxes();
                }

                base.Update();
            }

            public override void Draw() {
                SetDirty(false);
                DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor); // see note in SetBackgroundColor of TextField
                DrawStringWithNewLines(
                    font,
                    -ScrollX,
                    -ScrollY,
                    text.Length != 0 ? text : placeholder,
                    text.Length != 0 ? textColor : placeholderColor,
                    true,
                    false);
            }

            public void SetText(string text) {
                this.text = text;
                this.SetDirty(true);
            }

            public void SetPlaceholder(string placeholder) {
                this.placeholder = placeholder;
                this.SetDirty(true);
            }

            public void SetFont(Font font) {
                this.font = font;
                this.SetDirty(true);
            }

            public void SetTextColor(uint color) {
                this.textColor = color;
                this.SetDirty(true);
            }

            public void SetPlaceholderColor(uint color) {
                this.placeholderColor = color;
                this.SetDirty(true);
            }

            public void SetScrollX(int scrollX) {
                this.scrollX = scrollX;
                this.SetDirty(true);
            }

            public void SetScrollY(int scrollY) {
                this.scrollY = scrollY;
                this.SetDirty(true);
            }

            public void SetBackgroundColor(uint backgroundColor) {
                this.backgroundColor = backgroundColor;
                this.SetDirty(true);
            }

            public override void OnMouseDown(MouseState state) {
                WindowManager.FocusedElement = Parent;
                Parent.OnMouseDown(state);
            }

            public override void OnMouseUp(MouseState state, MouseState prev, bool mouseIsOver) {
                Parent.OnMouseUp(state, prev, mouseIsOver);
            }

            private (bool x, bool y) GetOverflowAxes() {
                var measurement = font.MeasureStringExhaustive(text);

                return (measurement.w > SizeX, measurement.h > SizeY);
            }
        }
    }
}
