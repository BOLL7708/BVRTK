namespace BVRTK.Data;

public static partial class SettingsChangeHandlers
{
    public delegate void ValueChangeHandler<in T>(T currentValue, T previousValue);
}