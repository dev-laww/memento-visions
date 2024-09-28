extends Node

var current_scene: Node:
	get:
		return get_tree().current_scene


func change(scene_path: String) -> void:
	get_tree().change_scene_to_packed(load(scene_path))


func transition_to(scene_path: String) -> void:
	# TODO: Add transition effect
	change(scene_path)


func transitsion_with_loading(scene_path: String) -> void:
	# TODO: Add loading screen
	change(scene_path)