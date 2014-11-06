using CircleReading.Common;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.IO.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CircleReading
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JustifyPage : Page
    {
		double FocusFactor;
        public JustifyPage()
        {
			this.InitializeComponent();
			DataContext = ViewModel;
		}

		private ObservableDictionary defaultViewModel = new ObservableDictionary();
		/// <summary>
		/// Gets the view model for this <see cref="Page"/>.
		/// This can be changed to a strongly typed view model.
		/// </summary>
		public ObservableDictionary ViewModel
		{
			get { return this.defaultViewModel; }
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
				if (!String.IsNullOrEmpty(node.InnerText))
					return new Run { Text = node.InnerText };
			}
			else if (node.Name == "a")
			{
				var container = new InlineUIContainer();
				var textBlock = new TextBlock();
				container.Child = textBlock;
				textBlock.Text = node.InnerText;
				textBlock.Tapped += textBlock_Tapped;
				textBlock.Foreground = (SolidColorBrush)App.Current.Resources["ForegroundBrush"];
				textBlock.FontSize = (double)App.Current.Resources["ReadingFontSize"];
				return container;
			}
			return null;
		}

		void textBlock_Tapped(object sender, TappedRoutedEventArgs e)
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
			//ContentScrollViewer.ViewChanged += ContentScrollViewer_LayoutUpdated;
			ContentScrollViewer.LayoutUpdated += ContentScrollViewer_LayoutUpdated;

			FocusFactor = (double)App.Current.Resources["FocusLineWidthFactor"];
			ViewModel["LineWidth"] = (double)App.Current.Resources["ScreenDiameter"] * FocusFactor;
			ViewModel["LineHeight"] = (double)App.Current.Resources["ReadingLineHeight"];

			if (Requst.Layout == CircleLayout.PagedAdaptive)
			{
				ContentScrollViewer.IsVerticalScrollChainingEnabled = true;

				var R = (double)App.Current.Resources["ScreenDiameter"] * 0.5;
				var margin = (1-Math.Sqrt(1 - FocusFactor * FocusFactor)) * R;
				TopCropButton.Height = margin;
				BottomCropButton.Height = margin;
				TopCropButton.Visibility = Visibility.Visible;
				BottomCropButton.Visibility = Visibility.Visible;
			}


			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var nodes = doc.DocumentNode;
			foreach (var node in nodes.ChildNodes)
			{
				var para = HtmlToParagragh(node);
				if (para != null)
					CircularContentBlock.Blocks.Add(para);
			}
		}

		void ContentScrollViewer_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
		{
			e.Handled = true;
		}

		void ContentScrollViewer_LayoutUpdated(object sender, object e)
		{
			Debug.WriteLine("LayoutUpdated");
			var scroll = ContentScrollViewer;
			var R = (scroll.ViewportHeight) * 0.5;
			var offset = scroll.VerticalOffset - ContentColumns.Margin.Top;
			var center = offset + R;
			int lb = Math.Max((int)((center - 2.4 * R) / ContentColumns.ColumnHeight), 0);
			int ls = Math.Min((int)((center + 2.4 * R) / ContentColumns.ColumnHeight), ContentColumns.Children.Count - 1);
			for (int i = lb; i <= ls; i++)
			{
				var p = (i + 0.5) * ContentColumns.ColumnHeight;
				var off = p - center;
				var trans = CircleTransform(off, R);
				var line = ContentColumns.Children[i] as UIElement;
				var transform = line.RenderTransform as CompositeTransform;
				transform.ScaleX = trans.Item1;
				transform.ScaleY = trans.Item1;
				line.Opacity = trans.Item1;
				transform.TranslateY = trans.Item2;

				//var ss = ((double)App.Current.Resources["ReadingFontSize"] *scale + 5) / ContentColumns.ColumnHeight;
			}
		}

		Tuple<double,double> CircleTransform(double offset, double R)
		{
			var bias = 0.45 * ContentColumns.ColumnHeight;
			offset = Math.Sign(offset) * (Math.Abs(offset) + bias);
			if (Math.Abs(offset) < Math.Sqrt(1- FocusFactor*FocusFactor) * R)
				return new Tuple<double, double>(1, 0);
			else
			{
				var ratio = R / Math.Sqrt(FocusFactor * FocusFactor * R * R + offset * offset);
				var noffset = offset * Math.Sqrt(ratio);
				if (Math.Abs(noffset) > R)
					return new Tuple<double, double>(1,0);
				var scale = Math.Sqrt(R * R - noffset * noffset) / (FocusFactor * R);
				if (scale < 0.3)
				{
					scale = 1.0;
					noffset = Math.Sign(noffset) * (Math.Abs(noffset) + R);
				}
				return new Tuple<double, double>(scale, (noffset - offset)); // 
			}
		}

		private void DownButton_Click(object sender, RoutedEventArgs e)
		{
			ContentScrollViewer.ChangeView(null, ContentScrollViewer.VerticalOffset + ContentColumns.ColumnHeight * ContentColumns.ColumnsPerPage,null,true);
		}

		private void UpButton_Click(object sender, RoutedEventArgs e)
		{
			ContentScrollViewer.ChangeView(null, ContentScrollViewer.VerticalOffset - ContentColumns.ColumnHeight * ContentColumns.ColumnsPerPage, null, true);
		}

		//void ContentScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		//{
		//	ContentScrollViewer_LayoutUpdated(null, null);
		//}


	}
}
