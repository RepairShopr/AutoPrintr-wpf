using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AutoPrintr.Controls
{
    internal partial class BusyIndicator : UserControl
    {
        private DispatcherTimer _timer;

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false, (sender, e) =>
            {
                var control = (BusyIndicator)sender;
                if ((bool)e.NewValue)
                    control._timer.Start();
                else
                {
                    control._timer.Stop();
                    if (control.Visibility == Visibility.Visible)
                    {
                        control.Visibility = Visibility.Collapsed;
                        var storyBoard = (Storyboard)control.Resources["BusyAnimation"];
                        storyBoard.Stop();
                    }
                }
            }));

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(BusyIndicator), new PropertyMetadata(null, (sender, e) =>
            {
                var control = (BusyIndicator)sender;
                control.CaptionTextBlock.Text = (string)e.NewValue;
            }));

        public BusyIndicator()
        {
            InitializeComponent();

            Grid.SetColumnSpan(this, 100);
            Grid.SetRowSpan(this, 100);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += _timer_Tick;

            Visibility = Visibility.Collapsed;
            SetBinding(IsBusyProperty, new Binding("IsBusy"));

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Messenger.Default.Register<ShowControlMessage>(this, OnShowControl);
                Messenger.Default.Register<HideControlMessage>(this, OnHideControl);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (IsBusy)
            {
                Visibility = Visibility.Visible;
                var storyBoard = (Storyboard)Resources["BusyAnimation"];
                storyBoard.Begin();
            }
        }

        private void OnShowControl(ShowControlMessage message)
        {
            switch (message.Type)
            {
                case ControlMessageType.Busy:
                    Caption = message.Caption;
                    IsBusy = true;
                    break;
                default: break;
            }
        }

        private void OnHideControl(HideControlMessage message)
        {
            switch (message.Type)
            {
                case ControlMessageType.Busy:
                    IsBusy = false;
                    break;
                default: break;
            }
        }
    }
}