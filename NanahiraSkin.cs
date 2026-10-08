using System;
using System.Linq;
using NanahiraSkin.Common;
using Terraria.ModLoader;

namespace NanahiraSkin
{
	public sealed class NanahiraSkin : Mod
	{
		public override void PostSetupContent()
		{
			int layerCount = GetContent<NanahiraSkinDrawLayer>().Count();
			// Fail during loading if a future edit disables layer registration,
			// rather than hiding the vanilla player with nothing to replace it.
			if (layerCount != 5)
				throw new InvalidOperationException($"Nanahira Skin needs 5 draw layers; {layerCount} were registered.");
			Logger.Info($"Registered {layerCount} Nanahira draw layers, including the dedicated map icon.");
		}
	}
}
