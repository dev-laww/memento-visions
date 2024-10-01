extends Node

var RNG := RandomNumberGenerator.new()


func _init() -> void:
	RNG.randomize()


func get_nodes_from_group(group: String) -> Array[Node]:
	return get_tree().get_nodes_in_group(group)


func delta_lerp(from: float, to: float, delta_time: float, smoothing: float) -> float:
	return lerp(from, to, 1.0 - exp(-delta_time * smoothing))


func seed_rng(seed_: int) -> void:
	RNG = RandomNumberGenerator.new()
	RNG.seed = seed_


func kill_tween_if_valid(tween: Tween) -> void:
	if tween.is_valid():
		tween.kill()
		
