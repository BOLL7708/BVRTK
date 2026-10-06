using System.Diagnostics;
using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics.Pages;

public static class ApplicationPages
{
    public static void About()
    {
        ImGui.Dummy(Vector2.Zero);
        
        GuiUtils.DrawCenteredImage(Session.GuiImages.Logo);
        GuiUtils.DrawCenteredText("BOLL's VR Toolkit", FontStyle.Bold, Constants.Gui.FontSize * 1.8f);
        
        ImGui.Dummy(Vector2.Zero);
        
        GuiUtils.DrawDivider();
        
        ImGui.Text("Links, opens in your default web browser.");
        
        if (ImGui.TextLink("Discord")) GuiUtils.OpenUrl(Constants.Url.DiscordInvite);
        GuiUtils.DrawTooltip(Constants.Url.DiscordInvite);
        
        ImGui.SameLine();
        
        if (ImGui.TextLink("Github")) GuiUtils.OpenUrl(Constants.Url.GithubRepository);
        GuiUtils.DrawTooltip(Constants.Url.GithubRepository);
        
        ImGui.SameLine();
        
        if (ImGui.TextLink("Website")) GuiUtils.OpenUrl(Constants.Url.DeveloperWebsite);
        GuiUtils.DrawTooltip(Constants.Url.DeveloperWebsite);
    }

    public static void VersionHistory()
    {
        ImGui.TextUnformatted("Load some version file here, Markdown renderer maybe? Hmm.");
    }

    public static void Licenses()
    {
        ImGui.TextUnformatted("Include the licenses for this project and dependencies and assets used in it.");
        if (ImGui.CollapsingHeader("First Party Licenses", ImGuiTreeNodeFlags.None))
        {
            ImGui.TextUnformatted("Pretend this is a license.");
        }
        if (ImGui.CollapsingHeader("Third Party Licenses", ImGuiTreeNodeFlags.None))
        {
            ImGui.TextUnformatted("Pretend this is a license.");
        }

    }
}