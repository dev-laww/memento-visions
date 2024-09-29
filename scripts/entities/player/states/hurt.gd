extends State

@onready var sprites: AnimatedSprite2D = %Sprites
@onready var player := owner as Player


func enter() -> void:
	player.can_move = false

func update(_delta: float) -> void:
	sprites.play('hurt_%s' % player.move_direction)
	
	await sprites.animation_finished
	
	player.state_machine.change_state('idle')
	

func physics_update(_delta: float) -> void:
	# TODO: Implement hurt state logic
	pass

	
func exit() -> void:
	player.can_move = true
