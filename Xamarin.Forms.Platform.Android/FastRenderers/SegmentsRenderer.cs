using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AColor = Android.Graphics.Color;
using AViews = Android.Views;
using ARes = Android.Resource;
using AView = Android.Views.View;
using Android.Support.V4.View;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public class SegmentsRenderer : RadioGroup, IVisualElementRenderer, IViewRenderer, ITabStop, IDisposable
	{
		private readonly Context _context;
		readonly float _defaultControlHeight = 30.0f;
		readonly float _defaultTextSize = 15.0f;
		readonly float _defaultStrokeWidth = 3.0f;
		readonly float _defaultCornerRadius = 6.0f;
		int? _defaultLabelFor;

		//AColor _strokeColor;
		AColor _unselectedTintColor;
		AColor _unSelectedTextColor;
		AColor _selectedTextColor;
		AColor _backgroundColor = Color.White.ToAndroid();
		AColor _disabledColor = AColor.Gray;

		int _buttonHeight;
		int _strokeWidth;
		int _cornerRadius;
		bool _disposed;

		Segments _element;
		VisualElementPackager _visualElementPackager;
		VisualElementRenderer _visualElementRenderer;

		//public string[] Children { get; set; }
		public AColor TintColor { get; set; } = AColor.Rgb(14, 98, 255);

		private RadioButton _currentSegment;
		VisualElementTracker _visualElementTracker;

		public RadioButton CurrentSegment
		{
			get => _currentSegment;
			set
			{
				UnsetSegment(_currentSegment);
				_currentSegment = value;
				SetSegment(value);
				UpdateButtonColors(value);
			}
		}

		public event EventHandler<SelectedPositionChangedEventArgs> SegmentSelected;

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ButtonRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SegmentsRenderer() : base(Forms.Context)
		{
			Initialize();
		}

		public SegmentsRenderer(Context context) : base(context)
		{
			_context = context;
			Initialize();
		}

		void Initialize()
		{
			_visualElementRenderer = new VisualElementRenderer(this);
		}

		void InitializeControl()
		{
			//_strokeColor = TintColor;
			_buttonHeight = (int)_context.ToPixels(_defaultControlHeight);
			_strokeWidth = (int)_context.ToPixels(_defaultStrokeWidth);
			_cornerRadius = (int)_context.ToPixels(_defaultCornerRadius);

			// Temporarily disabling these
			_unselectedTintColor = _backgroundColor;// Element.IsUnselectedTintColorSet() ? Element.UnselectedTintColor.ToAndroid() : _backgroundColor;
			_selectedTextColor = AColor.White;// Element.SelectedTextColor.ToAndroid();
			_unSelectedTextColor = TintColor;// Element.IsUnselectedTextColorSet() ? Element.UnselectedTextColor.ToAndroid() : TintColor;

			Orientation = Orientation.Horizontal;
			LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
			CheckedChange += OnCheckChanged;
			UpdateBackgroundColor();
			PopulateSegments(Element?.Items);
		}

		void PopulateSegments(IList<string> children)
		{
			if (children == null)
				return;

			for (var i = 0; i < children.Count; i++)
			{
				var position = i == 0 ? Position.Left : i == children.Count - 1 ? Position.Right : Position.Middle;
				InsertSegment(children[i]?.ToString(), i, position);
			}
		}

		void InsertSegment(string text, int index, Position position = Position.Right)
		{
			RadioButton rb;
			if (Element.DisplayMode == SegmentMode.Text)
				rb = GetRadioButton(text, position);
			//else if (Element.DisplayMode == SegmentMode.Image)

			//rb = GetRadioButton(new BitmapDrawable(text), position);
			else
				rb = null;
			ConfigureRadioButton(index, rb);
			AddView(rb);
			Console.WriteLine($">>> Children count: {ChildCount}");
		}

		void InvalidateControl()
		{
			CheckedChange -= OnCheckChanged;
			((INotifyCollectionChanged)Element.Items).CollectionChanged -= SegmentsCollectionChanged;
		}

		private void OnCheckChanged(object sender, CheckedChangeEventArgs e)
		{
			//var rb = FindViewById<RadioButton>(e.CheckedId);
			//SetSegment(rb);
			//var index = IndexOfChild(rb);
			////ConfigureRadioButton(index, rb);

			CurrentSegment = FindViewById<RadioButton>(e.CheckedId);
			SegmentSelected?.Invoke(this, new SelectedPositionChangedEventArgs(IndexOfChild(CurrentSegment)));
		}

		void ConfigureRadioButton(int i, RadioButton rb)
		{
			if (i == 0)
			{
				SetSegment(rb);
			}
			else
			{
				UnsetSegment(rb);
			}

			UpdateButtonColors(rb);
		}

		void SetSegment(RadioButton rb)
		{
			rb?.SetTextColor(_selectedTextColor);
		}

		void UnsetSegment(RadioButton rb)
		{
			rb?.SetTextColor(_unSelectedTextColor);
		}

		void UpdateButtonColors(RadioButton rb)
		{
			var gradientDrawable = (StateListDrawable)rb.Background;
			var drawableContainerState = (DrawableContainer.DrawableContainerState)gradientDrawable.GetConstantState();
			var children = drawableContainerState.GetChildren();

			// Make sure it works on API < 18
			var _selectedShape = (GradientDrawable)(children[0] as InsetDrawable)?.Drawable;
			_selectedShape.SetColor(TintColor);

			var _unselectedShape = children[1] is GradientDrawable ? (GradientDrawable)children[1] : (GradientDrawable)((InsetDrawable)children[1]).Drawable;
			_unselectedShape.SetColor(_unselectedTintColor);
		}

		#region Drawable Resources

		RadioButton GetRadioButton(string title, Position position)
		{
			var rb = new RadioButton(_context)
			{
				Text = title,
				Gravity = GravityFlags.Center,
				TextAlignment = AViews.TextAlignment.Center
			};

			rb.SetButtonDrawable(null);
			rb.SetBackground(GetRadioButtonStateListDrawable(position));
			rb.LayoutParameters = new RadioGroup.LayoutParams(0, LayoutParams.MatchParent, 1.0f);
			rb.SetHeight(_buttonHeight);
			rb.SetTextSize(ComplexUnitType.Sp, _defaultTextSize);
			rb.SetAllCaps(true);
			rb.SetTypeface(null, TypefaceStyle.Bold);
			return rb;
		}

		RadioButton GetRadioButton(BitmapDrawable image, Position position)
		{
			var rb = new RadioButton(_context)
			{
				Text = null,
				Gravity = GravityFlags.Center,
			};

			rb.SetButtonDrawable(image);
			rb.SetBackground(GetRadioButtonStateListDrawable(position));
			rb.LayoutParameters = new RadioGroup.LayoutParams(0, LayoutParams.MatchParent, 1.0f);
			rb.SetHeight(_buttonHeight);
			return rb;
		}

		StateListDrawable GetRadioButtonStateListDrawable(Position position)
		{
			var drawable = new StateListDrawable();
			drawable.AddState(new int[] { ARes.Attribute.StateChecked }, GetCheckedDrawable(position));
			drawable.AddState(new int[] { -ARes.Attribute.StateChecked }, GetUncheckedDrawable(position));
			return drawable;
		}

		InsetDrawable GetCheckedDrawable(Position position)
		{
			var rect = new GradientDrawable();
			rect.SetShape(ShapeType.Rectangle);
			rect.SetColor(TintColor);
			rect.SetStroke(_strokeWidth, TintColor);

			switch (position)
			{
				case Position.Left:
					rect.SetCornerRadii(new float[] { _cornerRadius, _cornerRadius, 0, 0, 0, 0, _cornerRadius, _cornerRadius });
					return new InsetDrawable(rect, 0);

				case Position.Right:
					rect.SetCornerRadii(new float[] { 0, 0, _cornerRadius, _cornerRadius, _cornerRadius, _cornerRadius, 0, 0 });
					break;
				default:
					rect.SetCornerRadius(0);
					break;
			}

			return new InsetDrawable(rect, -_strokeWidth, 0, 0, 0);
		}

		InsetDrawable GetUncheckedDrawable(Position position)
		{
			var rect = new GradientDrawable();
			rect.SetShape(ShapeType.Rectangle);
			rect.SetColor(_backgroundColor);
			rect.SetStroke(_strokeWidth, TintColor);

			switch (position)
			{
				case Position.Left:
					rect.SetCornerRadii(new float[] { _cornerRadius, _cornerRadius, 0, 0, 0, 0, _cornerRadius, _cornerRadius });
					return new InsetDrawable(rect, 0);
				case Position.Right:
					rect.SetCornerRadii(new float[] { 0, 0, _cornerRadius, _cornerRadius, _cornerRadius, _cornerRadius, 0, 0 });
					break;
				default:
					rect.SetCornerRadius(0);
					break;
			}

			return new InsetDrawable(rect, -_strokeWidth, 0, 0, 0);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing)
			{
				if (Element != null)
					Element.PropertyChanged -= OnElementPropertyChanged;

				CheckedChange -= OnCheckChanged;
				_visualElementRenderer?.Dispose();
				_visualElementTracker?.Dispose();
			}
			base.Dispose(disposing);
		}

		public Segments Element
		{
			get => _element;
			set
			{
				if (value == null)
					return;

				Segments oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Segments>(oldElement, _element));
			}
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Segments> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
				InvalidateControl();
			}

			if (e.NewElement != null)
			{
				this.EnsureId();
				InitializeControl();

				if (_visualElementTracker == null)
				{
					_visualElementTracker = new VisualElementTracker(this);
					_visualElementPackager = new VisualElementPackager(this);
					_visualElementPackager.Load();
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				((INotifyCollectionChanged)Element.Items).CollectionChanged += SegmentsCollectionChanged;
			}
		}

		private void SegmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					//PopulateSegments(e.NewItems);
					var startIndex = e.NewStartingIndex;
					foreach (var item in e.NewItems)
					{
						InsertSegment((string)item, startIndex++);
					}
					Console.WriteLine($">>> Inserted: Children count: {ChildCount}");
					break;
				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						RemoveViewAt(e.OldStartingIndex);
					}
					Console.WriteLine($">>> Removed: Children count: {ChildCount}");
					break;
				case NotifyCollectionChangedAction.Reset:
					RemoveAllViews();
					break;
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();

			if (Element == null)
				return;

			if (e.IsOneOf(Segments.SelectedItemProperty, Segments.SelectedIndexProperty))
				//CurrentSegment = IndexOfChild()

				if (e.Is(Segments.ColorProperty))
				{
					TintColor = Element.Color.ToAndroid();
				}
		}

		void UpdateBackgroundColor()
		{
			SetBackgroundColor(Element.BackgroundColor.ToAndroid());
		}


		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;
		ViewGroup IVisualElementRenderer.ViewGroup => this;
		VisualElement IVisualElementRenderer.Element => Element;
		AView IVisualElementRenderer.View => this;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			return new SizeRequest(_context.FromPixels(20, 20));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (!(element is Segments segments))
				throw new ArgumentException("Element must be of type Segments");

			Element = segments;
			if (!string.IsNullOrWhiteSpace(Element.AutomationId))
				ContentDescription = Element.AutomationId;
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			var tracker = _visualElementTracker;
			tracker?.UpdateLayout();
		}

		public void MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, _context);
		}

		public AViews.View TabStop => this;
	}

	/// <summary>
	/// Position of the segment. Left, Middle, Right.
	/// </summary>
	enum Position
	{
		Middle,
		Left,
		Right
	}
}
