using System.Text;

namespace TermuiX.Widgets;

/// <summary>
/// A tree view widget for displaying hierarchical data.
/// </summary>
public class TreeView : IWidget
{
    private readonly TreeNode _root = new("Root") { IsExpanded = true };
    private int _selectedIndex = 0;
    private Color _backgroundColor = Color.Inherit;
    private Color _foregroundColor = Color.Inherit;
    private Color[][]? _rawFg;
    private Color[][]? _rawBg;

    /// <summary>
    /// Gets the root node of the tree. Add child nodes to this.
    /// The root node itself is not displayed.
    /// </summary>
    public TreeNode Root => _root;

    /// <summary>
    /// Gets the currently selected node, or null.
    /// </summary>
    public TreeNode? SelectedNode
    {
        get
        {
            var visible = GetVisibleNodes();
            return _selectedIndex >= 0 && _selectedIndex < visible.Count ? visible[_selectedIndex] : null;
        }
    }

    /// <summary>
    /// Programmatically selects the given node (must be visible).
    /// </summary>
    public void SelectNode(TreeNode node)
    {
        var visible = GetVisibleNodes();
        int index = visible.IndexOf(node);
        if (index >= 0)
        {
            _selectedIndex = index;
            ScrollParentToSelectedRow();
        }
    }

    /// <summary>
    /// Gets or sets the unique name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the group name.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public string Width { get; set; } = "30ch";

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public string Height { get; set; } = "auto";

    /// <summary>
    /// Gets or sets the minimum width constraint.
    /// </summary>
    public string MinWidth { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum width constraint.
    /// </summary>
    public string MaxWidth { get; set; } = "";

    /// <summary>
    /// Gets or sets the minimum height constraint.
    /// </summary>
    public string MinHeight { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum height constraint.
    /// </summary>
    public string MaxHeight { get; set; } = "";

    /// <summary>
    /// Gets or sets the left padding.
    /// </summary>
    public string PaddingLeft { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the top padding.
    /// </summary>
    public string PaddingTop { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the right padding.
    /// </summary>
    public string PaddingRight { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the bottom padding.
    /// </summary>
    public string PaddingBottom { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the left margin.
    /// </summary>
    public string MarginLeft { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the top margin.
    /// </summary>
    public string MarginTop { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the right margin.
    /// </summary>
    public string MarginRight { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the bottom margin.
    /// </summary>
    public string MarginBottom { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the X position.
    /// </summary>
    public string PositionX { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets the Y position.
    /// </summary>
    public string PositionY { get; set; } = "0ch";

    /// <summary>
    /// Gets or sets whether the widget is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether text wrapping is allowed.
    /// </summary>
    public bool AllowWrapping { get; set; } = false;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color ForegroundColor
    {
        get => _foregroundColor;
        set => _foregroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color when focused.
    /// </summary>
    public Color FocusBackgroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the foreground color when focused.
    /// </summary>
    public Color FocusForegroundColor { get; set; } = Color.Inherit;

    /// <summary>
    /// Gets or sets the highlight color for the selected node.
    /// </summary>
    public Color HighlightBackgroundColor { get; set; } = ConsoleColor.Gray;

    /// <summary>
    /// Gets or sets the highlight foreground color.
    /// </summary>
    public Color HighlightForegroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets whether this widget can receive focus.
    /// </summary>
    public bool CanFocus => true;

    /// <summary>
    /// Gets whether horizontal scrolling is enabled.
    /// </summary>
    public bool ScrollX => false;

    /// <summary>
    /// Gets whether vertical scrolling is enabled.
    /// </summary>
    public bool ScrollY => false;

    IWidget? IWidget.Parent { get; set; }
    List<IWidget> IWidget.Children => [];
    bool IWidget.Focussed { get; set; }
    bool IWidget.Hovered { get; set; }
    int IWidget.ComputedWidth { get; set; }
    int IWidget.ComputedHeight { get; set; }
    bool IWidget.HasVerticalScrollbar { get; set; }
    bool IWidget.HasHorizontalScrollbar { get; set; }
    long IWidget.ScrollOffsetX { get; set; }
    long IWidget.ScrollOffsetY { get; set; }
    bool IWidget.Disabled { get; set; }
    Color? IWidget.DisabledBackgroundColor { get; set; }
    Color IWidget.DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Occurs when a node is selected.
    /// </summary>
    public event EventHandler<TreeNode>? NodeSelected;

    /// <summary>
    /// Occurs when a node is expanded or collapsed.
    /// </summary>
    public event EventHandler<TreeNode>? NodeToggled;

    Rune[][] IWidget.GetRaw()
    {
        int width = GetWidthInChars();
        if (width < 3) width = 3;

        var visible = GetVisibleNodes();
        int height = Math.Max(1, visible.Count);

        var result = new Rune[height][];
        _rawFg = new Color[height][];
        _rawBg = new Color[height][];

        bool isFocussed = ((IWidget)this).Focussed;
        var normalFg = isFocussed ? FocusForegroundColor : ForegroundColor;
        var normalBg = isFocussed ? FocusBackgroundColor : BackgroundColor;

        for (int i = 0; i < height; i++)
        {
            result[i] = new Rune[width];
            _rawFg[i] = new Color[width];
            _rawBg[i] = new Color[width];
            Array.Fill(result[i], new Rune(' '));
            Array.Fill(_rawFg[i], normalFg);
            Array.Fill(_rawBg[i], normalBg);
        }

        for (int row = 0; row < visible.Count; row++)
        {
            var node = visible[row];
            int depth = GetNodeDepth(node);
            bool isSelected = row == _selectedIndex;

            // Highlight selected row
            if (isSelected)
            {
                Array.Fill(_rawFg[row], HighlightForegroundColor);
                Array.Fill(_rawBg[row], HighlightBackgroundColor);
            }

            // Build the line: indentation + expand indicator + text
            int x = 0;

            // Indentation (2 chars per level)
            for (int d = 0; d < depth && x < width; d++)
            {
                result[row][x] = new Rune(' ');
                if (x + 1 < width) result[row][x + 1] = new Rune(' ');
                x += 2;
            }

            // Expand/collapse indicator or leaf marker
            if (x < width)
            {
                if (node.Children.Count > 0)
                    result[row][x] = node.IsExpanded ? new Rune('▾') : new Rune('▸');
                else
                    result[row][x] = new Rune(' ');
                x++;
            }

            // Node text
            foreach (Rune r in node.Text.EnumerateRunes())
            {
                int rw = Text.GetRuneDisplayWidth(r);
                if (x + rw > width) break;
                result[row][x] = r;
                x += rw;
            }
        }

        // Store actual content size for layout
        ((IWidget)this).ComputedWidth = width;
        ((IWidget)this).ComputedHeight = height;

        return result;
    }

    (Color[][] fg, Color[][] bg)? IWidget.GetRawColors()
    {
        if (_rawFg != null && _rawBg != null)
            return (_rawFg, _rawBg);
        return null;
    }

    void IWidget.KeyPress(ConsoleKeyInfo keyInfo)
    {
        var visible = GetVisibleNodes();
        if (visible.Count == 0) return;

        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                break;
            case ConsoleKey.DownArrow:
                _selectedIndex = Math.Min(visible.Count - 1, _selectedIndex + 1);
                break;
            case ConsoleKey.RightArrow:
                if (_selectedIndex < visible.Count)
                {
                    var node = visible[_selectedIndex];
                    if (node.Children.Count > 0 && !node.IsExpanded)
                    {
                        node.IsExpanded = true;
                        NodeToggled?.Invoke(this, node);
                    }
                    else if (node.Children.Count > 0 && node.IsExpanded)
                    {
                        // Move to first child
                        _selectedIndex = Math.Min(visible.Count - 1, _selectedIndex + 1);
                    }
                }
                break;
            case ConsoleKey.LeftArrow:
                if (_selectedIndex < visible.Count)
                {
                    var node = visible[_selectedIndex];
                    if (node.Children.Count > 0 && node.IsExpanded)
                    {
                        node.IsExpanded = false;
                        NodeToggled?.Invoke(this, node);
                        // Recalculate visible after collapse
                        visible = GetVisibleNodes();
                        _selectedIndex = Math.Min(_selectedIndex, visible.Count - 1);
                    }
                    else if (node.Parent != null && node.Parent != _root)
                    {
                        // Move to parent
                        int parentIdx = visible.IndexOf(node.Parent);
                        if (parentIdx >= 0) _selectedIndex = parentIdx;
                    }
                }
                break;
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                if (_selectedIndex < visible.Count)
                {
                    var node = visible[_selectedIndex];
                    if (node.Children.Count > 0)
                    {
                        node.IsExpanded = !node.IsExpanded;
                        NodeToggled?.Invoke(this, node);
                        visible = GetVisibleNodes();
                        _selectedIndex = Math.Min(_selectedIndex, visible.Count - 1);
                    }
                    NodeSelected?.Invoke(this, node);
                }
                break;
            case ConsoleKey.Home:
                _selectedIndex = 0;
                break;
            case ConsoleKey.End:
                _selectedIndex = visible.Count - 1;
                break;
        }

        ScrollParentToSelectedRow();
    }

    void IWidget.MousePress(MouseEventArgs args)
    {
        if (args.EventType != MouseEventType.LeftButtonPressed) return;

        var visible = GetVisibleNodes();
        int clickedIndex = args.LocalY;
        if (clickedIndex >= 0 && clickedIndex < visible.Count)
        {
            _selectedIndex = clickedIndex;
            var node = visible[clickedIndex];

            // Check if clicked on the expand indicator
            int depth = GetNodeDepth(node);
            int indicatorX = depth * 2;
            if (args.LocalX == indicatorX && node.Children.Count > 0)
            {
                node.IsExpanded = !node.IsExpanded;
                NodeToggled?.Invoke(this, node);
            }
            else
            {
                NodeSelected?.Invoke(this, node);
            }
        }
    }

    private void ScrollParentToSelectedRow()
    {
        // Find the nearest Y-scrollable ancestor and adjust its scroll offset
        // so the selected row stays visible.
        IWidget? scrollParent = ((IWidget)this).Parent;
        while (scrollParent is not null && !scrollParent.ScrollY)
            scrollParent = scrollParent.Parent;

        if (scrollParent is null) return;

        // The selected row's position within the TreeView is simply _selectedIndex (1 row per node).
        // The TreeView's position within the scroll container is PositionY.
        int treePosY = 0;
        if (PositionY.EndsWith("ch") && int.TryParse(PositionY[..^2].Trim(), out int py))
            treePosY = py;

        int rowY = treePosY + _selectedIndex;
        long currentScroll = scrollParent.ScrollOffsetY;

        // Estimate the visible content height of the scroll container
        int containerHeight = scrollParent.ComputedHeight;
        // Subtract padding/border approximation (Container with border = 2 rows)
        if (scrollParent is Container c && c.HasBorder)
            containerHeight -= 2;

        if (containerHeight <= 0) return;

        if (rowY < currentScroll)
            scrollParent.ScrollOffsetY = rowY;
        else if (rowY >= currentScroll + containerHeight)
            scrollParent.ScrollOffsetY = rowY - containerHeight + 1;
    }

    private List<TreeNode> GetVisibleNodes()
    {
        var result = new List<TreeNode>();
        CollectVisibleNodes(_root, result);
        return result;
    }

    private static void CollectVisibleNodes(TreeNode node, List<TreeNode> result)
    {
        // Root children are always shown (root itself is not displayed)
        foreach (var child in node.Children)
        {
            result.Add(child);
            if (child.IsExpanded)
                CollectVisibleNodes(child, result);
        }
    }

    private int GetNodeDepth(TreeNode node)
    {
        int depth = 0;
        var current = node.Parent;
        while (current != null && current != _root)
        {
            depth++;
            current = current.Parent;
        }
        return depth;
    }

    private int GetWidthInChars()
    {
        int computed = ((IWidget)this).ComputedWidth;
        if (computed > 0) return computed;
        if (Width.EndsWith("ch"))
        {
            var value = Width[..^2].Trim();
            if (int.TryParse(value, out int result)) return result;
        }
        return 30;
    }

    /// <summary>
    /// Creates a copy of this tree view.
    /// </summary>
    public IWidget Clone(bool deep = true)
    {
        var clone = new TreeView
        {
            _backgroundColor = _backgroundColor,
            _foregroundColor = _foregroundColor,
            Name = Name,
            Group = Group,
            Width = Width,
            Height = Height,
            MinWidth = MinWidth,
            MaxWidth = MaxWidth,
            MinHeight = MinHeight,
            MaxHeight = MaxHeight,
            PaddingLeft = PaddingLeft,
            PaddingTop = PaddingTop,
            PaddingRight = PaddingRight,
            PaddingBottom = PaddingBottom,
            MarginLeft = MarginLeft,
            MarginTop = MarginTop,
            MarginRight = MarginRight,
            MarginBottom = MarginBottom,
            PositionX = PositionX,
            PositionY = PositionY,
            Visible = Visible,
            AllowWrapping = AllowWrapping,
            FocusBackgroundColor = FocusBackgroundColor,
            FocusForegroundColor = FocusForegroundColor,
            HighlightBackgroundColor = HighlightBackgroundColor,
            HighlightForegroundColor = HighlightForegroundColor
        };

        if (deep)
        {
            foreach (var child in _root.Children)
                clone._root.AddChild(child.Clone());
        }

        return clone;
    }
}

/// <summary>
/// Represents a node in a TreeView.
/// </summary>
public class TreeNode
{
    private readonly List<TreeNode> _children = new();

    /// <summary>
    /// Gets or sets the display text of this node.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets whether this node is expanded.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets the parent node, or null for root-level nodes.
    /// </summary>
    public TreeNode? Parent { get; internal set; }

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    public IReadOnlyList<TreeNode> Children => _children;

    /// <summary>
    /// Gets or sets an optional tag object for storing user data.
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Initializes a new tree node.
    /// </summary>
    public TreeNode(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Adds a child node.
    /// </summary>
    public TreeNode AddChild(string text)
    {
        var child = new TreeNode(text) { Parent = this };
        _children.Add(child);
        return child;
    }

    /// <summary>
    /// Adds an existing node as a child.
    /// </summary>
    public void AddChild(TreeNode child)
    {
        child.Parent = this;
        _children.Add(child);
    }

    /// <summary>
    /// Removes a child node.
    /// </summary>
    public void RemoveChild(TreeNode child)
    {
        child.Parent = null;
        _children.Remove(child);
    }

    /// <summary>
    /// Creates a deep copy of this node and all children.
    /// </summary>
    public TreeNode Clone()
    {
        var clone = new TreeNode(Text) { IsExpanded = IsExpanded, Tag = Tag };
        foreach (var child in _children)
            clone.AddChild(child.Clone());
        return clone;
    }
}
