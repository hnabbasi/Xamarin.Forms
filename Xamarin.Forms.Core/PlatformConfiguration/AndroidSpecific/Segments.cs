namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.Segments;

	public static class Segments
	{
		public static BindableProperty CornerRadiusProperty = BindableProperty.Create("CornerRadius", typeof(double), typeof(Segments));

		public static double GetCornerRadius(BindableObject element)
		{
			return (double) element.GetValue(CornerRadiusProperty);
		}

		public static void SetCornerRadius(BindableObject element, double value)
		{
			element.SetValue(CornerRadiusProperty, value);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetCornerRadius(this IPlatformElementConfiguration<Android, FormsElement> config, double value)
		{
			SetCornerRadius(config.Element, value);
			return config;
		}

		public static double GetCornerRadius(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetCornerRadius(config.Element);
		}
	}
}