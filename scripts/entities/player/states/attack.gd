extends State

@onready var sprites: AnimatedSprite2D = %Sprites


func enter() -> void:
	state_owner.can_move = false


func update(_delta: float) -> void:
	sprites.play('attack_%s' % state_owner.move_direction)

	await sprites.animation_finished

	if state_owner.velocity.length() > 0:
		state_machine.change_state('walk')
	else:
		state_machine.change_state('idle')
		
		
# TODO: Create attack physics update logic


func exit() -> void:
	state_owner.can_move = true
