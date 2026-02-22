using System.Collections.Specialized;
using BVRTK.Data;

namespace BVRTK.Interface;

public class InputController
{
    public delegate void InputEventHandler(Data.Components.SectionEnum section, string option); 
    public event InputEventHandler? InputEvent;
    protected virtual void OnInputEvent(Data.Components.SectionEnum section, string option)
    {
        InputEvent?.Invoke(section, option);
    }
    
    public InputController()
    {
        
    }
}