using RemoteSensingProcessor.BLL;
using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;
using System.Threading.Tasks;

namespace RemoteSensingProcessor.UI
{
    public partial class MainForm : Form
    {
        private ImageInfo? _currentImage;
        private Bitmap? _currentBitmap;
        private readonly ImageCache _cache = new();
        private readonly GdalDataAccess _dal = new();
        private readonly BasicOperations _basicOps = new();
        private readonly VegetationIndices _vegIndices = new();
        private readonly ObjectIndices _objIndices = new();
        private readonly ImageEnhancement _enhancement = new();
        private readonly Classification _classification = new();
        private readonly BandExpressionCalculator _bandExprCalc = new();
        private double _zoom = 1.0;
        private Point _panOffset = Point.Empty;
        private bool _isPanning = false;
        private bool _isProcessing = false;
        private Point _lastMousePos;
        private ToolStripProgressBar? _progressBar;

        public MainForm()
        {
            InitializeComponent();
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.Paint += PictureBox1_Paint;
            this.KeyDown += MainForm_KeyDown;
            this.KeyUp += MainForm_KeyUp;
            SetDoubleBuffered(pictureBox1);
            _progressBar = new ToolStripProgressBar { Size = new Size(140, 16), Visible = false, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30 };
            statusStrip1.Items.Insert(0, _progressBar);
            UpdateHistoryButtons();
        }

        private void SetProcessingState(bool processing, string? message = null)
        {
            _isProcessing = processing;
            Cursor = processing ? Cursors.WaitCursor : Cursors.Default;
            if (_progressBar != null) _progressBar.Visible = processing;
            if (!string.IsNullOrEmpty(message)) toolStripStatusLabel7.Text = message;
            else if (!processing && _currentImage != null) toolStripStatusLabel7.Text = $"文件: {Path.GetFileName(_currentImage.FilePath)}";
            menuStrip1.Enabled = !processing;
            toolStrip1.Enabled = !processing;
            UpdateHistoryButtons();
        }

        private void UpdateHistoryButtons()
        {
            bool enabled = !_isProcessing;
            menuItemUndo.Enabled = enabled && _cache.CanUndo;
            menuItemRedo.Enabled = enabled && _cache.CanRedo;
            toolStripButtonUndo.Enabled = menuItemUndo.Enabled;
            toolStripButtonRedo.Enabled = menuItemRedo.Enabled;
        }

        private void CommitBitmap(Bitmap newBitmap)
        {
            _currentBitmap?.Dispose();
            _currentBitmap = newBitmap;
            _cache.Record(newBitmap);
            _zoom = 1.0;
            _panOffset = Point.Empty;
            pictureBox1.Invalidate();
            UpdateHistoryButtons();
        }

        private void SetDoubleBuffered(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(control, true);
        }

        private void PictureBox1_Paint(object? sender, PaintEventArgs e)
        {
            if (_currentBitmap != null)
            {
                e.Graphics.TranslateTransform(_panOffset.X, _panOffset.Y);
                e.Graphics.ScaleTransform((float)_zoom, (float)_zoom);
                e.Graphics.DrawImage(_currentBitmap, 0, 0);
            }
        }

        private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
        {
            // 支持鼠标中键拖动，或按住空格键+左键拖动
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Space))
            {
                _isPanning = true;
                _lastMousePos = e.Location;
                pictureBox1.Cursor = Cursors.Hand;
            }
        }

        private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _panOffset.X += e.X - _lastMousePos.X;
                _panOffset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                pictureBox1.Invalidate();
            }

            if (_currentImage != null && _currentBitmap != null)
            {
                int pixelX = (int)((e.X - _panOffset.X) / _zoom);
                int pixelY = (int)((e.Y - _panOffset.Y) / _zoom);

                if (pixelX >= 0 && pixelX < _currentImage.Width && pixelY >= 0 && pixelY < _currentImage.Height)
                {
                    var coord = _currentImage.GetCornerCoordinate(pixelY, pixelX);
                    toolStripStatusLabel5.Text = $"坐标: ({coord.X:F4}, {coord.Y:F4})";
                    toolStripStatusLabel6.Text = $"像素: ({pixelX}, {pixelY})";
                }
            }
        }

        private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
        {
            _isPanning = false;
            if (ModifierKeys != Keys.Space)
            {
                pictureBox1.Cursor = Cursors.Default;
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                pictureBox1.Cursor = Cursors.Hand;
            }
        }

        private void MainForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !_isPanning)
            {
                pictureBox1.Cursor = Cursors.Default;
            }
        }

        private void PictureBox1_MouseWheel(object? sender, MouseEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.1 : 0.9;
            _zoom = Math.Clamp(_zoom * factor, 0.1, 10.0);
            pictureBox1.Invalidate();
            toolStripStatusLabel4.Text = $"缩放: {_zoom:P0}";
        }

        private void UpdateStatus()
        {
            if (_currentImage != null)
            {
                toolStripStatusLabel1.Text = $"尺寸: {_currentImage.Width} x {_currentImage.Height}";
                toolStripStatusLabel2.Text = $"波段: {_currentImage.BandCount}";
                toolStripStatusLabel3.Text = $"分辨率: {_currentImage.ResolutionX:F2}m x {_currentImage.ResolutionY:F2}m";
                toolStripStatusLabel7.Text = $"文件: {Path.GetFileName(_currentImage.FilePath)}";
            }
            else
            {
                toolStripStatusLabel1.Text = "尺寸: -";
                toolStripStatusLabel2.Text = "波段: -";
                toolStripStatusLabel3.Text = "分辨率: -";
                toolStripStatusLabel4.Text = "缩放: 100%";
                toolStripStatusLabel5.Text = "坐标: -";
                toolStripStatusLabel6.Text = "像素: -";
                toolStripStatusLabel7.Text = "文件: -";
            }
        }

        private void OpenImage(string filePath)
        {
            try
            {
                _currentImage = _dal.OpenImage(filePath);
                
                Bitmap newBitmap;
                if (_currentImage.BandCount >= 3)
                {
                    newBitmap = _basicOps.RenderTrueColor(_currentImage);
                }
                else
                {
                    newBitmap = _basicOps.RenderGrayscale(_currentImage, 1);
                }
                
                _currentBitmap?.Dispose();
                _currentBitmap = newBitmap;
                _cache.Reset(newBitmap);
                _zoom = 1.0;
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                UpdateStatus();
                UpdateLayerPanel();
                UpdateHistoryButtons();
                MessageBox.Show($"影像加载成功！\n尺寸: {_currentImage.Width} x {_currentImage.Height}\n波段数: {_currentImage.BandCount}", 
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateLayerPanel()
        {
            listBoxBands.Items.Clear();
            if (_currentImage != null)
            {
                for (int i = 0; i < _currentImage.BandCount; i++)
                {
                    listBoxBands.Items.Add($"波段 {_currentImage.Bands[i].Index}: {_currentImage.Bands[i].Name}");
                }
            }
        }

        private void SaveImage()
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog dialog = new();
            dialog.Filter = "GeoTIFF|*.tif|所有文件|*.*";
            dialog.Title = "保存影像";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    float[][] bands = _dal.ReadMultiBandData(_currentImage.FilePath,
                        Enumerable.Range(1, _currentImage.BandCount).ToArray(),
                        0, 0, _currentImage.Width, _currentImage.Height);
                    
                    _dal.SaveImage(dialog.FileName, bands, _currentImage.Width, _currentImage.Height,
                        _currentImage.GeoTransform, _currentImage.Projection);
                    
                    MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowMetadata()
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string corners = string.Join(", ", _currentImage.GetAllCorners().Select(c => $"({c.X:F2}, {c.Y:F2})"));
            string metadata = $"影像信息:\n\n" +
                $"尺寸: {_currentImage.Width} x {_currentImage.Height}\n" +
                $"波段数: {_currentImage.BandCount}\n" +
                $"分辨率: {_currentImage.ResolutionX:F2}m x {_currentImage.ResolutionY:F2}m\n" +
                $"投影: {_currentImage.Projection}\n" +
                $"GeoTransform: {string.Join(", ", _currentImage.GeoTransform.ToArray())}\n" +
                $"四角坐标: {corners}";

            MessageBox.Show(metadata, "影像元数据", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void ApplyOperation(Func<Bitmap, Bitmap> operation, string name)
        {
            if (_currentBitmap == null) { MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_isProcessing) return;
            SetProcessingState(true, $"正在处理: {name}...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                BitmapCapture capture = BitmapHelper.Capture24bpp(_currentBitmap);
                Bitmap result = await Task.Run(() =>
                {
                    using Bitmap input = BitmapHelper.CreateFromCapture(capture);
                    return operation(input);
                }).ConfigureAwait(true);
                CommitBitmap(result);
                sw.Stop();
                SetProcessingState(false, $"{name}完成 ({sw.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                SetProcessingState(false, $"{name}失败");
                MessageBox.Show($"{name}失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ApplyImageOperation(Func<ImageInfo, Bitmap> operation, string name)
        {
            if (_currentImage == null || _currentBitmap == null) { MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_isProcessing) return;
            SetProcessingState(true, $"正在处理: {name}...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ImageInfo info = _currentImage;
            try
            {
                Bitmap result = await Task.Run(() => operation(info)).ConfigureAwait(true);
                CommitBitmap(result);
                sw.Stop();
                SetProcessingState(false, $"{name}完成 ({sw.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                SetProcessingState(false, $"{name}失败");
                MessageBox.Show($"{name}失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new();
            dialog.Filter = "遥感影像|*.tif;*.img;*.tiff|所有文件|*.*";
            dialog.Title = "打开遥感影像";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenImage(dialog.FileName);
            }
        }

        private void menuItemSave_Click(object sender, EventArgs e) => SaveImage();
        private void menuItemExit_Click(object sender, EventArgs e) => Application.Exit();
        private void menuItemMetadata_Click(object sender, EventArgs e) => ShowMetadata();

        private void menuItemTrueColor_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_currentImage.BandCount < 3)
            {
                MessageBox.Show("影像波段数不足3个", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Bitmap newBitmap = _basicOps.RenderTrueColor(_currentImage);
                _currentBitmap?.Dispose();
                _currentBitmap = newBitmap;
                _cache.Record(newBitmap);
                
                _zoom = 1.0;
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                MessageBox.Show("真彩色合成完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"合成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemFalseColor_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_currentImage.BandCount < 4)
            {
                MessageBox.Show("影像波段数不足4个", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Bitmap newBitmap = _basicOps.RenderFalseColor(_currentImage);
                _currentBitmap?.Dispose();
                _currentBitmap = newBitmap;
                _cache.Record(newBitmap);
                
                _zoom = 1.0;
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                MessageBox.Show("标准假彩色合成完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"合成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemCustomBand_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using InputDialog inputDialog = new($"请输入三个波段索引（用逗号分隔，范围1-{_currentImage.BandCount}）\n例如: 4,3,2", "自定义波段组合", "3,2,1");
            if (inputDialog.ShowDialog() == DialogResult.OK)
            {
                string input = inputDialog.InputText;
                try
                {
                    int[] bands = input.Split(',').Select(int.Parse).ToArray();
                    if (bands.Length != 3) throw new ArgumentException("必须输入3个波段");
                    
                    Bitmap newBitmap = _basicOps.RenderBandCombination(_currentImage, bands);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("自定义波段合成完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"输入错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemGrayscale_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using InputDialog inputDialog = new($"请输入波段索引（范围1-{_currentImage.BandCount}）", "灰度化", "1");
            if (inputDialog.ShowDialog() == DialogResult.OK)
            {
                string input = inputDialog.InputText;
                try
                {
                    int band = int.Parse(input);
                    Bitmap newBitmap = _basicOps.RenderGrayscale(_currentImage, band);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("灰度化完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"输入错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemLinearStretch_Click(object sender, EventArgs e) =>
            ApplyImageOperation(info => _basicOps.ApplyLinearStretchFromImage(info), "2%线性拉伸");
        private void menuItemInvert_Click(object sender, EventArgs e) => ApplyOperation(b => _basicOps.Invert(b), "影像取反");

        private void menuItemBrightnessContrast_Click(object sender, EventArgs e)
        {
            if (_currentBitmap == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using InputDialog brightnessDialog = new("请输入亮度值（-255到255）", "亮度调整", "0");
            using InputDialog contrastDialog = new("请输入对比度值（-100到100）", "对比度调整", "0");

            if (brightnessDialog.ShowDialog() == DialogResult.OK && contrastDialog.ShowDialog() == DialogResult.OK)
            {
                int brightness = int.Parse(brightnessDialog.InputText);
                int contrast = int.Parse(contrastDialog.InputText);

                brightness = Math.Clamp(brightness, -255, 255);
                contrast = Math.Clamp(contrast, -100, 100);

                try
                {
                    Bitmap newBitmap = _basicOps.AdjustBrightnessContrast(_currentBitmap, brightness, contrast);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("亮度/对比度调整完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"调整失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemNDVI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultNir = Math.Min(4, _currentImage.BandCount);
            int defaultRed = Math.Min(3, _currentImage.BandCount);

            using InputDialog nirDialog = new($"请输入NIR波段索引（1-{_currentImage.BandCount}）", "NDVI计算 - NIR波段", defaultNir.ToString());
            using InputDialog redDialog = new($"请输入Red波段索引（1-{_currentImage.BandCount}）", "NDVI计算 - Red波段", defaultRed.ToString());

            if (nirDialog.ShowDialog() == DialogResult.OK && redDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int nirBand = int.Parse(nirDialog.InputText);
                    int redBand = int.Parse(redDialog.InputText);

                    if (nirBand < 1 || nirBand > _currentImage.BandCount || redBand < 1 || redBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _vegIndices.CalculateNDVI(_currentImage, nirBand, redBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("NDVI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"NDVI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemEVI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultNir = Math.Min(4, _currentImage.BandCount);
            int defaultRed = Math.Min(3, _currentImage.BandCount);
            int defaultBlue = Math.Min(1, _currentImage.BandCount);

            using InputDialog nirDialog = new($"请输入NIR波段索引（1-{_currentImage.BandCount}）", "EVI计算 - NIR波段", defaultNir.ToString());
            using InputDialog redDialog = new($"请输入Red波段索引（1-{_currentImage.BandCount}）", "EVI计算 - Red波段", defaultRed.ToString());
            using InputDialog blueDialog = new($"请输入Blue波段索引（1-{_currentImage.BandCount}）", "EVI计算 - Blue波段", defaultBlue.ToString());

            if (nirDialog.ShowDialog() == DialogResult.OK && redDialog.ShowDialog() == DialogResult.OK && blueDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int nirBand = int.Parse(nirDialog.InputText);
                    int redBand = int.Parse(redDialog.InputText);
                    int blueBand = int.Parse(blueDialog.InputText);

                    if (nirBand < 1 || nirBand > _currentImage.BandCount || redBand < 1 || redBand > _currentImage.BandCount || blueBand < 1 || blueBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _vegIndices.CalculateEVI(_currentImage, nirBand, redBand, blueBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("EVI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"EVI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemSAVI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultNir = Math.Min(4, _currentImage.BandCount);
            int defaultRed = Math.Min(3, _currentImage.BandCount);

            using InputDialog nirDialog = new($"请输入NIR波段索引（1-{_currentImage.BandCount}）", "SAVI计算 - NIR波段", defaultNir.ToString());
            using InputDialog redDialog = new($"请输入Red波段索引（1-{_currentImage.BandCount}）", "SAVI计算 - Red波段", defaultRed.ToString());

            if (nirDialog.ShowDialog() == DialogResult.OK && redDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int nirBand = int.Parse(nirDialog.InputText);
                    int redBand = int.Parse(redDialog.InputText);

                    if (nirBand < 1 || nirBand > _currentImage.BandCount || redBand < 1 || redBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _vegIndices.CalculateSAVI(_currentImage, nirBand, redBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("SAVI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"SAVI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemMSAVI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultNir = Math.Min(4, _currentImage.BandCount);
            int defaultRed = Math.Min(3, _currentImage.BandCount);

            using InputDialog nirDialog = new($"请输入NIR波段索引（1-{_currentImage.BandCount}）", "MSAVI计算 - NIR波段", defaultNir.ToString());
            using InputDialog redDialog = new($"请输入Red波段索引（1-{_currentImage.BandCount}）", "MSAVI计算 - Red波段", defaultRed.ToString());

            if (nirDialog.ShowDialog() == DialogResult.OK && redDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int nirBand = int.Parse(nirDialog.InputText);
                    int redBand = int.Parse(redDialog.InputText);

                    if (nirBand < 1 || nirBand > _currentImage.BandCount || redBand < 1 || redBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _vegIndices.CalculateMSAVI(_currentImage, nirBand, redBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("MSAVI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"MSAVI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemNDWI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultGreen = Math.Min(2, _currentImage.BandCount);
            int defaultNir = Math.Min(4, _currentImage.BandCount);

            using InputDialog greenDialog = new($"请输入Green波段索引（1-{_currentImage.BandCount}）", "NDWI计算 - Green波段", defaultGreen.ToString());
            using InputDialog nirDialog = new($"请输入NIR波段索引（1-{_currentImage.BandCount}）", "NDWI计算 - NIR波段", defaultNir.ToString());

            if (greenDialog.ShowDialog() == DialogResult.OK && nirDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int greenBand = int.Parse(greenDialog.InputText);
                    int nirBand = int.Parse(nirDialog.InputText);

                    if (greenBand < 1 || greenBand > _currentImage.BandCount || nirBand < 1 || nirBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _objIndices.CalculateNDWI(_currentImage, greenBand, nirBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("NDWI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"NDWI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemMNDWI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultGreen = Math.Min(2, _currentImage.BandCount);
            int defaultSwir = Math.Min(5, _currentImage.BandCount);

            using InputDialog greenDialog = new($"请输入Green波段索引（1-{_currentImage.BandCount}）", "MNDWI计算 - Green波段", defaultGreen.ToString());
            using InputDialog swirDialog = new($"请输入SWIR波段索引（1-{_currentImage.BandCount}）", "MNDWI计算 - SWIR波段", defaultSwir.ToString());

            if (greenDialog.ShowDialog() == DialogResult.OK && swirDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int greenBand = int.Parse(greenDialog.InputText);
                    int swirBand = int.Parse(swirDialog.InputText);

                    if (greenBand < 1 || greenBand > _currentImage.BandCount || swirBand < 1 || swirBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _objIndices.CalculateMNDWI(_currentImage, greenBand, swirBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("MNDWI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"MNDWI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuItemNDBI_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int defaultSwir = Math.Min(5, _currentImage.BandCount);
            int defaultNir = Math.Min(4, _currentImage.BandCount);

            using InputDialog swirDialog = new($"请输入SWIR波段索引（1-{_currentImage.BandCount}）", "NDBI计算 - SWIR波段", defaultSwir.ToString());
            using InputDialog nirDialog = new($"请输入NIR波段索引（1-{_currentImage.BandCount}）", "NDBI计算 - NIR波段", defaultNir.ToString());

            if (swirDialog.ShowDialog() == DialogResult.OK && nirDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int swirBand = int.Parse(swirDialog.InputText);
                    int nirBand = int.Parse(nirDialog.InputText);

                    if (swirBand < 1 || swirBand > _currentImage.BandCount || nirBand < 1 || nirBand > _currentImage.BandCount)
                    {
                        MessageBox.Show("波段索引超出范围", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Bitmap newBitmap = _objIndices.CalculateNDBI(_currentImage, swirBand, nirBand);
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("NDBI计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"NDBI计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void menuItemBandExpression_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string example = "(B4-B3)/(B4+B3)";
            using InputDialog inputDialog = new(
                $"请输入波段表达式（支持 B1、B2... 表示波段，运算符 + - * / ^，函数 SQRT LOG ABS SIN COS 等）\n\n示例: {example}\n可用波段: B1~B{_currentImage.BandCount}", 
                "波段表达式计算", 
                example);
            
            if (inputDialog.ShowDialog() == DialogResult.OK)
            {
                string expression = inputDialog.InputText.Trim();
                if (string.IsNullOrEmpty(expression))
                {
                    MessageBox.Show("请输入有效的表达式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                try
                {
                    Bitmap newBitmap = await Task.Run(() => _bandExprCalc.CalculateExpression(_currentImage, expression));
                    _currentBitmap?.Dispose();
                    _currentBitmap = newBitmap;
                    _cache.Record(newBitmap);
                    
                    _zoom = 1.0;
                    _panOffset = Point.Empty;
                    pictureBox1.Invalidate();
                    MessageBox.Show("波段表达式计算完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"波段表达式计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void menuItemMeanFilter_Click(object sender, EventArgs e) => ApplyOperation(b => _enhancement.ApplyMeanFilter(b), "均值滤波");
        private void menuItemGaussianFilter_Click(object sender, EventArgs e) => ApplyOperation(b => _enhancement.ApplyGaussianFilter(b), "高斯滤波");
        private void menuItemLaplacian_Click(object sender, EventArgs e) => ApplyOperation(b => _enhancement.ApplyLaplacianSharpening(b), "拉普拉斯锐化");
        private void menuItemSobelEdge_Click(object sender, EventArgs e) => ApplyOperation(b => _enhancement.ApplySobelEdge(b), "Sobel边缘检测");

        private async void menuItemDensitySlice_Click(object sender, EventArgs e)
        {
            if (_currentBitmap == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using InputDialog inputDialog = new("请输入分级数量", "密度分割", "5");
            if (inputDialog.ShowDialog() != DialogResult.OK || _isProcessing) return;
            int classes = int.Parse(inputDialog.InputText);
            SetProcessingState(true, "正在处理: 密度分割...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                BitmapCapture capture = BitmapHelper.Capture24bpp(_currentBitmap!);
                Bitmap newBitmap = await Task.Run(() =>
                {
                    using Bitmap input = BitmapHelper.CreateFromCapture(capture);
                    return _enhancement.DensitySlice(input, classes);
                }).ConfigureAwait(true);
                CommitBitmap(newBitmap);
                sw.Stop();
                SetProcessingState(false, $"密度分割完成 ({sw.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                SetProcessingState(false, "密度分割失败");
                MessageBox.Show($"密度分割失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemKMeans_Click(object sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("请先打开影像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = _classification.KMeansClassification(_currentImage, 4);
                _currentBitmap?.Dispose();
                _currentBitmap = result.result;
                _cache.Record(result.result);
                
                _zoom = 1.0;
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                MessageBox.Show("K-Means分类完成！\n蓝色-水体, 绿色-植被, 红色-建筑, 黄色-裸土", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"K-Means分类失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void menuItemUndo_Click(object sender, EventArgs e)
        {
            if (_isProcessing) return;
            Bitmap? prev = _cache.Undo();
            if (prev != null)
            {
                _currentBitmap?.Dispose();
                _currentBitmap = prev;
                _zoom = 1.0;
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                toolStripStatusLabel7.Text = "已撤销";
                UpdateHistoryButtons();
            }
            else MessageBox.Show("没有可撤销的操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuItemRedo_Click(object sender, EventArgs e)
        {
            if (_isProcessing) return;
            Bitmap? next = _cache.Redo();
            if (next != null)
            {
                _currentBitmap?.Dispose();
                _currentBitmap = next;
                _zoom = 1.0;
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                toolStripStatusLabel7.Text = "已重做";
                UpdateHistoryButtons();
            }
            else MessageBox.Show("没有可重做的操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuItemZoomIn_Click(object sender, EventArgs e)
        {
            _zoom = Math.Min(_zoom * 1.2, 10.0);
            pictureBox1.Invalidate();
            toolStripStatusLabel4.Text = $"缩放: {_zoom:P0}";
        }

        private void menuItemZoomOut_Click(object sender, EventArgs e)
        {
            _zoom = Math.Max(_zoom / 1.2, 0.1);
            pictureBox1.Invalidate();
            toolStripStatusLabel4.Text = $"缩放: {_zoom:P0}";
        }

        private void menuItemZoomFit_Click(object sender, EventArgs e)
        {
            if (_currentBitmap != null)
            {
                _zoom = Math.Min(
                    (double)pictureBox1.ClientSize.Width / _currentBitmap.Width,
                    (double)pictureBox1.ClientSize.Height / _currentBitmap.Height);
                _panOffset = Point.Empty;
                pictureBox1.Invalidate();
                toolStripStatusLabel4.Text = $"缩放: {_zoom:P0}";
            }
        }

        private void menuItemHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("遥感影像处理系统 v1.0\n\n" +
                "开发环境: C# .NET 10 WinForm + GDAL\n" +
                "功能模块:\n" +
                "1. 基础影像操作 - 打开、保存、波段合成、灰度化、拉伸\n" +
                "2. 植被指数 - NDVI、EVI、SAVI、MSAVI\n" +
                "3. 地物指数 - NDWI、MNDWI、NDBI\n" +
                "4. 图像增强 - 滤波、锐化、边缘检测\n" +
                "5. 分类 - K-Means非监督分类\n" +
                "6. 自定义波段表达式计算\n\n" +
                "快捷键:\n" +
                "鼠标滚轮 - 缩放\n" +
                "中键拖动 - 平移", "帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e) => menuItemOpen_Click(sender, e);
        private void toolStripButtonSave_Click(object sender, EventArgs e) => menuItemSave_Click(sender, e);
        private void toolStripButtonZoomIn_Click(object sender, EventArgs e) => menuItemZoomIn_Click(sender, e);
        private void toolStripButtonZoomOut_Click(object sender, EventArgs e) => menuItemZoomOut_Click(sender, e);
        private void toolStripButtonFit_Click(object sender, EventArgs e) => menuItemZoomFit_Click(sender, e);
        private void toolStripButtonStretch_Click(object sender, EventArgs e) => menuItemLinearStretch_Click(sender, e);
        private void toolStripButtonNDVI_Click(object sender, EventArgs e) => menuItemNDVI_Click(sender, e);
        private void toolStripButtonFilter_Click(object sender, EventArgs e) => menuItemMeanFilter_Click(sender, e);
        private void toolStripButtonUndo_Click(object sender, EventArgs e) => menuItemUndo_Click(sender, e);
        private void toolStripButtonRedo_Click(object sender, EventArgs e) => menuItemRedo_Click(sender, e);
    }

    public class InputDialog : Form
    {
        private readonly TextBox _textBox = new();
        private readonly Button _okButton = new();
        private readonly Button _cancelButton = new();

        public string InputText => _textBox.Text;

        public InputDialog(string prompt, string title, string defaultValue = "")
        {
            Text = title;
            Size = new Size(500, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            Label label = new() { Text = prompt, Location = new Point(15, 15), Width = 460, Height = 60, AutoSize = false };
            _textBox = new TextBox { Text = defaultValue, Location = new Point(15, 90), Width = 460 };
            _okButton = new Button { Text = "确定", Location = new Point(300, 130), DialogResult = DialogResult.OK, Width = 80 };
            _cancelButton = new Button { Text = "取消", Location = new Point(400, 130), DialogResult = DialogResult.Cancel, Width = 80 };

            Controls.Add(label);
            Controls.Add(_textBox);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }
    }
}
