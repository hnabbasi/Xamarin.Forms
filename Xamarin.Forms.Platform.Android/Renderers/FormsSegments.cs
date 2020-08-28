using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AColor = Android.Graphics.Color;
using ARes = Android.Resource;
using AContentRes = Android.Content.Res;
using AViews = Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsSegments : RadioGroup, IDisposable
	{
		private readonly Context _context;
		readonly float _defaultControlHeight = 32.0f;
		readonly float _defaultTextSize = 15.0f;
		readonly float _defaultStrokeWidth = 2.5f;
		readonly int _defaultButtonPadding = 16;

		private SegmentMode _mode;
		public SegmentMode DisplayMode
		{
			get => _mode;
			set
			{
				_mode = value;
				InitializeSegments();
			}
		}

		private float _cornerRadius = 8.0f;
		public float CornerRadius
		{
			get => _cornerRadius;
			set {
				_cornerRadius = _context.ToPixels(value);

				//TODO: Fix this so only the end segments are updated
				InitializeSegments();

				//if (ChildCount > 0)
				//{
				//	RemoveViewAt(0);
				//	AddView(GetRadioButton(Children[0], Position.Left));
				//}

				//if(ChildCount > 1)
				//{
				//	RemoveViewAt(Children.Count - 1);
				//	AddView(GetRadioButton(Children[Children.Count - 1], Position.Right));
				//}
			}
		}

		private AColor _tintColor = AColor.Rgb(14, 98, 255);
		public AColor TintColor
		{
			get => _tintColor;
			set
			{
				_tintColor = value;
				InitializeSegments();
			}
		}

		private AColor _backgroundColor = AColor.Transparent;
		public AColor BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				SetBackgroundColor(value);
			}
		}

		public ObservableCollection<string> Children { get; } = new ObservableCollection<string>();

		private RadioButton _currentSegment;
		public RadioButton CurrentSegment
		{
			get => _currentSegment;
			set
			{
				_currentSegment = value;
			}
		}

		public event EventHandler<SelectedPositionChangedEventArgs> SegmentSelected;

		//AColor _strokeColor;
		AColor _unselectedTintColor;
		AColor _unSelectedTextColor;
		AColor _selectedTextColor;
		AColor _disabledColor = AColor.Gray;

		int _buttonHeight;
		int _strokeWidth;
		bool _disposed;

		public FormsSegments(Context context) : base(context)
		{
			_context = context;
			Children.CollectionChanged += CollectionChanged;
			Build();
		}

		private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			InitializeSegments();
		}

		void Build()
		{
			_buttonHeight = (int)_context.ToPixels(_defaultControlHeight);
			_strokeWidth = (int)_context.ToPixels(_defaultStrokeWidth);

			// Temporarily disabling these
			_unselectedTintColor = _backgroundColor;// Element.IsUnselectedTintColorSet() ? Element.UnselectedTintColor.ToAndroid() : _backgroundColor;
			_selectedTextColor = AColor.White;// Element.SelectedTextColor.ToAndroid();
			_unSelectedTextColor = TintColor;// Element.IsUnselectedTextColorSet() ? Element.UnselectedTextColor.ToAndroid() : TintColor;

			//_strokeColor = TintColor;
			SetBackgroundColor(_backgroundColor);

			Orientation = Orientation.Horizontal;
			LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
			CheckedChange += OnCheckChanged;
		}

		public void InitializeSegments()
		{
			if (ChildCount > 0)
				RemoveAllViews();
			
			for (var i = 0; i < Children.Count; i++)
			{
				var rb = InsertSegment(Children[i].ToString(), i);
				if (i == 0)
					rb.Checked = true;
			}
		}

		RadioButton InsertSegment(string title, int index)
		{
			var isLeft = index == 0;
			var position = isLeft ? Position.Left : index == Children.Count - 1 ? Position.Right : Position.Middle;
			var rb = GetRadioButton(title, position);
			AddView(rb);
			return rb;
		}

		private AContentRes.ColorStateList TextColorSelector
		{
			get
			{
				return new AContentRes.ColorStateList(
					new int[][] {
						//states
						new int[] { ARes.Attribute.StateChecked },
						new int[] {-ARes.Attribute.StateChecked } },
						//colors
						new int[] { _selectedTextColor, TintColor }
				);
			}
		}

		private AContentRes.ColorStateList SegmentColorSelector
		{
			get
			{
				return new AContentRes.ColorStateList(
						new int[][] {
						//states
						new int[] { ARes.Attribute.StateChecked },
						new int[] {-ARes.Attribute.StateChecked } },
							//colors
							new int[] { TintColor, _unselectedTintColor }
					);
			}
		}

		private void OnCheckChanged(object sender, CheckedChangeEventArgs e)
		{
			CurrentSegment = FindViewById<RadioButton>(e.CheckedId);
			SegmentSelected?.Invoke(this, new SelectedPositionChangedEventArgs(IndexOfChild(CurrentSegment)));
		}
		
		#region Drawable Resources

		RadioButton GetRadioButton(string title, Position position)
		{
			Drawable icon;
			var rb = new RadioButton(_context);
			rb.SetPadding(_defaultButtonPadding, _defaultButtonPadding, _defaultButtonPadding, _defaultButtonPadding);
			rb.SetBackground(GetRadioButtonStateListDrawable(position));
			rb.LayoutParameters = new RadioGroup.LayoutParams(0, LayoutParams.MatchParent, 1.0f);
			//rb.SetHeight(_buttonHeight);
			rb.Gravity = GravityFlags.Center;
			rb.SetButtonDrawable(null);

			//TODO: Reconsider SegmentsViewItem to have title and image
			switch (_mode)
			{				
				case SegmentMode.ImageLeft:
					icon = GetImage(title);
					rb.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
					break;
				case SegmentMode.ImageTop:
					icon = GetImage(title);
					rb.SetCompoundDrawablesWithIntrinsicBounds(null, icon, null, null);
					break;
				case SegmentMode.ImageRight:
					icon = GetImage(title);
					rb.SetCompoundDrawablesWithIntrinsicBounds(null, null, icon, null);
					break;
				case SegmentMode.ImageBottom:
					icon = GetImage(title);
					rb.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, icon);
					break;
				default:
				case SegmentMode.Text:
					rb.Text = title;
					rb.TextAlignment = AViews.TextAlignment.Center;

					rb.SetTextSize(ComplexUnitType.Sp, _defaultTextSize);
					rb.SetAllCaps(true);
					rb.SetTypeface(null, TypefaceStyle.Bold);
					rb.SetTextColor(TextColorSelector);
					break;
			}
			return rb;
		}

		private Drawable GetImage(ImageSource filePath)
		{
			var tcs = new TaskCompletionSource<Drawable>();

			Task.Run(async () =>
			{
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

			return tcs.Task.Result;
		}

		StateListDrawable GetRadioButtonStateListDrawable(Position position)
		{
			var drawable = new StateListDrawable();
			drawable.AddState(new int[] { ARes.Attribute.StateChecked }, GetSegmentDrawable(position));
			drawable.AddState(new int[] { -ARes.Attribute.StateChecked }, GetSegmentDrawable(position));
			return drawable;
		}

		InsetDrawable GetSegmentDrawable(Position position)
		{
			var rect = new GradientDrawable();
			rect.SetShape(ShapeType.Rectangle);
			rect.SetStroke(_strokeWidth, TintColor);
			rect.SetColor(SegmentColorSelector);
			
			switch (position)
			{
				case Position.Left:
					rect.SetCornerRadii(new float[] { CornerRadius, CornerRadius, 0, 0, 0, 0, CornerRadius, CornerRadius });
					return new InsetDrawable(rect, 0);

				case Position.Right:
					rect.SetCornerRadii(new float[] { 0, 0, CornerRadius, CornerRadius, CornerRadius, CornerRadius, 0, 0 });
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
				Children.CollectionChanged -= CollectionChanged;
				CheckedChange -= OnCheckChanged;
			}
			base.Dispose(disposing);
		}
	}

	/// <summary>
	/// Position of the segment. Left, Middle, Right.
	/// </summary>
	internal enum Position
	{
		Middle,
		Left,
		Right
	}
}
