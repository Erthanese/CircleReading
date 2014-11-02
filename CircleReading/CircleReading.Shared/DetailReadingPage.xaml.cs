using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CircleReading
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailReadingPage : Page
    {
		public ReadingPageRequestParameter Requst { get; set; }

        public DetailReadingPage()
        {
            this.InitializeComponent();
        }

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Requst = JsonConvert.DeserializeObject<ReadingPageRequestParameter>((string)e.Parameter);
			var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Articles");
			var html = await StringIOExtensions.ReadFromFile(Requst.ArticleName, folder);
			CircularContentBlock.SetLinkedHtmlFragmentString(html);


			if (Requst.Layout == CircleLayout.Crop)
			{
				var R = 0.5 * this.Frame.ActualWidth - this.Frame.Padding.Left - this.Frame.Padding.Right;
				var margin = R * (1.0 - 1.0 / Math.Sqrt(2));
				CircularContentBlock.Margin = new Thickness(margin);
				TopCropRect.Height = margin;
				BottomCropRect.Height = margin;
				TopCropRect.Visibility = Visibility.Visible;
				BottomCropRect.Visibility = Visibility.Visible;
			}
		}
    }
}
