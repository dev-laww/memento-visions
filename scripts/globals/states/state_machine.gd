@tool
class_name StateMachine
extends Node

## A state machine that manages child states

@export var initial_state: State = null:
	set(value):
		initial_state = value
		update_configuration_warnings()

@onready var current_state: State = initial_state:
	set(value):
		current_state = value
		state_changed.emit(value)

var states: Array[State]          = []
signal state_changed(state: State)


func _ready() -> void:
	# Add child states to the state machine
	for child in get_children():
		if not child is State: continue
		states.append(child)

	if Engine.is_editor_hint(): return

	if not initial_state:
		printerr("No initial state set in state machine")
		push_error("No initial state set in state machine")
		return

	current_state.enter()


func change_state(state: String) -> void:
	var to_state: State = null

	for s in states:
		if s.name.to_lower() != state.to_lower(): continue

		to_state = s
		break

	if not to_state in states:
		printerr("State not found in state machine")
		push_error("State not found in state machine")
		return

	if current_state:
		current_state.exit()

	current_state = to_state
	current_state.enter()


func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint(): return

	current_state.physics_update(delta)


func _process(delta: float) -> void:
	if Engine.is_editor_hint(): return

	current_state.update(delta)


func _get_configuration_warnings() -> PackedStringArray:
	var warnings: PackedStringArray = PackedStringArray()

	if not states:
		warnings.append("No states found in state machine")

	if not initial_state:
		warnings.append("No initial state set in state machine")

	for child in get_children():
		if child is State: continue

		warnings.append("Child node is not a state: %s" % child.name)

	if not get_parent() is CharacterBody2D:
		warnings.append("State machine must be a child of a CharacterBody2D")

	return warnings
