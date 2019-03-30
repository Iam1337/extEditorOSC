/* Copyright (c) 2019 ExT (V.Sigalkin) */

#if EXTOSC

using UnityEngine;

using UnityEditor;

using System.Net;

using extOSC;
using extOSC.Editor;
using extOSC.Editor.Panels;
using extOSC.Editor.Windows;

using extEditorOSC.Core;


namespace extEditorOSC.Panels
{
	public class OSCPanelManager : OSCPanel
	{
		#region Static Private Vars

		private static readonly GUIContent _localPortContent = new GUIContent("Local Port:");

		private static readonly GUIContent _localHostContent = new GUIContent("Local Host:");

		private static readonly GUIContent _remoteHostContent = new GUIContent("Remote Host:");

		private static readonly GUIContent _remotePortContent = new GUIContent("Remote Port:");

		private static readonly GUIContent _advancedContent = new GUIContent("Advanced Settings:");

		private static readonly GUIContent _localPortModeContent = new GUIContent("Local Port Mode:");

	    private static readonly GUIContent _localHostModeContent = new GUIContent("Local Host Mode:");

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
					if (!receiver.IsAvailable && connect) receiver.Connect();
					if (receiver.IsAvailable && disconnect) receiver.Close();
				}

				foreach (var transmitter in OSCEditorManager.Transmitters)
				{
					if (!transmitter.IsAvailable && connect) transmitter.Connect();
					if (transmitter.IsAvailable && disconnect) transmitter.Close();
				}
			}


			//var wide = contentRect.width > 600;

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
			GUILayout.Space(25f);
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
	        var baseColor = (editorBase.IsAvailable ? Color.green : Color.yellow) + new Color(0.75f, 0.75f, 0.75f);
	        remove = false;

	        GUI.color = baseColor;
	        GUILayout.BeginVertical("box");

	        GUI.color = editorBase.IsAvailable ? Color.green : Color.yellow;
	        GUILayout.BeginVertical("box");
	        GUILayout.Label(OSCEditorUtils.GetName(editorBase), OSCEditorStyles.CenterBoldLabel);
	        GUILayout.EndVertical();
	        GUI.color = baseColor;

	        var receiver = editorBase as OSCEditorReceiver;
	        if (receiver != null)
	        {
	            DrawReceiver(receiver, out remove);
	        }

	        var transmitter = editorBase as OSCEditorTransmitter;
	        if (transmitter != null)
	        {
	            DrawTransmitter(transmitter, out remove);
	        }

            GUILayout.EndVertical();
	        GUI.color = _defaultColor;
	    }

	    private void DrawReceiver(OSCEditorReceiver receiver, out bool remove)
		{
			var defaultColor = GUI.color;

			// SETTINGS BLOCK
			GUILayout.BeginVertical("box");

			EditorGUILayout.LabelField("Receiver Settings:", EditorStyles.boldLabel);

			// SETTINGS BOX
			GUILayout.BeginVertical("box");

		    if (receiver.LocalHostMode == OSCEditorLocalHostMode.Any)
		    {
		        GUILayout.BeginHorizontal();
		        EditorGUILayout.LabelField(_localHostContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
		        EditorGUILayout.SelectableLabel(_localHost, GUILayout.Height(EditorGUIUtility.singleLineHeight));
		        GUILayout.EndHorizontal();
		    }
		    else
		    {
		        receiver.LocalHost = EditorGUILayout.TextField(_localHostContent, receiver.LocalHost);
            }

		    // LOCAL PORT
			GUI.color = _defaultColor;
			receiver.LocalPort = EditorGUILayout.IntField(_localPortContent, receiver.LocalPort);
			GUI.color = defaultColor;

			// SETTINGS BOX END
			EditorGUILayout.EndVertical();

            // SETTINGS BLOCK END
			EditorGUILayout.EndVertical();

		    // ADVANCED BLOCK
		    EditorGUILayout.LabelField(_advancedContent, EditorStyles.boldLabel);
		    EditorGUILayout.BeginHorizontal("box");

		    receiver.LocalHostMode = (OSCEditorLocalHostMode) EditorGUILayout.EnumPopup(_localHostModeContent, receiver.LocalHostMode);

            // ADVANCED BLOCK END
            EditorGUILayout.EndHorizontal();

            // CONTROLS
            EditorGUILayout.LabelField(_controlsContent, EditorStyles.boldLabel);

			DrawControlls(receiver, out remove);

			// EDITOR BUTTONS
			GUI.color = defaultColor;
		}

		private void DrawTransmitter(OSCEditorTransmitter transmitter, out bool remove)
		{
			var defaultColor = GUI.color;
			GUILayout.BeginVertical("box");

			// SETTINGS BLOCK
			GUILayout.BeginVertical("box");

			EditorGUILayout.LabelField("Transmitter Settings:", EditorStyles.boldLabel);

			// SETTINGS BOX
			GUILayout.BeginVertical("box");
			EditorGUI.BeginChangeCheck();

			IPAddress tempAddress;

			var remoteFieldColor =
				IPAddress.TryParse(transmitter.RemoteHost, out tempAddress) ? _defaultColor : Color.red;

			// REMOTE HOST
			GUI.color = remoteFieldColor;
			transmitter.RemoteHost = EditorGUILayout.TextField(_remoteHostContent, transmitter.RemoteHost);
			GUI.color = defaultColor;

			// REMOTE PORT
			GUI.color = _defaultColor;
			transmitter.RemotePort = EditorGUILayout.IntField(_remotePortContent, transmitter.RemotePort);
			GUI.color = defaultColor;

			// MAP BUNDLE
			//EditorGUILayout.PropertyField(_mapBundleProperty, _mapBundleContent);

			// USE BUNDLE
			GUI.color = transmitter.UseBundle ? Color.green : Color.red;
			if (GUILayout.Button("Use Bundle"))
			{
				transmitter.UseBundle = !transmitter.UseBundle;
			}

			GUI.color = defaultColor;

			// SETTINGS BOX END
			EditorGUILayout.EndVertical();

			// ADVANCED SETTIGS BOX
			EditorGUILayout.LabelField(_advancedContent, EditorStyles.boldLabel);
			GUILayout.BeginVertical("box");
			EditorGUI.BeginChangeCheck();

			
			GUI.color = _defaultColor;

		    // LOCAL HOST MODE
            transmitter.LocalHostMode = (OSCEditorLocalHostMode)EditorGUILayout.EnumPopup(_localHostModeContent, transmitter.LocalHostMode);

		    if (transmitter.LocalHostMode == OSCEditorLocalHostMode.Any)
		    {
		        GUILayout.BeginHorizontal();
		        EditorGUILayout.LabelField(_localHostContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
		        EditorGUILayout.SelectableLabel(_localHost, GUILayout.Height(EditorGUIUtility.singleLineHeight));
		        GUILayout.EndHorizontal();
		    }
		    else
		    {
		        transmitter.LocalHost = EditorGUILayout.TextField(_localHostContent, transmitter.LocalHost);
		    }
            
            // LOCAL PORT MODE
            transmitter.LocalPortMode =
				(OSCEditorLocalPortMode) EditorGUILayout.EnumPopup(_localPortModeContent, transmitter.LocalPortMode);
			GUI.color = defaultColor;

			// LOCAL PORT
			if (transmitter.LocalPortMode == OSCEditorLocalPortMode.FromRemotePort)
			{
				// LOCAL FROM REMOTE PORT
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(_localPortContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
				EditorGUILayout.SelectableLabel(transmitter.RemotePort.ToString(),
				                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
				GUILayout.EndHorizontal();
			}
			else if (transmitter.LocalPortMode == OSCEditorLocalPortMode.Custom)
			{
				GUI.color = _defaultColor;
				// LOCAL PORT
				transmitter.LocalPort = EditorGUILayout.IntField(_localPortContent, transmitter.LocalPort);
				GUI.color = defaultColor;
			}

			// ADVANCED SETTIGS BOX END
			EditorGUILayout.EndVertical();

			// SETTINGS BLOCK END
			EditorGUILayout.EndVertical();

			// CONTROLS
			EditorGUILayout.LabelField(_controlsContent, EditorStyles.boldLabel);

			DrawControlls(transmitter, out remove);

			// EDITOR BUTTONS
			GUI.color = defaultColor;

			GUILayout.EndVertical();
		}

		private void DrawControlls(OSCEditorBase editorBase, out bool remove)
		{
			EditorGUILayout.BeginVertical("box");

			GUI.color = editorBase.IsAvailable ? Color.green : Color.red;
			var connection = GUILayout.Button(editorBase.IsAvailable ? "Connected" : "Disconnected");

			EditorGUILayout.BeginHorizontal();

			GUI.color = Color.yellow;
			EditorGUI.BeginDisabledGroup(!editorBase.IsAvailable);
			var reconect = GUILayout.Button("Reconnect");
			EditorGUI.EndDisabledGroup();

			GUI.color = Color.red;
			remove = GUILayout.Button("Remove");

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			if (connection)
			{
				if (editorBase.IsAvailable) editorBase.Close();
				else editorBase.Connect();
			}

			if (reconect)
			{
				if (editorBase.IsAvailable) editorBase.Close();

				editorBase.Connect();
			}
		}

		#endregion
	}
}

#endif