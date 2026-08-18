using System.Diagnostics;
using System.Numerics;
using BVRTK.Data;
using Hexa.NET.ImGui;

namespace BVRTK.Components.Graphics.Pages;

public static class Root
{
    public static void RenderAboutPage()
    {
        ImGui.Dummy(Vector2.Zero);
        
        GuiUtils.DrawCenteredImage(Session.GuiImages.Logo);
        GuiUtils.DrawCenteredText("BOLL's VR Toolkit", FontStyle.Bold, Constants.GuiFontSize * 1.8f);
        
        ImGui.Dummy(Vector2.Zero);
        
        GuiUtils.DrawSeparator();
        
        ImGui.Text("Links, opens in your default web browser.");
        
        if (ImGui.TextLink("Discord")) GuiUtils.OpenUrl(Constants.UrlDiscordInvite);
        GuiUtils.DrawTooltip(Constants.UrlDiscordInvite);
        
        ImGui.SameLine();
        
        if (ImGui.TextLink("Github")) GuiUtils.OpenUrl(Constants.UrlGithubRepository);
        GuiUtils.DrawTooltip(Constants.UrlGithubRepository);
        
        ImGui.SameLine();
        
        if (ImGui.TextLink("Website")) GuiUtils.OpenUrl(Constants.UrlDeveloperWebsite);
        GuiUtils.DrawTooltip(Constants.UrlDeveloperWebsite);
    }

    public static void RenderVersionHistoryPage()
    {
        ImGui.TextUnformatted("Load some version file here, Markdown renderer maybe? Hmm.");
    }

    public static void RenderLicensesPage()
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