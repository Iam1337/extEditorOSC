/* Copyright (c) 2019 ExT (V.Sigalkin) */

#if EXTOSC

using UnityEngine;

using UnityEditor;

using System;
using System.IO;
using System.Collections.Generic;

using extEditorOSC.Components;

namespace extEditorOSC
{
	[InitializeOnLoad]
	public static class OSCEditorManager
	{
		#region Public Vars

		public static OSCEditorReceiver[] Receivers
		{
			get { return _receivers.ToArray(); }
		}

		public static OSCEditorTransmitter[] Transmitters
		{
			get { return _transmitters.ToArray(); }
		}

		public static OSCEditorComponent[] Components
		{
			get { return _components.ToArray(); }
		}

		#endregion

		#region Private Methods

		private static List<OSCEditorReceiver> _receivers = new List<OSCEditorReceiver>();

		private static List<OSCEditorTransmitter> _transmitters = new List<OSCEditorTransmitter>();

		private static List<OSCEditorComponent> _components = new List<OSCEditorComponent>();

		private static readonly string _configsPath = "./extOSC/editor.configs.json";

		#endregion

		#region Public Methods

		static OSCEditorManager()
		{
			var directory = Path.GetDirectoryName(_configsPath);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			if (!File.Exists(_configsPath))
			{
				var configs = new OSCEditorConfigs();

				var transmitterConfig = new OSCEditorTransmitterConfig();
				transmitterConfig.RemoteHost = "127.0.0.1";
				transmitterConfig.RemotePort = 7100;
				transmitterConfig.UseBundle = false;
				transmitterConfig.AutoConnect = true;
			    transmitterConfig.LocalHostMode = OSCEditorLocalHostMode.Any;
			    transmitterConfig.LocalHost = "0.0.0.0";
                transmitterConfig.LocalPortMode = OSCEditorLocalPortMode.FromRemotePort;
				transmitterConfig.LocalPort = 7100;
			    
                var receiverConfig = new OSCEditorReceiverConfig();
			    receiverConfig.LocalHostMode = OSCEditorLocalHostMode.Any;
			    receiverConfig.LocalHost = "0.0.0.0";
                receiverConfig.LocalPort = 7100;
				receiverConfig.AutoConnect = true;

				configs.Transmitters.Add(transmitterConfig);
				configs.Receivers.Add(receiverConfig);

				File.WriteAllText(_configsPath, JsonUtility.ToJson(configs));
			}

			var componentsTypes = OSCEditorUtils.GetTypes(typeof(OSCEditorComponent));
			foreach (var componentType in componentsTypes)
			{
				if (componentType.IsAbstract) continue;

				_components.Add((OSCEditorComponent) Activator.CreateInstance(componentType));
			}

			LoadSettings();
		}

		public static void SaveSettigs()
		{
			var configs = new OSCEditorConfigs();

			foreach (var receiver in _receivers)
			{
				var receiverConfig = new OSCEditorReceiverConfig();
			    receiverConfig.LocalHostMode = receiver.LocalHostMode;
			    receiverConfig.LocalHost = receiver.LocalHost;
				receiverConfig.LocalPort = receiver.LocalPort;
				receiverConfig.AutoConnect = receiver.IsAvailable;

				configs.Receivers.Add(receiverConfig);
			}

			foreach (var transmitter in _transmitters)
			{
				var transmitterConfig = new OSCEditorTransmitterConfig();
				transmitterConfig.RemoteHost = transmitter.RemoteHost;
				transmitterConfig.RemotePort = transmitter.RemotePort;
				transmitterConfig.UseBundle = transmitter.UseBundle;
				transmitterConfig.AutoConnect = transmitter.IsAvailable;
			    transmitterConfig.LocalHostMode = transmitter.LocalHostMode;
			    transmitterConfig.LocalHost = transmitter.LocalHost;
				transmitterConfig.LocalPortMode = transmitter.LocalPortMode;
				transmitterConfig.LocalPort = transmitter.LocalPort;

				configs.Transmitters.Add(transmitterConfig);
			}

			foreach (var component in _components)
			{
				var componentType = component.GetType();

				var componentConfig = new OSCEditorComponentConfig();
				componentConfig.Type = componentType.FullName;
				componentConfig.Active = component.Active;

				var receiverComponent = component as OSCEditorReceiverComponent;
				if (receiverComponent != null && receiverComponent.Receiver != null)
				{
					componentConfig.Index = _receivers.IndexOf(receiverComponent.Receiver);
				}
				else
				{
					var transmitterComponent = component as OSCEditorTransmitterComponent;
					if (transmitterComponent != null && transmitterComponent.Transmitter != null)
					{
						componentConfig.Index = _transmitters.IndexOf(transmitterComponent.Transmitter);
					}
				}

				configs.Components.Add(componentConfig);
			}

			if (File.Exists(_configsPath))
				File.Delete(_configsPath);

			File.WriteAllText(_configsPath, JsonUtility.ToJson(configs, true));
		}

		public static void LoadSettings()
		{
			if (!File.Exists(_configsPath))
				return;

			var configs = JsonUtility.FromJson<OSCEditorConfigs>(File.ReadAllText(_configsPath));

			_receivers.ForEach(receiver =>
			{
				receiver.Close();
				receiver.Dispose();
			});
			_receivers.Clear();

			_transmitters.ForEach(transmitter =>
			{
				transmitter.Close();
				transmitter.Dispose();
			});
			_transmitters.Clear();

			foreach (var receiverConfig in configs.Receivers)
			{
				var receiver = new OSCEditorReceiver();
			    receiver.LocalHostMode = receiverConfig.LocalHostMode;
			    receiver.LocalHost = receiverConfig.LocalHost;
                receiver.LocalPort = receiverConfig.LocalPort;

				if (receiverConfig.AutoConnect)
					receiver.Connect();

				_receivers.Add(receiver);
			}

			foreach (var transmitterConfig in configs.Transmitters)
			{
				var transmitter = new OSCEditorTransmitter();
				transmitter.RemoteHost = transmitterConfig.RemoteHost;
				transmitter.RemotePort = transmitterConfig.RemotePort;
				transmitter.UseBundle = transmitterConfig.UseBundle;
			    transmitter.LocalHostMode = transmitterConfig.LocalHostMode;
			    transmitter.LocalHost = transmitterConfig.LocalHost;
				transmitter.LocalPortMode = transmitterConfig.LocalPortMode;
				transmitter.LocalPort = transmitterConfig.LocalPort;

				if (transmitterConfig.AutoConnect)
					transmitter.Connect();

				_transmitters.Add(transmitter);
			}

		    foreach (var componentConfig in configs.Components)
		    {
                var componentType = typeof(OSCEditorManager).Assembly.GetType(componentConfig.Type);
		        var component = GetComponent(componentType);
		        if (component == null) continue;
                
		        var receiverComponent = component as OSCEditorReceiverComponent;
		        if (receiverComponent != null)
		        {
		            receiverComponent.Receiver = GetEditorReceiver(componentConfig.Index);
		        }

		        var transmitterComponent = component as OSCEditorTransmitterComponent;
		        if (transmitterComponent != null)
		        {
		            transmitterComponent.Transmitter = GetEditorTransmitter(componentConfig.Index);
		        }

		        component.Active = componentConfig.Active;
		    }
		}

		public static T GetComponent<T>() where T : OSCEditorComponent
		{
			return (T) GetComponent(typeof(T));
		}

		public static OSCEditorComponent GetComponent(Type type)
		{
			if (_components == null || _components.Count == 0)
				return null;

			foreach (var component in _components)
			{
				if (component.GetType() == type)
					return component;
			}

			return null;
		}

		public static OSCEditorTransmitter CreateEditorTransmitter()
		{
			var transmitter = new OSCEditorTransmitter();
			transmitter.RemotePort = 7100 + _transmitters.Count;

			_transmitters.Add(transmitter);

			return transmitter;
		}

		public static void RemoveEditorTransmitter(OSCEditorTransmitter transmitter)
		{
			if (!_transmitters.Contains(transmitter))
				return;

			transmitter.Close();
			transmitter.Dispose();

			_transmitters.Remove(transmitter);
		}

		public static OSCEditorTransmitter GetEditorTransmitter(int index)
		{
			if (index >= _transmitters.Count ||
			    index < 0) return null;

			return _transmitters[index];
		}

		public static OSCEditorReceiver CreateEditorReceiver()
		{
			var receiver = new OSCEditorReceiver();
			receiver.LocalPort = 7100 + _receivers.Count;

			_receivers.Add(receiver);

			return receiver;
		}

		public static void RemoveEditorReceiver(OSCEditorReceiver receiver)
		{
			if (!_receivers.Contains(receiver))
				return;

			receiver.Close();
			receiver.Dispose();

			_receivers.Remove(receiver);
		}

		public static OSCEditorReceiver GetEditorReceiver(int index)
		{
			if (index >= _receivers.Count ||
			    index < 0) return null;

			return _receivers[index];
		}

		#endregion
	}
}

#endif