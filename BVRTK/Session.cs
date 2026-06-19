namespace BVRTK;

public static class Session
{
#if DEBUG
    public const bool isDebug = true;
#else
    public const bool isDebug = false;
#endif
}