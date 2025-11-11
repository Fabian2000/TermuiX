namespace Termui
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

            // Render content in FULL widget area (including padding area for borders)
            if (raw?.Length > 0)
            {
                int currentX = 0;
                int currentY = 0;

                for (int srcY = scrollY; srcY < raw.Length && currentY < height; srcY++)
                {
                    if (srcY < 0) continue;

                    for (int srcX = scrollX; srcX < raw[srcY].Length; srcX++)
                    {
                        if (srcX < 0) continue;

                        int targetX = absX + currentX;
                        int targetY = absY + currentY;

                        // Check if we hit the widget width boundary
                        if (currentX >= width)
                        {
                            if (widget.AllowWrapping)
                            {
                                currentX = 0;
                                currentY++;
                                if (currentY >= height) break;
                                targetX = absX;
                                targetY = absY + currentY;
                            }
                            else
                            {
                                break; // Stop at edge if no wrapping
                            }
                        }

                        // Clip to widget area and parent bounds
                        if (targetY >= absY && targetY < absY + height &&
                            targetX >= absX && targetX < absX + width &&
                            targetY >= 0 && targetY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && targetY >= parentY &&
                            targetX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            output[targetY][targetX] = raw[srcY][srcX];
                            // Colors are already set by background rendering
                        }

                        currentX++;
                    }

                    // Move to next line after each source line (if not wrapping mid-line)
                    currentX = 0;
                    currentY++;
                }

                // Render scrollbar if scrollable and content is larger than visible area
                if (widget.Scrollable && raw.Length > contentHeight && contentWidth > 0)
                {
                    int scrollbarHeight = Math.Max(1, (contentHeight * contentHeight) / raw.Length);
                    int scrollbarPos = raw.Length > contentHeight ? (int)((float)scrollY / (raw.Length - contentHeight) * (contentHeight - scrollbarHeight)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentHeight - scrollbarHeight));

                    for (int y = 0; y < contentHeight; y++)
                    {
                        int targetY = contentY + y;
                        int targetX = contentX + contentWidth - 1;

                        if (targetY >= 0 && targetY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && targetY >= parentY &&
                            targetX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            if (y >= scrollbarPos && y < scrollbarPos + scrollbarHeight)
                            {
                                output[targetY][targetX] = '█';
                            }
                            else
                            {
                                output[targetY][targetX] = '│';
                            }
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