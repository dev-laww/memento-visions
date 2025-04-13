using Godot;
using System;
using Game.Autoload;
using Game.Components;
using Game.Data;
using GodotUtilities;
using DialogueManagerRuntime;

namespace Game.World.Levels.Chapter2;
[Scene]
public partial class EverFieldEntrance : BaseLevel
{
    [Node] private TransitionArea transitionArea;
    [Node] private Node2D enemy;
    
    private Quest quest = QuestRegistry.Get("quest:forest_awakening");
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    public override void _Ready()
    {
        base._Ready();
        transitionArea.Toggle(false);
        
        QuestManager.QuestCompleted += OnCompleted; ;
        ShowDialogue();
    }
    
   private void OnCompleted(Quest quest)
   {
     GD.Print("triggers");
           transitionArea.Toggle(true);
       
   }
    private static void ShowDialogue()
    {
        var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/chapter_2/2.3.dialogue");
        DialogueManager.ShowDialogueBalloon(dialogue);
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        QuestManager.QuestCompleted -= OnCompleted;
        enemy.QueueFree();
    }


}
