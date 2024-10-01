class_name Player
extends CharacterBody2D

@export var dash_stamina_cost: float = 30.0

@onready var sprites: AnimatedSprite2D = $Sprites
@onready var state_machine: StateMachine = $StateMachine
@onready var stats_manager: StatsManager = $StatsManager

@onready var move_direction: String:
	get = get_direction

var last_direction: Vector2 = Vector2.DOWN
var grid_size: int = 8
var can_move: bool = true
var can_dash: bool = true
var dashing: bool = false
var dash_velocity: Vector2 = Vector2.ZERO


# TODO: Create a velocity component to handle entity velocity


func _physics_process(_delta: float) -> void:
	velocity = Input.get_vector('move_left', 'move_right', 'move_up', 'move_down').normalized() * stats_manager.speed
	velocity = Vector2(round(velocity.x / grid_size) * grid_size, round(velocity.y / grid_size) * grid_size)
	velocity = velocity if can_move else Vector2.ZERO

	if dashing:
		velocity = dash_velocity

	if velocity.length() > 0 and can_move:
		last_direction = velocity.normalized()

	move_and_slide()

	if Input.is_action_just_pressed('dash') and can_dash:
		stats_manager.consume_stamina(dash_stamina_cost)
		state_machine.change_state('dash')


func get_direction() -> String:
	if last_direction == Vector2.ZERO:
		return 'front'

	if abs(last_direction.x) > abs(last_direction.y):
		return 'right' if last_direction.x > 0 else 'left'
	else:
		return 'back' if last_direction.y < 0 else 'front'


func _on_hurt_box_damage_received(damage: float) -> void:
	state_machine.change_state('hurt')
	stats_manager.take_damage(damage)
	print('Player received %s damage' % damage)


func _on_stats_manager_stamina_changed(stamina: float) -> void:
	if stamina < dash_stamina_cost:
		can_dash = false
		return

	can_dash = true
