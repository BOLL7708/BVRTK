using BVRTKCG.Attributes;

namespace BVRTK.Data.Setting;

[Setting]
public partial class KeyboardSimulator
{
    [GuiCheckbox("Enabled", "Enable keyboard simulation")]
    private bool _enabled = false;
    public partial bool Enabled { get; set; }
}