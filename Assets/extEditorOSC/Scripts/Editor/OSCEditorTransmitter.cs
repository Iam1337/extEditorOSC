/* Copyright (c) 2019 ExT (V.Sigalkin) */

#if EXTOSC

using UnityEngine;

using System;
using System.Collections.Generic;

using extOSC;
using extOSC.Core;
using extOSC.Core.Network;

using extEditorOSC.Core;

namespace extEditorOSC
{
    [Serializable]
	public class OSCEditorTransmitter : OSCEditorBase
	{
		#region Public Vars

	    public override bool IsAvailable
	    {
	        get { return transmitterBackend.IsAvailable; }
	    }

	    public OSCEditorLocalHostMode LocalHostMode
	    {
	        get { return localHostMode; }
	        set
	        {
	            if (localHostMode == value)
	                return;

	            localHostMode = value;

	            if (IsAvailable)
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

	            if (IsAvailable && localHostMode == OSCEditorLocalHostMode.Custom)
	            {
	                Close();
	                Connect();
	            }
	        }
	    }

	    public OSCEditorLocalPortMode LocalPortMode
		{
			get { return localPortMode; }
			set
			{
				if (localPortMode == value)
					return;

				localPortMode = value;

				if (IsAvailable)
				{
					Close();
					Connect();
				}
			}
		}

		public int LocalPort
		{
			get { return RequestLocalPort(); }
			set
			{
				if (localPort == value)
					return;

				localPort = value;

				if (IsAvailable && localPortMode == OSCEditorLocalPortMode.Custom)
				{
					Close();
					Connect();
				}
			}
		}

		public string RemoteHost
		{
			get { return remoteHost; }
			set
			{
				if (remoteHost == value)
					return;

				remoteHost = value;

				transmitterBackend.RefreshRemote(remoteHost, remotePort);

				if (IsAvailable && localPortMode == OSCEditorLocalPortMode.FromRemotePort)
				{
					Close();
					Connect();
				}
			}
		}

		public int RemotePort
		{
			get { return remotePort; }
			set
			{
				value = OSCUtilities.ClampPort(value);

				if (remotePort == value)
					return;

				remotePort = value;

				transmitterBackend.RefreshRemote(remoteHost, remotePort);
			}
		}

		public bool UseBundle
		{
			get { return useBundle; }
			set { useBundle = value; }
		}

        #endregion

        #region Protected Vars

	    [SerializeField]
	    protected OSCEditorLocalHostMode localHostMode = OSCEditorLocalHostMode.Any;

	    [SerializeField]
	    protected string localHost;

        [SerializeField]
		protected OSCEditorLocalPortMode localPortMode = OSCEditorLocalPortMode.FromRemotePort;

        [SerializeField]
		protected int localPort = 0;

		[SerializeField]
		protected string remoteHost = "127.0.0.1";

		[SerializeField]
		protected int remotePort = 7100;

		[SerializeField]
		protected bool useBundle;

		protected OSCTransmitterBackend transmitterBackend
		{
			get
			{
				if (_transmitterBackend == null)
					_transmitterBackend = OSCTransmitterBackend.Create();

				return _transmitterBackend;
			}
		}

		#endregion

		#region Private Vars

		private readonly List<OSCPacket> _packetPool = new List<OSCPacket>();

		private OSCTransmitterBackend _transmitterBackend;

		#endregion

		#region Public Methods

		public override void Connect()
		{
            transmitterBackend.Connect(RequestLocalHost(), RequestLocalPort());
            transmitterBackend.RefreshRemote(remoteHost, remotePort);
		}

		public override void Close()
		{
            if (transmitterBackend.IsAvailable)
			    transmitterBackend.Close();
		}

		public override string ToString()
		{
		    return string.Format("<{0} (LocalHost: {1} LocalPort: {2} | RemoteHost: {3}, RemotePort: {4})>",
		        GetType().Name, localHost, localPort, remoteHost, remotePort);
        }

		public void Send(OSCPacket packet)
		{
			if (!transmitterBackend.IsAvailable) return;

			if (useBundle && packet != null && (packet is OSCMessage))
			{
				_packetPool.Add(packet);

				return;
			}

			var length = 0;
			var data = OSCConverter.Pack(packet, out length);

			transmitterBackend.Send(data, length);

			OSCEditorConsole.Transmitted(this, packet);
		}

		public virtual void Send(string address, OSCValue value)
		{
			var message = new OSCMessage(address);
			message.AddValue(value);

			Send(message);
		}

		#endregion

		#region Protected Methods

		protected override void Update()
		{
			if (_packetPool.Count > 0)
			{
				var bundle = new OSCBundle();

				foreach (var packet in _packetPool)
				{
					bundle.AddPacket(packet);
				}

				Send(bundle);

				_packetPool.Clear();
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

	    private int RequestLocalPort()
	    {
	        if (localPortMode == OSCEditorLocalPortMode.Random)
	            return 0;

	        if (localPortMode == OSCEditorLocalPortMode.Custom)
	            return localPort;

	        return remotePort;
	    }

        #endregion
    }
}

#endif