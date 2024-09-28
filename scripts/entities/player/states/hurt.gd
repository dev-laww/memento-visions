extends State

@onready var sprites: AnimatedSprite2D = %Sprites


func enter() -> void:
	owner.can_move = false

func update(_delta: float) -> void:
	sprites.play('hurt_%s' % owner.move_direction)
	
	await sprites.animation_finished
	
	owner.state_machine.change_state('idle')
	

func physics_update(_delta: float) -> void:
	# TODO: Implement hurt state logic
	pass

	
func exit() -> void:
	owner.can_move = true
