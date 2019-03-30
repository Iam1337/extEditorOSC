/* Copyright (c) 2019 ExT (V.Sigalkin) */

#if EXTOSC

using UnityEngine;
using UnityEngine.Events;

using System;
using System.Collections.Generic;

using extOSC;
using extOSC.Core;
using extOSC.Core.Network;

using extEditorOSC.Core;


namespace extEditorOSC
{
    [Serializable]
	public class OSCEditorReceiver : OSCEditorBase
	{
        #region Public Vars

	    public OSCEditorLocalHostMode LocalHostMode
	    {
	        get { return localHostMode; }
	        set
	        {
	            if (localHostMode == value)
	                return;

	            localHostMode = value;

	            if (receiverBackend.IsRunning && IsAvailable)
	            {
	                Close();
	                Connect();
	            }
	        }
	    }

	    public string LocalHost
	    {
	        get { return RequestLocalHost(); }
	        set
	        {
	            if (localHost == value)
	                return;

	            localHost = value;

	            if (receiverBackend.IsRunning && IsAvailable)
	            {
	                Close();
	                Connect();
	            }
	        }
	    }

	    public int LocalPort
		{
			get { return localPort; }
			set
			{
				value = OSCUtilities.ClampPort(value);

				if (localPort == value)
					return;

				localPort = value;

				if (receiverBackend.IsRunning && IsAvailable)
				{
					Close();
					Connect();
				}
			}
		}

		public override bool IsAvailable
		{
			get { return receiverBackend.IsAvailable; }
		}

		public bool IsRunning
		{
			get { return receiverBackend.IsRunning; }
		}

        #endregion

        #region Protected Vars

	    [SerializeField]
        protected OSCEditorLocalHostMode localHostMode = OSCEditorLocalHostMode.Any;

	    [SerializeField]
        protected string localHost;

        [SerializeField]
        protected int localPort = 7100;

		protected Queue<OSCPacket> packets = new Queue<OSCPacket>();

		protected List<IOSCBind> bindings = new List<IOSCBind>();

		protected OSCReceiverBackend receiverBackend
		{
			get
			{
				if (_receiverBackend == null)
				{
					_receiverBackend = OSCReceiverBackend.Create();
					_receiverBackend.ReceivedCallback = PacketReceived;
				}

				return _receiverBackend;
			}
		}

		#endregion

		#region Private Vars

		private readonly object _lock = new object();

		private OSCReceiverBackend _receiverBackend;

		#endregion

		#region Public Methods

		public override string ToString()
		{
			return string.Format("<{0} (Port: {1})>", GetType(), localPort);
		}

		public override void Connect()
		{
			receiverBackend.Connect(RequestLocalHost(), localPort);
		}

	    public override void Close()
	    {
	        if (receiverBackend.IsAvailable)
	            receiverBackend.Close();
	    }

	    public void Bind(IOSCBind bind)
		{
			if (bind == null) return;

			if (string.IsNullOrEmpty(bind.ReceiverAddress))
			{
				Debug.Log("[OSCEditorReceiver] Address can not be empty!");
				return;
			}

			if (!bindings.Contains(bind))
				bindings.Add(bind);
		}

		public OSCBind Bind(string address, UnityAction<OSCMessage> callback)
		{
			var bind = new OSCBind(address, callback);

			Bind(bind);

			return bind;
		}

		public void Unbind(IOSCBind bind)
		{
			if (bind == null) return;

			if (bindings.Contains(bind))
				bindings.Remove(bind);
		}

		public void UnbindAll()
		{
			bindings.Clear();
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			UnbindAll();
		}

		protected override void Update()
		{
			if (!IsAvailable || !IsRunning) return;

			lock (_lock)
			{
				while (packets.Count > 0)
				{
					var packet = packets.Dequeue();

					//TODO: Add bundles.
					//if (mapBundle != null)
					//	mapBundle.Map(packet);

					OSCEditorConsole.Received(this, packet);

					InvokePacket(packet);
				}
			}
		}

        #endregion

        #region Private Methods

	    private string RequestLocalHost()
	    {
	        if (localHostMode == OSCEditorLocalHostMode.Any)
	            return "0.0.0.0";

	        return localHost;
	    }

	    private void InvokePacket(OSCPacket packet)
		{
			if (packet.IsBundle())
			{
				InvokeBundle(packet as OSCBundle);
			}
			else
			{
				InvokeMessage(packet as OSCMessage);
			}
		}

		private void InvokeBundle(OSCBundle bundle)
		{
			if (bundle == null) return;

			foreach (var packet in bundle.Packets)
			{
				InvokePacket(packet);
			}
		}

		private void InvokeMessage(OSCMessage message)
		{
			if (message == null) return;

			foreach (var bind in bindings)
			{
				if (bind == null) continue;

				if (OSCUtilities.CompareAddresses(bind.ReceiverAddress, message.Address))
				{
					if (bind.Callback != null)
						bind.Callback.Invoke(message);
				}
			}
		}

		private void PacketReceived(OSCPacket packet)
		{
			lock (_lock)
			{
				packets.Enqueue(packet);
			}
		}

		#endregion
	}
}

#endif