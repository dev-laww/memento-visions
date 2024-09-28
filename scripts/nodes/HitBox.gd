@tool
class_name HitBox
extends Area2D

@export var damage: float:
	set(value):
		update_configuration_warnings()
		damage = value

		if not damage:
			return

		if value > damage:
			damage_increased.emit()
			return

		damage_decreased.emit()

signal damage_inflicted(damage: float)
signal damage_increased
signal damage_decreased


func _get_configuration_warnings() -> PackedStringArray:
	var warnings: PackedStringArray = []

	if not damage:
		warnings.push_back('HitBox has no damage value')

	return warnings
