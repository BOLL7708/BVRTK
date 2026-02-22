using System.Security.Cryptography.X509Certificates;
using SharpHook;
using SharpHook.Data;

namespace BVRTK.Components.MouseSimulator;

public class MouseSim
{
    private static readonly EventSimulator Simulator = new();

    static UioHookResult? Move(short x, short y, bool relative = true)
    {
        var result = relative 
            ? Simulator.SimulateMouseMovementRelative(x, y) 
            : Simulator.SimulateMouseMovement(x, y);
        return result;
    }

    static UioHookResult Click(MouseButton button)
    {
        return Simulator.SimulateMousePress(button);
        
    }
}