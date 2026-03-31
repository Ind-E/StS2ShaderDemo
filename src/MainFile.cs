using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace ShaderDemo;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "ShaderDemo";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        if (Engine.GetMainLoop() as SceneTree is { } tree)
        {
            tree.NodeAdded += OnNodeAdded;
        }
    }

    private static void OnNodeAdded(Node node)
    {
        if (node is not Control control)
            return;

        switch (control)
        {
            case Control c when c.Name == "MapScreen":
                c.OnReady(() => CanvasGroupDemo(c));
                break;

            case Control c when c.Name == "Card":
                c.OnReady(() => SubViewportDemo(c));
                break;

            case Control c when c.Name == "Game":
                c.OnReady(() => PostProcessDemo(c));
                break;
        }
    }

    private static void CanvasGroupDemo(Control mapScreen)
    {
        Control legend = (Control)mapScreen.FindChild("MapLegend");
        Vector2 originalGlobalPos = legend.GlobalPosition;

        ShaderMaterial material = new()
        {
            Shader = GD.Load<Shader>("res://ShaderDemo/shaders/canvas_group_hsv.gdshader"),
        };
        material.SetShaderParameter("h", 0.45);
        material.SetShaderParameter("s", 2.0);

        CanvasGroup canvasGroup = new() { Material = material };

        mapScreen.RemoveChild(legend);

        mapScreen.AddChild(canvasGroup);
        canvasGroup.AddChild(legend);

        legend.GlobalPosition = originalGlobalPos;
    }

    private static void SubViewportDemo(Control card)
    {
        if (!card.HasNode("%CardContainer"))
        {
            return;
        }
        Control cardContainer = card.GetNode<Control>("%CardContainer");

        ShaderMaterial material = new() { Shader = TintShader };
        material.SetShaderParameter("tint_color", new Color(0.25f, 0.25f, 0.25f));

        SubViewport viewport = new() { TransparentBg = true };

        SubViewportContainer viewportContainer = new()
        {
            Material = material,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = -viewport.Size / 2,
            PivotOffset = -viewport.Size / 2,
            Size = viewport.Size,
        };

        card.RemoveChild(cardContainer);
        cardContainer.Position = viewport.Size / 2;

        card.AddChild(viewportContainer);
        viewportContainer.AddChild(viewport);
        viewport.AddChild(cardContainer);
    }

    private static readonly Shader TintShader = new()
    {
        Code = """
                shader_type canvas_item;

                uniform vec4 tint_color : source_color = vec4(1.0);

                void fragment() {
                    COLOR.rgb *= tint_color.rgb;
                }
            """,
    };

    private static void PostProcessDemo(Control game)
    {
        // Commented out by deafult because it makes things hard to see

        // CanvasLayer canvasLayer = new();
        // ColorRect rect = new() { MouseFilter = Control.MouseFilterEnum.Ignore };
        // rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        //
        // rect.Material = new ShaderMaterial()
        // {
        //     Shader = GD.Load<Shader>("res://ShaderDemo/shaders/hex_pixelization.gdshader"),
        // };
        //
        // game.AddChild(canvasLayer);
        // canvasLayer.AddChild(rect);
    }
}

public static class NodeExtensions
{
    public static void OnReady(this Node node, Action action)
    {
        if (node.IsNodeReady())
            action();
        else
            node.Ready += action;
    }
}
