using Godot;
using GodotUtilities;

namespace Game.Quests
{
    [Scene]
    public partial class QuestGui : CanvasLayer
    {
        private Tree Tree;
        private TreeItem TreeRoot;

        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated) return;
            WireNodes();
        }

        public override void _Ready()
        {
            Tree = GetNode<Tree>("Container/Column/Tree");
            Tree.Clear();

            // Create the root item
            TreeRoot = Tree.CreateItem();
            TreeRoot.SetText(0, "Quests");

            // Create columns
            Tree.Columns = 2;
            Tree.SetColumnTitle(0, "Quest Name");
            Tree.SetColumnTitle(1, "Status");
        }

        public void addQuests()
        {
            foreach (var quest in QuestManager.Quests)
            {
                var item = Tree.CreateItem(TreeRoot);
                item.SetText(0, quest.QuestTitle);
                item.SetText(1, quest.Status.ToString());
                GD.Print(quest);
            }
        }
    }
}