using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using PlatformSegments = Xamarin.Forms.PlatformConfiguration.AndroidSpecific.Segments;

namespace Xamarin.Forms.Platform.Android
{
	//TODO: Move over to FastRenderers
	//public class SegmentsRenderer : FastRenderers.SegmentsRenderer
	//{
	//	[Obsolete("This constructor is obsolete as of version 2.5. Please use ButtonRenderer(Context) instead.")]
	//	[EditorBrowsable(EditorBrowsableState.Never)]
	//	public SegmentsRenderer() : base(Forms.Context)
	//	{
	//	}

	//	public SegmentsRenderer(Context context) : base(context)
	//	{
	//	}
	//}

	public class SegmentsRenderer : ViewRenderer<Segments, FormsSegments>
	{
		readonly Context _context;
		FormsSegments _control;
		Segments Segments => Element as Segments;

		IPlatformElementConfiguration<PlatformConfiguration.Android, Segments> _platformElementConfiguration;

		public SegmentsRenderer(Context context) : base(context)
		{
			_context = context;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Segments> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				InitializeControl(Element.Items);
			}

			if (e.OldElement != null && Control != null)
				InvalidateControl();
		}

		private void InvalidateControl()
		{
			_control.SegmentSelected -= SegmentSelected;
			((INotifyCollectionChanged)Element.Items).CollectionChanged -= SegmentsCollectionChanged;
		}

		void InitializeControl(IList<string> segments)
		{
			_control = new FormsSegments(_context);
			_control.SegmentSelected += SegmentSelected;

			((INotifyCollectionChanged)Element.Items).CollectionChanged += SegmentsCollectionChanged;

			PopulateSegments(segments);

			SetNativeControl(_control);
		}

		void PopulateSegments(IList<string> segments)
		{
			for (int i = 0; i < segments.Count; i++)
			{
				_control.Children.Add(segments[i]);
			}
		}

		private void SegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					for (int s = 0; s < e.NewItems.Count; s++)
					{
						_control.Children.Add(e.NewItems[s].ToString());
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					for (int s = 0; s < e.OldItems.Count; s++)
					{
						_control.Children.RemoveAt(e.OldStartingIndex);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
				default:
					_control.Children.Clear();
					break;
			}
		}

		private void SegmentSelected(object sender, SelectedPositionChangedEventArgs e)
		{
			Element.SelectedIndex = (int)e.SelectedPosition;
		}

		/// <summary>
		/// When a property of an element changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Xamarin.Forms Elements</param>
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (Control == null || Element == null)
				return;

			if (e.IsOneOf(Segments.SelectedIndexProperty))
			{
				var selectedRadioButton = (RadioButton)Control.GetChildAt(Element.SelectedIndex);
				Control.CurrentSegment = selectedRadioButton;
			}

			if (e.Is(Segments.ColorProperty))
			{
				Control.TintColor = Element.Color.ToAndroid();
			}

			if (e.Is(Segments.DisplayModeProperty))
				Control.DisplayMode = Element.DisplayMode;

			if (e.Is(PlatformSegments.CornerRadiusProperty)) {
				Control.CornerRadius = (float) Segments.On<PlatformConfiguration.Android>().GetCornerRadius();
			}
		}

		RadioButton GetSegment(int index)
		{
			return (RadioButton)Control?.GetChildAt(index);
		}

		IPlatformElementConfiguration<PlatformConfiguration.Android, Segments> OnThisPlatform()
		{
			if (_platformElementConfiguration == null)
				_platformElementConfiguration = Element.OnThisPlatform();

			return _platformElementConfiguration;
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			InvalidateControl();
		}
	}
}
