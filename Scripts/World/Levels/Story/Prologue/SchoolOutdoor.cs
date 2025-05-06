using Godot;
using Game.Components;
using Game.Entities;
using GodotUtilities;
using DialogueManagerRuntime;
using Game.Autoload;
using Game.Data;
using Game.Utils;

namespace Game.World;

[Scene]
public partial class SchoolOutdoor : BaseLevel
{
    [Node] private Entity storyTeller;
    public int ObjectiveInteracted = 0;
    [Node] TransitionArea transitionArea;
    [Node] AnimationPlayer animationPlayer;
    [Node] private Node2D enemies;
    [Node] private ScreenMarker transitionMarker;
    private Quest quest = QuestRegistry.Get("quest:night_of_shadows");


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }
    public override void _Ready()
    {
        ShowDialogue();
        transitionArea.Toggle(false);
        QuestManager.QuestUpdated += OnQuestUpdated;
        transitionMarker.Toggle(false);
        SaveManager.AddEnemyDetails("enemy:aswang");
    }
    
    private void OnQuestUpdated(Quest quest)
    {

       
            if (this.quest.Objectives[0].Completed)
            {
                animationPlayer.Play("show_story_teller");
                transitionArea.Toggle(true);
                transitionMarker.Toggle(true);
                QuestManager.QuestUpdated -= OnQuestUpdated;
            }
            
        
    }
    
    private static void ShowDialogue()
    {
        var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/prologue/1.5.dialogue");
        DialogueManager.ShowDialogueBalloon(dialogue);
    }
    private void ShowInstructions()
    {
        var text = new OverlayFactory.CenterTextBuilder(GetTree())
            .SetText("Press [B] to Open Inventory")
            .SetDuration(2f) 
            .Build();
        
        
    }
    

    public override void _ExitTree()
    {
        GD.Print("Exiting tree");
        enemies.QueueFree();
        QuestManager.QuestUpdated -= OnQuestUpdated;
    }
}