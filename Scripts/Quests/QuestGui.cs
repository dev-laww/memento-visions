using Godot;
using GodotUtilities;

namespace Game.Quests;

[Scene]
public partial class QuestGui : CanvasLayer
{
    private Tree Tree;
    private TreeItem TreeRoot;
    private bool IsVisible;
    private Label QuestTitle;
    private Label QuestDescription;
    private Label QuestStatus;
    private Label QuestReward;
    private PanelContainer QuestPanel;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        Tree = GetNode<Tree>("Container/Column/Tree");
        QuestPanel = GetNode<PanelContainer>("Container/PanelContainer");
        QuestTitle = GetNode<Label>("Container/PanelContainer/MarginContainer/VBoxContainer/Title");
        QuestDescription = GetNode<Label>("Container/PanelContainer/MarginContainer/VBoxContainer/Description");
        QuestStatus = GetNode<Label>("Container/PanelContainer/MarginContainer/VBoxContainer/Status");
        QuestReward = GetNode<Label>("Container/PanelContainer/MarginContainer/VBoxContainer/Reward");

        QuestManager.OnQuestsChanged += UpdateQuestList;
        InitializeTree();
        Tree.ItemSelected += OnItemSelected;
        Visible = false;
    }

    private void InitializeTree()
    {
        Tree.Clear();
        TreeRoot = Tree.CreateItem();
        TreeRoot.SetText(0, "Quests");
        Tree.Columns = 2;
        Tree.SetColumnTitle(0, "Quest Name");
        Tree.SetColumnTitle(1, "Status");
    }

    public void ToggleQuestGui()
    {
        IsVisible = !IsVisible;
        Visible = IsVisible;
        if (IsVisible) UpdateQuestList();
    }

    public void UpdateQuestList()
    {
        if (!Visible) return;
        while (TreeRoot.GetChildCount() > 0) TreeRoot.GetChild(0).Free();
        foreach (var quest in QuestManager.GetCompletedQuests())
        {
            var item = Tree.CreateItem(TreeRoot);
            item.SetText(0, quest.QuestTitle);
            item.SetText(1, quest.Status.ToString());
        }

        foreach (var quest in QuestManager.GetActiveQuests())
        {
            var item = Tree.CreateItem(TreeRoot);
            item.SetText(0, quest.QuestTitle);
            item.SetText(1, quest.Status.ToString());
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
        GD.Print(quest.QuestDescription);
        QuestTitle.Text = quest.QuestTitle;
        QuestDescription.Text = quest.QuestDescription;
        QuestStatus.Text = quest.Status.ToString();
        QuestReward.Text = quest.Reward.ToString();
        QuestPanel.Visible = true;
    }
}