using System.Text;
using Valve.VR;

namespace BVRTK;

public static partial class Utils
{
    public static int ConvertMouseButton(uint steamVrButton) => (int)steamVrButton switch
    {
        (int)EVRMouseButton.Left => 0,
        (int)EVRMouseButton.Right => 1,
        (int)EVRMouseButton.Middle => 2,
        _ => -1
    };

    /**
     * Take the input bytes, trim it to the right length, encode them to UTF8.
     */
    public static string ConvertKeyboardChar(in VREvent_Keyboard_t kb)
    {
        Span<byte> bytes =
        [
            kb.cNewInput0, kb.cNewInput1, kb.cNewInput2, kb.cNewInput3,
            kb.cNewInput4, kb.cNewInput5, kb.cNewInput6, kb.cNewInput7
        ];
        var len = bytes.IndexOf((byte)0); // Locating the null terminator
        if (len < 0) len = bytes.Length;
        return Encoding.UTF8.GetString(bytes[..len]);
    }
}