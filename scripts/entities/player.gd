extends CharacterBody2D

@export var speed: float = 50.0
@export var dash_cooldown: float = 5.0

@onready var sprites: AnimatedSprite2D = $Sprites
@onready var move_direction: String = get_direction()

var state_machine           = DelagateStateMachine.new()
var last_direction: Vector2 = Vector2.ZERO
var grid_size: int          = 8
var can_move: bool          = true
var can_dash: bool          = true


func _ready() -> void:

	state_machine.add_states(idle)
	state_machine.add_states(walk)
	state_machine.add_states(attack, attack_enter, attack_exit)
	state_machine.add_states(dash, dash_enter)
	state_machine.set_initial_state(idle)


func _physics_process(_delta: float) -> void:
	velocity = Input.get_vector("move_left", "move_right", "move_up", "move_down").normalized() * speed
	velocity =  Vector2(round(velocity.x / grid_size) * grid_size, round(velocity.y / grid_size) * grid_size)

	if velocity.length() > 0:
		last_direction = velocity.normalized()

	state_machine.update()

	if not can_move:
		velocity = Vector2.ZERO
		
	if Input.is_action_just_pressed("dash") and can_dash:
		state_machine.change_state(dash)

		can_dash = false

		get_tree().create_timer(dash_cooldown).timeout.connect(func (): can_dash = true)

	move_and_slide()


func idle() -> void:
	sprites.play('idle_%s' % get_direction())
	if velocity.length() > 0:
		state_machine.change_state(walk)
	elif Input.is_action_just_pressed("attack"):
		state_machine.change_state(attack)


func walk() -> void:
	sprites.play('walk_%s' % get_direction())

	if velocity.length() == 0:
		state_machine.change_state(idle)
	elif Input.is_action_just_pressed("attack"):
		state_machine.change_state(attack)


func attack() -> void:
	# lock the player in place and direction while attacking
	sprites.play('attack_%s' % move_direction)

	await sprites.animation_finished

	if velocity.length() > 0:
		state_machine.change_state(walk)
	else:
		state_machine.change_state(idle)


func attack_enter() -> void:
	move_direction = get_direction()
	can_move = false


func attack_exit() -> void:
	can_move = true


func dash_enter() -> void:
	move_direction = get_direction()
	can_dash = false


func dash() -> void:
	velocity = last_direction * (speed * 10)

	move_and_slide()

	state_machine.change_state(idle)


func get_direction() -> String:
	if last_direction == Vector2.ZERO:
		return "front"

	if abs(last_direction.x) > abs(last_direction.y):
		return "right" if last_direction.x > 0 else "left"
	else:
		return "back" if last_direction.y < 0 else "front"  # Corrected direction handling
