using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class SegmentsPageAndroid : ContentPage
	{
		double _cornerRadius;
		Segments _element;
		Stepper _radiusStepper;
		Label _radiusValue;

		public SegmentsPageAndroid()
		{
			_element = new Segments
			{
				Items = { "Item 1", "Item 2", "Item 3" }
			};

			_radiusStepper = new Stepper
			{
				Minimum = 0,
				Maximum = 100,
				Value = 8.0,
				VerticalOptions = LayoutOptions.Center
			};
			_radiusStepper.ValueChanged += _radiusStepper_ValueChanged;

			_radiusValue = new Label
			{
				HorizontalOptions = LayoutOptions.EndAndExpand,
				VerticalOptions = LayoutOptions.Center
			};

			Content = new StackLayout {
				Padding = new Thickness(20.0),
				Children = {
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							new Label {
								Text = "Corner Radius:",
								VerticalOptions = LayoutOptions.Center
							},
							_radiusValue,
							_radiusStepper
						},
						VerticalOptions = LayoutOptions.CenterAndExpand
					},
					_element
				}
			};
		}

		private void _radiusStepper_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			_element.On<PlatformConfiguration.Android>().SetCornerRadius(e.NewValue);
			_radiusValue.Text = e.NewValue.ToString();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_radiusStepper.ValueChanged -= _radiusStepper_ValueChanged;
		}
	}
}
