using Godot;
using System.Linq;
using GodotUtilities;
using Game.Components;
using Game.Utils.Extensions;
using QuestResource = Game.Data.Quest;

namespace Game.UI.Overlays;


[Scene]
public partial class Quest : Overlay
{
    [Node] public Tree QuestTree;
    [Node] public RichTextLabel Description;
    [Node] public Label Title;
    [Node] public RichTextLabel Objectives;
    [Node] public Label Reward;
    public QuestManager questManager;

    private TreeItem TreeRoot;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        var player = this.GetPlayer();
        questManager = player.QuestManager;
        questManager.Added += OnQuestAdded;
        questManager.Updated += OnQuestUpdated;
        questManager.Removed += OnQuestRemoved;
        QuestTree.ItemSelected += OnItemSelected;

        InitializeTree();
    }

    public void InitializeTree()
    {
        QuestTree.Clear();
        QuestTree.SetColumnTitle(0, "Quests");
        TreeRoot = QuestTree.CreateItem();
    }

    private void OnQuestAdded(QuestResource quest)
    {
        var questItem = QuestTree.CreateItem(TreeRoot);
        questItem.SetText(0, quest.Title);
        questItem.SetMetadata(0, quest); 
    }

    private void OnQuestUpdated(QuestResource quest)
    {
        foreach (TreeItem item in TreeRoot.GetChildren())
        {
            if ((QuestResource)item.GetMetadata(0) != quest) continue;
            item.SetText(0, quest.Title);
            UpdateQuestDetails(quest);
            break;
        }
    }

    private void UpdateQuestDetails(QuestResource quest)
    {
        Title.Text = quest.Title;
        Description.Text = quest.Description;

        // Convert all objectives to a single string with BBCode formatting
        Objectives.Text = string.Join("\n", quest.Objectives.Select(objective =>
        {
            var color = objective.Completed ? "green" : "white";
            return $"[color={color}]{objective.Description}[/color]";
        }));
    }

    private void OnQuestRemoved(QuestResource quest)
    {
        foreach (TreeItem item in TreeRoot.GetChildren())
        {
            if ((QuestResource)item.GetMetadata(0) != quest) continue;
            item.Free();
            break;
        }
    }

    private void OnItemSelected()
    {
        var selectedItem = QuestTree.GetSelected();
        if (selectedItem == null) return;

        var quest = (QuestResource)selectedItem.GetMetadata(0);
        if (quest == null) return;

        UpdateQuestDetails(quest);
    }
    
}