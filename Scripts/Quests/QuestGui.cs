using System;
using System.Linq;
using Godot;
using GodotUtilities;

namespace Game.Quests;

[Scene]
public partial class QuestGui : Control
{
    private Tree Tree;
    private TreeItem TreeRoot;
    private bool IsVisible = false;
    private Label QuestTitle;
    private Label QuestDescription;
    private Label QuestStatus;
    private Label QuestReward;
    private PanelContainer QuestPanel;
    private Button CloseButton;


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

public override void _Ready()
{
    Tree = GetNode<Tree>("Column/PanelContainer/MarginContainer/Tree");
    Tree.Columns = 3;
    QuestPanel = GetNode<PanelContainer>("PanelContainer");
    QuestTitle = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Title");
    QuestDescription = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Description");
    QuestStatus = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Status");
    QuestReward = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Reward");
    CloseButton = GetNode<Button>("CloseButton");
    

    QuestManager.OnQuestsChanged += UpdateQuestList;
    Tree.ItemSelected += OnItemSelected;
    Visible = false;
    CloseButton.Pressed += ToggleQuestGui;
    QuestObjectives.OnProgressUpdated += UpdateQuestList;
}

private void UpdateQuestList()
{
    Tree.Clear();
    TreeRoot = Tree.CreateItem();
    TreeRoot.SetText(0, "Quests");
        
    foreach (var quest in QuestManager.GetActiveQuests()) 
    {
        var item = Tree.CreateItem(TreeRoot);
        item.SetText(0, quest.QuestTitle);
        item.SetText(1, quest.Status.ToString());

        if (quest.Objectives != null)
        {
            item.SetText(2, $"{quest.Objectives.currentCount}/{quest.Objectives.TargetCount}");
        }
        else
        {
            item.SetText(2, "N/A");
        }
    }
        
    foreach (var quest in QuestManager.GetCompletedQuests())
    {
        var item = Tree.CreateItem(TreeRoot);
        item.SetText(0, quest.QuestTitle);
        item.SetText(1, quest.Status.ToString());

        if (quest.Objectives != null)
        {
            item.SetText(2, $"{quest.Objectives.TargetCount}/{quest.Objectives.TargetCount}");
        }
        else
        {
            item.SetText(2, "Complete");
        }
    }
}
    
    public override void _ExitTree()
    {
        QuestManager.OnQuestsChanged -= UpdateQuestList;
        QuestObjectives.OnProgressUpdated -= UpdateQuestList;
        Tree.ItemSelected -= OnItemSelected;
        QuestPanel.Visible = false;
    }

    public void OnItemSelected()
    {
        var item = Tree.GetSelected();
        if (item == null) return;
        var quest = QuestManager.GetQuestByTitle(item.GetText(0));
        if(quest == null) return;
        UpdateQuestDetails(quest);
    }
    
    private void UpdateQuestDetails(Quest quest)
    {
        QuestPanel.Visible = true;
        QuestTitle.Text = quest.QuestTitle;
        QuestDescription.Text = quest.QuestDescription;
        
        if (quest.Objectives != null)
        {
            if (quest.Status == Quest.QuestStatus.Completed)
            {
                QuestStatus.Text = $"Completed ({quest.Objectives.TargetCount}/{quest.Objectives.TargetCount})";
            }
            else
            {
                QuestStatus.Text = $"Progress: {quest.Objectives.currentCount}/{quest.Objectives.TargetCount}";
            }
        }
        else
        {
            QuestStatus.Text = quest.Status.ToString();
        }
        
        var rewardText = $"Gold: {quest.Gold} Experience: {quest.Experience}";
        
        if (quest.QuestItems != null && quest.QuestItems.Length > 0)
        {
            var itemNames = quest.QuestItems
                .Where(item => item != null)
                .Select(item => item.Name)
                .ToList();
            
            if (itemNames.Any())
            {
                rewardText += $"\nItems: {string.Join(", ", itemNames)}";
            }
        }

        QuestReward.Text = rewardText;
    }

    public void ToggleQuestGui()
    {
        IsVisible = !IsVisible;
        Visible = IsVisible;
        UpdateQuestList();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Quest"))
        {
            ToggleQuestGui();
        }
    }
}