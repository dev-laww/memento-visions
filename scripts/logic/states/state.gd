@tool
class_name State
extends Node

@onready var state_machine: StateMachine = get_parent()
@onready var state_owner: CharacterBody2D = state_machine.get_parent()


func enter() -> void:
	pass


func exit() -> void:
	pass


func update(delta: float) -> void:
	pass


func physics_update(delta: float) -> void:
	pass


func _get_configuration_warnings() -> PackedStringArray:
	var warnings: PackedStringArray = []

	if not get_parent() is StateMachine:
		warnings.push_back('State must be a child of a StateMachine')

	return warnings
