using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NanahiraSkin.Common
{
	public sealed class NanahiraPlayer : ModPlayer
	{
		public bool NanahiraSkinActive { get; set; }

		public override void ResetEffects()
		{
			NanahiraSkinActive = false;
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (!NanahiraSkinActive)
				return;

			// Hide the vanilla hair, skin, eyes, and clothing so they do not show
			// through transparent pixels in the Nanahira artwork.
			drawInfo.hideHair = true;
			drawInfo.colorArmorBody = Color.Transparent;
			drawInfo.colorArmorHead = Color.Transparent;
			drawInfo.colorArmorLegs = Color.Transparent;
			drawInfo.colorBodySkin = Color.Transparent;
			drawInfo.colorEyes = Color.Transparent;
			drawInfo.colorEyeWhites = Color.Transparent;
			drawInfo.colorHair = Color.Transparent;
			drawInfo.colorHead = Color.Transparent;
			drawInfo.colorLegs = Color.Transparent;
			drawInfo.colorPants = Color.Transparent;
			drawInfo.colorShirt = Color.Transparent;
			drawInfo.colorShoes = Color.Transparent;
			drawInfo.colorUnderShirt = Color.Transparent;
		}
	}
}
