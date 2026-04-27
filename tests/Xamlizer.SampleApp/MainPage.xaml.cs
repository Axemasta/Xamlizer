namespace Xamlizer.SampleApp;

public partial class MainPage : ContentPage
{
    // Cycle through these resource keys on each button click.
    // The keys come from the Xamlizer-generated AppColors class — no magic strings needed.
    //
    // Before Xamlizer:  Application.Current!.Resources["Primary"]
    // After  Xamlizer:  Application.Current!.Resources[AppColors.Color.Primary]
    private static readonly string[] ColorKeys =
    [
        ColorsKeys.Color.Primary,
        ColorsKeys.Color.Secondary,
        ColorsKeys.Color.Tertiary,
        ColorsKeys.Color.Magenta,
        ColorsKeys.Color.MidnightBlue,
    ];
    
    public MainPage()
    {
        InitializeComponent();
        ApplyColor(ColorKeys[0]);
    }

    private void ApplyColor(string resourceKey)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var value) == true
            && value is Color color)
        {
            ColorSwatch.Color = color;
        }

        ColorKeyLabel.Text = $"Resource key: \"{resourceKey}\"";
    }
}
