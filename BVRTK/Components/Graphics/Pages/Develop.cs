using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets.Dialogs;

namespace BVRTK.Components.Graphics.Pages;

public static class Develop
{
    private static int modalTestValue = 0;
    private static int modalTempValue = 0;

    private static float sliderAngle = 0f;
    private static float sliderFloatValue = 0f;
    private static Vector2 sliderFloatValue2 = Vector2.Zero;
    private static Vector3 sliderFloatValue3 = Vector3.Zero;
    private static Vector4 sliderFloatValue4 = Vector4.Zero;
    private static int sliderInt = 0;
    private static int[] sliderInt2 = new int[2];
    private static int[] sliderInt3 = new int[3];
    private static int[] sliderInt4 = new int[4];

    private static double inputDoubleValue = 0.0;
    private static float inputFloatValue = 0f;
    private static Vector2 inputFloatValue2 = Vector2.Zero;
    private static Vector3 inputFloatValue3 = Vector3.Zero;
    private static Vector4 inputFloatValue4 = Vector4.Zero;
    private static int inputIntValue = 0;
    private static int[] inputIntValue2 = new int[2];
    private static int[] inputIntValue3 = new int[3];
    private static int[] inputIntValue4 = new int[4];

    private static string inputTextValue = "";
    private static string inputTextExValue = "";

    private static int radioButtonValue = 0;

    private static bool checkboxValue = false;
    private static int comboValue = 0;
    private static OpenFileDialog openFileDialog = new();
    private static OpenFolderDialog openFolderDialog = new();

    private static Vector3 colorEditValue3 = Vector3.Zero;
    private static Vector4 colorEditValue4 = Vector4.Zero;
    private static Vector3 colorPicker3 = Vector3.Zero;
    private static Vector4 colorPicker4 = Vector4.Zero;

    public static void RenderZooPage()
    {
        GuiUtils.OpenModalForInt(
            "TheTestInt##thetestint",
            "Please update this",
            "Edit it",
            64f,
            Settings.Current.Server.Port
        );
        GuiUtils.DrawModalForInt(
            "TheTestInt##thetestint",
            "A label",
            64f,
            Settings.Current.Server.Port,
            value => Settings.Current.Server.Port = value
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
        ImGui.SliderAngle("Slider Angle##sa", ref sliderAngle);
        ImGui.SliderFloat("Slider Float##sa1", ref sliderFloatValue, -10f, 10f, "%.2f");
        ImGui.SliderFloat2("Slider Float 2##sa2", ref sliderFloatValue2, -10f, 10f);
        ImGui.SliderFloat3("Slider Float 3##sa3", ref sliderFloatValue3, -10f, 10f);
        ImGui.SliderFloat4("Slider Float 4##sa4", ref sliderFloatValue4, -10f, 10f);
        ImGui.SliderInt("Slider Int##si1", ref sliderInt, -10, 10);
        ImGui.SliderInt2("Slider Int 2##si2", ref sliderInt2[0], -10, 10);
        ImGui.SliderInt3("Slider Int 3##si3", ref sliderInt3[0], -10, 10);
        ImGui.SliderInt4("Slider Int 4##si4", ref sliderInt4[0], -10, 10);
        // ImGui.SliderScalar();
        // ImGui.SliderScalarN();

        ImGui.SeparatorText("Inputs");
        ImGui.InputDouble("Input Double", ref inputDoubleValue);
        ImGui.InputFloat("Input Float", ref inputFloatValue);
        ImGui.InputFloat2("Input Float2", ref inputFloatValue2);
        ImGui.InputFloat3("Input Float3", ref inputFloatValue3);
        ImGui.InputFloat4("Input Float4", ref inputFloatValue4);
        ImGui.InputInt("Input Int", ref inputIntValue);
        ImGui.InputInt2("Input Int 2", ref inputIntValue2[0]);
        ImGui.InputInt3("Input Int 3", ref inputIntValue3[0]);
        ImGui.InputInt4("Input Int 4", ref inputIntValue4[0]);
        // ImGui.InputText("Input Text", ref inputTextValue);
        // ImGui.InputTextEx("Input Text Ex", ref inputTextExValue);

        ImGui.SeparatorText("Buttons");
        ImGui.Button("Button");
        ImGui.ArrowButton("Arrow Button", ImGuiDir.Up);
        ImGui.ColorButton("Color Button", GuiColor.Overlays);
        ImGui.ImageButton("Image Button", Session.GuiImages.Logo.ToTextureRef(), new Vector2(Session.GuiImages.Logo.Width, Session.GuiImages.Logo.Height));
        ImGui.InvisibleButton("Invisible Button", new Vector2(64f, 64f));
        ImGui.RadioButton("Radio Button 1", ref radioButtonValue, 0);
        ImGui.RadioButton("Radio Button 2", ref radioButtonValue, 1);
        ImGui.RadioButton("Radio Button 3", ref radioButtonValue, 2);
        ImGui.SmallButton("Small Button");
        ImGui.LogButtons();

        ImGui.SeparatorText("Misc");
        ImGui.Checkbox("Checkbox", ref checkboxValue);
        string[] comboItems = ["One", "Two", "Three"];
        ImGui.Combo("Combo", ref comboValue, comboItems, comboItems.Length);

        if (ImGui.Button("Open File Dialog"))
        {
            openFileDialog.Show();
        }

        openFileDialog.Draw(ImGuiWindowFlags.Modal);
        if (ImGui.Button("Open Folder Dialog"))
        {
            openFolderDialog.Show();
        }

        openFolderDialog.Draw(ImGuiWindowFlags.Modal);

        // if (fileDialog.Draw(ImGuiWindowFlags.Modal)) {}        

        ImGui.SeparatorText("Simple Color Edit");
        ImGui.ColorEdit3("Color Edit 3", ref colorEditValue3);
        ImGui.ColorEdit4("Color Edit 4", ref colorEditValue4);
        ImGui.SeparatorText("Full Color Edit");
        ImGui.ColorPicker3("Color Picker 3", ref colorPicker3);
        ImGui.ColorPicker4("Color Picker 4", ref colorPicker4);

        // ImGui.ListBox();

        ImGui.SeparatorText("More?");
    }
}