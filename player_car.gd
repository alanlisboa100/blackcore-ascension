extends CharacterBody2D

@export var max_speed: float = 450.0
@export var acceleration: float = 350.0
@export var friction: float = 250.0
@export var steering_speed: float = 3.5

var current_speed: float = 0.0
var steer_input: float = 0.0

func _physics_process(delta: float) -> void:
	# Input handling (keyboard / touch simulation)
	var throttle = 0.0
	if Input.is_action_pressed("ui_up") or Input.is_action_pressed("ui_accept"):
		throttle = 1.0
	elif Input.is_action_pressed("ui_down"):
		throttle = -0.5
	else:
		throttle = 0.0

	steer_input = 0.0
	if Input.is_action_pressed("ui_left"):
		steer_input = -1.0
	if Input.is_action_pressed("ui_right"):
		steer_input = 1.0

	# Speed logic
	if throttle != 0:
		current_speed = move_toward(current_speed, max_speed * throttle, acceleration * delta)
	else:
		current_speed = move_toward(current_speed, 0.0, friction * delta)

	# Steering
	if abs(current_speed) > 5.0:
		rotation += steer_input * steering_speed * delta * (current_speed / max_speed)

	# Movement
	var direction = Vector2.UP.rotated(rotation)
	velocity = direction * current_speed
	move_and_slide()
