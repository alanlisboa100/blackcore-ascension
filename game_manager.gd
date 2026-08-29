extends Node

var total_laps: int = 3
var current_lap: int = 1
var race_time: float = 0.0
var lap_time: float = 0.0
var game_over: bool = false
var race_started: bool = true

signal lap_completed(lap_num: int, time: float)
signal race_finished(total_time: float)

func _process(delta: float) -> void:
	if race_started and not game_over:
		race_time += delta
		lap_time += delta

func pass_finish_line() -> void:
	if game_over or lap_time < 3.0:
		return
	
	emit_signal("lap_completed", current_lap, lap_time)
	if current_lap >= total_laps:
		game_over = true
		race_started = false
		emit_signal("race_finished", race_time)
	else:
		current_lap += 1
		lap_time = 0.0
