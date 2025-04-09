using Godot;
using System;
using Game.Autoload;
using Game.Components;
using Game.Data;
using GodotUtilities;

namespace Game.World.Levels.Chapter2;
[Scene]
public partial class EverFieldEntrance : Node2D
{
    [Node] private TransitionArea transitionArea;
    
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
    }
    
   private void OnCompleted(Quest quest)
   {
     GD.Print("triggers");
           transitionArea.Toggle(true);
       
   }
   
    
}
