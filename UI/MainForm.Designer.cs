namespace RemoteSensingProcessor.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMetadata = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemProcess = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemTrueColor = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemFalseColor = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCustomBand = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemGrayscale = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemLinearStretch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemInvert = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemBrightnessContrast = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemIndices = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemNDVI = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEVI = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSAVI = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMSAVI = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemNDWI = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMNDWI = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemNDBI = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemBandExpression = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEnhancement = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMeanFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemGaussianFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemLaplacian = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSobelEdge = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemDensitySlice = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemClassification = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemKMeans = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemZoomFit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFit = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonStretch = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNDVI = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFilter = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.listBoxBands = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel7 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFile,
            this.menuItemProcess,
            this.menuItemIndices,
            this.menuItemEnhancement,
            this.menuItemClassification,
            this.menuItemView,
            this.menuItemHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1200, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            
            this.menuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemOpen,
            this.menuItemSave,
            this.menuItemMetadata,
            this.toolStripSeparator1,
            this.menuItemExit});
            this.menuItemFile.Name = "menuItemFile";
            this.menuItemFile.Size = new System.Drawing.Size(37, 20);
            this.menuItemFile.Text = "文件";
            
            this.menuItemOpen.Name = "menuItemOpen";
            this.menuItemOpen.Size = new System.Drawing.Size(152, 22);
            this.menuItemOpen.Text = "打开";
            this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
            
            this.menuItemSave.Name = "menuItemSave";
            this.menuItemSave.Size = new System.Drawing.Size(152, 22);
            this.menuItemSave.Text = "保存";
            this.menuItemSave.Click += new System.EventHandler(this.menuItemSave_Click);
            
            this.menuItemMetadata.Name = "menuItemMetadata";
            this.menuItemMetadata.Size = new System.Drawing.Size(152, 22);
            this.menuItemMetadata.Text = "元数据";
            this.menuItemMetadata.Click += new System.EventHandler(this.menuItemMetadata_Click);
            
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(152, 22);
            this.menuItemExit.Text = "退出";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            
            this.menuItemProcess.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemTrueColor,
            this.menuItemFalseColor,
            this.menuItemCustomBand,
            this.toolStripSeparator2,
            this.menuItemGrayscale,
            this.menuItemLinearStretch,
            this.toolStripSeparator3,
            this.menuItemInvert,
            this.menuItemBrightnessContrast});
            this.menuItemProcess.Name = "menuItemProcess";
            this.menuItemProcess.Size = new System.Drawing.Size(68, 20);
            this.menuItemProcess.Text = "影像处理";
            
            this.menuItemTrueColor.Name = "menuItemTrueColor";
            this.menuItemTrueColor.Size = new System.Drawing.Size(180, 22);
            this.menuItemTrueColor.Text = "真彩色合成";
            this.menuItemTrueColor.Click += new System.EventHandler(this.menuItemTrueColor_Click);
            
            this.menuItemFalseColor.Name = "menuItemFalseColor";
            this.menuItemFalseColor.Size = new System.Drawing.Size(180, 22);
            this.menuItemFalseColor.Text = "标准假彩色合成";
            this.menuItemFalseColor.Click += new System.EventHandler(this.menuItemFalseColor_Click);
            
            this.menuItemCustomBand.Name = "menuItemCustomBand";
            this.menuItemCustomBand.Size = new System.Drawing.Size(180, 22);
            this.menuItemCustomBand.Text = "自定义波段组合";
            this.menuItemCustomBand.Click += new System.EventHandler(this.menuItemCustomBand_Click);
            
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            
            this.menuItemGrayscale.Name = "menuItemGrayscale";
            this.menuItemGrayscale.Size = new System.Drawing.Size(180, 22);
            this.menuItemGrayscale.Text = "灰度化";
            this.menuItemGrayscale.Click += new System.EventHandler(this.menuItemGrayscale_Click);
            
            this.menuItemLinearStretch.Name = "menuItemLinearStretch";
            this.menuItemLinearStretch.Size = new System.Drawing.Size(180, 22);
            this.menuItemLinearStretch.Text = "2%线性拉伸";
            this.menuItemLinearStretch.Click += new System.EventHandler(this.menuItemLinearStretch_Click);
            
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            
            this.menuItemInvert.Name = "menuItemInvert";
            this.menuItemInvert.Size = new System.Drawing.Size(180, 22);
            this.menuItemInvert.Text = "影像取反";
            this.menuItemInvert.Click += new System.EventHandler(this.menuItemInvert_Click);
            
            this.menuItemBrightnessContrast.Name = "menuItemBrightnessContrast";
            this.menuItemBrightnessContrast.Size = new System.Drawing.Size(180, 22);
            this.menuItemBrightnessContrast.Text = "亮度/对比度";
            this.menuItemBrightnessContrast.Click += new System.EventHandler(this.menuItemBrightnessContrast_Click);
            
            this.menuItemIndices.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNDVI,
            this.menuItemEVI,
            this.menuItemSAVI,
            this.menuItemMSAVI,
            this.toolStripSeparator4,
            this.menuItemNDWI,
            this.menuItemMNDWI,
            this.menuItemNDBI,
            this.toolStripSeparator11,
            this.menuItemBandExpression});
            this.menuItemIndices.Name = "menuItemIndices";
            this.menuItemIndices.Size = new System.Drawing.Size(63, 20);
            this.menuItemIndices.Text = "指数计算";
            
            this.menuItemNDVI.Name = "menuItemNDVI";
            this.menuItemNDVI.Size = new System.Drawing.Size(180, 22);
            this.menuItemNDVI.Text = "NDVI";
            this.menuItemNDVI.Click += new System.EventHandler(this.menuItemNDVI_Click);
            
            this.menuItemEVI.Name = "menuItemEVI";
            this.menuItemEVI.Size = new System.Drawing.Size(180, 22);
            this.menuItemEVI.Text = "EVI";
            this.menuItemEVI.Click += new System.EventHandler(this.menuItemEVI_Click);
            
            this.menuItemSAVI.Name = "menuItemSAVI";
            this.menuItemSAVI.Size = new System.Drawing.Size(180, 22);
            this.menuItemSAVI.Text = "SAVI";
            this.menuItemSAVI.Click += new System.EventHandler(this.menuItemSAVI_Click);
            
            this.menuItemMSAVI.Name = "menuItemMSAVI";
            this.menuItemMSAVI.Size = new System.Drawing.Size(180, 22);
            this.menuItemMSAVI.Text = "MSAVI";
            this.menuItemMSAVI.Click += new System.EventHandler(this.menuItemMSAVI_Click);
            
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(177, 6);
            
            this.menuItemNDWI.Name = "menuItemNDWI";
            this.menuItemNDWI.Size = new System.Drawing.Size(180, 22);
            this.menuItemNDWI.Text = "NDWI";
            this.menuItemNDWI.Click += new System.EventHandler(this.menuItemNDWI_Click);
            
            this.menuItemMNDWI.Name = "menuItemMNDWI";
            this.menuItemMNDWI.Size = new System.Drawing.Size(180, 22);
            this.menuItemMNDWI.Text = "MNDWI";
            this.menuItemMNDWI.Click += new System.EventHandler(this.menuItemMNDWI_Click);
            
            this.menuItemNDBI.Name = "menuItemNDBI";
            this.menuItemNDBI.Size = new System.Drawing.Size(180, 22);
            this.menuItemNDBI.Text = "NDBI";
            this.menuItemNDBI.Click += new System.EventHandler(this.menuItemNDBI_Click);
            
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(177, 6);
            
            this.menuItemBandExpression.Name = "menuItemBandExpression";
            this.menuItemBandExpression.Size = new System.Drawing.Size(180, 22);
            this.menuItemBandExpression.Text = "波段表达式计算";
            this.menuItemBandExpression.Click += new System.EventHandler(this.menuItemBandExpression_Click);
            
            this.menuItemEnhancement.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemMeanFilter,
            this.menuItemGaussianFilter,
            this.toolStripSeparator5,
            this.menuItemLaplacian,
            this.menuItemSobelEdge,
            this.toolStripSeparator6,
            this.menuItemDensitySlice});
            this.menuItemEnhancement.Name = "menuItemEnhancement";
            this.menuItemEnhancement.Size = new System.Drawing.Size(71, 20);
            this.menuItemEnhancement.Text = "图像增强";
            
            this.menuItemMeanFilter.Name = "menuItemMeanFilter";
            this.menuItemMeanFilter.Size = new System.Drawing.Size(160, 22);
            this.menuItemMeanFilter.Text = "均值滤波";
            this.menuItemMeanFilter.Click += new System.EventHandler(this.menuItemMeanFilter_Click);
            
            this.menuItemGaussianFilter.Name = "menuItemGaussianFilter";
            this.menuItemGaussianFilter.Size = new System.Drawing.Size(160, 22);
            this.menuItemGaussianFilter.Text = "高斯滤波";
            this.menuItemGaussianFilter.Click += new System.EventHandler(this.menuItemGaussianFilter_Click);
            
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(157, 6);
            
            this.menuItemLaplacian.Name = "menuItemLaplacian";
            this.menuItemLaplacian.Size = new System.Drawing.Size(160, 22);
            this.menuItemLaplacian.Text = "拉普拉斯锐化";
            this.menuItemLaplacian.Click += new System.EventHandler(this.menuItemLaplacian_Click);
            
            this.menuItemSobelEdge.Name = "menuItemSobelEdge";
            this.menuItemSobelEdge.Size = new System.Drawing.Size(160, 22);
            this.menuItemSobelEdge.Text = "Sobel边缘检测";
            this.menuItemSobelEdge.Click += new System.EventHandler(this.menuItemSobelEdge_Click);
            
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(157, 6);
            
            this.menuItemDensitySlice.Name = "menuItemDensitySlice";
            this.menuItemDensitySlice.Size = new System.Drawing.Size(160, 22);
            this.menuItemDensitySlice.Text = "密度分割";
            this.menuItemDensitySlice.Click += new System.EventHandler(this.menuItemDensitySlice_Click);
            
            this.menuItemClassification.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemKMeans});
            this.menuItemClassification.Name = "menuItemClassification";
            this.menuItemClassification.Size = new System.Drawing.Size(46, 20);
            this.menuItemClassification.Text = "分类";
            
            this.menuItemKMeans.Name = "menuItemKMeans";
            this.menuItemKMeans.Size = new System.Drawing.Size(180, 22);
            this.menuItemKMeans.Text = "K-Means分类";
            this.menuItemKMeans.Click += new System.EventHandler(this.menuItemKMeans_Click);
            
            this.menuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemZoomIn,
            this.menuItemZoomOut,
            this.menuItemZoomFit,
            this.toolStripSeparator7,
            this.menuItemUndo,
            this.menuItemRedo});
            this.menuItemView.Name = "menuItemView";
            this.menuItemView.Size = new System.Drawing.Size(44, 20);
            this.menuItemView.Text = "视图";
            
            this.menuItemZoomIn.Name = "menuItemZoomIn";
            this.menuItemZoomIn.Size = new System.Drawing.Size(180, 22);
            this.menuItemZoomIn.Text = "放大";
            this.menuItemZoomIn.Click += new System.EventHandler(this.menuItemZoomIn_Click);
            
            this.menuItemZoomOut.Name = "menuItemZoomOut";
            this.menuItemZoomOut.Size = new System.Drawing.Size(180, 22);
            this.menuItemZoomOut.Text = "缩小";
            this.menuItemZoomOut.Click += new System.EventHandler(this.menuItemZoomOut_Click);
            
            this.menuItemZoomFit.Name = "menuItemZoomFit";
            this.menuItemZoomFit.Size = new System.Drawing.Size(180, 22);
            this.menuItemZoomFit.Text = "自适应";
            this.menuItemZoomFit.Click += new System.EventHandler(this.menuItemZoomFit_Click);
            
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(177, 6);
            
            this.menuItemUndo.Name = "menuItemUndo";
            this.menuItemUndo.Size = new System.Drawing.Size(180, 22);
            this.menuItemUndo.Text = "撤销";
            this.menuItemUndo.Click += new System.EventHandler(this.menuItemUndo_Click);
            
            this.menuItemRedo.Name = "menuItemRedo";
            this.menuItemRedo.Size = new System.Drawing.Size(180, 22);
            this.menuItemRedo.Text = "重做";
            this.menuItemRedo.Click += new System.EventHandler(this.menuItemRedo_Click);
            
            this.menuItemHelp.Name = "menuItemHelp";
            this.menuItemHelp.Size = new System.Drawing.Size(44, 20);
            this.menuItemHelp.Text = "帮助";
            this.menuItemHelp.Click += new System.EventHandler(this.menuItemHelp_Click);
            
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpen,
            this.toolStripButtonSave,
            this.toolStripSeparator8,
            this.toolStripButtonZoomIn,
            this.toolStripButtonZoomOut,
            this.toolStripButtonFit,
            this.toolStripSeparator9,
            this.toolStripButtonStretch,
            this.toolStripButtonNDVI,
            this.toolStripButtonFilter,
            this.toolStripSeparator10,
            this.toolStripButtonUndo,
            this.toolStripButtonRedo});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1200, 25);
            this.toolStrip1.TabIndex = 1;
            
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(36, 22);
            this.toolStripButtonOpen.Text = "打开";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.toolStripButtonOpen_Click);
            
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(36, 22);
            this.toolStripButtonSave.Text = "保存";
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSave_Click);
            
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            
            this.toolStripButtonZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonZoomIn.Name = "toolStripButtonZoomIn";
            this.toolStripButtonZoomIn.Size = new System.Drawing.Size(36, 22);
            this.toolStripButtonZoomIn.Text = "放大";
            this.toolStripButtonZoomIn.Click += new System.EventHandler(this.toolStripButtonZoomIn_Click);
            
            this.toolStripButtonZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonZoomOut.Name = "toolStripButtonZoomOut";
            this.toolStripButtonZoomOut.Size = new System.Drawing.Size(36, 22);
            this.toolStripButtonZoomOut.Text = "缩小";
            this.toolStripButtonZoomOut.Click += new System.EventHandler(this.toolStripButtonZoomOut_Click);
            
            this.toolStripButtonFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonFit.Name = "toolStripButtonFit";
            this.toolStripButtonFit.Size = new System.Drawing.Size(48, 22);
            this.toolStripButtonFit.Text = "自适应";
            this.toolStripButtonFit.Click += new System.EventHandler(this.toolStripButtonFit_Click);
            
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
            
            this.toolStripButtonStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonStretch.Name = "toolStripButtonStretch";
            this.toolStripButtonStretch.Size = new System.Drawing.Size(60, 22);
            this.toolStripButtonStretch.Text = "线性拉伸";
            this.toolStripButtonStretch.Click += new System.EventHandler(this.toolStripButtonStretch_Click);
            
            this.toolStripButtonNDVI.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonNDVI.Name = "toolStripButtonNDVI";
            this.toolStripButtonNDVI.Size = new System.Drawing.Size(48, 22);
            this.toolStripButtonNDVI.Text = "NDVI";
            this.toolStripButtonNDVI.Click += new System.EventHandler(this.toolStripButtonNDVI_Click);
            
            this.toolStripButtonFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonFilter.Name = "toolStripButtonFilter";
            this.toolStripButtonFilter.Size = new System.Drawing.Size(48, 22);
            this.toolStripButtonFilter.Text = "滤波";
            this.toolStripButtonFilter.Click += new System.EventHandler(this.toolStripButtonFilter_Click);
            
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 25);
            
            this.toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonUndo.Name = "toolStripButtonUndo";
            this.toolStripButtonUndo.Size = new System.Drawing.Size(36, 22);
            this.toolStripButtonUndo.Text = "撤销";
            this.toolStripButtonUndo.Click += new System.EventHandler(this.toolStripButtonUndo_Click);
            
            this.toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonRedo.Name = "toolStripButtonRedo";
            this.toolStripButtonRedo.Size = new System.Drawing.Size(36, 22);
            this.toolStripButtonRedo.Text = "重做";
            this.toolStripButtonRedo.Click += new System.EventHandler(this.toolStripButtonRedo_Click);
            
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 49);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Panel1.Controls.Add(this.panelLeft);
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1200, 601);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 2;
            
            this.panelLeft.Controls.Add(this.listBoxBands);
            this.panelLeft.Controls.Add(this.label1);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(200, 601);
            this.panelLeft.TabIndex = 0;
            
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "波段列表";
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            
            this.listBoxBands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxBands.Name = "listBoxBands";
            this.listBoxBands.Size = new System.Drawing.Size(200, 574);
            this.listBoxBands.TabIndex = 0;
            
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(996, 601);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel5,
            this.toolStripStatusLabel6,
            this.toolStripStatusLabel7});
            this.statusStrip1.Location = new System.Drawing.Point(0, 650);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip1.TabIndex = 3;
            
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(56, 17);
            this.toolStripStatusLabel1.Text = "尺寸: -";
            
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(56, 17);
            this.toolStripStatusLabel2.Text = "波段: -";
            
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(70, 17);
            this.toolStripStatusLabel3.Text = "分辨率: -";
            
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(62, 17);
            this.toolStripStatusLabel4.Text = "缩放: 100%";
            
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(60, 17);
            this.toolStripStatusLabel5.Text = "坐标: -";
            
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(56, 17);
            this.toolStripStatusLabel6.Text = "像素: -";
            
            this.toolStripStatusLabel7.Name = "toolStripStatusLabel7";
            this.toolStripStatusLabel7.Size = new System.Drawing.Size(56, 17);
            this.toolStripStatusLabel7.Text = "文件: -";
            this.toolStripStatusLabel7.Spring = true;
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 672);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "遥感影像处理系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuItemFile;
        private System.Windows.Forms.ToolStripMenuItem menuItemOpen;
        private System.Windows.Forms.ToolStripMenuItem menuItemSave;
        private System.Windows.Forms.ToolStripMenuItem menuItemMetadata;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        private System.Windows.Forms.ToolStripMenuItem menuItemProcess;
        private System.Windows.Forms.ToolStripMenuItem menuItemTrueColor;
        private System.Windows.Forms.ToolStripMenuItem menuItemFalseColor;
        private System.Windows.Forms.ToolStripMenuItem menuItemCustomBand;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuItemGrayscale;
        private System.Windows.Forms.ToolStripMenuItem menuItemLinearStretch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuItemInvert;
        private System.Windows.Forms.ToolStripMenuItem menuItemBrightnessContrast;
        private System.Windows.Forms.ToolStripMenuItem menuItemIndices;
        private System.Windows.Forms.ToolStripMenuItem menuItemNDVI;
        private System.Windows.Forms.ToolStripMenuItem menuItemEVI;
        private System.Windows.Forms.ToolStripMenuItem menuItemSAVI;
        private System.Windows.Forms.ToolStripMenuItem menuItemMSAVI;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem menuItemNDWI;
        private System.Windows.Forms.ToolStripMenuItem menuItemMNDWI;
        private System.Windows.Forms.ToolStripMenuItem menuItemNDBI;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem menuItemBandExpression;
        private System.Windows.Forms.ToolStripMenuItem menuItemEnhancement;
        private System.Windows.Forms.ToolStripMenuItem menuItemMeanFilter;
        private System.Windows.Forms.ToolStripMenuItem menuItemGaussianFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem menuItemLaplacian;
        private System.Windows.Forms.ToolStripMenuItem menuItemSobelEdge;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem menuItemDensitySlice;
        private System.Windows.Forms.ToolStripMenuItem menuItemClassification;
        private System.Windows.Forms.ToolStripMenuItem menuItemKMeans;
        private System.Windows.Forms.ToolStripMenuItem menuItemView;
        private System.Windows.Forms.ToolStripMenuItem menuItemZoomIn;
        private System.Windows.Forms.ToolStripMenuItem menuItemZoomOut;
        private System.Windows.Forms.ToolStripMenuItem menuItemZoomFit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem menuItemUndo;
        private System.Windows.Forms.ToolStripMenuItem menuItemRedo;
        private System.Windows.Forms.ToolStripMenuItem menuItemHelp;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomIn;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButtonFit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton toolStripButtonStretch;
        private System.Windows.Forms.ToolStripButton toolStripButtonNDVI;
        private System.Windows.Forms.ToolStripButton toolStripButtonFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripButton toolStripButtonUndo;
        private System.Windows.Forms.ToolStripButton toolStripButtonRedo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBoxBands;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel7;
    }
}
