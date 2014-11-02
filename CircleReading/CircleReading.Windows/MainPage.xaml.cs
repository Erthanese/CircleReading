using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.IO.Extensions;
using WinRTXamlToolkit.Controls.Extensions;
using WinRTXamlToolkit.IO.Serialization;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CircleReading
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		System.DateTime StartTime;
		System.DateTime EndTime;
		ObservableCollection<TimeSpan> AccomplishTimes = new ObservableCollection<TimeSpan>();

		public MainPage()
        {
            this.InitializeComponent();
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Articles");
			var html = await StringIOExtensions.ReadFromFile("Article1.html", folder);
			//ContentBlock.BaseUri = new Uri("http://www.wpcentral.com/");
			CircularContentBlock.SetLinkedHtmlFragmentString(html);
			TimeListView.ItemsSource = AccomplishTimes;
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			ContentScrollViewer.Visibility = Windows.UI.Xaml.Visibility.Visible;
			StartButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			FinishButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			StartTime = System.DateTime.Now;
		}

		private void FinishButton_Click(object sender, RoutedEventArgs e)
		{
			ContentScrollViewer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			StartButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
			FinishButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			EndTime = System.DateTime.Now;
			var dur = EndTime - StartTime;
			AccomplishTimes.Add(dur);
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{

		}

		private void CenterCropButton_Checked(object sender, RoutedEventArgs e)
		{
			var button = sender as AppBarToggleButton;
			if (ContainerGrid == null || button.IsChecked == null)
				return;
			var R = 0.5*ContainerGrid.ActualWidth;
			if (button.IsChecked.Value)
			{
				var margin = R*(1.0 - 1.0/Math.Sqrt(2));
				CircularContentBlock.Margin = new Thickness(margin);
			}
			else
			{
				CircularContentBlock.Margin = new Thickness(10, 0, 10, 0);
			}
		}
    }
}
