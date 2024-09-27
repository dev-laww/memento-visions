extends State

@onready var sprites: AnimatedSprite2D = %Sprites


func update(_delta: float) -> void:
	sprites.play('walk_%s' % state_owner.move_direction)


func physics_update(_delta: float) -> void:
	if state_owner.velocity.length() == 0:
		state_machine.change_state('idle')
	elif Input.is_action_just_pressed('attack'):
		state_machine.change_state('attack')
