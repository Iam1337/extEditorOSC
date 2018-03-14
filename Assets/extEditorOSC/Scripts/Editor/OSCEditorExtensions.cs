﻿/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEditor;

using System;
using System.Collections.Generic;
using extOSC;

namespace extEditorOSC
{
	public static class OSCEditorExtensions
	{
		#region Static Public Methods

		public static Type[] GetTypes(Type type)
		{
			var types = new List<Type>();
			var guids = AssetDatabase.FindAssets("t:" + typeof(MonoScript).Name);

			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);

				var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
				if (monoScript == null) continue;

				var componentType = monoScript.GetClass();
				if (componentType == null || !OSCUtilities.IsSubclassOf(componentType, type)) continue;

				types.Add(componentType);
			}

			return types.ToArray();
		}

		#endregion
	}
}