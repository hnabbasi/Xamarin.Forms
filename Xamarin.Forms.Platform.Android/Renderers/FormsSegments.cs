using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using AColor = Android.Graphics.Color;
using AViews = Android.Views;
using ARes = Android.Resource;
using Java.Util;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsSegments : RadioGroup, IDisposable
	{
		private readonly Context _context;
		readonly float _defaultControlHeight = 32.0f;
		readonly float _defaultTextSize = 15.0f;
		readonly float _defaultStrokeWidth = 2.5f;
		readonly float _defaultCornerRadius = 6.0f;
		readonly int _defaultButtonPadding = 16;

		readonly SegmentMode _mode = SegmentMode.Image;

		public IList<string> Children { get; set; } = new List<string>();
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

		public FormsSegments(Context context, IList<string> segments) : base(context)
		{
			_context = context;
			Children = segments;
			//((INotifyCollectionChanged)Children).CollectionChanged += CollectionChanged;
			Build();
		}

		//private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		//{
		//	//Build();
		//	InitializeSegments();
		//}

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
			InitializeSegments();
			CheckedChange += OnCheckChanged;
		}

		void InitializeSegments()
		{
			//RemoveViews(0, Children.Count - 1);
			for (var i = 0; i < Children.Count; i++)
			{
				var position = i == 0 ? Position.Left : i == Children.Count - 1 ? Position.Right : Position.Middle;
				var rb = GetRadioButton(Children[i], position);
				ConfigureRadioButton(i, rb);
				AddView(rb);
			}
		}

		void InsertSegment(string title)
		{

		}

		private void OnCheckChanged(object sender, CheckedChangeEventArgs e)
		{
			//var rb = FindViewById<RadioButton>(e.CheckedId);
			//SetSegment(rb);
			//var index = IndexOfChild(rb);
			//ConfigureRadioButton(index, rb);

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

			rb.SetPadding(_defaultButtonPadding, _defaultButtonPadding, _defaultButtonPadding, _defaultButtonPadding);
			rb.SetBackground(GetRadioButtonStateListDrawable(position));
			rb.LayoutParameters = new RadioGroup.LayoutParams(0, LayoutParams.MatchParent, 1.0f);
			rb.SetHeight(_buttonHeight);
			rb.SetTextSize(ComplexUnitType.Sp, _defaultTextSize);
			rb.SetAllCaps(true);
			rb.SetTypeface(null, TypefaceStyle.Bold);

			if (_mode == SegmentMode.Image)
			{
				rb.Text = "Image";

				var icon = GetImage(title);
				rb.SetButtonDrawable(null);
				rb.SetCompoundDrawablesWithIntrinsicBounds(
					icon,
					icon,
					icon,
					icon);

			}

			return rb;
		}

		private Drawable GetImage(ImageSource filePath)
		{
			var tcs = new TaskCompletionSource<Drawable>();

			Task.Run(async () => {
				try
				{
					var result = await ResourceManager.GetFormsDrawableAsync(Context, filePath);
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			//	.ContinueWith((task) => {
			//	Console.WriteLine(">>> Something went wrong");
			//}, TaskContinuationOptions.OnlyOnFaulted);
			return tcs.Task.Result;
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
				//((INotifyCollectionChanged)Children).CollectionChanged += CollectionChanged;
				CheckedChange -= OnCheckChanged;
			}
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
