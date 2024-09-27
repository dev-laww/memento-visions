extends RefCounted
class_name DelagateStateMachine

# Stores the current state function reference
var current_state: Callable
# Dictionary to hold states and their associated flows
var states: Dictionary = {}


# Adds a new state with optional enter and leave states
func add_states(normal: Callable, enter_state: Callable = Callable(), leave_state: Callable = Callable()) -> void:
	var state_flows = StateFlows.new(normal, enter_state, leave_state)
	states[normal] = state_flows


# Changes the current state to the provided state delegate
func change_state(to_state_delegate: Callable) -> void:
	var state_flows = states.get(to_state_delegate, null)
	if state_flows:
		call_deferred('set_state', state_flows)


# Sets the initial state directly without deferral
func set_initial_state(state_delegate: Callable) -> void:
	var state_flows = states.get(state_delegate, null)

	if state_flows:
		set_state(state_flows)


# Returns the current state
func get_current_state() -> Callable:
	return current_state


# Updates the current state by invoking it
func update() -> void:
	if current_state and current_state.is_valid():
		current_state.call()


# Private method to set the state, calling leave and enter states appropriately
func set_state(state_flows: StateFlows) -> void:
	if current_state and states.has(current_state):
		var current_state_flows = states[current_state]
		if current_state_flows.leave_state and current_state_flows.leave_state.is_valid():
			current_state_flows.leave_state.call()
	current_state = state_flows.normal
	if state_flows.enter_state and state_flows.enter_state.is_valid():
		state_flows.enter_state.call()


# Inner class to hold state flows
class StateFlows:
	var normal: Callable
	var enter_state: Callable
	var leave_state: Callable


	func _init(normal: Callable, enter_state: Callable = Callable(), leave_state: Callable = Callable()) -> void:
		self.normal = normal
		self.enter_state = enter_state
		self.leave_state = leave_state
