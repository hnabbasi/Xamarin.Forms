using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using AColor = Android.Graphics.Color;
using AViews = Android.Views;
using ARes = Android.Resource;
using Java.Util;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsSegments : RadioGroup, IDisposable
	{
		private readonly Context _context;
		readonly float _defaultControlHeight = 30.0f;
		readonly float _defaultTextSize = 15.0f;
		readonly float _defaultStrokeWidth = 3.0f;
		readonly float _defaultCornerRadius = 6.0f;

		public string[] Children { get; set; }
		public AColor TintColor { get; set; } = AColor.Rgb(14, 98, 255);

		private RadioButton _currentSegment;
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

		public FormsSegments(Context context) : this(context, new string[] { "Not Set", "Not Set" }) { }

		public FormsSegments(Context context, string[] segments) : base(context)
		{
			_context = context;
			Children = segments;
			Build();
		}

		void Build()
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

			for (var i = 0; i < Children.Length; i++)
			{
				var position = i == 0 ? Position.Left : i == Children.Length - 1 ? Position.Right : Position.Middle;
				var rb = GetRadioButton(Children[i], position);
				ConfigureRadioButton(i, rb);
				AddView(rb);
			}

			CheckedChange += OnCheckChanged;
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
				CheckedChange -= OnCheckChanged;

			base.Dispose(disposing);
		}
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
