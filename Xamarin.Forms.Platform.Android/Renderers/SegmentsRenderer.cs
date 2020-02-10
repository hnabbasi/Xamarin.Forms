using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
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
			_control = new FormsSegments(_context, segments);
			_control.SegmentSelected += SegmentSelected;
			_control.Children = segments;
			SetNativeControl(_control);

			((INotifyCollectionChanged)Element.Items).CollectionChanged += SegmentsCollectionChanged;
		}

		private void SegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					//var index = e.NewStartingIndex;
					foreach (var segment in e.NewItems)
					{
						_control.Children.Add(segment.ToString());
					}
					//for (int s = 0; s < e.NewItems.Count; s++)
					//{
					//	_control.Children.Add(e.NewItems[index++].ToString());
					//}
					break;
				case NotifyCollectionChangedAction.Remove:
					for (int s = 0; s < e.OldItems.Count; s++)
					{
						_control.Children.RemoveAt(e.OldStartingIndex);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
				default:
					InitializeControl(Element.Items);
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
		}

		RadioButton GetSegment(int index)
		{
			return (RadioButton)Control?.GetChildAt(index);
		}

		protected override void Dispose(bool disposing)
		{
			_control.SegmentSelected -= SegmentSelected;
		}
	}
}
