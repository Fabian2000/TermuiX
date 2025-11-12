namespace TermuiX
{
    internal class Renderer
    {
        private int _width;
        private int _height;

        internal void Size(int width, int height)
        {
            _width = width;
            _height = height;
        }

        internal (char[][] chars, ConsoleColor[][] fg, ConsoleColor[][] bg) Render(IWidget widget)
        {
            if (_width == 0 || _height == 0)
            {
                throw new InvalidOperationException("Renderer size not set. Call Size(width, height) before Render().");
            }

            // Create output buffers
            var output = new char[_height][];
            var fgColors = new ConsoleColor[_height][];
            var bgColors = new ConsoleColor[_height][];

            for (int i = 0; i < _height; i++)
            {
                output[i] = new char[_width];
                fgColors[i] = new ConsoleColor[_width];
                bgColors[i] = new ConsoleColor[_width];
                Array.Fill(output[i], ' ');
                Array.Fill(fgColors[i], ConsoleColor.White);
                Array.Fill(bgColors[i], ConsoleColor.Black);
            }

            // Render the root widget
            RenderWidget(output, fgColors, bgColors, widget, 0, 0, _width, _height, 0, 0);

            return (output, fgColors, bgColors);
        }

        private static void RenderWidget(char[][] output, ConsoleColor[][] fgColors, ConsoleColor[][] bgColors, IWidget widget, int parentX, int parentY, int parentWidth, int parentHeight, int parentScrollX, int parentScrollY)
        {
            if (!widget.Visible) return;

            // Parse dimensions with validation
            int width = ParseSize(widget.Width, parentWidth);
            int height = ParseSize(widget.Height, parentHeight);
            int posX = ParseSize(widget.PositionX, parentWidth);
            int posY = ParseSize(widget.PositionY, parentHeight);

            // Apply parent scroll offset to position
            posX -= parentScrollX;
            posY -= parentScrollY;

            // Parse padding with validation
            int padLeft = ParseSize(widget.PaddingLeft, width);
            int padTop = ParseSize(widget.PaddingTop, height);
            int padRight = ParseSize(widget.PaddingRight, width);
            int padBottom = ParseSize(widget.PaddingBottom, height);

            // If widget is a Container with a border, add 1ch to all padding for the border
            if (widget is Widgets.Container container && container.HasBorder)
            {
                padLeft += 1;
                padTop += 1;
                padRight += 1;
                padBottom += 1;
            }

            // Calculate absolute position
            int absX = parentX + posX;
            int absY = parentY + posY;

            // Clip to parent bounds
            if (absX >= parentX + parentWidth || absY >= parentY + parentHeight) return;
            if (absX + width < parentX || absY + height < parentY) return;

            // Get widget content
            var raw = widget.GetRaw();

            // Calculate content area (after padding, this is where content and children go)
            int contentWidth = Math.Max(0, width - padLeft - padRight);
            int contentHeight = Math.Max(0, height - padTop - padBottom);
            int contentX = absX + padLeft;
            int contentY = absY + padTop;

            // Get scroll offsets for this widget
            int scrollX = widget.Scrollable ? (int)widget.ScrollOffsetX : 0;
            int scrollY = widget.Scrollable ? (int)widget.ScrollOffsetY : 0;

            // Determine colors based on focus state
            var bgColor = widget.Focussed ? widget.FocusBackgroundColor : widget.BackgroundColor;
            var fgColor = widget.Focussed ? widget.FocusForegroundColor : widget.ForegroundColor;

            // Render background (entire widget area including padding)
            for (int y = 0; y < height && absY + y < output.Length; y++)
            {
                for (int x = 0; x < width && absX + x < output[0].Length; x++)
                {
                    if (absX + x >= 0 && absY + y >= 0 && absX + x >= parentX && absY + y >= parentY && absX + x < parentX + parentWidth && absY + y < parentY + parentHeight)
                    {
                        output[absY + y][absX + x] = ' ';
                        bgColors[absY + y][absX + x] = bgColor;
                        fgColors[absY + y][absX + x] = fgColor;
                    }
                }
            }

            // Render content - widgets render in FULL widget area (for borders)
            // But scrolling only affects content INSIDE padding area
            if (raw?.Length > 0)
            {
                // First render the raw content WITHOUT scroll (for borders, static content)
                // This is rendered in the full widget area
                for (int y = 0; y < raw.Length && y < height; y++)
                {
                    for (int x = 0; x < raw[y].Length && x < width; x++)
                    {
                        int targetX = absX + x;
                        int targetY = absY + y;

                        // Clip to widget area and parent bounds
                        if (targetY >= absY && targetY < absY + height &&
                            targetX >= absX && targetX < absX + width &&
                            targetY >= 0 && targetY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && targetY >= parentY &&
                            targetX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            output[targetY][targetX] = raw[y][x];
                            // Colors are already set by background rendering
                        }
                    }
                }
            }

            // Render children (later children on top)
            // Children are positioned relative to content area and affected by scroll
            foreach (var child in widget.Children)
            {
                RenderWidget(output, fgColors, bgColors, child, contentX, contentY, contentWidth, contentHeight, scrollX, scrollY);
            }

            // Render scrollbars AFTER children so they're always on top
            if (widget.Scrollable && widget.Children.Count > 0)
            {
                // Calculate total content dimensions by finding max child positions
                int maxChildBottom = 0;
                int maxChildRight = 0;
                foreach (var child in widget.Children)
                {
                    int childPosY = ParseSize(child.PositionY, contentHeight);
                    int childHeight = ParseSize(child.Height, contentHeight);
                    maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);

                    int childPosX = ParseSize(child.PositionX, contentWidth);
                    int childWidth = ParseSize(child.Width, contentWidth);
                    maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
                }

                // Vertical scrollbar
                if (contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0))
                {
                    int scrollbarHeight = Math.Max(1, (contentHeight * contentHeight) / maxChildBottom);
                    int scrollbarPos = maxChildBottom > contentHeight ? (int)((float)scrollY / (maxChildBottom - contentHeight) * (contentHeight - scrollbarHeight)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentHeight - scrollbarHeight));

                    // Position scrollbar inside content area, 1 char from right edge
                    int scrollbarX = contentX + contentWidth - 1;

                    for (int y = 0; y < contentHeight; y++)
                    {
                        int targetY = contentY + y;

                        if (targetY >= 0 && targetY < output.Length &&
                            scrollbarX >= 0 && scrollbarX < output[0].Length &&
                            scrollbarX >= parentX && targetY >= parentY &&
                            scrollbarX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            if (y >= scrollbarPos && y < scrollbarPos + scrollbarHeight)
                            {
                                output[targetY][scrollbarX] = '▐';
                            }
                            else
                            {
                                output[targetY][scrollbarX] = '┊';
                            }
                        }
                    }
                }

                // Horizontal scrollbar
                if (contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0))
                {
                    int scrollbarWidth = Math.Max(1, (contentWidth * contentWidth) / maxChildRight);
                    int scrollbarPos = maxChildRight > contentWidth ? (int)((float)scrollX / (maxChildRight - contentWidth) * (contentWidth - scrollbarWidth)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentWidth - scrollbarWidth));

                    // Position scrollbar inside content area, 1 line from bottom edge
                    int scrollbarY = contentY + contentHeight - 1;

                    for (int x = 0; x < contentWidth; x++)
                    {
                        int targetX = contentX + x;

                        if (scrollbarY >= 0 && scrollbarY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && scrollbarY >= parentY &&
                            targetX < parentX + parentWidth && scrollbarY < parentY + parentHeight)
                        {
                            if (x >= scrollbarPos && x < scrollbarPos + scrollbarWidth)
                            {
                                output[scrollbarY][targetX] = '▄';
                            }
                            else
                            {
                                output[scrollbarY][targetX] = '┈';
                            }
                        }
                    }
                }
            }
        }

        private static int ParseSize(string size, int parentSize)
        {
            if (string.IsNullOrEmpty(size)) return 0;

            size = size.Trim();

            if (size.EndsWith("ch"))
            {
                var value = size[..^2].Trim();
                if (int.TryParse(value, out int result))
                {
                    return result;
                }
                throw new FormatException($"Invalid size value: '{size}'. Expected format: '{{number}}ch' (e.g., '10ch')");
            }
            else if (size.EndsWith('%'))
            {
                var value = size[..^1].Trim();
                if (float.TryParse(value, out float percent))
                {
                    return (int)(parentSize * percent / 100.0f);
                }
                throw new FormatException($"Invalid size value: '{size}'. Expected format: '{{number}}%' (e.g., '50%')");
            }

            // If we reach here, the size string doesn't end with 'ch' or '%'
            throw new FormatException($"Invalid size value: '{size}'. Size must end with 'ch' or '%' (e.g., '10ch' or '50%')");
        }
    }
}