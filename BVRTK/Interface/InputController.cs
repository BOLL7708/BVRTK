using System.Collections.Specialized;
using BVRTK.Data;

namespace BVRTK.Interface;

public class InputController
{
    public delegate void InputEventHandler(Data.Components.ESection eSection, string option); 
    public event InputEventHandler? InputEvent;
    protected virtual void OnInputEvent(Data.Components.ESection eSection, string option)
    {
        InputEvent?.Invoke(eSection, option);
    }
    
    public InputController()
    {
        
    }
}