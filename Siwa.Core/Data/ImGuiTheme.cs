using System.Drawing;
using System.Numerics;
using ImGuiNET;

namespace Siwa.Core.Data;

public static class ImGuiTheme
{
    public static void Nord()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;
        ImGui.StyleColorsDark();
        colors[(int)ImGuiCol.Text]                   = new Vector4(0.85f, 0.87f, 0.91f, 0.88f);
        colors[(int)ImGuiCol.TextDisabled]           = new Vector4(0.49f, 0.50f, 0.53f, 1.00f);
        colors[(int)ImGuiCol.WindowBg]               = new Vector4(0.18f, 0.20f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.ChildBg]                = new Vector4(0.16f, 0.17f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.PopupBg]                = new Vector4(0.23f, 0.26f, 0.32f, 1.00f);
        colors[(int)ImGuiCol.Border]                 = new Vector4(0.14f, 0.16f, 0.19f, 1.00f);
        colors[(int)ImGuiCol.BorderShadow]           = new Vector4(0.09f, 0.09f, 0.09f, 0.00f);
        colors[(int)ImGuiCol.FrameBg]                = new Vector4(0.23f, 0.26f, 0.32f, 1.00f);
        colors[(int)ImGuiCol.FrameBgHovered]         = new Vector4(0.56f, 0.74f, 0.73f, 1.00f);
        colors[(int)ImGuiCol.FrameBgActive]          = new Vector4(0.53f, 0.75f, 0.82f, 1.00f);
        colors[(int)ImGuiCol.TitleBg]                = new Vector4(0.16f, 0.16f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive]          = new Vector4(0.16f, 0.16f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed]       = new Vector4(0.16f, 0.16f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.MenuBarBg]              = new Vector4(0.16f, 0.16f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg]            = new Vector4(0.18f, 0.20f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrab]          = new Vector4(0.23f, 0.26f, 0.32f, 0.60f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered]   = new Vector4(0.23f, 0.26f, 0.32f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabActive]    = new Vector4(0.23f, 0.26f, 0.32f, 1.00f);
        colors[(int)ImGuiCol.CheckMark]              = new Vector4(0.37f, 0.51f, 0.67f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab]             = new Vector4(0.51f, 0.63f, 0.76f, 1.00f);
        colors[(int)ImGuiCol.SliderGrabActive]       = new Vector4(0.37f, 0.51f, 0.67f, 1.00f);
        colors[(int)ImGuiCol.Button]                 = new Vector4(0.18f, 0.20f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.ButtonHovered]          = new Vector4(0.51f, 0.63f, 0.76f, 1.00f);
        colors[(int)ImGuiCol.ButtonActive]           = new Vector4(0.37f, 0.51f, 0.67f, 1.00f);
        colors[(int)ImGuiCol.Header]                 = new Vector4(0.51f, 0.63f, 0.76f, 1.00f);
        colors[(int)ImGuiCol.HeaderHovered]          = new Vector4(0.53f, 0.75f, 0.82f, 1.00f);
        colors[(int)ImGuiCol.HeaderActive]           = new Vector4(0.37f, 0.51f, 0.67f, 1.00f);
        colors[(int)ImGuiCol.SeparatorHovered]       = new Vector4(0.56f, 0.74f, 0.73f, 1.00f);
        colors[(int)ImGuiCol.SeparatorActive]        = new Vector4(0.53f, 0.75f, 0.82f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip]             = new Vector4(0.53f, 0.75f, 0.82f, 0.86f);
        colors[(int)ImGuiCol.ResizeGripHovered]      = new Vector4(0.61f, 0.74f, 0.87f, 1.00f);
        colors[(int)ImGuiCol.ResizeGripActive]       = new Vector4(0.37f, 0.51f, 0.67f, 1.00f);
        colors[(int)ImGuiCol.Tab]                    = new Vector4(0.18f, 0.20f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.TabHovered]             = new Vector4(0.22f, 0.24f, 0.31f, 1.00f);
        colors[(int)ImGuiCol.TabActive]              = new Vector4(0.23f, 0.26f, 0.32f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocused]           = new Vector4(0.13f, 0.15f, 0.18f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocusedActive]     = new Vector4(0.17f, 0.19f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram]          = new Vector4(0.56f, 0.74f, 0.73f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered]   = new Vector4(0.53f, 0.75f, 0.82f, 1.00f);
        colors[(int)ImGuiCol.TextSelectedBg]         = new Vector4(0.37f, 0.51f, 0.67f, 1.00f);
        colors[(int)ImGuiCol.NavHighlight]           = new Vector4(0.53f, 0.75f, 0.82f, 0.86f);
        style.WindowBorderSize                       = 1.00f;
        style.ChildBorderSize                        = 1.00f;
        style.PopupBorderSize                        = 1.00f;
        style.FrameBorderSize                        = 1.00f;
    }
}