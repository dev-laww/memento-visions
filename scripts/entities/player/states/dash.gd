extends State

@onready var sprites: AnimatedSprite2D = %Sprites
@onready var stats_manager: StatsManager = %StatsManager
@onready var player := owner as Player


func enter() -> void:
	if not player.can_dash:
		state_machine.change_state('idle')
		return
		
	player.dashing = true


func physics_update(_delta: float) -> void:
	player.dash_velocity = player.last_direction * (stats_manager.speed * 20)

	var tween: Tween = create_tween()
	tween.set_parallel().tween_property(player, 'dash_velocity', Vector2.ZERO, 0.1)
	tween.set_parallel().tween_property(player, 'dashing', false, 0.1)

	tween.finished.connect(func (): state_machine.change_state('idle'))
