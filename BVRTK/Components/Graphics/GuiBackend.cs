using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BVRTK.Data;
using Hexa.NET.GLFW;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.OpenGL;
using Hexa.NET.StbImage;
using HexaGen.Runtime;
using Software.Boll.EasyUtils;
using Valve.VR;
using GLFWwindow = Hexa.NET.GLFW.GLFWwindow;
using HexaUtils = HexaGen.Runtime.Utils;
using GLFWwindowPtr = Hexa.NET.GLFW.GLFWwindowPtr;

namespace BVRTK.Components.Graphics;

/// <summary>
/// Handles the OpenGL backend with OpenGLFW, handles input from both SteamVR and GLFW events, and draws the ImGui interface.
/// </summary>
public class GuiBackend(CancellationToken cancellationToken)
{
    private readonly ConcurrentQueue<VREvent_t> _overlayEvents = new();

    public void EnqueueOverlayEvent(in VREvent_t vrEvent)
    {
        _overlayEvents.Enqueue(vrEvent);
        WakeRenderLoop();
    }

    private GLFWwindowPtr? _window = null;
    private bool _glfwInitialized = false;

    // Based on: https://github.com/HexaEngine/Hexa.NET.ImGui/blob/main/Examples/ExampleGLFWOpenGL3/Program.cs

    /// Setup ImGui config.
    private static unsafe void UpdateConfig()
    {
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard; // Enable Keyboard Controls
        io.ConfigWindowsMoveFromTitleBarOnly = true;

        ImGui.StyleColorsDark();
        var style = ImGui.GetStyle();
        style.ScaleAllSizes(Constants.Overlay.GuiScale);
        style.FontScaleDpi = Constants.Overlay.GuiScale;

        io.ConfigDpiScaleFonts = true;
        io.ConfigDpiScaleViewports = true;

        Session.GuiFonts.Regular = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-Regular.ttf"]));
        Session.GuiFonts.Bold = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-Bold.ttf"]));
        Session.GuiFonts.Italic = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-Italic.ttf"]));
        Session.GuiFonts.BoldItalic = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-BoldItalic.ttf"]));
    }

    private static void LoadImages(GL gl)
    {
        Session.GuiImages.Logo = LoadImageFromFile(gl, Utils.GetAbsoluteFilePath(["Resources", "Media", "bvrtk.logo.nobg.png"]));
    }

    private static unsafe ImFontPtr LoadFontFromResource(string resourceName)
    {
        var io = ImGui.GetIO();
        var cfg = ImGui.ImFontConfig();
        var bytes = Utils.LoadEmbeddedResource(resourceName);
        ImFontPtr pointer;
        fixed (byte* p = bytes)
        {
            pointer = io.Fonts.AddFontFromMemoryTTF(p, bytes.Length, 0f, cfg);
        }

        return pointer;
    }

    private static GlImage LoadImageFromFile(GL gl, string filePath)
    {
        var fileResult = FileUtils.ReadBinary(filePath);
        if(!fileResult.Success) Console.WriteLine($"File problem: {filePath}, {fileResult.Exception?.Message}, {fileResult.BytesRead}"); // TODO: Proper logging
        var bytes = fileResult.Bytes ?? [];
        return BytesToImage(gl, bytes);
    }
    
    private static GlImage LoadImageFromResource(GL gl, string resourceName)
    {
        var bytes = Utils.LoadEmbeddedResource(resourceName);
        return BytesToImage(gl, bytes);
       
    }

    private static unsafe GlImage BytesToImage(GL gl, byte[] bytes)
    {
        int width, height;
        uint tex;
        fixed (byte* pointer = bytes)
        {
            int channels;
            var pixels = StbImage.LoadFromMemory(pointer, bytes.Length, &width, &height, &channels, 4);
            gl.GenTextures(1, &tex); // Generate one new texture handle.
            gl.BindTexture(GLTextureTarget.Texture2D, tex);
            gl.TexImage2D(
                GLTextureTarget.Texture2D,
                0,
                GLInternalFormat.Rgba8,
                width,
                height,
                0,
                GLPixelFormat.Rgba,
                GLPixelType.UnsignedByte,
                pixels
            );
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)GLEnum.Linear);
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)GLEnum.Linear);
        }

        return new GlImage(tex, width, height);
    }

    private static void RenderUiToFbo(GL gl, uint fbo)
    {
        // Render ImGui into the FBO (single render)
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, fbo);
        gl.ClearColor(0, 0, 0, 0);
        gl.Clear(GLClearBufferMask.ColorBufferBit);

        ImGuiImplOpenGL3.NewFrame();
        ImGuiImplGLFW.NewFrame();
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(Constants.Overlay.TextureWidth, Constants.Overlay.TextureHeight); // match the FBO
        io.DisplayFramebufferScale = new Vector2(1, 1);
        if (Session.OverlayFocus && !_mouseWheelUsedThisFrame)
        {
            // This is performed so that hover effects still work when the desktop cursor has left the window.
            // A side effect is that the overlay cursor now overrides the desktop one completely if active.
            ImGui.GetIO().AddMousePosEvent(_lastOverlayMouse.x, _lastOverlayMouse.y);
        }

        ImGui.NewFrame();

        // ImGui.ShowDemoWindow(); // Use to ensure stuff is still working.
        Gui.RenderWindow();

        ImGui.Render();
        ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);
    }

    private static void SubmitOverlayTexture(ulong mainHandle, uint fboTex)
    {
        // Submit the same texture to the overlay
        var tex = new Texture_t
        {
            handle = (IntPtr)fboTex,
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto
        };
        OpenVR.Overlay.SetOverlayTexture(mainHandle, ref tex);
    }

    private static Vector2? _pendingMousePos;
    private static bool _mouseWheelUsedThisFrame = false;

    /// <summary>
    /// We read the queue of incoming Overlay events from VR and
    /// convert those to valid input events for ImGui
    /// </summary>
    private void ApplyOverlayEventsAsInput()
    {
        if (cancellationToken.IsCancellationRequested) return; // If the loop and thus contexts are terminating, we should no longer accept events.
        
        _mouseWheelUsedThisFrame = false;
        var io = ImGui.GetIO();
        VREvent_Scroll_t scroll = new();
        while (_overlayEvents.TryDequeue(out var vrEvent))
        {
            switch ((EVREventType)vrEvent.eventType)
            {
                case EVREventType.VREvent_MouseMove:
                    _pendingMousePos = new Vector2(vrEvent.data.mouse.x, vrEvent.data.mouse.y);
                    _lastOverlayMouse = vrEvent.data.mouse;
                    break;
                case EVREventType.VREvent_MouseButtonDown:
                case EVREventType.VREvent_MouseButtonUp:
                    FlushMousePos(); // Flush here to apply the current mouse pos for any press/release.
                    var down = vrEvent.eventType == (uint)EVREventType.VREvent_MouseButtonDown;
                    io.AddMouseButtonEvent(Utils.ConvertMouseButton(vrEvent.data.mouse.button), down);
                    break;
                case EVREventType.VREvent_ScrollDiscrete:
                case EVREventType.VREvent_ScrollSmooth:
                    scroll.xdelta += vrEvent.data.scroll.xdelta;
                    scroll.ydelta += vrEvent.data.scroll.ydelta;
                    break;
                case EVREventType.VREvent_KeyboardCharInput:
                    // TODO: Possibly move this big blob into a separate method.
                    var str = Utils.ConvertKeyboardChar(vrEvent.data.keyboard);
                    Console.WriteLine($"Decoded input chars: {str}");
                    var runes = str.EnumerateRunes();
                    var lastRune = runes.Last();
                    var isEscapeSymbol = false;

                    foreach (var rune in runes)
                    {
                        if (isEscapeSymbol)
                        {
                            switch (rune.Value)
                            {
                                case 'A': PressKey(ImGuiKey.UpArrow); break;
                                case 'B': PressKey(ImGuiKey.DownArrow); break;
                                case 'C': PressKey(ImGuiKey.RightArrow); break;
                                case 'D': PressKey(ImGuiKey.LeftArrow); break;
                            }

                            continue;
                        }

                        switch (rune.Value)
                        {
                            case '\b': // backspace
                                PressKey(ImGuiKey.Backspace);
                                break;
                            case '\n':
                            case '\r': // enter/submit
                                PressKey(ImGuiKey.Enter);
                                break;
                            case 0x1b: // escape -> close keyboard
                                if (lastRune.Value == 0x1b)
                                {
                                    Services.Vr.Overlay.HideKeyboard();
                                    _softKeyboardShown = false;
                                }
                                else
                                {
                                    isEscapeSymbol = true;
                                }

                                break;
                            default:
                                if (rune.Value >= 0x20)
                                {
                                    // printable only
                                    io.AddInputCharactersUTF8(rune.ToString());
                                }

                                break;
                        }
                    }

                    break;

                    void PressKey(ImGuiKey k)
                    {
                        io.AddKeyEvent(k, true);
                        io.AddKeyEvent(k, false);
                    }
                case EVREventType.VREvent_FocusEnter:
                    Session.OverlayFocus = true;
                    UpdateFocus();
                    break;
                case EVREventType.VREvent_FocusLeave:
                    Session.OverlayFocus = false;
                    UpdateFocus();
                    break;
                case EVREventType.VREvent_OverlayShown:
                    _overlayVisible = true;
                    break;
                case EVREventType.VREvent_OverlayHidden:
                case EVREventType.VREvent_OverlayClosed:
                    _overlayVisible = false;
                    break;
                default:
                    // Console.WriteLine($"UNHANDLED: {Enum.GetName((EVREventType)vrEvent.eventType)}");
                    break;
            }
        }

        if (scroll.xdelta != 0f || scroll.ydelta != 0f) // Only apply scroll if used.
        {
            _mouseWheelUsedThisFrame = true;
            io.AddMouseWheelEvent(scroll.xdelta, scroll.ydelta);
        }
        else
        {
            FlushMousePos(); // We don't need to move the mouse while scrolling.
        }
        
        return;
        
        void FlushMousePos()
        {
            if (_pendingMousePos is not { } pos) return;

            io.AddMousePosEvent(pos.X, pos.Y);
            _pendingMousePos = null;
        }
    }

    #region Focus

    private static bool _hasFocus = false;
    private static VREvent_Mouse_t _lastOverlayMouse = new();

    private static unsafe void OnWindowFocus(GLFWwindow* window, int focused)
    {
        Session.DesktopFocus = focused != 0;
        UpdateFocus();
    }

    private static void UpdateFocus()
    {
        var io = ImGui.GetIO();
        var focus = Session.DesktopFocus || Session.OverlayFocus;
        if (_hasFocus == focus) return;
        _hasFocus = focus;
        io.AddFocusEvent(focus);
    }

    #endregion

    #region Termination

    /// <summary>
    /// Instead of closing the desktop window, we hide it, to allow the overlay to still render.
    /// </summary>
    /// <param name="window"></param>
    private static unsafe void OnWindowClose(GLFWwindow* window)
    {
        GLFW.SetWindowShouldClose(window, GLFW.GLFW_FALSE); // cancel the close request
        GLFW.HideWindow(window);
    }

    #endregion

    #region Keyboard

    private static bool _softKeyboardShown = false;

    private static void DisplayVrKeyboardOnTextInput(ulong handle)
    {
        var io = ImGui.GetIO();
        if (io.WantTextInput && !_softKeyboardShown)
        {
            _softKeyboardShown = true;
            Services.Vr.Overlay.ShowDirectModeKeyboard(handle);
        }
        else if (!io.WantTextInput && _softKeyboardShown)
        {
            _softKeyboardShown = false;
            Services.Vr.Overlay.HideKeyboard();
        }
    }

    #endregion

    public Task Run(ulong overlayHandle)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); // Option lets the task be awaited in the main program.
        var thread = new Thread(() =>
        {
            try
            {
                RunLoop(overlayHandle);
                tcs.SetResult();
            } catch ( Exception e) { tcs.SetException(e); }
            
        }) { Name = "GuiRenderThread", IsBackground = false };
        thread.Start();
        return tcs.Task;
    }
    
    private unsafe void RunLoop(ulong overlayHandle)
    {
        var error = new NativeCallback<GLFWerrorfun>(static (errorCode, description) =>
        {
            // TODO: Switch to proper logging
            Console.WriteLine(HexaUtils.DecodeStringUTF8(description));
        });
        GLFW.SetErrorCallback(error);

        _glfwInitialized = GLFW.Init() == GLFW.GLFW_TRUE;

        const string glslVersion = "#version 150";
        GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
        GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 2);
        GLFW.WindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE); // 3.2+ only
        GLFW.WindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_FALSE);

        // Handle monitor scaling, scale the window but not the frame buffer, to avoid affecting VR overlay.
        GLFW.WindowHint(GLFW.GLFW_SCALE_TO_MONITOR, GLFW.GLFW_TRUE);
        GLFW.WindowHint(GLFW.GLFW_SCALE_FRAMEBUFFER, GLFW.GLFW_FALSE);

        GLFW.WindowHint(GLFW.GLFW_VISIBLE, Settings.Current.Application.ShowDesktopWindowOnLaunch ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE);

        var window = GLFW.CreateWindow(
            Constants.Overlay.TextureWidth,
            Constants.Overlay.TextureHeight,
            "BVRTK", null, null
        );
        if (window.IsNull)
        {
            Console.WriteLine("Failed to create GLFW window.");
            GLFW.Terminate();
            return;
        }

        SetWindowIcon(window, Utils.GetAbsoluteFilePath(["Resources", "Media", "bvrtk.thumbnail.64.png"]));

        _window = window;
        GLFW.MakeContextCurrent(window);

        var guiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(guiContext);
        UpdateConfig();
        ImGuiImplGLFW.SetCurrentContext(guiContext);

        if (!ImGuiImplGLFW.InitForOpenGL(
                Unsafe.BitCast<GLFWwindowPtr, Hexa.NET.ImGui.Backends.GLFW.GLFWwindowPtr>(window),
                true
            )
           )
        {
            Console.WriteLine("Failed to init ImGui Impl GLFW");
            GLFW.Terminate();
            return;
        }

        // Replace the default window focus callback as it will disable all
        // input handling when the desktop mouse cursor leaves the window.
        GLFW.SetWindowFocusCallback(window, &OnWindowFocus);
        Session.DesktopFocus = GLFW.GetWindowAttrib(window, GLFW.GLFW_FOCUSED) != 0;

        // Replace the default window close callback as it will terminate
        // the GL window which houses the texture the overlay is using.
        GLFW.SetWindowCloseCallback(window, &OnWindowClose);

        ImGuiImplOpenGL3.SetCurrentContext(guiContext);
        if (!ImGuiImplOpenGL3.Init(glslVersion))
        {
            Console.WriteLine("Failed to init ImGui Impl OpenGL3");
            GLFW.Terminate();
            return;
        }

        GL gl = new(new BindingsContext(window));
        LoadImages(gl);

        // --- Offscreen FBO ---
        var fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, fbo);

        var fboTex = gl.GenTexture();
        gl.BindTexture(GLTextureTarget.Texture2D, fboTex);
        gl.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba8, Constants.Overlay.TextureWidth, Constants.Overlay.TextureHeight, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedByte, 0);
        gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)GLEnum.Linear);
        gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)GLEnum.Linear);
        gl.FramebufferTexture2D(GLFramebufferTarget.Framebuffer, GLFramebufferAttachment.ColorAttachment0, GLTextureTarget.Texture2D, fboTex, 0);
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);

        var hasSavedOnHidden = false;
        var stopwatch = Stopwatch.StartNew();
        long endOfNextFrame = 0;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            // Poll for and process events
            GLFW.PollEvents();
            ApplyOverlayEventsAsInput();
            DisplayVrKeyboardOnTextInput(overlayHandle);

            if (!(_overlayVisible || IsWindowVisible()))
            {
                if (!hasSavedOnHidden) Settings.WriteToDisk();
                hasSavedOnHidden = true;
                GLFW.WaitEventsTimeout(1); // Will interrupt on an event, which happens if the window is shown.
                continue;
            }

            hasSavedOnHidden = false;

            GLFW.MakeContextCurrent(window);
            RenderUiToFbo(gl, fbo);
            if (cancellationToken.IsCancellationRequested) break; // We break here just in case, as we somehow got a crash from SubmitOverlayTexture on exit, and there might be a winddown desync problem.
            
            SubmitOverlayTexture(overlayHandle, fboTex);
            HapticVibrationOnHover(overlayHandle);
            if (Session.GuiFlags.SkipFrames > 0) continue; // Used to allow the GUI to settle things like swapping pages, as the tab view populates and tables render.

            // Mirror to the desktop window (GPU copy, not a re-render)
            int ww, wh;
            GLFW.GetFramebufferSize(window, &ww, &wh);
            gl.BindFramebuffer(GLFramebufferTarget.ReadFramebuffer, fbo);
            gl.BindFramebuffer(GLFramebufferTarget.DrawFramebuffer, 0);
            gl.BlitFramebuffer(0, 0, Constants.Overlay.TextureWidth, Constants.Overlay.TextureHeight, 0, 0, ww, wh, GLClearBufferMask.ColorBufferBit, GLBlitFramebufferFilter.Linear);
            gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);

            GLFW.SwapBuffers(window);

            var frameRate = Math.Max(1, Settings.Current.Application.GuiFrameRate);
            var ticksPerFrame = Stopwatch.Frequency / frameRate;
            endOfNextFrame += ticksPerFrame;

            var now = stopwatch.ElapsedTicks;
            if (now > endOfNextFrame) endOfNextFrame = now; // We  are behind or just un-hidden, we don't try to catch up but skip ahead.

            var remainingTime = Stopwatch.GetElapsedTime(now, endOfNextFrame);
            if (remainingTime.Ticks > 0) cancellationToken.WaitHandle.WaitOne(remainingTime);
        }

        ImGuiImplOpenGL3.Shutdown();
        ImGuiImplOpenGL3.SetCurrentContext(null);
        ImGuiImplGLFW.Shutdown();
        ImGuiImplGLFW.SetCurrentContext(null);
        ImGui.DestroyContext();
        gl.DeleteFramebuffer(fbo);
        gl.DeleteTexture(fboTex);
        gl.Dispose();

        // Clean up and terminate GLFW
        GLFW.DestroyWindow(window);
        GLFW.Terminate();
    }
    
    public void SetWindowVisible(bool visible)
    {
        if (_window is null) return; // TODO: Log to log system here.
        if (visible)
        {
            GLFW.ShowWindow(_window.Value);
            WakeRenderLoop();
        }
        else GLFW.HideWindow(_window.Value);
    }

    public bool IsWindowVisible()
    {
        if (_window is null) return false;
        return GLFW.GetWindowAttrib(_window.Value, GLFW.GLFW_VISIBLE) != 0
               && GLFW.GetWindowAttrib(_window.Value, GLFW.GLFW_ICONIFIED) == 0;
    }

    private bool _overlayVisible = false;

    /// <summary>
    /// Used to provide the overlay state to help decide if the UI should render.
    /// </summary>
    /// <param name="visible"></param>
    public void SetOverlayVisible(bool visible)
    {
        _overlayVisible = visible;
        if (visible) WakeRenderLoop();
    }

    private void WakeRenderLoop()
    {
        if (_glfwInitialized) GLFW.PostEmptyEvent();
    }

    private static unsafe void SetWindowIcon(GLFWwindowPtr window, string filePath)
    {
        var fileResult = FileUtils.ReadBinary(filePath);
        var bytes = fileResult.Bytes ?? [];
        fixed (byte* bytesPointer = bytes)
        {
            int width, height, channels;
            var pixels = StbImage.LoadFromMemory(bytesPointer, bytes.Length, &width, &height, &channels, 4);
            if (pixels == null)
            {
                var reason = Marshal.PtrToStringAnsi((IntPtr)StbImage.FailureReason());
                Console.WriteLine($"Overlay: StbImage decode FAILED: {reason}"); // TODO: Switch to proper logging
                return;
            }

            // Instantiate image object
            GLFWimage image;
            image.Width = width;
            image.Height = height;
            image.Pixels = pixels;

            // Submit the pixels to GLFW that copies them, then free the resource.
            try
            {
                // Supposedly this does not always work on Linux.
                GLFW.SetWindowIcon(window, 1, &image);
            }
            catch (Exception e)
            {
                Console.WriteLine($"$Overlay: GLFV cannot set SetWindowIcon: {e.Message}"); // TODO: Logging
            }
            StbImage.ImageFree(pixels);
        }
    }

    private static uint _lastHoveredId;

    private static void HapticVibrationOnHover(ulong handle)
    {
        if (!Settings.Current.Application.EnableHapticsOnHover) return;

        var hoveredId = ImGui.GetCurrentContext().HoveredId;
        if (hoveredId != 0 && hoveredId != _lastHoveredId)
        {
            Services.Vr.Overlay.TriggerHapticVibration(handle);
        }

        _lastHoveredId = hoveredId;
    }
}

internal unsafe class BindingsContext(GLFWwindowPtr window) : IGLContext
{
    public nint Handle => (nint)window.Handle;

    public bool IsCurrent => GLFW.GetCurrentContext() == window;

    public void Dispose()
    {
    }

    public nint GetProcAddress(string procName)
    {
        return (nint)GLFW.GetProcAddress(procName);
    }

    public bool IsExtensionSupported(string extensionName)
    {
        return GLFW.ExtensionSupported(extensionName) != 0;
    }

    public void MakeCurrent()
    {
        GLFW.MakeContextCurrent(window);
    }

    public void SwapBuffers()
    {
        GLFW.SwapBuffers(window);
    }

    public void SwapInterval(int interval)
    {
        GLFW.SwapInterval(interval);
    }

    public bool TryGetProcAddress(string procName, out nint procAddress)
    {
        procAddress = (nint)GLFW.GetProcAddress(procName);
        return procAddress != 0;
    }
}