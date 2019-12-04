using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class SegmentsCoreGalleryPage : CoreGalleryPage<Segments>
	{
		private List<string> _segments;

		protected override void InitializeElement(Segments element)
		{
			base.InitializeElement(element);
			_segments = new List<string> {
				"View A",
				"View B",
				"View C"
			};
			element.ItemsSource = _segments;
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var icons = new List<string> {
				"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png",
				"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png",
				"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png"
			};

			Add(new ValueViewContainer<Segments>(Test.Segments.Color, new Segments
			{
				Color = Color.LightBlue,
				ItemsSource = _segments
			}, "Color", value => value.ToString()));

			Add(new ValueViewContainer<Segments>(Test.Segments.Image, new Segments
			{
				DisplayMode = SegmentMode.Image,
				ItemsSource = icons
			}, "Image", value => value.ToString()));

			Add(new ValueViewContainer<Segments>(Test.Segments.SelectedIndexChanged, new Segments
			{
				ItemsSource = _segments
			}, "SelectedIndex", value => value.ToString()));
		}
	}
}
