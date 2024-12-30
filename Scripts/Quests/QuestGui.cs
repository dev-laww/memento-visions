using Godot;
using GodotUtilities;

namespace Game.Quests;

[Scene]
public partial class QuestGui : Control
{
    private Tree Tree;
    private TreeItem TreeRoot;
    private bool IsVisible;
    private Label QuestTitle;
    private Label QuestDescription;
    private Label QuestStatus;
    private Label QuestReward;
    private PanelContainer QuestPanel;
    QuestObjectives questObjectives = new QuestObjectives();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        Tree = GetNode<Tree>("Column/Tree");
        QuestPanel = GetNode<PanelContainer>("PanelContainer");
        QuestTitle = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Title");
        QuestDescription = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Description");
        QuestStatus = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Status");
        QuestReward = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Reward");
       
        QuestManager.OnQuestsChanged += UpdateQuestList;
        // QuestManager.OnQuestStarted += OnQuestStarted;
        // QuestManager.OnQuestCompleted += OnQuestCompleted;
        Tree.ItemSelected += OnItemSelected;
        Visible = false;
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
            item.SetText(2,$"{questObjectives.GetProgress():P0}");
        }
    }
    
    public override void _ExitTree()
    {
        QuestManager.OnQuestsChanged -= UpdateQuestList;
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
        QuestStatus.Text = quest.Status.ToString();
        QuestReward.Text = quest.Reward.ToString();
    }

    public void ToggleQuestGui()
    {
        IsVisible = !IsVisible;
        Visible = IsVisible;
        if (IsVisible)
        {
            QuestManager.OnQuestsChanged += UpdateQuestList;
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Quest"))
        {
            ToggleQuestGui();
        }
    }
}