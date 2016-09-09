using System.Windows;

namespace AutoPrintr.Views
{
    internal partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DescriptionTextBlock.Width = sizeInfo.NewSize.Width - 70;
        }
    }
}