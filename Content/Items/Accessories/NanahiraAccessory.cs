using NanahiraSkin.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NanahiraSkin.Content.Items.Accessories
{
	public sealed class NanahiraAccessory : ModItem
	{
		// NanahiraAccessory.png is the original single-frame head icon.
		// Keep the inventory icon separate from the 20-frame animation sheet.

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.White;
			Item.value = 0;
			Item.maxStack = 1;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			EnableSkin(player);
		}

		public override void UpdateVanity(Player player)
		{
			EnableSkin(player);
		}

		public override void AddRecipes()
		{
			Recipe.Create(Type)
				.AddIngredient(ItemID.Wood, 10)
				.Register();
		}

		private static void EnableSkin(Player player)
		{
			player.GetModPlayer<NanahiraPlayer>().NanahiraSkinActive = true;
		}
	}
}
