using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_SegmentsRenderer))]
	public class Segments : View, IColorElement, IElementConfiguration<Segments>
	{
		private readonly Lazy<PlatformConfigurationRegistry<Segments>> _platformConfigurationRegistry;

		public event EventHandler<SelectedItemChangedEventArgs> SelectedIndexChanged;
		public static BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(Segments));
		public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Segments), 0, propertyChanged: OnSegmentSelected);

		public static BindableProperty DisplayModeProperty = BindableProperty.Create(nameof(DisplayMode), typeof(SegmentMode), typeof(Segments), SegmentMode.Text);

		public Segments()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Segments>>(() => new PlatformConfigurationRegistry<Segments>(this));
		}

		public IList<string> Items { get; } = new LockableObservableListWrapper();

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Segments), default(IList),
									propertyChanged: OnItemsSourceChanged);

		public static readonly BindableProperty SelectedItemProperty =
			BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(Segments), null, BindingMode.TwoWay,
									propertyChanged: OnSelectedItemChanged);

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public SegmentMode DisplayMode
		{
			get { return (SegmentMode)GetValue(DisplayModeProperty); }
			set { SetValue(DisplayModeProperty, value); }
		}

		BindingBase _itemDisplayBinding;
		public BindingBase ItemDisplayBinding
		{
			get { return _itemDisplayBinding; }
			set
			{
				if (_itemDisplayBinding == value)
					return;

				OnPropertyChanging();
				_itemDisplayBinding = value;
				ResetItems();
				OnPropertyChanged();
			}
		}

		static readonly BindableProperty s_displayProperty =
			BindableProperty.Create("Display", typeof(string), typeof(Segments), default(string));

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Segments)bindable)?.OnItemsSourceChanged((IList)oldValue, (IList)newValue);
		}

		void OnItemsSourceChanged(IList oldValue, IList newValue)
		{
			var oldObservable = oldValue as INotifyCollectionChanged;
			if (oldObservable != null)
				oldObservable.CollectionChanged -= CollectionChanged;

			var newObservable = newValue as INotifyCollectionChanged;
			if (newObservable != null)
			{
				newObservable.CollectionChanged += CollectionChanged;
			}

			if (newValue != null)
				ResetItems();
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(e);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveItems(e);
					break;
				default: //Move, Replace, Reset
					ResetItems();
					break;
			}
		}
		void AddItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
			foreach (object newItem in e.NewItems)
				((LockableObservableListWrapper)Items).InternalInsert(index++, GetDisplayMember(newItem));
		}

		void RemoveItems(NotifyCollectionChangedEventArgs e)
		{
			int index = e.OldStartingIndex < Items.Count ? e.OldStartingIndex : Items.Count;
			foreach (object _ in e.OldItems)
				((LockableObservableListWrapper)Items).InternalRemoveAt(index--);
		}

		void ResetItems()
		{
			if (ItemsSource == null)
				return;
			((LockableObservableListWrapper)Items).InternalClear();
			foreach (object item in ItemsSource)
				((LockableObservableListWrapper)Items).InternalAdd(GetDisplayMember(item));
			UpdateSelectedItem(SelectedIndex);
		}

		string GetDisplayMember(object item)
		{
			if (ItemDisplayBinding == null)
				return item.ToString();

			ItemDisplayBinding.Apply(item, this, s_displayProperty);
			ItemDisplayBinding.Unapply();

			return (string)GetValue(s_displayProperty);
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var segments = (Segments)bindable;
			segments.SelectedItem = newValue;
		}

		void UpdateSelectedItem(int index)
		{
			if (index == -1)
			{
				SelectedItem = null;
				return;
			}

			if (ItemsSource != null)
			{
				SelectedItem = ItemsSource[index];
				return;
			}

			SelectedItem = Items[index];
		}

		public int SelectedIndex
		{
			get => (int)GetValue(SelectedIndexProperty);
			set => SetValue(SelectedIndexProperty, value);
		}

		private static void OnSegmentSelected(BindableObject bindable, object oldValue, object newValue)
		{
			if (!(bindable is Segments segment))
				return;
			int.TryParse(newValue?.ToString(), out int index);
			segment.SelectedIndexChanged?.Invoke(segment, new SelectedItemChangedEventArgs(segment?.Items[index], index));
		}

		// IColorElement
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set { SetValue(ColorProperty, value); }
		}

		// IElementConfiguration<>
		public IPlatformElementConfiguration<T, Segments> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		public bool IsColorSet => IsSet(ColorProperty);
	}

	/// <summary>
	/// Segment mode e.g. Text, Image, Both (Android ONLY)
	/// </summary>
	public enum SegmentMode
	{
		Text,
		Image,
		Both,
		ImageLeft,
		ImageRight,
		ImageTop,
		ImageBottom
	}
}
