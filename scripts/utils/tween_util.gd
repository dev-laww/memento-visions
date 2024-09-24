class_name TweenUtil

static func kill_if_valid(tween: Tween) -> void:
	if tween.is_valid():
		tween.kill()