@tool
class_name ContinuousDamageHitBox
extends HitBox

@export var damage_interval: float = 0.5

var timer: Timer


func _ready() -> void:
	timer = Timer.new()
	timer.wait_time = damage_interval
	timer.timeout.connect(_on_timeout)
	timer.autostart = true

	add_child(timer)


func _on_timeout() -> void:
	for hurt_box in get_overlapping_areas():
		if not hurt_box.is_in_group('HurtBox'):
			continue

		damage_inflicted.emit(damage)
		hurt_box.receive_damage(damage)
