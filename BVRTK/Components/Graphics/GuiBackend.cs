using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using BVRTK.Data;
using Hexa.NET.GLFW;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.OpenGL;
using Hexa.NET.StbImage;
using HexaGen.Runtime;
using Valve.VR;
using GLFWwindow = Hexa.NET.GLFW.GLFWwindow;
using HexaUtils = HexaGen.Runtime.Utils;
using GLFWwindowPtr = Hexa.NET.GLFW.GLFWwindowPtr;

namespace BVRTK.Components.Graphics;

/// <summary>
/// Handles the OpenGL backend with OpenGLFW, handles input from both SteamVR and GLFW events, and draws the ImGui interface.
/// </summary>
public class GuiBackend
{
    public GuiBackend()
    {
    }

    private readonly ConcurrentQueue<VREvent_t> _overlayEvents = new();

    public void EnqueueOverlayEvent(in VREvent_t vrEvent)
    {
        _overlayEvents.Enqueue(vrEvent);
        WakeRenderLoop();
    }

    private GLFWwindowPtr? _window = null;
    private bool _glfwInitialized = false;
    private bool _shouldTerminate = false;

    // Based on: https://github.com/HexaEngine/Hexa.NET.ImGui/blob/main/Examples/ExampleGLFWOpenGL3/Program.cs

    /// Setup ImGui config.
    private static unsafe void UpdateConfig()
    {
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard; // Enable Keyboard Controls
        io.ConfigWindowsMoveFromTitleBarOnly = true;

        ImGui.StyleColorsDark();
        var style = ImGui.GetStyle();
        style.ScaleAllSizes(Constants.OverlayGuiScale);
        style.FontScaleDpi = Constants.OverlayGuiScale;

        io.ConfigDpiScaleFonts = true;
        io.ConfigDpiScaleViewports = true;

        Session.GuiFonts.Regular = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-Regular.ttf"]));
        Session.GuiFonts.Bold = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-Bold.ttf"]));
        Session.GuiFonts.Italic = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-Italic.ttf"]));
        Session.GuiFonts.BoldItalic = io.Fonts.AddFontFromFileTTF(Utils.GetAbsoluteFilePath(["Resources", "Fonts", "AtkinsonHyperlegible-BoldItalic.ttf"]));
    }

    private static unsafe void LoadEmbeds(GL gl)
    {
        #region Images

        Session.GuiImages.Logo = LoadImage(gl, "BVRTK.Resources.Media.bvrtk.logo.png");

        #endregion
    } 

    private static unsafe ImFontPtr LoadFont(string resourceName)
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

    private static unsafe GlImage LoadImage(GL gl, string resourceName)
    {
        var bytes = Utils.LoadEmbeddedResource(resourceName);
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
        io.DisplaySize = new Vector2(Constants.OverlayTextureWidth, Constants.OverlayTextureHeight); // match the FBO
        io.DisplayFramebufferScale = new Vector2(1, 1);
        if (_overlayFocus)
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
    
    /// <summary>
    /// We read the queue of incoming Overlay events from VR and
    /// convert those to valid input events for ImGui
    /// </summary>
    private void ApplyOverlayEventsAsInput()
    {
        var io = ImGui.GetIO();
        while (_overlayEvents.TryDequeue(out var vrEvent))
        {
            switch ((EVREventType)vrEvent.eventType)
            {
                case EVREventType.VREvent_MouseMove:
                    _lastOverlayMouse = vrEvent.data.mouse;
                    io.AddMousePosEvent(vrEvent.data.mouse.x, vrEvent.data.mouse.y);
                    break;
                case EVREventType.VREvent_MouseButtonDown:
                    io.AddMouseButtonEvent(Utils.ConvertMouseButton(vrEvent.data.mouse.button), true);
                    break;
                case EVREventType.VREvent_MouseButtonUp:
                    io.AddMouseButtonEvent(Utils.ConvertMouseButton(vrEvent.data.mouse.button), false);
                    break;
                case EVREventType.VREvent_ScrollDiscrete:
                case EVREventType.VREvent_ScrollSmooth:
                    io.AddMouseWheelEvent(vrEvent.data.scroll.xdelta, vrEvent.data.scroll.ydelta);
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
                    _overlayFocus = true;
                    UpdateFocus();
                    break;
                case EVREventType.VREvent_FocusLeave:
                    _overlayFocus = false;
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
    }

    #region Focus

    private static bool _overlayFocus = false;
    private static bool _desktopFocus = false;
    private static bool _hasFocus = false;
    private static VREvent_Mouse_t _lastOverlayMouse = new();

    private static unsafe void OnWindowFocus(GLFWwindow* window, int focused)
    {
        _desktopFocus = focused != 0;
        UpdateFocus();
    }

    private static void UpdateFocus()
    {
        var io = ImGui.GetIO();
        var focus = _desktopFocus || _overlayFocus;
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

    public unsafe void Run(ulong overlayHandle)
    {
        _shouldTerminate = false;

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
            Constants.OverlayTextureWidth,
            Constants.OverlayTextureHeight,
            "BVRTK", null, null
        );
        if (window.IsNull)
        {
            Console.WriteLine("Failed to create GLFW window.");
            GLFW.Terminate();
            return;
        }
        
        SetWindowIcon(window, "BVRTK.Resources.Media.bvrtk.thumbnail.png");

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
        LoadEmbeds(gl);

        // --- Offscreen FBO ---
        var fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, fbo);

        var fboTex = gl.GenTexture();
        gl.BindTexture(GLTextureTarget.Texture2D, fboTex);
        gl.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba8, Constants.OverlayTextureWidth, Constants.OverlayTextureHeight, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedByte, 0);
        gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)GLEnum.Linear);
        gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)GLEnum.Linear);
        gl.FramebufferTexture2D(GLFramebufferTarget.Framebuffer, GLFramebufferAttachment.ColorAttachment0, GLTextureTarget.Texture2D, fboTex, 0);
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);

        // Main loop
        while (!_shouldTerminate)
        {
            // Poll for and process events
            GLFW.PollEvents();
            ApplyOverlayEventsAsInput();
            DisplayVrKeyboardOnTextInput(overlayHandle);
            
            if (!(_overlayVisible || IsWindowVisible()))
            {
                Settings.WriteToDisk();
                GLFW.WaitEventsTimeout(1); // Will interrupt on an event, which happens if the window is shown.
                continue;
            }

            GLFW.MakeContextCurrent(window);
            RenderUiToFbo(gl, fbo);
            SubmitOverlayTexture(overlayHandle, fboTex);
            HapticVibrationOnHover(overlayHandle);

            // Mirror to the desktop window (GPU copy, not a re-render)
            int ww, wh;
            GLFW.GetFramebufferSize(window, &ww, &wh);

            // Figure out what the below does
            gl.BindFramebuffer(GLFramebufferTarget.ReadFramebuffer, fbo);
            gl.BindFramebuffer(GLFramebufferTarget.DrawFramebuffer, 0);
            gl.BlitFramebuffer(0, 0, Constants.OverlayTextureWidth, Constants.OverlayTextureHeight, 0, 0, ww, wh, GLClearBufferMask.ColorBufferBit, GLBlitFramebufferFilter.Linear);
            gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);

            GLFW.SwapBuffers(window);
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

    public void Terminate()
    {
        _shouldTerminate = true;
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

    private static unsafe void SetWindowIcon(GLFWwindowPtr window, string resourceName)
    {
        var bytes = Utils.LoadEmbeddedResource(resourceName);
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
            GLFW.SetWindowIcon(window, 1, &image);
            StbImage.ImageFree(pixels);
        }
    }
    
    private static uint _lastHoveredId;

    private static void HapticVibrationOnHover(ulong handle)
    {
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