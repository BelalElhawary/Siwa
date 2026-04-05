using System.Numerics;
using ImGuiNET;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;

namespace Siwa.Core.Editor;

public static class EditorImGui
{
    extension(ImGui)
    {
        public static void InputMaterialHandle(string label, ref MaterialHandle handle)
        {
            // 1. ALWAYS hash the label, never the value, so the ID stays stable during assignments
            ImGui.PushID(label.GetHashCode());

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 100.0f); // Keep consistent with your Vec3 inputs
            ImGui.Text(label);
            ImGui.NextColumn();

            // 2. Determine empty state (Assuming Index 0 is empty/null)
            bool isEmpty = handle.Handle.Index == 0;
            string buttonText = isEmpty ? $"None (Material)" : $"Material (ID: {handle.Handle.Index})";

            // Fade the text if it's empty to visually communicate an open slot
            if (isEmpty) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

            // Calculate widths so the "X" button fits perfectly next to the slot
            float clearButtonWidth = ImGui.GetFrameHeight(); // Makes a perfect square
            float slotWidth = ImGui.GetContentRegionAvail().X - clearButtonWidth - ImGui.GetStyle().ItemSpacing.X;

            // 3. The main Drop Slot Button
            ImGui.Button(buttonText, new Vector2(slotWidth, 0));

            if (isEmpty) ImGui.PopStyleColor();

            // 4. Drag & Drop Logic (Remember to use the memory-safe payload from earlier!)
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload(nameof(MaterialHandle));
                unsafe
                {
                    if (payload.NativePtr != null)
                    {
                        // Using the strictly packed struct from our previous fix
                        var data = *(MaterialHandle*)payload.Data;
                        handle.Handle = new RawHandle(data.Handle.Index, handle.Handle.Generation); // Update based on your constructor
                        handle.Type = data.Type;
                    }
                }
                ImGui.EndDragDropTarget();
            }

            // 5. The Clear Button
            ImGui.SameLine();
            if (ImGui.Button("X", new Vector2(clearButtonWidth, 0)))
            {
                handle = default; // Resets the slot to empty
            }

            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void InputHandle<T>(string label, ref Handle<T> handle) where T : struct
        {
            ImGui.PushID(label.GetHashCode());

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 100.0f);
            ImGui.Text(label);
            ImGui.NextColumn();

            bool isEmpty = handle.Index == 0;
            string buttonText = isEmpty ? $"None ({typeof(T).Name})" : $"{typeof(T).Name} (ID: {handle.Index})";

            if (isEmpty) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

            float clearButtonWidth = ImGui.GetFrameHeight();
            float slotWidth = ImGui.GetContentRegionAvail().X - clearButtonWidth - ImGui.GetStyle().ItemSpacing.X;

            ImGui.Button(buttonText, new Vector2(slotWidth, 0));

            if (isEmpty) ImGui.PopStyleColor();

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload(typeof(T).Name);
                unsafe
                {
                    if (payload.NativePtr != null)
                    {
                        var uid = *(long*)payload.Data;
                        handle = Handle<T>.FromLong(uid);
                    }
                }
                ImGui.EndDragDropTarget();
            }

            ImGui.SameLine();
            if (ImGui.Button("X", new Vector2(clearButtonWidth, 0)))
            {
                handle = default; // Resets the slot to empty
            }

            ImGui.Columns(1);
            ImGui.PopID();
        }
                
        public static bool InputVector3(string label, ref Vector3 values, float resetValue = 0.0f, float columnWidth = 100.0f)
        {
            bool isModified = false;

            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, columnWidth);
            ImGui.Text(label);
            ImGui.NextColumn();

            // 1. Calculate sizes
            float lineHeight = ImGui.GetFont().FontSize + ImGui.GetStyle().FramePadding.Y * 2.0f;
            Vector2 buttonSize = new Vector2(lineHeight + 3.0f, lineHeight);

            // 2. Calculate how much width each DragFloat gets 
            // (Total width - 3 buttons) divided by 3
            float widthEach = (ImGui.CalcItemWidth() - buttonSize.X * 3.0f) / 3.0f;

            // Remove automatic spacing so the buttons tightly hug the input boxes
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            float groupSpacing = ImGui.GetStyle().ItemSpacing.X; // Save original spacing

            // --- X Axis (Red) ---
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.15f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.2f, 0.2f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.8f, 0.1f, 0.15f, 1.0f));
            if (ImGui.Button("X", buttonSize)) { values.X = resetValue; isModified = true; }
            ImGui.PopStyleColor(3);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(widthEach); // Set width for this specific input
            isModified |= ImGui.DragFloat("##X", ref values.X, 0.1f, 0.0f, 0.0f, "%.2f");

            // --- Y Axis (Green) ---
            ImGui.SameLine(0, groupSpacing); // Add normal spacing between X and Y groups
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.7f, 0.2f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.8f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.7f, 0.2f, 1.0f));
            if (ImGui.Button("Y", buttonSize)) { values.Y = resetValue; isModified = true; }
            ImGui.PopStyleColor(3);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(widthEach);
            isModified |= ImGui.DragFloat("##Y", ref values.Y, 0.1f, 0.0f, 0.0f, "%.2f");

            // --- Z Axis (Blue) ---
            ImGui.SameLine(0, groupSpacing); // Add normal spacing between Y and Z groups
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.25f, 0.8f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.35f, 0.9f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.1f, 0.25f, 0.8f, 1.0f));
            if (ImGui.Button("Z", buttonSize)) { values.Z = resetValue; isModified = true; }
            ImGui.PopStyleColor(3);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(widthEach);
            isModified |= ImGui.DragFloat("##Z", ref values.Z, 0.1f, 0.0f, 0.0f, "%.2f");

            // Cleanup
            ImGui.PopStyleVar(); // Restore standard ItemSpacing
            ImGui.Columns(1);
            ImGui.PopID();

            return isModified;
        }
    }
}