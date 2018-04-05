﻿/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System.Net;

using extOSC;
using extOSC.Editor.Panels;
using extOSC.Editor.Windows;

using extEditorOSC.Core;
using extOSC.Editor;


namespace extEditorOSC.Panels
{
	public class OSCPanelManager : OSCPanel
	{
		#region Static Private Vars

		private static readonly GUIContent _localPortContent = new GUIContent("Local Port:");

		private static readonly GUIContent _localHostContent = new GUIContent("Local Host:");

		private static readonly GUIContent _remoteHostContent = new GUIContent("Remote Host:");

		private static readonly GUIContent _remotePortContent = new GUIContent("Remote Port:");

		//private static readonly GUIContent _mapBundleContent = new GUIContent("Map Bundle:");

		private static readonly GUIContent _controlsContent = new GUIContent("Controls:");

		#endregion

		#region Private Vars

		private Color _defaultColor;

		private string _localHost;

		private Vector2 _scrollPosition;

		#endregion

		#region Public Methods

		public OSCPanelManager(OSCWindow parentWindow, string panelId) : base(parentWindow, panelId)
		{
			_localHost = OSCUtilities.GetLocalHost();
		}

		#endregion

		#region Protected Methods

		protected override void DrawContent(Rect contentRect)
		{
			_defaultColor = GUI.color;

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			var connect = GUILayout.Button("Connect All", EditorStyles.toolbarButton);
			GUILayout.Space(5);
			var disconnect = GUILayout.Button("Disconnect All", EditorStyles.toolbarButton);
			GUILayout.FlexibleSpace();
			GUILayout.Label("OSC Editor Manager");
			//GUI.color = Color.red;
			//var removeAll = GUILayout.Button("Remove All");
			//GUI.color = _defaultColor;
			EditorGUILayout.EndHorizontal();

			if (connect || disconnect)
			{
				foreach (var receiver in OSCEditorManager.Receivers)
				{
					if (!receiver.IsAvaible && connect) receiver.Connect();
					if (receiver.IsAvaible && disconnect) receiver.Close();
				}

				foreach (var transmitter in OSCEditorManager.Transmitters)
				{
					if (!transmitter.IsAvaible && connect) transmitter.Connect();
					if (transmitter.IsAvaible && disconnect) transmitter.Close();
				}
			}


			//var wide = contentRect.width > 520;

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			GUILayout.BeginHorizontal();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Space(5);

			//if (wide)
			//	GUILayout.BeginHorizontal();
			//else
			GUILayout.BeginVertical();
			
			DrawReceivers(OSCEditorManager.Receivers);
			DrawTransmitters(OSCEditorManager.Transmitters);

			//if (wide)
			//	GUILayout.EndHorizontal();
			//else
			GUILayout.EndVertical();

			GUILayout.Space(5);
			GUILayout.EndVertical();

			GUILayout.Space(5);
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();
		}

		#endregion

		#region Private Methods

		private void DrawReceivers(OSCEditorReceiver[] receivers)
		{
			GUILayout.BeginVertical("box");

			GUI.color = Color.red;
			GUILayout.BeginVertical("box");
			GUILayout.Label("Receivers:", OSCEditorStyles.CenterBoldLabel);
			GUILayout.EndVertical();
			GUI.color = _defaultColor;

			GUILayout.Space(5f);

			if (receivers.Length > 0)
			{
				OSCEditorReceiver removingReceiver = null;

				foreach (var receiver in receivers)
				{
					bool remove;

					DrawBase(receiver, out remove);
					GUILayout.Space(5f);

					if (remove) removingReceiver = receiver;
				}

				if (removingReceiver != null)
					OSCEditorManager.RemoveEditorReceiver(removingReceiver);
			}
			else
			{
				GUILayout.BeginVertical("box");
				GUILayout.Label("- none -", OSCEditorStyles.CenterLabel);
				GUILayout.EndVertical();
			}

			GUILayout.Space(5f);

			GUILayout.BeginVertical("box");
			GUI.color = Color.green;
			var addButton = GUILayout.Button("Add Receiver");
			GUI.color = _defaultColor;
			GUILayout.EndVertical();

			if (addButton)
			{
				OSCEditorManager.CreateEditorReceiver();
			}

			GUILayout.EndVertical();
		}

		private void DrawTransmitters(OSCEditorTransmitter[] transmitters)
		{
			GUILayout.BeginVertical("box");

			GUI.color = Color.red;
			GUILayout.BeginVertical("box");
			GUILayout.Label("Transmitters:", OSCEditorStyles.CenterBoldLabel);
			GUILayout.EndVertical();
			GUI.color = _defaultColor;

			GUILayout.Space(5f);

			if (transmitters.Length > 0)
			{
				OSCEditorTransmitter removingTransmitter = null;

				foreach (var transmitter in transmitters)
				{
					bool remove;

					DrawBase(transmitter, out remove);
					GUILayout.Space(5f);

					if (remove) removingTransmitter = transmitter;
				}

				if (removingTransmitter != null)
				{
					OSCEditorManager.RemoveEditorTransmitter(removingTransmitter);
				}
			}
			else
			{
				GUILayout.BeginVertical("box");
				GUILayout.Label("- none -", OSCEditorStyles.CenterLabel);
				GUILayout.EndVertical();
			}

			GUILayout.Space(5f);

			GUILayout.BeginVertical("box");
			GUI.color = Color.green;
			var addButton = GUILayout.Button("Add Receiver");
			GUI.color = _defaultColor;
			GUILayout.EndVertical();

			if (addButton)
			{
				OSCEditorManager.CreateEditorTransmitter();
			}

			GUILayout.EndVertical();
		}

		private void DrawBase(OSCEditorBase editorBase, out bool remove)
		{
			remove = false;

			GUILayout.BeginVertical("box");

			GUI.color = editorBase.IsAvaible ? Color.green : Color.yellow;
			GUILayout.BeginVertical("box");
			GUILayout.Label(OSCEditorUtils.GetName(editorBase), OSCEditorStyles.CenterBoldLabel);
			GUILayout.EndVertical();
			GUI.color = _defaultColor;

			var receiver = editorBase as OSCEditorReceiver;
			if (receiver != null)
			{
				DrawReceiver(receiver, out remove);
			}
			else
			{
				var transmitter = editorBase as OSCEditorTransmitter;
				if (transmitter != null)
				{
					DrawTransmitter(transmitter, out remove);
				}
			}

			GUILayout.EndVertical();
		}

		private void DrawReceiver(OSCEditorReceiver receiver, out bool remove)
		{
			//if (width > 0)
			//	GUILayout.BeginVertical("box", GUILayout.Width(width));
			//else
			//	GUILayout.BeginVertical("box");

			// SETTINGS BLOCK
			GUILayout.BeginVertical("box");

			EditorGUILayout.LabelField("Receiver Settings:", EditorStyles.boldLabel);

			// SETTINGS BOX
			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(_localHostContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
			EditorGUILayout.SelectableLabel(_localHost, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			GUILayout.EndHorizontal();

			// LOCAL PORT
			receiver.LocalPort = EditorGUILayout.IntField(_localPortContent, receiver.LocalPort);

			// MAP BUNDLE
			//EditorGUILayout.PropertyField(_mapBundleProperty, _mapBundleContent);

			// SETTINGS BOX END
			EditorGUILayout.EndVertical();

			/*
			// PARAMETERS BLOCK
			EditorGUILayout.BeginHorizontal("box");

			GUI.color = OSCEditorManager.AutoConnectReceiver ? Color.green : Color.red;
			if (GUILayout.Button("Auto Connect"))
			{
				OSCEditorManager.AutoConnectReceiver = !OSCEditorManager.AutoConnectReceiver;
			}
			GUI.color = _defaultColor;

			// PARAMETERS BLOCK END
			EditorGUILayout.EndHorizontal();
			*/

			// SETTINGS BLOCK END
			EditorGUILayout.EndVertical();

			// CONTROLS
			EditorGUILayout.LabelField(_controlsContent, EditorStyles.boldLabel);

			DrawControlls(receiver, out remove);

			// EDITOR BUTTONS
			GUI.color = _defaultColor;

			//GUILayout.EndVertical();
		}

		private void DrawTransmitter(OSCEditorTransmitter transmitter, out bool remove)
		{
			GUILayout.BeginVertical("box");

			// SETTINGS BLOCK
			GUILayout.BeginVertical("box");

			EditorGUILayout.LabelField("Transmitter Settings:", EditorStyles.boldLabel);

			// SETTINGS BOX
			GUILayout.BeginVertical("box");
			EditorGUI.BeginChangeCheck();

			IPAddress tempAddress;

			var remoteFieldColor = IPAddress.TryParse(transmitter.RemoteHost, out tempAddress) ? _defaultColor : Color.red;

			// REMOTE HOST
			GUI.color = remoteFieldColor;
			transmitter.RemoteHost = EditorGUILayout.TextField(_remoteHostContent, transmitter.RemoteHost);
			GUI.color = _defaultColor;

			// REMOTE PORT
			transmitter.RemotePort = EditorGUILayout.IntField(_remotePortContent, transmitter.RemotePort);

			// MAP BUNDLE
			//EditorGUILayout.PropertyField(_mapBundleProperty, _mapBundleContent);

			// USE BUNDLE
			GUI.color = transmitter.UseBundle ? Color.green : Color.red;
			if (GUILayout.Button("Use Bundle"))
			{
				transmitter.UseBundle = !transmitter.UseBundle;
			}
			GUI.color = _defaultColor;

			// SETTINGS BOX END
			EditorGUILayout.EndVertical();

			/*
			// PARAMETETS BLOCK
			EditorGUILayout.BeginHorizontal("box");

			GUI.color = OSCEditorManager.AutoConnectTransmitter ? Color.green : Color.red;
			if (GUILayout.Button("Auto Connect"))
			{
				OSCEditorManager.AutoConnectTransmitter = !OSCEditorManager.AutoConnectTransmitter;
			}
			GUI.color = _defaultColor;

			// PARAMETERS BLOCK END
			EditorGUILayout.EndHorizontal();
			*/

			// SETTINGS BLOCK END
			EditorGUILayout.EndVertical();

			// CONTROLS
			EditorGUILayout.LabelField(_controlsContent, EditorStyles.boldLabel);

			DrawControlls(transmitter, out remove);

			// EDITOR BUTTONS
			GUI.color = Color.white;

			GUILayout.EndVertical();
		}

		private void DrawControlls(OSCEditorBase editorBase, out bool remove)
		{
			EditorGUILayout.BeginVertical("box");
			
			GUI.color = editorBase.IsAvaible ? Color.green : Color.red;
			var connection = GUILayout.Button(editorBase.IsAvaible ? "Connected" : "Disconnected");

			EditorGUILayout.BeginHorizontal();

			GUI.color = Color.yellow;
			EditorGUI.BeginDisabledGroup(!editorBase.IsAvaible);
			var reconect = GUILayout.Button("Reconnect");
			EditorGUI.EndDisabledGroup();

			GUI.color = Color.red;
			remove = GUILayout.Button("Remove");

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			if (connection)
			{
				if (editorBase.IsAvaible) editorBase.Close();
				else editorBase.Connect();
			}

			if (reconect)
			{
				if (editorBase.IsAvaible) editorBase.Close();

				editorBase.Connect();
			}
		}

		#endregion
	}
}