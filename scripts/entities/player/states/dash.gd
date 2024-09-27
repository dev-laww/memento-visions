extends State

@onready var sprites: AnimatedSprite2D = %Sprites


func enter() -> void:
	owner.can_dash = false
	owner.dashing = true


func physics_update(_delta: float) -> void:
	owner.dash_velocity = owner.last_direction * (owner.speed * 20)

	var tween: Tween = create_tween()
	tween.set_parallel().tween_property(owner, 'dash_velocity', Vector2.ZERO, 0.1)
	tween.set_parallel().tween_property(owner, 'dashing', false, 0.1)

	tween.finished.connect(func (): state_machine.change_state('idle'))		
