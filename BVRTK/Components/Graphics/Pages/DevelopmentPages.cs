using System.Numerics;
using BVRTK.Data;
using EasyOpenVR;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets.Dialogs;

namespace BVRTK.Components.Graphics.Pages;

public static class DevelopmentPages
{
    private static int _modalTestValue = 0;
    private static int _modalTempValue = 0;

    private static float _sliderAngle = 0f;
    private static float _sliderFloatValue = 0f;
    private static Vector2 _sliderFloatValue2 = Vector2.Zero;
    private static Vector3 _sliderFloatValue3 = Vector3.Zero;
    private static Vector4 _sliderFloatValue4 = Vector4.Zero;
    private static int _sliderInt = 0;
    private static readonly int[] SliderInt2 = new int[2];
    private static readonly int[] SliderInt3 = new int[3];
    private static readonly int[] SliderInt4 = new int[4];

    private static double _inputDoubleValue = 0.0;
    private static float _inputFloatValue = 0f;
    private static Vector2 _inputFloatValue2 = Vector2.Zero;
    private static Vector3 _inputFloatValue3 = Vector3.Zero;
    private static Vector4 _inputFloatValue4 = Vector4.Zero;
    private static int _inputIntValue = 0;
    private static readonly int[] InputIntValue2 = new int[2];
    private static readonly int[] InputIntValue3 = new int[3];
    private static readonly int[] InputIntValue4 = new int[4];

    private static string _inputTextValue = "";
    private static string _inputTextExValue = "";

    private static int _radioButtonValue = 0;

    private static bool _checkboxValue = false;
    private static int _comboValue = 0;
    private static OpenFileDialog _openFileDialog = new();
    private static OpenFolderDialog _openFolderDialog = new();

    private static Vector3 _colorEditValue3 = Vector3.Zero;
    private static Vector4 _colorEditValue4 = Vector4.Zero;
    private static Vector3 _colorPicker3 = Vector3.Zero;
    private static Vector4 _colorPicker4 = Vector4.Zero;

    private static int _testInt = 100;
    private static string _testStr = "";

    public static void Zoo()
    {
        GuiUtils.DoModalForInt(
            "TheTestInt##thetestint",
            "Please update this INT",
            "Edit",
            64f,
            _testInt,
            value => _testInt = value
        );
        
        GuiUtils.DoModalForString(
            "TheTestString##theteststring",
            "Please update this STRING",
            "Edit",
            64f,
            _testStr,
            8,
            value => _testStr = value
        );

        ImGui.SeparatorText("Texts");
        ImGui.Text("Text");
        ImGui.TextUnformatted("Text Unformatted");
        ImGui.TextWrapped("Text Wrapped");
        ImGui.TextAligned(0.5f, ImGui.GetContentRegionAvail().X, "Text Aligned");
        ImGui.TextColored(GuiColor.Server, "Text Colored");
        ImGui.TextDisabled("Text Disabled");
        ImGui.TextDisabledV("Text Disabled V", 0);
        ImGui.TextLink("A link");
        ImGui.TextLinkOpenURL("A link that opens");

        ImGui.SeparatorText("Sliders");
        ImGui.SliderAngle("Slider Angle##sa", ref _sliderAngle);
        ImGui.SliderFloat("Slider Float##sa1", ref _sliderFloatValue, -10f, 10f, "%.2f");
        ImGui.SliderFloat2("Slider Float 2##sa2", ref _sliderFloatValue2, -10f, 10f);
        ImGui.SliderFloat3("Slider Float 3##sa3", ref _sliderFloatValue3, -10f, 10f);
        ImGui.SliderFloat4("Slider Float 4##sa4", ref _sliderFloatValue4, -10f, 10f);
        ImGui.SliderInt("Slider Int##si1", ref _sliderInt, -10, 10);
        ImGui.SliderInt2("Slider Int 2##si2", ref SliderInt2[0], -10, 10);
        ImGui.SliderInt3("Slider Int 3##si3", ref SliderInt3[0], -10, 10);
        ImGui.SliderInt4("Slider Int 4##si4", ref SliderInt4[0], -10, 10);
        // ImGui.SliderScalar();
        // ImGui.SliderScalarN();

        ImGui.SeparatorText("Inputs");
        ImGui.InputDouble("Input Double", ref _inputDoubleValue);
        ImGui.InputFloat("Input Float", ref _inputFloatValue);
        ImGui.InputFloat2("Input Float2", ref _inputFloatValue2);
        ImGui.InputFloat3("Input Float3", ref _inputFloatValue3);
        ImGui.InputFloat4("Input Float4", ref _inputFloatValue4);
        ImGui.InputInt("Input Int", ref _inputIntValue);
        ImGui.InputInt2("Input Int 2", ref InputIntValue2[0]);
        ImGui.InputInt3("Input Int 3", ref InputIntValue3[0]);
        ImGui.InputInt4("Input Int 4", ref InputIntValue4[0]);
        // ImGui.InputText("Input Text", ref inputTextValue);
        // ImGui.InputTextEx("Input Text Ex", ref inputTextExValue);

        ImGui.SeparatorText("Buttons");
        ImGui.Button("Button");
        ImGui.ArrowButton("Arrow Button", ImGuiDir.Up);
        ImGui.ColorButton("Color Button", GuiColor.Overlays);
        ImGui.ImageButton("Image Button", Session.GuiImages.Logo.ToTextureRef(), new Vector2(Session.GuiImages.Logo.Width, Session.GuiImages.Logo.Height));
        ImGui.InvisibleButton("Invisible Button", new Vector2(64f, 64f));
        ImGui.RadioButton("Radio Button 1", ref _radioButtonValue, 0);
        ImGui.RadioButton("Radio Button 2", ref _radioButtonValue, 1);
        ImGui.RadioButton("Radio Button 3", ref _radioButtonValue, 2);
        ImGui.SmallButton("Small Button");
        ImGui.LogButtons();

        ImGui.SeparatorText("Misc");
        ImGui.Checkbox("Checkbox", ref _checkboxValue);
        string[] comboItems = ["One##1", "Two##2", "Three##3"];
        ImGui.Combo("Combo", ref _comboValue, comboItems, comboItems.Length);
            
        // if (ImGui.Button("Open File Dialog")) openFileDialog.Show();
        // openFileDialog.Draw(ImGuiWindowFlags.Modal);
        // if (ImGui.Button("Open Folder Dialog")) openFolderDialog.Show();
        // openFolderDialog.Draw(ImGuiWindowFlags.Modal);
        // if (fileDialog.Draw(ImGuiWindowFlags.Modal)) {}        

        ImGui.SeparatorText("Simple Color Edit");
        ImGui.ColorEdit3("Color Edit 3", ref _colorEditValue3);
        ImGui.ColorEdit4("Color Edit 4", ref _colorEditValue4);
        ImGui.SeparatorText("Full Color Edit");
        ImGui.ColorPicker3("Color Picker 3", ref _colorPicker3);
        ImGui.ColorPicker4("Color Picker 4", ref _colorPicker4);

        // ImGui.ListBox();

        ImGui.SeparatorText("More?");
    }


    public static void Debug()
    {
        GuiUtils.DrawTitle("Input Source Handles");
        ImGui.Text(string.Join('\n', Services.Vr.Data.InputSourceToInputSourceHandle.Select(kvp => $"{kvp.Key}\t{kvp.Value}")));
    }
}