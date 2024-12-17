using Godot;
using GodotUtilities;

namespace Game.Quests
{
    [Scene]
    public partial class QuestGui : CanvasLayer
    {
        private Tree _tree;
        private TreeItem _treeRoot;
        private bool _isVisible = false;

        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated) return;
            WireNodes();
        }

        public override void _Ready()
        {
            _tree = GetNode<Tree>("Container/Column/Tree");
            QuestManager.OnQuestsChanged += UpdateQuestList;
            InitializeTree();
            Visible = false;
        }

        private void InitializeTree()
        {
            _tree.Clear();
            _treeRoot = _tree.CreateItem();
            _treeRoot.SetText(0, "Quests");
            _tree.Columns = 2;
            _tree.SetColumnTitle(0, "Quest Name");
            _tree.SetColumnTitle(1, "Status");
        }

        public void ToggleQuestGui()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            if (_isVisible)
            {
                UpdateQuestList();
            }
        }

        public void UpdateQuestList()
        {
            if (!Visible) return;
            while (_treeRoot.GetChildCount() > 0)
            {
                _treeRoot.GetChild(0).Free();
            }
            foreach (var quest in QuestManager.GetActiveQuests())
            {
                var item = _tree.CreateItem(_treeRoot);
                item.SetText(0, quest.QuestTitle);
                item.SetText(1, quest.Status.ToString());
            }
        }

        public override void _ExitTree()
        {
            QuestManager.OnQuestsChanged -= UpdateQuestList;
        }
    }
}