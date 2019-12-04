using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UIKit;
using NativeImage = UIKit.UIImage;

namespace Xamarin.Forms.Platform.iOS
{
	public class SegmentsRenderer : ViewRenderer<Segments, UISegmentedControl>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Segments> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control == null)
				SetNativeControl(new UISegmentedControl());

			if (e.OldElement != null && Control != null)
				InvalidateControl();

			if (e.NewElement != null)
				InitializeControl();
		}

		void InitializeControl()
		{
			PopulateSegments(Element.Items);
			Control.ClipsToBounds = true;
			Control.SelectedSegment = Element.SelectedIndex;
			Control.BackgroundColor = Element.BackgroundColor.ToUIColor();
			Control.Layer.MasksToBounds = true;
			UpdateSelectedSegment(Element.SelectedIndex);

			Control.ValueChanged += OnSelectedIndexChanged;
			((INotifyCollectionChanged)Element.Items).CollectionChanged += SegmentsCollectionChanged;

			if (Element.IsColorSet)
			{
				Control.SelectedSegmentTintColor = Element.Color.ToUIColor();
				Control.TintColor = Element.Color.ToUIColor();
			}
		}

		void PopulateSegments(IList<string> segments)
		{
			for (int i = 0; i < segments.Count; i++)
			{
				InsertSegment(segments.ElementAt(i), i);
			}
		}

		void InsertSegment(string segment, int position)
		{
			switch (Element.DisplayMode)
			{
				case SegmentMode.Both:
					// Not native to iOS OTB.
					break;
				case SegmentMode.Image:
					var img = ((ImageSource)segment).GetNativeImageAsync().Result; // hmm...
					if (img != null)
						Control.InsertSegment(img, position, false);
					break;
				default:
				case SegmentMode.Text:
					Control.InsertSegment(segment, position, false);
					break;
			}
		}

		void InvalidateControl()
		{
			Control.ValueChanged -= OnSelectedIndexChanged;
			((INotifyCollectionChanged)Element.Items).CollectionChanged -= SegmentsCollectionChanged;
		}

		void SegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var startIndex = e.NewStartingIndex;
					foreach (var item in e.NewItems)
					{
						InsertSegment((string)item, startIndex++);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						Control.RemoveSegmentAtIndex(e.OldStartingIndex, false);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					Control.RemoveAllSegments();
					break;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (Control == null || Element == null)
				return;

			if (e.IsOneOf(Segments.SelectedItemProperty, Segments.SelectedIndexProperty))
				UpdateSelectedSegment(Element.SelectedIndex);

			if (e.Is(Segments.ColorProperty))
			{
				Control.SelectedSegmentTintColor = Element.Color.ToUIColor();
			}
		}

		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			Element.SelectedIndex = (int)Control.SelectedSegment;
		}

		void UpdateSelectedSegment(int index)
		{
			Control.SelectedSegment = index;
		}

		protected override void Dispose(bool disposing)
		{
			Control.ValueChanged -= OnSelectedIndexChanged;
			Control.Dispose();
			base.Dispose(disposing);
		}
	}
}