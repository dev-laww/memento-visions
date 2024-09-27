@tool
class_name HurtBox
extends Area2D

signal damage_received(damage: float)


func _ready() -> void:
	area_entered.connect(hurt_box_entered)


func receive_damage(damage: float) -> void:
	damage_received.emit(damage)


func hurt_box_entered(hitbox: HitBox) -> void:
	var damage: float = hitbox.damage

	hitbox.damage_inflicted.emit(damage)

	receive_damage(damage)
