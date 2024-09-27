extends State

@onready var sprites: AnimatedSprite2D = %Sprites


func enter() -> void:
	state_owner.can_dash = false
	state_owner.dashing = true


func physics_update(_delta: float) -> void:
	state_owner.dash_velocity = state_owner.last_direction * (state_owner.speed * 20)

	var tween: Tween = create_tween()
	tween.set_parallel().tween_property(state_owner, 'dash_velocity', Vector2.ZERO, 0.1)
	tween.set_parallel().tween_property(state_owner, 'dashing', false, 0.1)

	tween.finished.connect(func (): state_machine.change_state('idle'))		
