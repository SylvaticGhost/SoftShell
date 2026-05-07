using Spectre.Console;

namespace SoftShell.Utils;

public record RGBColor(byte Red, byte Green, byte Blue)
{
    public override string ToString() => $"rgb({Red},{Green},{Blue})";
    
    public static implicit operator string(RGBColor color) => color.ToString();
    
    public Color ToColor() => new Color(Red, Green, Blue);
    public static implicit operator Color(RGBColor color) => color.ToColor();
    
    public Style ToStyle() => new Style(new Color(Red, Green, Blue));
}