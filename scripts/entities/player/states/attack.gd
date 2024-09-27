extends State

@onready var sprites: AnimatedSprite2D = %Sprites


func enter() -> void:
	owner.can_move = false


func update(_delta: float) -> void:
	sprites.play('attack_%s' % owner.move_direction)

	await sprites.animation_finished

	if owner.velocity.length() > 0:
		state_machine.change_state('walk')
	else:
		state_machine.change_state('idle')


# TODO: Create attack physics update logic


func exit() -> void:
	owner.can_move = true
