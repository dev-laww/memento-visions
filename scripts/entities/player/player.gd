extends CharacterBody2D

@export var speed: float = 50.0
@export var dash_cooldown: float = 3.0

@onready var sprites: AnimatedSprite2D = $Sprites
@onready var move_direction: String = get_direction()
@onready var state_machine: StateMachine = $StateMachine

var last_direction: Vector2 = Vector2.ZERO
var grid_size: int          = 8
var can_move: bool          = true
var can_dash: bool          = true
var dash_velocity: Vector2  = Vector2.DOWN


# TODO: Create stats manager component to entity stats
# TODO: Create health system to handle entity health


func _physics_process(_delta: float) -> void:
	velocity = (Input.get_vector('move_left', 'move_right', 'move_up', 'move_down').normalized() * speed) + dash_velocity
	velocity =  Vector2(round(velocity.x / grid_size) * grid_size, round(velocity.y / grid_size) * grid_size)

	if velocity.length() > 0 and can_move:
		last_direction = velocity.normalized()
		move_direction = get_direction()

	if not can_move:
		velocity = Vector2.ZERO

	if Input.is_action_just_pressed('dash') and can_dash:
		state_machine.change_state('dash')

		can_dash = false

		get_tree().create_timer(dash_cooldown).timeout.connect(func (): can_dash = true)

	move_and_slide()


func get_direction() -> String:
	if last_direction == Vector2.ZERO:
		return 'front'

	if abs(last_direction.x) > abs(last_direction.y):
		return 'right' if last_direction.x > 0 else 'left'
	else:
		return 'back' if last_direction.y < 0 else 'front'  # Corrected direction handling
