class_name StatsManager
extends Node

@export var max_health: float = 100
@export var speed: float = 50
@export var max_stamina: float = 100

@export_range(1, 10) var stamina_regeneration_rate: float = 1

@onready var health: float = max_health
@onready var stamina: float = max_stamina
@onready var stamina_regeneration_timer: Timer = $StaminaRegenerationTimer
# Health Signals
signal health_changed(health: float)
signal health_increased(amount: float)
signal health_decreased(amount: float)
signal health_depleted()
# Stamina Signals
signal stamina_changed(stamina: float)
signal stamina_decreased(amount: float)
signal stamina_increased(amount: float)
signal stamina_depleted()


# TODO: add the attack system to handle entity attacks (create a class for attacks)
# TODO: add the buff system to handle entity buffs and debuffs

func _ready() -> void:
	stamina_regeneration_timer.timeout.connect(func (): regenerate_stamina(stamina_regeneration_rate))
	stamina_regeneration_timer.start()


func heal(amount: float) -> void:
	if health == max_health:
		return

	health_changed.emit(health)
	health_increased.emit(amount)
	health = min(health + amount, max_health)


func take_damage(amount: float) -> void:
	if health == 0:
		return

	health_changed.emit(health)
	health_decreased.emit(amount)
	health = max(health - amount, 0)

	if health == 0:
		health_depleted.emit()


func consume_stamina(amount: float) -> void:
	if stamina == 0:
		return

	stamina_changed.emit(stamina)
	stamina_decreased.emit(amount)
	stamina = max(stamina - amount, 0)

	if stamina == 0:
		stamina_depleted.emit()


func regenerate_stamina(amount: float) -> void:
	if stamina == max_stamina:
		return

	stamina_changed.emit(stamina)
	stamina_increased.emit(amount)
	stamina = min(stamina + amount, max_stamina)
