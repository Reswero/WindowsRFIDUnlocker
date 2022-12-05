﻿using System;
using System.Text.Json.Serialization;

namespace RFIDUnlocker.GUI.Models
{
	internal class ActionInfo
	{
		public ActionInfo() { }

		public ActionInfo(string uid, ActionType type)
		{
			UID = uid;
			Type = type;
		}

		public int Id { get; set; }
		public string UID { get; init; }

		[JsonPropertyName("action")]
		public ActionType Type { get; init; }

		public override string ToString()
		{
			string actionMessage = Type switch
			{
				ActionType.Entered => "Успешный вход",
				ActionType.NotEntered => "Попытка входа",
				_ => "Неизвестное действие",
			};
			return $"{DateTime.Now:dd.MM.yy HH.mm} | [{UID}] {actionMessage}";
		}
	}

	internal enum ActionType
	{
		Entered,
		NotEntered
	}
}
