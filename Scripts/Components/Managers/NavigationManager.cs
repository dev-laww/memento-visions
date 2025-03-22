using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Common.Extensions;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class NavigationManager : Polygon2D
{
    private const float MAP_CELL_SIZE = 1f;
    private const float CELL_SIZE = 1f;
    private const int CHUNK_SIZE = 256;
    private const float AGENT_RADIUS = 10f;

    [ExportToolButton("Bake Navigation", Icon = "Bake")] private Callable Place => Callable.From(PlaceNavigationRegions);

    [Export]
    private Node2D ParseRootNode
    {
        get => _parseRootNode;
        set
        {
            _parseRootNode = value;
            UpdateConfigurationWarnings();
        }
    }

    [Export]
    private NavigationPolygon NavigationPolygon
    {
        get => _navigationPolygon;
        set
        {
            _navigationPolygon = value;
            UpdateConfigurationWarnings();
        }
    }

    [ExportGroup("Navigation Settings")]
    [Export] private bool useEdgeConnections = true;
    [Export] private float agentRadius = AGENT_RADIUS;
    [Export] private int chunkSize = CHUNK_SIZE;
    [Export] private float cellSize = CELL_SIZE;
    [Export] private float mapCellSize = MAP_CELL_SIZE;


    private Node2D _parseRootNode;
    private NavigationPolygon _navigationPolygon;

    public override void _Ready()
    {
        ZIndex = 1000;
        Color = new Color(1f, 1f, 1f, 0f);
        NotifyPropertyListChanged();
    }

    public void PlaceNavigationRegions()
    {
        if (ParseRootNode is null || NavigationPolygon is null)
        {
            GD.PushWarning("Parse Root Node or Navigation Polygon is not set.");
            return;
        }

        if (Polygon.Length < 3)
        {
            GD.PushWarning("Polygon must have at least 3 points.");
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Initialize();

        var sourceGeometry = new NavigationMeshSourceGeometryData2D();

        NavigationServer2D.ParseSourceGeometryData(NavigationPolygon, sourceGeometry, ParseRootNode);
        sourceGeometry.AddTraversableOutline(Polygon);

        CreateRegionChunks(sourceGeometry);

        stopwatch.Stop();

        if (OS.IsDebugBuild())
        {
            GD.Print($"Navigation generation took {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    public void Clear()
    {
        foreach (var region in GetChildren().OfType<NavigationRegion2D>())
        {
            region.QueueFree();
        }
    }

    private void Initialize()
    {
        Clear();

        NavigationServer2D.SetDebugEnabled(true);

        var map = GetWorld2D().NavigationMap;

        NavigationServer2D.MapSetCellSize(map, mapCellSize);
        NavigationServer2D.MapSetUseEdgeConnections(map, useEdgeConnections);
    }

    private void CreateRegionChunks(NavigationMeshSourceGeometryData2D sourceGeometry)
    {
        var bounds = sourceGeometry.GetBounds();

        var startChunk = new Vector2I(
            (int)Math.Floor(bounds.Position.X / chunkSize),
            (int)Math.Floor(bounds.Position.Y / chunkSize)
        );

        var endChunk = new Vector2I(
            (int)Math.Floor((bounds.Position.X + bounds.Size.X) / chunkSize),
            (int)Math.Floor((bounds.Position.Y + bounds.Size.Y) / chunkSize)
        );

        for (var y = startChunk.Y; y <= endChunk.Y; y++)
        {
            for (var x = startChunk.X; x <= endChunk.X; x++)
            {
                var chunkBounds = new Rect2(
                  new Vector2(x * chunkSize, y * chunkSize),
                  new Vector2(chunkSize, chunkSize)
                );

                var bakingBounds = chunkBounds.Grow(chunkSize);

                var chunkNavigationMesh = new NavigationPolygon
                {
                    ParsedGeometryType = NavigationPolygon.ParsedGeometryTypeEnum.StaticColliders,
                    BakingRect = bakingBounds,
                    BorderSize = chunkSize,
                    AgentRadius = agentRadius,
                };

                NavigationServer2D.BakeFromSourceGeometryData(chunkNavigationMesh, sourceGeometry);

                chunkNavigationMesh.BakingRect = new Rect2();

                var navMeshVertices = chunkNavigationMesh.GetVertices();

                for (var i = 0; i < navMeshVertices.Length; i++)
                {
                    var vertex = navMeshVertices[i];

                    navMeshVertices[i] = vertex.Snapped(mapCellSize * 0.1f);
                }

                chunkNavigationMesh.SetVertices(navMeshVertices);

                var region = new NavigationRegion2D { NavigationPolygon = chunkNavigationMesh };

                this.EditorAddChild(region);
            }
        }
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (ParseRootNode is null)
            warnings.Add("Parse Root Node is not set.");

        if (NavigationPolygon is null)
            warnings.Add("Navigation Polygon is not set.");

        return [.. warnings];
    }
}
