using Godot;
using System;
using System.Collections.Generic;

public partial class LevelGenerator : Node2D
{
    private Random rng = new Random();

    private enum generationProgress
    {
        Main,
        Branches,
        Caps,
        Done,
    }

    private generationProgress progress = generationProgress.Main;

    [Export]
    public int numberOfRooms = 10; // Rooms in addition to the special rooms and starting room (seed room)
    [Export]
    public int roomWidth = 16;
    [Export]
    public int roomHeight = 16;

    // The Godot.Collections.Array is wierd but you need to use it instead of List to see it in the editor and it just has more features like shuffle
    [Export]
    public Godot.Collections.Array<string> basicRooms = new Godot.Collections.Array<string>(); // Rooms to pick from to reach desired amount
    [Export]
    public Godot.Collections.Array<string> specialRooms = new Godot.Collections.Array<string>(); // Rooms guarunteed to generate
    [Export]
    public Godot.Collections.Array<string> seedRooms = new Godot.Collections.Array<string>(); // Rooms to pick one from to be the starting room
    [Export]
    public Godot.Collections.Array<string> lootRooms = new Godot.Collections.Array<string>(); // Rooms that generate at the end of branches
    [Export]
    public Godot.Collections.Array<int> branches = new Godot.Collections.Array<int>();

    public Godot.Collections.Array<string> roomsToGenerate = new Godot.Collections.Array<string>();

    public Godot.Collections.Array<Vector2> existingRoomPositions = new Godot.Collections.Array<Vector2>();


    public override void _Ready()
    {
        generateRoomList();
        // NEXT: Put stuff in other files too
    }
    
    public override void _Process(double delta)
    {
        switch (progress)
        {
            case generationProgress.Main:
                
                // initial seed
                int randomSeed = rng.Next(0, seedRooms.Count);
                var seedRoom = GD.Load<PackedScene>(seedRooms[randomSeed]);
                var inst = seedRoom.Instantiate();
                existingRoomPositions.Add(new Vector2(0, 0));
                AddChild(inst);
                progress = generationProgress.Branches;
                break;

            case generationProgress.Branches:
                // Generate Branches
                foreach (int branchLength in branches)
                {
                    Room branchSeed = GetChild<Room>(rng.Next(0, GetChildren().Count));
                    while (branchSeed.getOppositeSidesList().Count == 0)
                    {
                        branchSeed = GetChild<Room>(rng.Next(0, GetChildren().Count));
                    }
                    for (int i = 0; i < branchLength - 1; i++)
                    {
                        roomsToGenerate.Add(basicRooms[rng.Next(0, basicRooms.Count)]);
                    }
                    roomsToGenerate.Add(lootRooms[rng.Next(0, lootRooms.Count)]); // adds a loot room to the end of the branch

                    branchSeed.startGeneration();
                }
                progress = generationProgress.Caps;
                break;

            case generationProgress.Caps:
                // Generate caps
                foreach (Room room in GetChildren())
                {
                    room.generateCaps();
                }
                progress = generationProgress.Done;
                //SetProcess(false);
                break;

        }
    }

    private void generateRoomList()
    {
        foreach (string room in specialRooms)
        {
            roomsToGenerate.Add(room);
        }

        for (int i = 0; i < numberOfRooms; i++)
        {
            string newRoom = basicRooms[rng.Next(0, basicRooms.Count)];
            roomsToGenerate.Add(newRoom);
        }

        roomsToGenerate.Shuffle();
    }
}
