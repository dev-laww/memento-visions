using System.Collections.Generic;
            using DialogueManagerRuntime;
            using Game.Autoload;
            using Game.Common.Interfaces;
            using Game.UI.Common;
            using Game.Common.Extensions;
            using Godot;
            
            namespace Game.Components;
            
            [Tool]
            [GlobalClass]
            public partial class DialogueTrigger : Area2D, IInteractable
            {
                [Export(hintString: "DialogueResource")]
                private Resource DialogueResource
                {
                    get => dialogueResource;
                    set
                    {
                        dialogueResource = value;
                        UpdateConfigurationWarnings();
                    }
                }
            
                [Export]
                private bool ShouldInteract
                {
                    get => shouldInteract;
                    set
                    {
                        shouldInteract = value;
                        InitializeUI();
                    }
                }
            
                [Export]
                private string InteractionLabel
                {
                    get => interactionLabel;
                    set
                    {
                        interactionLabel = value;
                        if (InteractionUI != null)
                            InteractionUI.Text = value;
                    }
                }
            
                public bool IsDialogueActive
                {
                    get => isDialogueActive;
                    private set => isDialogueActive = value;
                }
            
                public Vector2 InteractionPosition => GlobalPosition;
                private InteractionUI InteractionUI => GetNodeOrNull<InteractionUI>("Node2D/InteractionUI");
                private bool shouldInteract;
                private string interactionLabel = "Interact";
                private Resource dialogueResource;
                private bool isInteractable = true;
                private bool isDialogueActive;
            
                public override void _Ready()
                {
                    CollisionLayer = 0;
                    CollisionMask = 1 << 2;
            
                    if (Engine.IsEditorHint()) return;
            
                    BodyEntered += OnBodyEntered;
                    BodyExited += OnBodyExited;
            
                    InteractionUI?.Hide();
                    DialogueManager.DialogueStarted += _ => IsDialogueActive = true;
                    DialogueManager.DialogueEnded += _ => IsDialogueActive = false;
                }
            
                private void OnBodyEntered(Node2D _)
                {
                    if (!ShouldInteract)
                    {
                        if (!isDialogueActive)
                            DialogueManager.ShowDialogueBalloon(DialogueResource);
                        return;
                    }
            
                    InteractionManager.Register(this);
                }
            
                private void OnBodyExited(Node2D _)
                {
                    if (ShouldInteract)
                        InteractionManager.Unregister(this);
                }
            
                public override string[] _GetConfigurationWarnings()
                {
                    var warnings = new List<string>();
            
                    if (DialogueResource == null)
                        warnings.Add("DialogueResource is not set.");
            
                    return warnings.ToArray();
                }
            
                public override void _EnterTree()
                {
                    if (GetNodeOrNull("CollisionShape2D") == null)
                        this.EditorAddChild(new CollisionShape2D
                            { Name = "CollisionShape2D", DebugColor = new Color(0.88f, 0.525f, 0.898f, 0.42f) });
            
                    if (!ShouldInteract)
                    {
                        GetNodeOrNull("Node2D")?.QueueFree();
                        return;
                    }
            
                    this.AddInteractionUI();
                }
            
                public void Interact()
                {
                    if (!isDialogueActive)
                        DialogueManager.ShowDialogueBalloon(DialogueResource);
                }
            
                public void ShowUI() => InteractionUI?.Show();
                public void HideUI() => InteractionUI?.Hide();
            
                private void InitializeUI()
                {
                    var node = GetNodeOrNull("Node2D");
            
                    if (!IsNodeReady()) return;
            
                    if (!ShouldInteract)
                    {
                        node?.QueueFree();
                        return;
                    }
            
                    if (node == null)
                    {
                        this.AddInteractionUI();
                        if (InteractionUI != null)
                            InteractionUI.Text = interactionLabel;
                    }
                }
            }