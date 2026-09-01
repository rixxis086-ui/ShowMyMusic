using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ShowMyMusic.Models;
using ShowMyMusic.ViewModels;

namespace ShowMyMusic.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void Nav_Appearance_Checked(object sender, RoutedEventArgs e)
        {
            SwitchPage(Page_Appearance);
        }

        private void Nav_Position_Checked(object sender, RoutedEventArgs e)
        {
            SwitchPage(Page_Position);
        }

        private void Nav_Animations_Checked(object sender, RoutedEventArgs e)
        {
            SwitchPage(Page_Animations);
        }

        private void Nav_System_Checked(object sender, RoutedEventArgs e)
        {
            SwitchPage(Page_System);
        }

        private void SwitchPage(ScrollViewer? targetPage)
        {
            if (Page_Appearance == null || Page_Position == null || Page_Animations == null || Page_System == null)
                return;

            Page_Appearance.Visibility = Visibility.Collapsed;
            Page_Position.Visibility = Visibility.Collapsed;
            Page_Animations.Visibility = Visibility.Collapsed;
            Page_System.Visibility = Visibility.Collapsed;

            if (targetPage != null)
            {
                targetPage.Visibility = Visibility.Visible;
            }
        }

        private void OnGlassStyleChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && e.AddedItems.Count > 0 && e.AddedItems[0] is GlassStyle style)
            {
                _viewModel.Settings.GlassStyle = style;
                _viewModel.UpdatePreviewColorsAsync();
                _viewModel.SaveSettings();
            }
        }

        private void OnPositionZoneChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && e.AddedItems.Count > 0 && e.AddedItems[0] is SimpleZone zone)
            {
                _viewModel.Settings.SimpleZone = zone;
                _viewModel.SaveSettings();
            }
        }

        private void OnVisualSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _viewModel?.UpdatePreviewColorsAsync();
            _viewModel?.SaveSettings();
        }

        private void OnPositionSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _viewModel?.SaveSettings();
        }

        private void OnAdaptiveColorCheckChanged(object sender, RoutedEventArgs e)
        {
            _viewModel?.UpdatePreviewColorsAsync();
            _viewModel?.SaveSettings();
        }

        private void OnCustomColorTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel?.UpdatePreviewColorsAsync();
            _viewModel?.SaveSettings();
        }

        private void OnDualStateCheckChanged(object sender, RoutedEventArgs e)
        {
            _viewModel?.SaveSettings();
            LivePreviewControl?.ApplyVisualStates(false);
        }

        private void ApplyAndClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveSettings();
            Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            _viewModel.SaveSettings();
            Hide();
        }
    }
}