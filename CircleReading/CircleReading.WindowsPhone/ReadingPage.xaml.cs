using CircleReading.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;
using WinRTXamlToolkit.Controls;
using WinRTXamlToolkit.IO.Extensions;
using Windows.UI.Xaml.Media.Animation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CircleReading
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ReadingPage : Page , INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public TrailRecord Result { get; private set; }

		private NavigationHelper navigationHelper;
		private ObservableDictionary defaultViewModel = new ObservableDictionary();

		private bool _showMask = false;
		public bool ShowMask
		{
			get { return _showMask; }
			set
			{
				if (_showMask != value)
				{
					_showMask = value;
					NotifyPropertyChanged();
				}
			}
		}

		public CircleLayout RequestLayout { get; set; }

		private TestingState _state = TestingState.Initializing;

		public TestingState CurrentState
		{
			get { return _state; }
			set 
			{
				if (_state != value)
				{
					_state = value;
					bool result;
					switch (_state)
					{
						case TestingState.UserInformation:
							TitleTextBlock.Text = "Basic Information";
							DescriptionTextBlock.Text = "Please check the information below to match your condition";
							StageFrame.Navigate(typeof(UserInfoPage));
							result = VisualStateManager.GoToState(this, "UserInfoState", false);
							break;
						case TestingState.BaselineReadingPrepare:
							{
								TitleTextBlock.Text = "Baseline Reading";
								DescriptionTextBlock.Text = "Please read the article 'word by word', and click 'finish' when you do";
								var request = new ReadingPageRequestParameter
								{
									Layout = CircleLayout.Overflow,
									ArticleName = "Article0.html"
								};
								var param = JsonConvert.SerializeObject(request);
								StageFrame.Navigate(typeof(ConventionalReadingPage), param);
								result = VisualStateManager.GoToState(this, "BaselineReadingPrepareState", false);
							}
							break;
						case TestingState.BaselineReading:
							StageFrame.Focus(FocusState.Pointer);
							result = VisualStateManager.GoToState(this, "BaselineReadingState", false);
							break;
						case TestingState.DetailReadingPrepare:
							{
								TitleTextBlock.Text = "Detail Reading";
								DescriptionTextBlock.Text = "Please read the article 'word by word', and click 'finish' when you do";
								var request = new ReadingPageRequestParameter
								{
									Layout = this.RequestLayout,
									ArticleName = "Article1.html"
								};
								var param = JsonConvert.SerializeObject(request);
								if (RequestLayout < CircleLayout.Justify)
									StageFrame.Navigate(typeof(ConventionalReadingPage), param);
								else
									StageFrame.Navigate(typeof(JustifyPage), param);
								result = VisualStateManager.GoToState(this, "DetailReadingPrepareState", false);
							}
							break;
						case TestingState.DetailReading:
							result = VisualStateManager.GoToState(this, "DetailReadingState", false);
							break;
						case TestingState.SearchReadingPrepare1:
						case TestingState.SearchReadingPrepare2:
						case TestingState.SearchReadingPrepare3:
							{
								var No = (_state - TestingState.SearchReadingPrepare1) / 2 + 1;
								TitleTextBlock.Text = "Search Reading";
								DescriptionTextBlock.Text = "Please Tap the anwser keyword of the qustion as quick as you can";
								QustionTextBlock.Text = (string)App.Current.Resources[String.Format("Qustion{0}", No)];
								var request = new ReadingPageRequestParameter
								{
									Layout = this.RequestLayout,
									ArticleName = String.Format("Article1_S{0}.html", No)
								};
								var param = JsonConvert.SerializeObject(request);
								if (RequestLayout < CircleLayout.Justify)
									StageFrame.Navigate(typeof(ConventionalReadingPage), param);
								else
									StageFrame.Navigate(typeof(JustifyPage), param);
								result = VisualStateManager.GoToState(this, "SearchReadingPrepareState", false);
							}
							break;
						case TestingState.SearchReading1:
						case TestingState.SearchReading2:
						case TestingState.SearchReading3:
							result = VisualStateManager.GoToState(this, "SearchReadingState", false);
							break;
						case TestingState.Rating:
							TitleTextBlock.Text = "Rating";
							DescriptionTextBlock.Text = "Please Give the rate the general experience";
							StageFrame.Navigate(typeof(RatingPage));
							result = VisualStateManager.GoToState(this, "RatingState", false);
							break;
						case TestingState.Finish:
						default:
							StageFrame.GoBack();
							TitleTextBlock.Text = "Finish!";
							DescriptionTextBlock.Text = "Thank you for corperating our study! You will get a choclate from us!";
							result = VisualStateManager.GoToState(this, "FinishState", false);
							break;
					}
				}
			}
		}

		public ReadingPage()
		{
			this.InitializeComponent();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
			StageFrame.ContentTransitions = null; //new TransitionCollection { new ContentThemeTransition() };
			StageFrame.Navigated += StageFrame_Navigated;
			//StageFrame.Navigated += StageFrame_FirstNavigated;//.PageTransition = new WipeTransition();
		}

		private async void StageFrame_Navigated(object sender, NavigationEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.Back)
			{
				EndTime = DateTime.Now;
				var dur = EndTime - StartTime;

				switch (CurrentState)
				{
					case TestingState.UserInformation:
						break;
					case TestingState.BaselineReading:
						Result.BaseLineTestTime = dur;
						break;
					case TestingState.DetailReading:
						Result.DetailReadingTime = dur;
						break;
					case TestingState.SearchReading1:
						Result.SearchReadingTime1 = dur;
						await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { CurrentState = CurrentState + 1; });
						break;
					case TestingState.SearchReading2:
						Result.SearchReadingTime2 = dur;
						await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { CurrentState = CurrentState + 1; });
						break;
					case TestingState.SearchReading3:
						Result.SearchReadingTime3 = dur;
						await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { CurrentState = CurrentState + 1; });
						break;
					case TestingState.Rating:
						break;
					case TestingState.Finish:
						break;
					default:
						break;
				}
			}
			return;
		}

		/// <summary>
		/// Restores the content transitions after the app has launched.
		/// </summary>
		/// <param name="sender">The object where the handler is attached.</param>
		/// <param name="e">Details about the navigation event.</param>
		private void StageFrame_FirstNavigated(object sender, NavigationEventArgs e)
		{
			var frame = sender as Frame;
			frame.ContentTransitions = null;//new TransitionCollection() { new NavigationThemeTransition { DefaultNavigationTransitionInfo = new SlideNavigationTransitionInfo() } };
			frame.Navigated -= this.StageFrame_FirstNavigated;
		}

		/// <summary>
		/// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
		/// </summary>
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
		}

		/// <summary>
		/// Gets the view model for this <see cref="Page"/>.
		/// This can be changed to a strongly typed view model.
		/// </summary>
		public ObservableDictionary DefaultViewModel
		{
			get { return this.defaultViewModel; }
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="sender">
		/// The source of the event; typically <see cref="NavigationHelper"/>
		/// </param>
		/// <param name="e">Event data that provides both the navigation parameter passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
		/// a dictionary of state preserved by this page during an earlier
		/// session.  The state will be null the first time a page is visited.</param>
		private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Articles");
			RequestLayout = (CircleLayout)e.NavigationParameter;
			//switch (layoutNo)
			//{
			//	case 1:
			//		RequestLayout = CircleLayout.Overflow;
			//		break;
			//	case 2:
			//		RequestLayout = CircleLayout.Crop;
			//		break;
			//	case 3:
			//		RequestLayout = CircleLayout.Justify;
			//		//await MessageDialogExtensions.ShowTwoOptionsDialog("Justify Layout is not supported yet.", "Ingnore", "Back", null, new Action(this.Frame.GoBack));
			//		break;
			//}

			CurrentState = TestingState.UserInformation;
			Result = new TrailRecord();
			Result.TimeStemp = DateTime.Now;
			Result.Layout = RequestLayout;
			App.Current.CurrentTrialRecord = Result;
		}

		/// <summary>
		/// Preserves state associated with this page in case the application is suspended or the
		/// page is discarded from the navigation cache.  Values must conform to the serialization
		/// requirements of <see cref="SuspensionManager.SessionState"/>.
		/// </summary>
		/// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
		/// <param name="e">Event data that provides an empty dictionary to be populated with
		/// serializable state.</param>
		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			StartButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			StageFrame.Visibility = Visibility.Visible;

			CurrentState = CurrentState + 1; 
			StartTime = System.DateTime.Now;
			//var ffi = FinishButton.FadeIn(new TimeSpan(0, 0, 1));
			//await ffi;
		}

		private void NextButton_Click(object sender, RoutedEventArgs e)
		{
			EndTime = System.DateTime.Now;
			var dur = EndTime - StartTime;
			switch (CurrentState)
			{
				case TestingState.BaselineReading:
					Result.BaseLineTestTime = dur;
					break;
				case TestingState.DetailReading:
					Result.DetailReadingTime = dur;
					break;
				//case TestingState.SearchReading1:
				//	Result.SearchReadingTime1 = TimeSpan.MaxValue;
				//	break;
				//case TestingState.SearchReading2:
				//	Result.SearchReadingTime2 = TimeSpan.MaxValue;
				//	break;
				//case TestingState.SearchReading3:
				//	Result.SearchReadingTime3 = TimeSpan.MaxValue;
				//	break;
				case TestingState.Finish:
					{
						App.Current.TrialRecords.Add(Result);
						App.Current.CurrentTrialRecord = null;
						if (this.Frame.CanGoBack)
							this.Frame.GoBack();
					}
					break;
				default:
					break;
			}
			CurrentState = CurrentState + 1;
		}

		#region NavigationHelper registration

		/// <summary>
		/// The methods provided in this section are simply used to allow
		/// NavigationHelper to respond to the page's navigation methods.
		/// <para>
		/// Page specific logic should be placed in event handlers for the  
		/// <see cref="NavigationHelper.LoadState"/>
		/// and <see cref="NavigationHelper.SaveState"/>.
		/// The navigation parameter is available in the LoadState method 
		/// in addition to page state preserved during an earlier session.
		/// </para>
		/// </summary>
		/// <param name="e">Provides data for navigation methods and event
		/// handlers that cannot cancel the navigation request.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
		}

		#endregion

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }
	}
}
