using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Room : Node2D
{
    // TODO: Do stuff
    // BUG: OH GOD EVERYTHGH+IUNG"S ONB FIRE
    // HACK: I mean it works ig
    [Export]
    bool rightCheckEnabled = true;
    [Export]
    bool leftCheckEnabled = true;
    [Export]
    bool upCheckEnabled = true;
    [Export]
    bool downCheckEnabled = true;

    private LevelGenerator generator;
    private Vector2 gridPosition;


    public override void _Ready()
    {
        generator = GetParent<LevelGenerator>();

        gridPosition = new Vector2(Position.X / generator.roomWidth, Position.Y / generator.roomHeight);

        generator.existingRoomPositions.Add(gridPosition);


        startGeneration();
    }

    private bool isCheckColliding(Vector2 dir, bool checkEnabled)
    {
        Vector2 space = gridPosition + dir;
        // If the space is open and the side we are trying to generate
        if (generator.existingRoomPositions.Contains(space) || !checkEnabled)
        {
            return true;
        }
        return false;
    }
    public Godot.Collections.Array<string> getOppositeSidesList()
    {

        Godot.Collections.Array<string> sides = new Godot.Collections.Array<string>();
        if (!isCheckColliding(new Vector2(1, 0), rightCheckEnabled))
        {
            sides.Add("LeftCheck");
        }
        if (!isCheckColliding(new Vector2(-1, 0), leftCheckEnabled))
        {
            sides.Add("RightCheck");
        }
        if (!isCheckColliding(new Vector2(0, -1), upCheckEnabled))
        {
            sides.Add("DownCheck");
        }
        if (!isCheckColliding(new Vector2(0, 1), downCheckEnabled))
        {
            sides.Add("UpCheck");
        }
        sides.Shuffle();
        return sides;
    }

    public void startGeneration()
    {
        var oppositeSides = getOppositeSidesList();

        foreach (string side in oppositeSides)
        {
            generateRoom(side);
        }
    }
    /* GetNodePropertyValue property index list for raycast2d:
     * 0: position
     * 1: target_position
     * 2: collision_mask
     * 3: hit_from_inside
     * 4: collide_with_areas
     * 5: collide_with_bodies
     * 
     * Using hit_from_inside to check if enabled <-- IMPORTANT INFO FOR SETTING UP ROOMS
     */
    private void generateRoom(string oppositeCheckName)
    {
        int oppositeCheckIndex = 0;
        switch (oppositeCheckName)
        {
            case "LeftCheck":
                oppositeCheckIndex = 2;
                break;
            case "RightCheck":
                oppositeCheckIndex = 1;
                break;
            case "DownCheck":
                oppositeCheckIndex = 4;
                break;
            case "UpCheck":
                oppositeCheckIndex = 3;
                break;
        }

        if (generator.roomsToGenerate.Count > 0)
        {
            foreach (var room in generator.roomsToGenerate)
            {
                var loadedRoom = GD.Load<PackedScene>(room);
                var roomState = loadedRoom.GetState();
                string checkName = (string)roomState.GetNodeName(oppositeCheckIndex);
                Vector2 checkPosition = (Vector2)roomState.GetNodePropertyValue(oppositeCheckIndex, 0);
                if (checkName == oppositeCheckName)
                {
                    Vector2 newPosition = Position - checkPosition;
                    var inst = loadedRoom.Instantiate<Node2D>();
                    inst.Position = newPosition;
                    generator.roomsToGenerate.Remove(room);

                    GetParent().AddChild(inst);
                    return;
                }
            }
        }
    }

    public void generateCaps()
    {
        if (rightCheckEnabled)
        {
            GetNode<RayCast2D>("RightCheck").ForceRaycastUpdate();
            var collider = (Area2D)GetNode<RayCast2D>("RightCheck").GetCollider();
            bool adjacentRoomConnects = true; 
            if (collider != null)
            {
                adjacentRoomConnects = collider.GetParent<Room>().leftCheckEnabled;
            }
            if (!generator.existingRoomPositions.Contains(gridPosition + new Vector2(1, 0)))
            {
                GetNode<Node2D>("RightCap").Visible = true;
            }
            else if (!adjacentRoomConnects)
            {
                GetNode<Node2D>("RightCap").Visible = true;
            }
        }
        if (leftCheckEnabled)
        {
            GetNode<RayCast2D>("LeftCheck").ForceRaycastUpdate();
            var collider = (Area2D)GetNode<RayCast2D>("LeftCheck").GetCollider();
            bool adjacentRoomConnects = true; 
            if (collider != null)
            {
                adjacentRoomConnects = collider.GetParent<Room>().rightCheckEnabled;
            }
            if (!generator.existingRoomPositions.Contains(gridPosition + new Vector2(-1, 0)))
            {
                GetNode<Node2D>("LeftCap").Visible = true;
            }
            else if (!adjacentRoomConnects)
            {
                GetNode<Node2D>("LeftCap").Visible = true;
            }
        }
        if (upCheckEnabled)
        {
            GetNode<RayCast2D>("UpCheck").ForceRaycastUpdate();
            var collider = (Area2D)GetNode<RayCast2D>("UpCheck").GetCollider();
            bool adjacentRoomConnects = true; 
            if (collider != null)
            {
                adjacentRoomConnects = collider.GetParent<Room>().downCheckEnabled;
            }
            if (!generator.existingRoomPositions.Contains(gridPosition + new Vector2(0, -1)) || !adjacentRoomConnects)
            {
                GetNode<Node2D>("UpCap").Visible = true;
            }
        }
        if (downCheckEnabled)
        {
            GetNode<RayCast2D>("DownCheck").ForceRaycastUpdate();
            var collider = (Area2D)GetNode<RayCast2D>("DownCheck").GetCollider();
            bool adjacentRoomConnects = true; 
            if (collider != null)
            {
                adjacentRoomConnects = collider.GetParent<Room>().upCheckEnabled;
            }
            if (!generator.existingRoomPositions.Contains(gridPosition + new Vector2(0, 1)) || !adjacentRoomConnects)
            {
                GetNode<Node2D>("DownCap").Visible = true;
            }
        }
    }

}
