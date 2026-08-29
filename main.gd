extends Node2D

@onready var game_manager: Node = $GameManager
@onready var lap_label: Label = $CanvasLayer/Control/LapLabel
@onready var time_label: Label = $CanvasLayer/Control/TimeLabel
@onready var victory_panel: Panel = $CanvasLayer/Control/VictoryPanel
@onready var final_time_label: Label = $CanvasLayer/Control/VictoryPanel/FinalTimeLabel

func _ready() -> void:
	if game_manager:
		game_manager.lap_completed.connect(_on_lap_completed)
		game_manager.race_finished.connect(_on_race_finished)
	if victory_panel:
		victory_panel.visible = false

func _process(_delta: float) -> void:
	if game_manager and not game_manager.game_over:
		lap_label.text = "Volta: " + str(game_manager.current_lap) + " / " + str(game_manager.total_laps)
		var t = game_manager.race_time
		var mins = int(t) / 60
		var secs = fmod(t, 60.0)
		time_label.text = "Tempo: %02d:%05.2f" % [mins, secs]

func _on_lap_completed(lap_num: int, l_time: float) -> void:
	print("BLACKCORE 2D: Lap ", lap_num, " completed in ", l_time, "s")

func _on_race_finished(total_time: float) -> void:
	if victory_panel:
		victory_panel.visible = true
		if final_time_label:
			final_time_label.text = "Tempo Final: %.2f s" % total_time

func _on_finish_line_body_entered(body: Node2D) -> void:
	if body.name == "PlayerCar" and game_manager:
		game_manager.pass_finish_line()
