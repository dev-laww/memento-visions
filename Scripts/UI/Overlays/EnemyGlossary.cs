using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.UI.Common;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class EnemyGlossary : Overlay
{
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private TextureButton previousButton;
    [Node] private TextureButton nextButton;
    [Node] private TextureButton closeButton;
    [Node] private HBoxContainer enemyDetailsContainer;

    private readonly Dictionary<int, List<EntityDetail>> enemyDetails = [];
    private int currentPage = 1;
    private int totalPages;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        var details = EntityDetailRegistry.Get(EntityDetail.EntityType.Enemy).ToList();

        var pageSize = 2;
        totalPages = details.Count / pageSize + (details.Count % pageSize == 0 ? 0 : 1);

        for (var i = 0; i < totalPages; i++)
        {
            var pageDetails = details.Skip(i * pageSize).Take(pageSize).ToList();
            enemyDetails[i] = [.. pageDetails];
        }

        UpdateEnemyDetails();

        previousButton.Pressed += OnPreviousButtonPressed;
        nextButton.Pressed += OnNextButtonPressed;
        closeButton.Pressed += Close;
    }

    private void OnPreviousButtonPressed()
    {
        UpdateButtons();
        if (currentPage == 1) return;

        currentPage--;
        UpdateEnemyDetails();
    }

    private void OnNextButtonPressed()
    {
        UpdateButtons();
        if (currentPage == totalPages) return;

        currentPage++;
        UpdateEnemyDetails();
    }

    private void UpdateEnemyDetails()
    {
        enemyDetailsContainer.QueueFreeChildren();

        foreach (var detail in enemyDetails[currentPage - 1])
        {
            var enemyDetail = resourcePreloader.InstanceSceneOrNull<EnemyDetails>();
            enemyDetail.Detail = detail;
            enemyDetailsContainer.AddChild(enemyDetail);
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        previousButton.Disabled = currentPage == 1;
        nextButton.Disabled = currentPage == totalPages;
    }
}

