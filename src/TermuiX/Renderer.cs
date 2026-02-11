using System.Text;

namespace TermuiX
{
    /// <summary>
    /// Internal renderer that converts widget trees into character arrays for console display.
    /// </summary>
    internal class Renderer
    {
        private int _width;
        private int _height;

        // Cached frame buffers — reused across frames when size stays the same
        private Rune[][]? _cachedOutput;
        private ConsoleColor[][]? _cachedFg;
        private ConsoleColor[][]? _cachedBg;
        private int _cachedWidth;
        private int _cachedHeight;

        internal void Size(int width, int height)
        {
            _width = width;
            _height = height;
        }

        internal (Rune[][] chars, ConsoleColor[][] fg, ConsoleColor[][] bg) Render(IWidget widget, HitTestMap? hitTestMap = null, bool focusVisible = true)
        {
            if (_width == 0 || _height == 0)
            {
                throw new InvalidOperationException("Renderer size not set. Call Size(width, height) before Render().");
            }

            // Only reallocate when terminal size changes
            if (_cachedOutput == null || _cachedWidth != _width || _cachedHeight != _height)
            {
                _cachedOutput = new Rune[_height][];
                _cachedFg = new ConsoleColor[_height][];
                _cachedBg = new ConsoleColor[_height][];

                for (int i = 0; i < _height; i++)
                {
                    _cachedOutput[i] = new Rune[_width];
                    _cachedFg[i] = new ConsoleColor[_width];
                    _cachedBg[i] = new ConsoleColor[_width];
                }

                _cachedWidth = _width;
                _cachedHeight = _height;
            }

            var output = _cachedOutput;
            var fgColors = _cachedFg!;
            var bgColors = _cachedBg!;

            // Clear buffers for new frame
            for (int i = 0; i < _height; i++)
            {
                Array.Fill(output[i], new Rune(' '));
                Array.Fill(fgColors[i], ConsoleColor.White);
                Array.Fill(bgColors[i], ConsoleColor.Black);
            }

            hitTestMap?.Reset(_width, _height);
            RenderWidget(output, fgColors, bgColors, widget, 0, 0, _width, _height, 0, 0, hitTestMap, focusVisible);

            return (output, fgColors, bgColors);
        }

        private static void RenderWidget(Rune[][] output, ConsoleColor[][] fgColors, ConsoleColor[][] bgColors, IWidget widget, int parentX, int parentY, int parentWidth, int parentHeight, int parentScrollX, int parentScrollY, HitTestMap? hitTestMap, bool focusVisible)
        {
            // Skip invisible widgets and their children
            if (!widget.Visible)
            {
                return;
            }

            int width = ParseSize(widget.Width, parentWidth);
            int height = ParseSize(widget.Height, parentHeight);

            // StackPanel auto-size: compute size from children when Width/Height is "auto"
            if (widget is Widgets.StackPanel autoStack)
            {
                var (autoW, autoH) = ResolveAutoSize(autoStack, parentWidth, parentHeight);
                if (autoW > 0) width = autoW;
                if (autoH > 0) height = autoH;
            }

            // For children inside a StackPanel, use ComputedWidth/Height from GetRaw()
            // instead of ParseSize("100%", parentWidth) which would give the full parent width.
            if (widget.Parent is Widgets.StackPanel)
            {
                if (widget.ComputedWidth > 0) width = widget.ComputedWidth;
                if (widget.ComputedHeight > 0) height = widget.ComputedHeight;
            }

            int posX = ParseSize(widget.PositionX, parentWidth);
            int posY = ParseSize(widget.PositionY, parentHeight);

            posX -= parentScrollX;
            posY -= parentScrollY;

            int marginLeft = ParseSize(widget.MarginLeft, parentWidth);
            int marginTop = ParseSize(widget.MarginTop, parentHeight);

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

            int absX = parentX + posX + marginLeft;
            int absY = parentY + posY + marginTop;

            hitTestMap?.Set(absX, absY, width, height, widget, parentX, parentY, parentWidth, parentHeight);

            if (absX >= parentX + parentWidth || absY >= parentY + parentHeight)
            {
                return;
            }

            if (absX + width < parentX || absY + height < parentY)
            {
                return;
            }

            var raw = widget.GetRaw();

            int contentWidth = Math.Max(0, width - padLeft - padRight);
            int contentHeight = Math.Max(0, height - padTop - padBottom);
            int contentX = absX + padLeft;
            int contentY = absY + padTop;

            int scrollX = widget.Scrollable ? (int)widget.ScrollOffsetX : 0;
            int scrollY = widget.Scrollable ? (int)widget.ScrollOffsetY : 0;

            bool highlighted = (widget.Focussed && focusVisible) || widget.Hovered;
            var bgColor = widget.Disabled && widget.DisabledBackgroundColor.HasValue ? widget.DisabledBackgroundColor.Value :
                          (highlighted ? widget.FocusBackgroundColor : widget.BackgroundColor);
            var fgColor = widget.Disabled ? widget.DisabledForegroundColor :
                          (highlighted ? widget.FocusForegroundColor : widget.ForegroundColor);
            for (int y = 0; y < height && absY + y < output.Length; y++)
            {
                for (int x = 0; x < width && absX + x < output[0].Length; x++)
                {
                    if (absX + x >= 0 && absY + y >= 0 && absX + x >= parentX && absY + y >= parentY && absX + x < parentX + parentWidth && absY + y < parentY + parentHeight)
                    {
                        output[absY + y][absX + x] = new Rune(' ');
                        bgColors[absY + y][absX + x] = bgColor;
                        fgColors[absY + y][absX + x] = fgColor;
                    }
                }
            }

            if (raw?.Length > 0)
            {
                // First render the raw content WITHOUT scroll (for borders, static content)
                for (int y = 0; y < raw.Length && y < height; y++)
                {
                    for (int x = 0; x < raw[y].Length && x < width; x++)
                    {
                        int targetX = absX + x;
                        int targetY = absY + y;

                        if (targetY >= absY && targetY < absY + height &&
                            targetX >= absX && targetX < absX + width &&
                            targetY >= 0 && targetY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && targetY >= parentY &&
                            targetX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            output[targetY][targetX] = raw[y][x];
                        }
                    }
                }
            }

            // Children are positioned relative to content area and affected by scroll
            // Clipping region is the intersection of parent bounds and content area
            int childClipX = Math.Max(parentX, contentX);
            int childClipY = Math.Max(parentY, contentY);
            int childClipRight = Math.Min(parentX + parentWidth, contentX + contentWidth);
            int childClipBottom = Math.Min(parentY + parentHeight, contentY + contentHeight);
            int childClipWidth = Math.Max(0, childClipRight - childClipX);
            int childClipHeight = Math.Max(0, childClipBottom - childClipY);

            // StackPanel layout pass: compute child positions before scrollbar checks
            if (widget is Widgets.StackPanel stackPanel && widget.Children.Count > 0)
            {
                bool isVertical = stackPanel.Direction == Widgets.StackDirection.Vertical;
                var justify = stackPanel.Justify;
                var align = stackPanel.Align;
                bool wrap = stackPanel.Wrap;

                int mainAxisSize = isVertical ? contentHeight : contentWidth;
                int crossAxisSize = isVertical ? contentWidth : contentHeight;

                // Measure all visible children with cumulative % rounding
                var measured = new List<(IWidget child, int mainSize, int crossSize, int mMain0, int mMain1, int mCross0, int mCross1)>();
                float cumulativePercent = 0f;
                int cumulativePixels = 0;

                foreach (var child in widget.Children)
                {
                    if (!child.Visible) continue;
                    child.GetRaw();

                    int mLeft = ParseSize(child.MarginLeft, contentWidth);
                    int mTop = ParseSize(child.MarginTop, contentHeight);
                    int mRight = ParseSize(child.MarginRight, contentWidth);
                    int mBottom = ParseSize(child.MarginBottom, contentHeight);

                    int mMain0 = isVertical ? mTop : mLeft;
                    int mMain1 = isVertical ? mBottom : mRight;
                    int mCross0 = isVertical ? mLeft : mTop;
                    int mCross1 = isVertical ? mRight : mBottom;

                    string sizeStr = isVertical ? child.Height : child.Width;
                    int mainChildSize;

                    // Cumulative rounding for %-based sizes
                    if (sizeStr.TrimEnd().EndsWith('%') && float.TryParse(sizeStr.TrimEnd()[..^1].Trim(), out float pct))
                    {
                        cumulativePercent += pct;
                        int newCumulative = (int)(mainAxisSize * cumulativePercent / 100.0f);
                        mainChildSize = newCumulative - cumulativePixels;
                        cumulativePixels = newCumulative;
                    }
                    else
                    {
                        mainChildSize = isVertical
                            ? (child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight))
                            : (child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth));
                    }

                    int crossChildSize = isVertical
                        ? (child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth))
                        : (child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight));

                    // Store corrected sizes
                    if (isVertical)
                        child.ComputedHeight = mainChildSize;
                    else
                        child.ComputedWidth = mainChildSize;

                    measured.Add((child, mainChildSize, crossChildSize, mMain0, mMain1, mCross0, mCross1));
                }

                // Split into lines (wrap) or single line (no wrap)
                var lines = new List<List<int>>(); // indices into measured
                var lineMainSizes = new List<int>();
                var lineCrossSizes = new List<int>();

                var currentLine = new List<int>();
                int currentMainUsed = 0;
                int currentMaxCross = 0;

                for (int i = 0; i < measured.Count; i++)
                {
                    var m = measured[i];
                    int totalMain = m.mMain0 + m.mainSize + m.mMain1;
                    int totalCross = m.mCross0 + m.crossSize + m.mCross1;

                    if (wrap && currentLine.Count > 0 && currentMainUsed + totalMain > mainAxisSize)
                    {
                        // Finish current line, start new one
                        lines.Add(currentLine);
                        lineMainSizes.Add(currentMainUsed);
                        lineCrossSizes.Add(currentMaxCross);

                        currentLine = new List<int>();
                        currentMainUsed = 0;
                        currentMaxCross = 0;
                    }

                    currentLine.Add(i);
                    currentMainUsed += totalMain;
                    if (totalCross > currentMaxCross) currentMaxCross = totalCross;
                }

                if (currentLine.Count > 0)
                {
                    lines.Add(currentLine);
                    lineMainSizes.Add(currentMainUsed);
                    lineCrossSizes.Add(currentMaxCross);
                }

                // Position each line
                int crossOffset = 0;
                for (int lineIdx = 0; lineIdx < lines.Count; lineIdx++)
                {
                    var line = lines[lineIdx];
                    int lineMainUsed = lineMainSizes[lineIdx];
                    int lineMaxCross = lineCrossSizes[lineIdx];
                    int lineCount = line.Count;

                    // Calculate justify spacing for this line
                    int freeSpace = Math.Max(0, mainAxisSize - lineMainUsed);
                    int startOff = 0;
                    int spacing = 0;
                    int remainder = 0;

                    switch (justify)
                    {
                        case Widgets.StackJustify.Start:
                            break;
                        case Widgets.StackJustify.End:
                            startOff = freeSpace;
                            break;
                        case Widgets.StackJustify.Center:
                            startOff = freeSpace / 2;
                            break;
                        case Widgets.StackJustify.SpaceBetween:
                            if (lineCount > 1)
                            {
                                spacing = freeSpace / (lineCount - 1);
                                remainder = freeSpace % (lineCount - 1);
                            }
                            break;
                        case Widgets.StackJustify.SpaceAround:
                            if (lineCount > 0)
                            {
                                spacing = freeSpace / lineCount;
                                remainder = freeSpace % lineCount;
                                startOff = spacing / 2;
                            }
                            break;
                        case Widgets.StackJustify.SpaceEvenly:
                            if (lineCount > 0)
                            {
                                spacing = freeSpace / (lineCount + 1);
                                remainder = freeSpace % (lineCount + 1);
                                startOff = spacing;
                            }
                            break;
                    }

                    // Position children in this line
                    int mainOffset = startOff;
                    for (int li = 0; li < line.Count; li++)
                    {
                        var m = measured[line[li]];
                        var child = m.child;

                        if (isVertical)
                        {
                            child.PositionY = $"{mainOffset}ch";
                            mainOffset += m.mMain0 + m.mainSize + m.mMain1;

                            // Cross-axis (X) position
                            int crossAvail = wrap ? lineMaxCross : crossAxisSize;
                            int crossFree = Math.Max(0, crossAvail - m.crossSize - m.mCross0 - m.mCross1);
                            int crossPos = align switch
                            {
                                Widgets.StackAlign.Center => m.mCross0 + (crossFree / 2),
                                Widgets.StackAlign.End => m.mCross0 + crossFree,
                                _ => 0
                            };
                            child.PositionX = $"{crossOffset + crossPos}ch";
                        }
                        else
                        {
                            child.PositionX = $"{mainOffset}ch";
                            mainOffset += m.mMain0 + m.mainSize + m.mMain1;

                            // Cross-axis (Y) position
                            int crossAvail = wrap ? lineMaxCross : crossAxisSize;
                            int crossFree = Math.Max(0, crossAvail - m.crossSize - m.mCross0 - m.mCross1);
                            int crossPos = align switch
                            {
                                Widgets.StackAlign.Center => m.mCross0 + (crossFree / 2),
                                Widgets.StackAlign.End => m.mCross0 + crossFree,
                                _ => 0
                            };
                            child.PositionY = $"{crossOffset + crossPos}ch";
                        }

                        // Justify spacing after each child
                        if (justify != Widgets.StackJustify.Start && justify != Widgets.StackJustify.End && justify != Widgets.StackJustify.Center)
                        {
                            bool isLast = li == line.Count - 1;
                            if (justify == Widgets.StackJustify.SpaceBetween && isLast)
                                continue;

                            mainOffset += spacing;
                            if (remainder > 0)
                            {
                                mainOffset++;
                                remainder--;
                            }
                        }
                    }

                    crossOffset += lineMaxCross;
                }
            }

            // Check if scrollbars will be rendered (need to check before rendering children)
            bool willRenderVerticalScrollbar = false;
            bool willRenderHorizontalScrollbar = false;

            if (widget.Scrollable && widget.Children.Count > 0)
            {
                int maxChildBottom = 0;
                int maxChildRight = 0;
                foreach (var child in widget.Children)
                {
                    int childPosY = ParseSize(child.PositionY, contentHeight);
                    int childPosX = ParseSize(child.PositionX, contentWidth);

                    // Use already computed sizes if available, otherwise calculate
                    int childHeight = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight);
                    int childWidth = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth);

                    maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
                    maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
                }

                // For wrapping StackPanels, only allow overflow on the cross axis:
                // - Horizontal wrap: content wraps down (vertical overflow only)
                // - Vertical wrap: content wraps right (horizontal overflow only)
                bool suppressHorizontal = false;
                bool suppressVertical = false;
                if (widget is Widgets.StackPanel sp && sp.Wrap)
                {
                    if (sp.Direction == Widgets.StackDirection.Horizontal)
                        suppressHorizontal = true; // wrapping prevents horizontal overflow
                    else
                        suppressVertical = true; // wrapping prevents vertical overflow
                }

                willRenderVerticalScrollbar = !suppressVertical && contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0);
                willRenderHorizontalScrollbar = !suppressHorizontal && contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0);
            }

            // Reduce clip region if scrollbars will be rendered
            if (willRenderVerticalScrollbar)
            {
                childClipWidth = Math.Max(0, childClipWidth - 1);
            }
            if (willRenderHorizontalScrollbar)
            {
                childClipHeight = Math.Max(0, childClipHeight - 1);
            }

            // Adjust scroll to account for content offset relative to clip region
            int childScrollX = scrollX + (childClipX - contentX);
            int childScrollY = scrollY + (childClipY - contentY);

            foreach (var child in widget.Children)
            {
                RenderWidget(output, fgColors, bgColors, child, childClipX, childClipY, childClipWidth, childClipHeight, childScrollX, childScrollY, hitTestMap, focusVisible);
            }

            // Render scrollbars AFTER children so they're always on top
            if (widget.Scrollable && widget.Children.Count > 0)
            {
                int maxChildBottom = 0;
                int maxChildRight = 0;
                foreach (var child in widget.Children)
                {
                    int childPosY = ParseSize(child.PositionY, contentHeight);
                    int childPosX = ParseSize(child.PositionX, contentWidth);

                    // Use already computed sizes if available, otherwise calculate
                    int childHeight = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight);
                    int childWidth = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth);

                    maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
                    maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
                }

                // Reset scrollbar flags before checking
                widget.HasVerticalScrollbar = false;
                widget.HasHorizontalScrollbar = false;

                // For wrapping StackPanels, only allow overflow on the cross axis
                bool renderSuppressH = false;
                bool renderSuppressV = false;
                if (widget is Widgets.StackPanel spRender && spRender.Wrap)
                {
                    if (spRender.Direction == Widgets.StackDirection.Horizontal)
                        renderSuppressH = true;
                    else
                        renderSuppressV = true;
                }

                if (!renderSuppressV && contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0))
                {
                    // Set flag indicating vertical scrollbar is rendered in this frame
                    widget.HasVerticalScrollbar = true;

                    int scrollbarHeight = Math.Max(1, (contentHeight * contentHeight) / maxChildBottom);
                    int scrollbarPos = maxChildBottom > contentHeight ? (int)((float)scrollY / (maxChildBottom - contentHeight) * (contentHeight - scrollbarHeight)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentHeight - scrollbarHeight));

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
                                output[targetY][scrollbarX] = new Rune('▐');
                            }
                            else
                            {
                                output[targetY][scrollbarX] = new Rune('┊');
                            }
                        }
                    }
                }

                if (!renderSuppressH && contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0))
                {
                    // Set flag indicating horizontal scrollbar is rendered in this frame
                    widget.HasHorizontalScrollbar = true;

                    int scrollbarWidth = Math.Max(1, (contentWidth * contentWidth) / maxChildRight);
                    int scrollbarPos = maxChildRight > contentWidth ? (int)((float)scrollX / (maxChildRight - contentWidth) * (contentWidth - scrollbarWidth)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentWidth - scrollbarWidth));

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
                                output[scrollbarY][targetX] = new Rune('▄');
                            }
                            else
                            {
                                output[scrollbarY][targetX] = new Rune('┈');
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively resolves auto-sized dimensions for a StackPanel by measuring its children.
        /// Returns (width, height) where 0 means "not auto / use ParseSize result".
        /// </summary>
        private static (int width, int height) ResolveAutoSize(Widgets.StackPanel stack, int parentWidth, int parentHeight)
        {
            bool autoW = stack.Width.Equals("auto", StringComparison.OrdinalIgnoreCase);
            bool autoH = stack.Height.Equals("auto", StringComparison.OrdinalIgnoreCase);

            if (!autoW && !autoH)
            {
                return (0, 0);
            }

            bool isVert = stack.Direction == Widgets.StackDirection.Vertical;
            bool doWrap = stack.Wrap;

            // Measure all visible children
            var items = new List<(int mainSize, int crossSize)>();
            foreach (var child in ((IWidget)stack).Children)
            {
                if (!child.Visible) continue;

                // Recursively resolve nested auto-sized StackPanels first
                if (child is Widgets.StackPanel childStack)
                {
                    var (cAutoW, cAutoH) = ResolveAutoSize(childStack, parentWidth, parentHeight);
                    if (cAutoW > 0) child.ComputedWidth = cAutoW;
                    if (cAutoH > 0) child.ComputedHeight = cAutoH;
                }

                // Measure the child's actual rendered dimensions from GetRaw()
                var childRaw = child.GetRaw();
                int cw = 0, ch = 0;
                if (childRaw != null && childRaw.Length > 0)
                {
                    ch = childRaw.Length;
                    for (int i = 0; i < childRaw.Length; i++)
                    {
                        if (childRaw[i] != null && childRaw[i].Length > cw)
                            cw = childRaw[i].Length;
                    }
                }
                if (cw == 0)
                    cw = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, parentWidth);
                if (ch == 0)
                    ch = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, parentHeight);

                child.ComputedWidth = cw;
                child.ComputedHeight = ch;

                int mLeft = ParseSize(child.MarginLeft, 0);
                int mTop = ParseSize(child.MarginTop, 0);
                int mRight = ParseSize(child.MarginRight, 0);
                int mBottom = ParseSize(child.MarginBottom, 0);

                int mainTotal = isVert ? (mTop + ch + mBottom) : (mLeft + cw + mRight);
                int crossTotal = isVert ? (cw + mLeft + mRight) : (ch + mTop + mBottom);
                items.Add((mainTotal, crossTotal));
            }

            int border = stack.HasBorder ? 2 : 0;
            int padH = ParseSize(stack.PaddingLeft, 0) + ParseSize(stack.PaddingRight, 0);
            int padV = ParseSize(stack.PaddingTop, 0) + ParseSize(stack.PaddingBottom, 0);

            int totalStack = 0;
            int maxCross = 0;

            if (doWrap)
            {
                // For wrap: need the fixed main-axis size to know where to break
                int mainAxisLimit = isVert
                    ? (autoH ? int.MaxValue : ParseSize(stack.Height, parentHeight) - border - padV)
                    : (autoW ? int.MaxValue : ParseSize(stack.Width, parentWidth) - border - padH);

                int lineMain = 0;
                int lineMaxCross = 0;
                int maxLineMain = 0;
                int totalCross = 0;

                foreach (var item in items)
                {
                    if (lineMain > 0 && lineMain + item.mainSize > mainAxisLimit)
                    {
                        // Finish line
                        if (lineMain > maxLineMain) maxLineMain = lineMain;
                        totalCross += lineMaxCross;
                        lineMain = 0;
                        lineMaxCross = 0;
                    }

                    lineMain += item.mainSize;
                    if (item.crossSize > lineMaxCross) lineMaxCross = item.crossSize;
                }

                // Last line
                if (lineMain > maxLineMain) maxLineMain = lineMain;
                totalCross += lineMaxCross;

                totalStack = maxLineMain;
                maxCross = totalCross;
            }
            else
            {
                // No wrap: simple sum/max
                foreach (var item in items)
                {
                    totalStack += item.mainSize;
                    if (item.crossSize > maxCross) maxCross = item.crossSize;
                }
            }

            int width = 0;
            int height = 0;

            if (isVert)
            {
                if (autoH) height = totalStack + border + padV;
                if (autoW) width = maxCross + border + padH;
            }
            else
            {
                if (autoW) width = totalStack + border + padH;
                if (autoH) height = maxCross + border + padV;
            }

            // Store computed values so GetRaw() and external code can use them
            ((IWidget)stack).ComputedWidth = width > 0 ? width : ParseSize(stack.Width, parentWidth);
            ((IWidget)stack).ComputedHeight = height > 0 ? height : ParseSize(stack.Height, parentHeight);

            return (width, height);
        }

        private static int ParseSize(string size, int parentSize)
        {
            if (string.IsNullOrEmpty(size))
            {
                return 0;
            }

            size = size.Trim();

            if (size.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

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

            throw new FormatException($"Invalid size value: '{size}'. Size must end with 'ch', '%', or be 'auto' (e.g., '10ch', '50%', 'auto')");
        }
    }
}
