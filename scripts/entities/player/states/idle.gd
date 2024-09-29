extends State

@onready var sprites: AnimatedSprite2D = %Sprites
@onready var player := owner as Player


func update(_delta: float) -> void:
	sprites.play('idle_%s' % player.move_direction)


func physics_update(_delta: float) -> void:
	if player.velocity.length() > 0:
		state_machine.change_state('walk')
	elif Input.is_action_just_pressed('attack'):
		state_machine.change_state('attack')
