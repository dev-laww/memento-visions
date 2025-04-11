using DialogueManagerRuntime;
using Game.Autoload;
using Game.Data;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class TownSquare : BaseLevel
{
    [Node] private Entity mayor;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private AnimationPlayer animationPlayer,storyTellerAnimationPlayer;
    [Node] private AudioStreamPlayer2D bossBgm;
    [Node] private AudioStreamPlayer2D chapter1Bgm;
    [Node] private StoryTeller storyTeller;
    private Quest quest = QuestRegistry.Get("quest:boss_quest");

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
        }
    }

    public override void _Ready()
    {
        QuestManager.QuestUpdated += OnQuestUpdated;
    }
    
    private void OnQuestUpdated(Quest quest)
    {
        if (this.quest.Objectives[0].Completed)
        {
            storyTellerAnimationPlayer.Play("show_story_teller");
            var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/chapter_1/1.9.dialogue");
            DialogueManager.ShowDialogueBalloon(dialogue);
        }
    }


    public void PlayFadeAnimation()
    {
        animationPlayer.Play("Fade");
        chapter1Bgm.StreamPaused = true;
        bossBgm.StreamPaused = false;

    }

    public void Spawn()
    {
        mayor.Visible = false;
        var aghon = resourcePreloader.InstanceSceneOrNull<Aghon>();
        aghon.GlobalPosition = mayor.GlobalPosition;
        aghon.Death += _ => SaveManager.UnlockFrenzyMode();

        AddChild(aghon);
    }
    
    
    
    
}