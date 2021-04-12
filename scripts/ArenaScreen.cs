using Godot;
using System.Collections.Generic;

public class ArenaScreen : Node {
    private ArenaViewport NewArenaViewport(Viewport v) {
        return new ArenaViewport {
            viewport = v,
            camera = v.GetNode<ArenaCameraNode>("Camera"),
            canvasLayer = v.GetNode<CanvasLayer>("CanvasLayer"),
            container = v.GetParent<ViewportContainer>(),
        };
    }

    public override void _Ready() {
        Input.SetMouseMode(Input.MouseMode.Hidden);

        var grid = GetNode<GridContainer>("Viewports");
        var viewport1 = grid.GetNode<Viewport>("C1/Viewport");
        var viewport2 = grid.GetNode<Viewport>("C2/Viewport");

        var arena = GetNode<Arena>("Viewports/C1/Viewport/Arena");
        var viewportList = new List<ArenaViewport>{
            NewArenaViewport(viewport1),
            NewArenaViewport(viewport2),
        };
        arena.Run(viewportList);

        int numActiveViewports = 0;
        ArenaViewport activeViewport = null;
        foreach (var v in viewportList) {
            if (v.used) {
                activeViewport = v;
                numActiveViewports++;
                if (v.viewport != viewport1) {
                    v.viewport.World2d = viewport1.World2d;
                }
            } else {
                v.container.QueueFree();
            }
        }
        if (numActiveViewports == 1) {
            grid.Columns = 1;
            var sizeRect = activeViewport.container.RectSize;
            activeViewport.container.SetSize(sizeRect + new Vector2(sizeRect.x, 0));
            activeViewport.viewport.Size = sizeRect;
        }
    }
}
