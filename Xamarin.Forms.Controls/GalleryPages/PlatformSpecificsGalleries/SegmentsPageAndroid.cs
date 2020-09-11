using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class SegmentsPageAndroid : ContentPage
	{
		readonly static double DefaultCornerRadius = 8.0;

		readonly static string[] Icons = {
			"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png",
			"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png",
			"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png"
		};

		readonly Segments _element = new Segments
		{
			ItemsSource = new string[] { "Item 1", "Item 2", "Item 3" }
		};

		readonly Segments _elementIcons = new Segments
		{
			DisplayMode = SegmentMode.Image,
			ItemsSource = Icons
		};

		//readonly Segments _elementLeft = new Segments
		//{
		//	DisplayMode = SegmentMode.ImageLeft,
		//	ItemsSource = Icons
		//};

		//readonly Segments _elementRight = new Segments
		//{
		//	DisplayMode = SegmentMode.ImageRight,
		//	ItemsSource = Icons
		//};

		//readonly Segments _elementTop = new Segments
		//{
		//	DisplayMode = SegmentMode.ImageTop,
		//	ItemsSource = Icons
		//};

		//readonly Segments _elementBottom = new Segments
		//{
		//	DisplayMode = SegmentMode.ImageBottom,
		//	ItemsSource = Icons
		//};

		readonly Stepper _radiusStepper = new Stepper
		{
			Minimum = 0,
			Maximum = 100,
			Value = DefaultCornerRadius,
			VerticalOptions = LayoutOptions.Center
		};

		readonly Label _radiusValue = new Label
		{
			Text = DefaultCornerRadius.ToString(),
			HorizontalOptions = LayoutOptions.EndAndExpand,
			VerticalOptions = LayoutOptions.Center
		};

		public SegmentsPageAndroid()
		{
			_radiusStepper.ValueChanged += _radiusStepper_ValueChanged;
			
			Content = new StackLayout {
				Children = {
					new StackLayout {
						Padding = new Thickness(20.0),
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label {
								Text = "Corner Radius:",
								VerticalOptions = LayoutOptions.Center
							},
							_radiusValue,
							_radiusStepper
						},
						VerticalOptions = LayoutOptions.EndAndExpand
					},
					new ScrollView
					{
						Content = new StackLayout
						{
							Padding = new Thickness(20.0),
							Children = {
								new Label { Text = "Text" },
								_element,
								new Label { Text = "Image" },
								_elementIcons,
								//new Label { Text = "ImageLeft" },
								//_elementLeft,
								//new Label { Text = "ImageRight" },
								//_elementRight,
								//new Label { Text = "ImageTop" },
								//_elementTop,
								//new Label { Text = "ImageBottom" },
								//_elementBottom
							}
						}
					}
				}
			};
		}

		private void _radiusStepper_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			_element.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);
			_elementIcons.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);
			//_elementLeft.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);
			//_elementRight.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);
			//_elementTop.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);
			//_elementBottom.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);

			_radiusValue.Text = e.NewValue.ToString();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_radiusStepper.ValueChanged -= _radiusStepper_ValueChanged;
		}
	}
}
