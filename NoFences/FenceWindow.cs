using NoFences.Model;
using NoFences.Util;
using NoFences.Win32;
using Peter;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static NoFences.Win32.WindowUtil;

namespace NoFences
{
    public partial class FenceWindow : Form
    {
        private int logicalTitleHeight;
        private int titleHeight;
        private const int titleOffset = 3;
        private const int itemWidth = 75;
        private const int itemHeight = 32 + itemPadding + textHeight;
        private const int textHeight = 35;
        private const int itemPadding = 15;
        private const float shadowDist = 1.5f;

        private readonly FenceInfo fenceInfo;
        private readonly Logger logger;

        private Font titleFont;
        private Font iconFont;

        private string selectedItem;
        private string hoveringItem;
        private bool shouldUpdateSelection;
        private bool shouldRunDoubleClick;
        private bool hasSelectionUpdated;
        private bool hasHoverUpdated;
        private bool isMinified;
        private int prevHeight;

        private int scrollHeight;
        private int scrollOffset;

        // New fields for transparency and autohide
        private bool isAutoHidden = false;
        private Timer autoHideTimer;
        private double normalOpacity = 1.0;
        private bool isMouseInside = false;

        private readonly ThrottledExecution throttledMove = new ThrottledExecution(TimeSpan.FromSeconds(4));
        private readonly ThrottledExecution throttledResize = new ThrottledExecution(TimeSpan.FromSeconds(4));

        private readonly ShellContextMenu shellContextMenu = new ShellContextMenu();

        private readonly ThumbnailProvider thumbnailProvider = new ThumbnailProvider();

        private void ReloadFonts()
        {
            var family = new FontFamily("Segoe UI");
            titleFont = new Font(family, (int)Math.Floor(logicalTitleHeight / 2.0));
            iconFont = new Font(family, 9);
        }

        public FenceWindow(FenceInfo fenceInfo)
        {
            logger = Logger.Instance;
            logger.Debug($"Creating fence window for '{fenceInfo.Name}'", "FenceWindow");
            
            InitializeComponent();
            DropShadow.ApplyShadows(this);
            BlurUtil.EnableBlur(Handle);
            WindowUtil.HideFromAltTab(Handle);
            DesktopUtil.GlueToDesktop(Handle);
            //DesktopUtil.PreventMinimize(Handle);
            logicalTitleHeight = (fenceInfo.TitleHeight < 16 || fenceInfo.TitleHeight > 100) ? 35 : fenceInfo.TitleHeight;
            titleHeight = LogicalToDeviceUnits(logicalTitleHeight);
            
            this.MouseWheel += FenceWindow_MouseWheel;
            thumbnailProvider.IconThumbnailLoaded += ThumbnailProvider_IconThumbnailLoaded;

            ReloadFonts();

            AllowDrop = true;

            this.fenceInfo = fenceInfo;
            Text = fenceInfo.Name;
            Location = new Point(fenceInfo.PosX, fenceInfo.PosY);

            Width = fenceInfo.Width;
            Height = fenceInfo.Height;

            prevHeight = Height;
            lockedToolStripMenuItem.Checked = fenceInfo.Locked;
            minifyToolStripMenuItem.Checked = fenceInfo.CanMinify;

            // Initialize transparency and autohide
            SetTransparency(fenceInfo.Transparency);
            InitializeAutoHide();
            
            Minify();
            
            logger.Info($"Fence window '{fenceInfo.Name}' created successfully at ({fenceInfo.PosX}, {fenceInfo.PosY})", "FenceWindow");
        }

        private void fenceSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FenceManager.Instance.ShowFenceSettings(fenceInfo);
        }

        private void globalSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FenceManager.Instance.ShowGlobalSettings();
        }

        // Add methods for external control from FenceManager
        public void UpdateAutoHideState()
        {
            if (fenceInfo.AutoHide)
            {
                StartAutoHideTimer();
            }
            else
            {
                ShowFence();
                StopAutoHideTimer();
            }
        }

        public void ApplySettings()
        {
            // Apply transparency
            SetTransparency(fenceInfo.Transparency);

            // Apply auto-hide settings
            autoHideTimer.Interval = fenceInfo.AutoHideDelay;
            UpdateAutoHideState();

            // Apply other settings
            lockedToolStripMenuItem.Checked = fenceInfo.Locked;
            minifyToolStripMenuItem.Checked = fenceInfo.CanMinify;

            // Update title and size if changed
            Text = fenceInfo.Name;
            Width = fenceInfo.Width;
            Height = fenceInfo.Height;

            Refresh();
            Save();
        }

        private void InitializeAutoHide()
        {
            autoHideTimer = new Timer();
            autoHideTimer.Interval = fenceInfo.AutoHideDelay;
            autoHideTimer.Tick += AutoHideTimer_Tick;
        }

        private void SetTransparency(int transparencyPercent)
        {
            // Clamp transparency between 25 and 100
            transparencyPercent = Math.Max(25, Math.Min(100, transparencyPercent));
            fenceInfo.Transparency = transparencyPercent;
            
            normalOpacity = transparencyPercent / 100.0;
            if (!isAutoHidden)
            {
                this.Opacity = normalOpacity;
            }
            
            Save();
        }

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            if (fenceInfo.AutoHide && !isMouseInside && !isMinified)
            {
                HideFence();
            }
            autoHideTimer.Stop();
        }

        private void HideFence()
        {
            if (!isAutoHidden)
            {
                isAutoHidden = true;
                this.Opacity = 0.1; // Nearly invisible but still responsive to mouse
            }
        }

        private void ShowFence()
        {
            if (isAutoHidden)
            {
                isAutoHidden = false;
                this.Opacity = normalOpacity;
            }
        }

        private void StartAutoHideTimer()
        {
            if (fenceInfo.AutoHide && !isAutoHidden)
            {
                autoHideTimer.Stop();
                autoHideTimer.Start();
            }
        }

        private void StopAutoHideTimer()
        {
            autoHideTimer.Stop();
        }

        protected override void WndProc(ref Message m)
        {
            // Remove border
            if (m.Msg == 0x0083)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            // Mouse leave
            var myrect = new Rectangle(Location, Size);
            if (m.Msg == 0x02a2 && !myrect.IntersectsWith(new Rectangle(MousePosition, new Size(1, 1))))
            {
                Minify();
            }

            // Prevent maximize
            if ((m.Msg == WM_SYSCOMMAND) && m.WParam.ToInt32() == 0xF032)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            // Prevent foreground
            if (m.Msg == WM_SETFOCUS)
            {
                SetWindowPos(Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                return;
            }

            // Other messages
            base.WndProc(ref m);

            // If not locked and using the left mouse button
            if (MouseButtons == MouseButtons.Right || lockedToolStripMenuItem.Checked)
                return;

            // Then, allow dragging and resizing
            if (m.Msg == WM_NCHITTEST)
            {

                var pt = PointToClient(new Point(m.LParam.ToInt32()));

                if ((int)m.Result == HTCLIENT && pt.Y < titleHeight)     // drag the form
                {
                    m.Result = (IntPtr)HTCAPTION;
                    FenceWindow_MouseEnter(null, null);
                }

                if (pt.X < 10 && pt.Y < 10)
                    m.Result = new IntPtr(HTTOPLEFT);
                else if (pt.X > (Width - 10) && pt.Y < 10)
                    m.Result = new IntPtr(HTTOPRIGHT);
                else if (pt.X < 10 && pt.Y > (Height - 10))
                    m.Result = new IntPtr(HTBOTTOMLEFT);
                else if (pt.X > (Width - 10) && pt.Y > (Height - 10))
                    m.Result = new IntPtr(HTBOTTOMRIGHT);
                else if (pt.Y > (Height - 10))
                    m.Result = new IntPtr(HTBOTTOM);
                else if (pt.X < 10)
                    m.Result = new IntPtr(HTLEFT);
                else if (pt.X > (Width - 10))
                    m.Result = new IntPtr(HTRIGHT);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle keyboard shortcuts
            if (keyData == (Keys.Control | Keys.Alt | Keys.T))
            {
                ToggleTransparency();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Alt | Keys.S))
            {
                ShowAllFences();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ToggleTransparency()
        {
            // Cycle through transparency levels: 100 -> 75 -> 50 -> 25 -> 100
            int newTransparency;
            switch (fenceInfo.Transparency)
            {
                case 100:
                    newTransparency = 75;
                    break;
                case 75:
                    newTransparency = 50;
                    break;
                case 50:
                    newTransparency = 25;
                    break;
                default:
                    newTransparency = 100;
                    break;
            }
            SetTransparency(newTransparency);
        }

        private void ShowAllFences()
        {
            // This will be implemented in FenceManager
            FenceManager.Instance.ShowAllFences();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Really remove this fence?", "Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FenceManager.Instance.RemoveFence(fenceInfo);
                Close();
            }
        }

        private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.Files.Remove(hoveringItem);
            hoveringItem = null;
            Save();
            Refresh();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            deleteItemToolStripMenuItem.Visible = hoveringItem != null;
        }

        private void FenceWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // This handles the KeyDown event for the form
            // ProcessCmdKey already handles our shortcuts, but this can be used for other keys
        }

        private void FenceWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !lockedToolStripMenuItem.Checked)
                e.Effect = DragDropEffects.Move;
        }

        private void FenceWindow_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
                var addedFiles = 0;
                
                logger.Debug($"Processing {dropped.Length} dropped files", "FenceWindow");
                
                foreach (var file in dropped)
                {
                    if (!fenceInfo.Files.Contains(file) && ItemExists(file))
                    {
                        fenceInfo.Files.Add(file);
                        addedFiles++;
                        logger.Debug($"Added file to fence: {file}", "FenceWindow");
                    }
                    else
                    {
                        logger.Debug($"Skipped file (already exists or invalid): {file}", "FenceWindow");
                    }
                }
                
                if (addedFiles > 0)
                {
                    logger.Info($"Added {addedFiles} files to fence '{fenceInfo.Name}'", "FenceWindow");
                    Save();
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to process dropped files for fence '{fenceInfo.Name}'", "FenceWindow", ex);
            }
        }

        private void FenceWindow_Resize(object sender, EventArgs e)
        {
            throttledResize.Run(() =>
            {
                fenceInfo.Width = Width;
                fenceInfo.Height = isMinified ? prevHeight : Height;
                Save();
            });

            Refresh();
        }

        private void FenceWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Refresh();
        }

        private void FenceWindow_MouseEnter(object sender, EventArgs e)
        {
            isMouseInside = true;
            StopAutoHideTimer();
            ShowFence();

            if (minifyToolStripMenuItem.Checked && isMinified)
            {
                isMinified = false;
                Height = prevHeight;
            }
        }

        private void FenceWindow_MouseLeave(object sender, EventArgs e)
        {
            isMouseInside = false;
            StartAutoHideTimer();
            Minify();
            selectedItem = null;
            Refresh();
        }

        private void Minify()
        {
            if (minifyToolStripMenuItem.Checked && !isMinified)
            {
                isMinified = true;
                prevHeight = Height;
                Height = titleHeight;
                Refresh();
            }
        }

        private void minifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isMinified)
            {
                Height = prevHeight;
                isMinified = false;
            }
            fenceInfo.CanMinify = minifyToolStripMenuItem.Checked;
            Save();
        }

        private void FenceWindow_Click(object sender, EventArgs e)
        {
            shouldUpdateSelection = true;
            Refresh();
        }

        private void FenceWindow_DoubleClick(object sender, EventArgs e)
        {
            shouldRunDoubleClick = true;
            Refresh();
        }

        private void FenceWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clip = new Region(ClientRectangle);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Background
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Black)), ClientRectangle);

            // Title
            e.Graphics.DrawString(Text, titleFont, Brushes.White, new PointF(Width / 2, titleOffset), new StringFormat { Alignment = StringAlignment.Center });
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, Color.Black)), new RectangleF(0, 0, Width, titleHeight));

            // Items
            var x = itemPadding;
            var y = itemPadding;
            scrollHeight = 0;
            e.Graphics.Clip = new Region(new Rectangle(0, titleHeight, Width, Height - titleHeight));
            foreach (var file in fenceInfo.Files)
            {
                var entry = FenceEntry.FromPath(file);
                if (entry == null)
                    continue;

                RenderEntry(e.Graphics, entry, x, y + titleHeight - scrollOffset);

                var itemBottom = y + itemHeight;
                if (itemBottom > scrollHeight)
                    scrollHeight = itemBottom;

                x += itemWidth + itemPadding;
                if (x + itemWidth > Width)
                {
                    x = itemPadding;
                    y += itemHeight + itemPadding;
                }
            }

            scrollHeight -= (ClientRectangle.Height - titleHeight);

            // Scroll bars
            if (scrollHeight > 0)
            {
                var contentHeight = Height - titleHeight;
                var scrollbarHeight = contentHeight - scrollHeight;
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Black)), new Rectangle(Width - 5, titleHeight + scrollOffset, 5, scrollbarHeight));

                scrollOffset = Math.Min(scrollOffset, scrollHeight);
            }

            // Click handlers
            if (shouldUpdateSelection && !hasSelectionUpdated)
                selectedItem = null;

            if (!hasHoverUpdated)
                hoveringItem = null;

            shouldRunDoubleClick = false;
            shouldUpdateSelection = false;
            hasSelectionUpdated = false;
            hasHoverUpdated = false;
        }

        private void RenderEntry(Graphics g, FenceEntry entry, int x, int y)
        {
            var icon = entry.ExtractIcon(thumbnailProvider);
            var name = entry.Name;

            var textPosition = new PointF(x, y + icon.Height + 5);
            var textMaxSize = new SizeF(itemWidth, textHeight);

            var stringFormat = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };

            var textSize = g.MeasureString(name, iconFont, textMaxSize, stringFormat);
            var outlineRect = new Rectangle(x - 2, y - 2, itemWidth + 2, icon.Height + (int)textSize.Height + 5 + 2);
            var outlineRectInner = outlineRect.Shrink(1);

            var mousePos = PointToClient(MousePosition);
            var mouseOver = mousePos.X >= x && mousePos.Y >= y && mousePos.X < x + outlineRect.Width && mousePos.Y < y + outlineRect.Height;

            if (mouseOver)
            {
                hoveringItem = entry.Path;
                hasHoverUpdated = true;
            }

            if (mouseOver && shouldUpdateSelection)
            {
                selectedItem = entry.Path;
                shouldUpdateSelection = false;
                hasSelectionUpdated = true;
            }

            if (mouseOver && shouldRunDoubleClick)
            {
                shouldRunDoubleClick = false;
                entry.Open();
            }

            if (selectedItem == entry.Path)
            {
                if (mouseOver)
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(120, SystemColors.ActiveBorder)), outlineRectInner);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(100, SystemColors.GradientActiveCaption)), outlineRect);
                }
                else
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(120, SystemColors.ActiveBorder)), outlineRectInner);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(80, SystemColors.GradientInactiveCaption)), outlineRect);
                }
            }
            else
            {
                if (mouseOver)
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(120, SystemColors.ActiveBorder)), outlineRectInner);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(80, SystemColors.ActiveCaption)), outlineRect);
                }
            }

            g.DrawIcon(icon, x + itemWidth / 2 - icon.Width / 2, y);
            g.DrawString(name, iconFont, new SolidBrush(Color.FromArgb(180, 15, 15, 15)), new RectangleF(textPosition.Move(shadowDist, shadowDist), textMaxSize), stringFormat);
            g.DrawString(name, iconFont, Brushes.White, new RectangleF(textPosition, textMaxSize), stringFormat);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new EditDialog(Text);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                Text = dialog.NewName;
                fenceInfo.Name = Text;
                Refresh();
                Save();
            }
        }

        private void newFenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FenceManager.Instance.CreateFence("New fence");
        }

        private void FenceWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms.Count == 0)
                Application.Exit();
        }

        // Add method to expose FenceInfo for manager
        public FenceInfo GetFenceInfo()
        {
            return fenceInfo;
        }

        // Methods for external control
        public void ForceShow()
        {
            ShowFence();
            StopAutoHideTimer();
        }

        public void ForceHide()
        {
            HideFence();
        }

        // Improve the Save method with better error handling
        private readonly object saveLock = new object();
        private void Save()
        {
            lock (saveLock)
            {
                try
                {
                    FenceManager.Instance.UpdateFence(fenceInfo);
                    logger.Debug($"Fence '{fenceInfo.Name}' saved successfully", "FenceWindow");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to save fence '{fenceInfo.Name}'", "FenceWindow", ex);
                }
            }
        }

        private void FenceWindow_LocationChanged(object sender, EventArgs e)
        {
            throttledMove.Run(() =>
            {
                fenceInfo.PosX = Location.X;
                fenceInfo.PosY = Location.Y;
                Save();
            });
        }

        private void lockedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.Locked = lockedToolStripMenuItem.Checked;
            Save();
        }

        private void FenceWindow_Load(object sender, EventArgs e)
        {

        }

        private void titleSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new HeightDialog(fenceInfo.TitleHeight);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                fenceInfo.TitleHeight = dialog.TitleHeight;
                logicalTitleHeight = dialog.TitleHeight;
                titleHeight = LogicalToDeviceUnits(logicalTitleHeight);
                ReloadFonts();
                Minify();
                if (isMinified)
                {
                    Height = titleHeight;
                }
                Refresh();
                Save();
            }
        }

        private void FenceWindow_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (hoveringItem != null && !ModifierKeys.HasFlag(Keys.Shift))
            {
                shellContextMenu.ShowContextMenu(new[] { new FileInfo(hoveringItem) }, MousePosition);
            }
            else
            {
                appContextMenu.Show(this, e.Location);
            }
        }

        private void FenceWindow_MouseWheel(object sender, MouseEventArgs e)
        {
            if (scrollHeight < 1)
                return;

            scrollOffset -= Math.Sign(e.Delta) * 10;
            if (scrollOffset < 0)
                scrollOffset = 0;
            if (scrollOffset > scrollHeight)
                scrollOffset = scrollHeight;

            Invalidate();
        }

        private void ThumbnailProvider_IconThumbnailLoaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        // Add validation for file operations
        private bool ItemExists(string path)
        {
            try
            {
                var exists = File.Exists(path) || Directory.Exists(path);
                if (!exists)
                {
                    logger.Warning($"Item does not exist: {path}", "FenceWindow");
                }
                return exists;
            }
            catch (Exception ex)
            {
                logger.Error($"Error checking if item exists: {path}", "FenceWindow", ex);
                return false;
            }
        }
    }
}

