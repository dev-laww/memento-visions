extends State

@onready var sprites: AnimatedSprite2D = %Sprites
@onready var player := owner as Player


func enter() -> void:
	player.can_move = false


func update(_delta: float) -> void:
	sprites.play('attack_%s' % player.move_direction)

	await sprites.animation_finished

	if player.velocity.length() > 0:
		state_machine.change_state('walk')
	else:
		state_machine.change_state('idle')


# TODO: Create attack physics update logic


func exit() -> void:
	player.can_move = true
