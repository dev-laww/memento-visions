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
    private RichTextLabel QuestDescription;
    private Label QuestStatus;
    private Label QuestReward;
    private TextureButton CloseButton;
    private string CurrentQuestTitle; // Track the currently selected quest

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        Tree = GetNode<Tree>("MarginContainer/VBoxContainer/NinePatchRect/MarginContainer/HBoxContainer/Tree");
        Tree.Columns = 2;

        Tree.SetColumnCustomMinimumWidth(0, 200);
        Tree.SetColumnExpand(0, true);
        
        QuestTitle = GetNode<Label>("MarginContainer/VBoxContainer/NinePatchRect/MarginContainer/HBoxContainer/NinePatchRect/MarginContainer/VBoxContainer/Title");
        QuestDescription = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/NinePatchRect/MarginContainer/HBoxContainer/NinePatchRect/MarginContainer/VBoxContainer/Description");
        QuestStatus = GetNode<Label>("MarginContainer/VBoxContainer/NinePatchRect/MarginContainer/HBoxContainer/NinePatchRect/MarginContainer/VBoxContainer/Status");
        QuestReward = GetNode<Label>("MarginContainer/VBoxContainer/NinePatchRect/MarginContainer/HBoxContainer/NinePatchRect/MarginContainer/VBoxContainer/Reward");
        CloseButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/Header/CloseButton");

        QuestManager.OnQuestsChanged += UpdateQuestList;
        QuestManager.OnQuestsChanged += UpdateCurrentQuestDetails;
        Tree.ItemSelected += OnItemSelected;
        Visible = false;
        CloseButton.Pressed += ToggleQuestGui;
        QuestObjectives.OnProgressUpdated += UpdateQuestList;
        QuestObjectives.OnProgressUpdated += UpdateCurrentQuestDetails;
    }

    private void UpdateQuestList()
    {
        Tree.Clear();
        TreeRoot = Tree.CreateItem();
        TreeRoot.SetText(0, "Quests");
            
        foreach (var quest in QuestManager.GetActiveQuests()) 
        {
            var item = Tree.CreateItem(TreeRoot);
            item.SetText(0, $"{quest.QuestTitle}\n{quest.QuestSubtitle.Split('.')[0]}");
            item.SetText(1, quest.Status.ToString());
            item.SetMetadata(0, quest.QuestTitle);
            if (quest.QuestTitle == CurrentQuestTitle)
            {
                item.Select(0);
            }
        }
            
        foreach (var quest in QuestManager.GetCompletedQuests())
        {
            var item = Tree.CreateItem(TreeRoot);
            item.SetText(0, $"{quest.QuestTitle}\n{quest.QuestSubtitle.Split('.')[0]}");
            item.SetText(1, quest.Status.ToString());
            item.SetMetadata(0, quest.QuestTitle);

            if (quest.QuestTitle == CurrentQuestTitle)
            {
                item.Select(0);
            }
        }
    }
    
    public override void _ExitTree()
    {
        QuestManager.OnQuestsChanged -= UpdateQuestList;
        QuestManager.OnQuestsChanged -= UpdateCurrentQuestDetails;
        QuestObjectives.OnProgressUpdated -= UpdateQuestList;
        QuestObjectives.OnProgressUpdated -= UpdateCurrentQuestDetails;
        Tree.ItemSelected -= OnItemSelected;
    }

    public void OnItemSelected()
    {
        var item = Tree.GetSelected();
        if (item == null) return;
        var questTitle = (string)item.GetMetadata(0);
        CurrentQuestTitle = questTitle;
        var quest = QuestManager.GetQuestByTitle(questTitle);
        if(quest == null) return;
        UpdateQuestDetails(quest);
    }
    
    private void UpdateCurrentQuestDetails()
    {
        if (string.IsNullOrEmpty(CurrentQuestTitle)) return;
        var quest = QuestManager.GetQuestByTitle(CurrentQuestTitle);
        if (quest != null)
        {
            UpdateQuestDetails(quest);
        }
    }
    
    private void UpdateQuestDetails(Quest quest)
    {
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
        UpdateCurrentQuestDetails(); // Update details when toggling GUI
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Quest"))
        {
            ToggleQuestGui();
        }
    }
}