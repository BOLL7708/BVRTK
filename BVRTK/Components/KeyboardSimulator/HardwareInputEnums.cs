using BVRTK.Resources;

namespace BVRTK.Components.KeyboardSimulator;

public enum HardwareInputEnums
{
    StickNorth,
    StickEast,
    StickSouth,
    StickWest,
    StickButton,
    
    TrackpadNorth,
    TrackpadEast,
    TrackpadSouth,
    TrackpadWest,
    TrackpadCenter,
    
    FaceButtonNorth,
    FaceButtonEast,
    FaceButtonSouth,
    FaceButtonWest,
    
    SystemButtonNorth,
    SystemButtonSouth,
    
    TriggerPrimary,
    TriggerSecondary,
    GripTrigger,
    GripButton
}

public enum HardwareInputShared
{
    OtherButton1,
    OtherButton2,
    OtherButton3,
    OtherButton4,
    OtherButton5,
    OtherButton6,
    OtherButton7,
    OtherButton8,
    OtherButton9,
    OtherButton10,
    OtherButton11,
    OtherButton12,
    OtherButton13,
    OtherButton14,
    OtherButton15,
    OtherButton16,
    
    Chord1,
    Chord2,
    Chord3,
    Chord4,
    Chord5,
    Chord6,
    Chord7,
    Chord8,
    Chord9,
    Chord10,
    Chord11,
    Chord12,
    Chord13,
    Chord14,
    Chord15,
    Chord16
}

public static class HardwareInputSharedExtensions
{
    extension(HardwareInputShared input)
    {
        public bool IsChord => input is >= HardwareInputShared.Chord1 and <= HardwareInputShared.Chord16;
    }
}

public enum HardwareInputTrigger
{
    Press,
    Release,
    Held,
    Repeat
}

public static class HardwareInputTriggerExtensions
{
    extension(HardwareInputTrigger trigger)
    {
        public static Dictionary<string, Func<string>> GetTriggerGuiIdPairs =>  new()
        {
            [nameof(HardwareInputTrigger.Press)]   = () => HardwareInputPrompts.KeyTriggerPress,
            [nameof(HardwareInputTrigger.Release)] = () => HardwareInputPrompts.KeyTriggerRelease,
            [nameof(HardwareInputTrigger.Held)]    = () => HardwareInputPrompts.KeyTriggerHeld,
            [nameof(HardwareInputTrigger.Repeat)]  = () => HardwareInputPrompts.KeyTriggerRepeat,
        };
        
        public static string[] GetTriggerGuiIds =>
            [.. HardwareInputTrigger.GetTriggerGuiIdPairs.Select(pair => $"{pair.Value()}##{pair.Key}")];
    }
}