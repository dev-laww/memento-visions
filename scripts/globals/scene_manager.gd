extends Node

const LEVEL_H: int = 360
const LEVEL_W: int = 640

signal load_start(loading_screen)
signal scene_added(loaded_scene: Node, loading_screen)
signal load_complete(loaded_scene: Node)
signal _content_finished_loading(content)
signal _content_invalid(content_path: String)
signal _content_failed_to_load(content_path: String)

var _loading_screen_scene: PackedScene = preload('res://scenes/screens/loading_screen.tscn')
var _loading_screen: LoadingScreen
var _transition: String
var _zelda_transition_direction: Vector2
var _content_path: String
var _load_progress_timer: Timer
var _load_scene_into: Node
var _scene_to_unload: Node
var _loading_in_progress: bool = false

enum Transition {
	FADE,
	WIPE,
	NONE
}

var scene_transition_dict: Dictionary = {
	Transition.FADE: 'fade_to_black',
	Transition.WIPE: 'wipe_to_right',
	Transition.NONE: 'no_to_transition'
}


func transition(to: String, from: Node = get_tree().current_scene, transition_type: Transition = Transition.FADE, load_into: Node = get_tree().root) -> void:
	if _loading_in_progress:
		push_warning('SceneManager is already loading something')
		return

	_loading_in_progress = true
	_load_scene_into = load_into
	_scene_to_unload = from

	_transition = scene_transition_dict.get(transition_type)
	_loading_screen = _loading_screen_scene.instantiate() as LoadingScreen

	get_tree().root.add_child(_loading_screen)
	_loading_screen.start_transition(_transition)

	_load_content(to)


func transition_zelda(to: String, from: Node, move_dir: Vector2, load_into: Node = get_tree().root) -> void:
	if _loading_in_progress:
		push_warning('SceneManager is already loading something')
		return

	_transition = 'zelda'
	_load_scene_into = load_into
	_scene_to_unload = from
	_zelda_transition_direction = move_dir
	_load_content(to)


func _ready() -> void:
	_content_invalid.connect(_on_content_invalid)
	_content_failed_to_load.connect(_on_content_failed_to_load)
	_content_finished_loading.connect(_on_content_finished_loading)


func _load_content(content_path: String) -> void:
	load_start.emit(_loading_screen)

	if _transition != 'zelda':
		await _loading_screen.transition_in_complete

	_content_path = content_path
	var loader: int = ResourceLoader.load_threaded_request(content_path)
	if not ResourceLoader.exists(content_path) or loader == null:
		_content_invalid.emit(content_path)
		return

	_load_progress_timer = Timer.new()
	_load_progress_timer.wait_time = 0.1
	_load_progress_timer.timeout.connect(_monitor_load_status)

	get_tree().root.add_child(_load_progress_timer)
	_load_progress_timer.start()


func _monitor_load_status() -> void:
	var load_progress: Array[Variant] = []
	var load_status: int = ResourceLoader.load_threaded_get_status(_content_path, load_progress)

	match load_status:
		ResourceLoader.THREAD_LOAD_INVALID_RESOURCE:
			_content_invalid.emit(_content_path)
			_load_progress_timer.stop()
			return
		ResourceLoader.THREAD_LOAD_IN_PROGRESS:
			if _loading_screen != null:
				_loading_screen.update_bar(load_progress[0] * 100)
		ResourceLoader.THREAD_LOAD_FAILED:
			_content_failed_to_load.emit(_content_path)
			_load_progress_timer.stop()
			return
		ResourceLoader.THREAD_LOAD_LOADED:
			_load_progress_timer.stop()
			_load_progress_timer.queue_free()
			_content_finished_loading.emit(ResourceLoader.load_threaded_get(_content_path).instantiate())
			return


func _on_content_failed_to_load(path: String) -> void:
	printerr('error: Failed to load resource: \'%s\'' % [path])


func _on_content_invalid(path: String) -> void:
	printerr('error: Cannot load resource: \'%s\'' % [path])


func _on_content_finished_loading(incoming_scene) -> void:
	var outgoing_scene: Node = _scene_to_unload

	if outgoing_scene != null:
		if outgoing_scene.has_method('get_data') and incoming_scene.has_method('receive_data'):
			incoming_scene.receive_data(outgoing_scene.get_data())

	_load_scene_into.add_child(incoming_scene)
	scene_added.emit(incoming_scene, _loading_screen)

	if _transition == 'zelda':
		incoming_scene.position.x = _zelda_transition_direction.x * LEVEL_W
		incoming_scene.position.y = _zelda_transition_direction.y * LEVEL_H
		var tween_in: Tween = get_tree().create_tween()
		tween_in.tween_property(incoming_scene, 'position', Vector2.ZERO, 1).set_trans(Tween.TRANS_SINE)

		var tween_out: Tween = get_tree().create_tween()
		var vector_off_screen: Vector2 = Vector2.ZERO
		vector_off_screen.x = -_zelda_transition_direction.x * LEVEL_W
		vector_off_screen.y = -_zelda_transition_direction.y * LEVEL_H
		tween_out.tween_property(outgoing_scene, 'position', vector_off_screen, 1).set_trans(Tween.TRANS_SINE)
		await tween_in.finished

	if _scene_to_unload != null and _scene_to_unload != get_tree().root:
		_scene_to_unload.queue_free()

	if incoming_scene.has_method('init_scene'):
		incoming_scene.init_scene()

	if _loading_screen != null:
		_loading_screen.finish_transition()

		await _loading_screen.anim_player.animation_finished

	if incoming_scene.has_method('start_scene'):
		incoming_scene.start_scene()

	_loading_in_progress = false
	load_complete.emit(incoming_scene)
