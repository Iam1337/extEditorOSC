/* Copyright (c) 2019 ExT (V.Sigalkin) */

#if EXTOSC

using UnityEngine;

using System.Collections.Generic;

using extOSC;
using extOSC.Core;

using extEditorOSC.Components;

namespace extEditorOSC.Examples
{
	[OSCEditorComponent("Examples", "Example Receiver Component")]
	public class OSCEditorReceiverComponentExample : OSCEditorReceiverComponent
	{
		#region Protected Methods

		protected override void PopulateBinds(List<IOSCBind> binds)
		{
			binds.Add(new OSCBind("/editor/example", MessageReceive));
		}

		#endregion

		#region Private Methods

		private void MessageReceive(OSCMessage message)
		{
			Debug.LogFormat("Received message: {0}", message);
		}

		#endregion
	}
}

#endif