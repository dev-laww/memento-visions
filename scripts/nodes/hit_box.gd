@tool
class_name HitBox
extends Area2D

@export var damage: float:
	set(value):
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


func _init() -> void:
	collision_layer = 11
	collision_mask = 0
