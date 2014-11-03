using Newtonsoft.Json;
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
using HtmlAgilityPack;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CircleReading
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchReadingPage : Page
    {
        public SearchReadingPage()
        {
            this.InitializeComponent();
        }

		public ReadingPageRequestParameter Requst { get; set; }

		Paragraph HtmlToParagragh(HtmlNode node)
		{
			Paragraph para = null;
			if (node.NodeType == HtmlNodeType.Text || node.Name == "a")
			{
				var inline = HtmlToInline(node);
				if (inline != null)
				{
					para = new Paragraph();
					para.Inlines.Add(inline);
				}
			}
			else if (node.Name == "p")
			{
				para = new Paragraph();
				foreach (var child in node.ChildNodes)
				{
					var inline = HtmlToInline(child);
					if (inline != null)
						para.Inlines.Add(inline);
				}
			}
			return para;
		}

		Inline HtmlToInline(HtmlNode node)
		{
			if (node.NodeType == HtmlNodeType.Text)
			{
				if (!String.IsNullOrWhiteSpace(node.InnerText))
					return new Run { Text = node.InnerText };
			}
			else if (node.Name == "a")
			{
				var container = new InlineUIContainer();
				var textBlock = new TextBlock();
				container.Child = textBlock;
				textBlock.Text = node.InnerText;
				textBlock.Tapped += textBlock_Tapped;
				textBlock.FontSize = container.FontSize;
				return container;

				//var link = new Hyperlink();
				//link.Foreground = (SolidColorBrush)App.Current.Resources["ForegroundBrush"];
				//link.Inlines.Add( new Run { Text = node.InnerText });
				//link.NavigateUri = new Uri("DetailReadingPage.xaml", UriKind.Relative);
				//link.Click += link_Click;
				//return link;
			}
			return null;
		}

		void textBlock_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (this.Frame.CanGoBack)
				this.Frame.GoBack();
		}

		void link_Click(Hyperlink sender, HyperlinkClickEventArgs args)
		{
			if (this.Frame.CanGoBack)
				this.Frame.GoBack();
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Requst = JsonConvert.DeserializeObject<ReadingPageRequestParameter>((string)e.Parameter);
			var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Articles");
			var html = await StringIOExtensions.ReadFromFile(Requst.ArticleName, folder);

			
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var nodes = doc.DocumentNode;
			foreach (var node in nodes.ChildNodes)
			{
				var para = HtmlToParagragh(node);
				if (para != null)
					CircularContentBlock.Blocks.Add(para);
			}



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
