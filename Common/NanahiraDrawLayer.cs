using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NanahiraSkin.Common
{
	// Abstract classes are excluded from autoload automatically. Do NOT put
	// [Autoload(false)] here: it also disables the concrete derived layers.
	public abstract class NanahiraSkinDrawLayer : PlayerDrawLayer
	{
		private Asset<Texture2D> texture;
		private Asset<Texture2D> ribbonTexture;

		protected abstract string TexturePath { get; }
		protected abstract Vector2 Offset { get; }
		protected abstract NanahiraPart Part { get; }

		public override void Load()
		{
			if (Main.dedServ)
				return;

			texture = ModContent.Request<Texture2D>(TexturePath, AssetRequestMode.ImmediateLoad);
			if (Part == NanahiraPart.Head)
			{
				ribbonTexture = ModContent.Request<Texture2D>(
					"NanahiraSkin/Assets/Animation/Ribbon", AssetRequestMode.ImmediateLoad);
				if (ribbonTexture.Value.Width != NanahiraAnimation.FrameWidth ||
					ribbonTexture.Value.Height != NanahiraAnimation.FrameHeight * NanahiraAnimation.FrameCount)
					throw new InvalidOperationException("Assets/Animation/Ribbon.png must be a 40x1120 PNG.");
			}
			if (Part == NanahiraPart.MapHead)
			{
				if (texture.Value.Width != 40 || texture.Value.Height != 62)
					throw new InvalidOperationException("Assets/MapHead.png must be the supplied 40x62 head icon.");
				return;
			}

			if (texture.Value.Width != NanahiraAnimation.FrameWidth ||
				texture.Value.Height != NanahiraAnimation.FrameHeight * NanahiraAnimation.FrameCount)
				throw new InvalidOperationException($"{TexturePath} must be a 40x1120 PNG (20 vertical 40x56 frames).");
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			bool active = drawInfo.drawPlayer.GetModPlayer<NanahiraPlayer>().NanahiraSkinActive
				&& !drawInfo.drawPlayer.invis;
			return active && IsCorrectRenderContext(drawInfo.headOnlyRender);
		}

		// Child layers are traversed without checking IsHeadLayer again.
		// HairBack and Head therefore need explicit exclusion from head-only draws.
		private bool IsCorrectRenderContext(bool headOnlyRender) =>
			headOnlyRender ? Part == NanahiraPart.MapHead : Part != NanahiraPart.MapHead;

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			// Also guard the draw itself if another mod changes layer visibility.
			if (!IsCorrectRenderContext(drawInfo.headOnlyRender))
				return;

			Player player = drawInfo.drawPlayer;
			// Upper-body item use must not stop the walking animation of the legs.
			int frame = NanahiraAnimation.GetFrameIndex(
				Part == NanahiraPart.Legs ? player.legFrame : player.bodyFrame);
			Rectangle source = NanahiraAnimation.GetSourceRectangle(Part, frame);

			Vector2 fullFrameOrigin;
			Vector2 partPosition;
			float rotation;
			switch (Part)
			{
				case NanahiraPart.Legs:
					fullFrameOrigin = drawInfo.legVect;
					partPosition = player.legPosition + drawInfo.legsOffset;
					rotation = player.legRotation;
					break;
				case NanahiraPart.Body:
					fullFrameOrigin = drawInfo.bodyVect;
					partPosition = player.bodyPosition;
					rotation = player.bodyRotation;
					break;
				default:
					fullFrameOrigin = drawInfo.headVect;
					partPosition = player.headPosition;
					rotation = player.headRotation;
					break;
			}

			Vector2 offset = Offset;
			if ((drawInfo.playerEffect & SpriteEffects.FlipHorizontally) != 0)
				offset.X *= -1f;
			if ((drawInfo.playerEffect & SpriteEffects.FlipVertically) != 0)
				offset.Y *= -1f;

			// Match the vanilla 40x56 armor anchor. Centering the new frame like
			// the old 62px image would move the feet three pixels downward.
			Vector2 topLeft = new Vector2(
				(int)(drawInfo.Position.X - Main.screenPosition.X + player.width / 2f - NanahiraAnimation.FrameWidth / 2f),
				(int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - NanahiraAnimation.FrameHeight + 4f));
			Vector2 position;
			Vector2 origin;
			Texture2D drawTexture = texture.Value;
			if (Part == NanahiraPart.MapHead)
			{
				// This dedicated IsHeadLayer runs only for head-only rendering. The
				// animated head layer is excluded there and used only on the player.
				drawTexture = texture.Value;
				source = new Rectangle(0, 0, drawTexture.Width, drawTexture.Height);
				origin = new Vector2(drawTexture.Width * 0.5f, drawTexture.Height * 0.5f);
				position = topLeft + partPosition + origin + offset;
			}
			else
			{
				position = topLeft + partPosition + fullFrameOrigin + offset;
				origin = NanahiraAnimation.GetSliceOrigin(fullFrameOrigin, source, frame, drawInfo.playerEffect);
			}

			Color lightColor = drawInfo.headOnlyRender ? Color.White : Lighting.GetColor(
				(int)(player.Center.X / 16f), (int)(player.Center.Y / 16f));
			lightColor = player.GetImmuneAlphaPure(lightColor, drawInfo.shadow);

			// Terraria applies whole-player rotation later. Use only the part's
			// own rotation here so that mounts/sleeping do not rotate twice.
			drawInfo.DrawDataCache.Add(new DrawData(drawTexture, position, source,
				lightColor, rotation, origin, 1f, drawInfo.playerEffect, 0));

			if (Part == NanahiraPart.Head)
				DrawSwayingRibbon(ref drawInfo, topLeft, partPosition, frame, lightColor, rotation);
		}

		private void DrawSwayingRibbon(ref PlayerDrawSet drawInfo, Vector2 topLeft,
			Vector2 partPosition, int frame, Color lightColor, float headRotation)
		{
			// The ribbon has been separated from the head artwork. Move it by less
			// than one pixel and rotate it a few degrees around its attachment point.
			float wave = MathF.Sin(Main.GlobalTimeWrappedHourly * NanahiraDrawLayerOffsets.RibbonSwaySpeed);
			SpriteEffects effects = drawInfo.playerEffect;
			float facing = (effects & SpriteEffects.FlipHorizontally) != 0 ? -1f : 1f;
			float gravity = (effects & SpriteEffects.FlipVertically) != 0 ? -1f : 1f;
			Vector2 pivot = NanahiraDrawLayerOffsets.RibbonPivot;
			Vector2 sway = new Vector2(
				wave * NanahiraDrawLayerOffsets.RibbonSwayPixels * facing,
				wave * NanahiraDrawLayerOffsets.RibbonSwayPixels * 0.2f * gravity);

			Rectangle ribbonSource = NanahiraAnimation.GetSourceRectangle(NanahiraPart.Head, frame);
			Vector2 ribbonOrigin = NanahiraAnimation.GetSliceOrigin(
				pivot, ribbonSource, frame, effects);
			float rotationDirection = facing * gravity;
			float ribbonRotation = headRotation + wave * NanahiraDrawLayerOffsets.RibbonSwayRotation * rotationDirection;
			Vector2 ribbonPosition = topLeft + partPosition + pivot + sway;
			drawInfo.DrawDataCache.Add(new DrawData(ribbonTexture.Value, ribbonPosition,
				ribbonSource, lightColor, ribbonRotation, ribbonOrigin, 1f, effects, 0));
		}
	}

	public sealed class NanahiraBackHairLayer : NanahiraSkinDrawLayer
	{
		// Inherit HairBack's torso transform while drawing behind legs and body.
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HairBack);
		protected override string TexturePath => "NanahiraSkin/Assets/Animation/HeadNoRibbon";
		protected override Vector2 Offset => NanahiraDrawLayerOffsets.BackHairOffset;
		protected override NanahiraPart Part => NanahiraPart.BackHair;
	}

	public sealed class NanahiraLegsLayer : NanahiraSkinDrawLayer
	{
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Shoes);
		protected override string TexturePath => "NanahiraSkin/Assets/Animation/Legs";
		protected override Vector2 Offset => NanahiraDrawLayerOffsets.LegsOffset;
		protected override NanahiraPart Part => NanahiraPart.Legs;
	}

	public sealed class NanahiraBodyLayer : NanahiraSkinDrawLayer
	{
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);
		protected override string TexturePath => "NanahiraSkin/Assets/Animation/Body";
		protected override Vector2 Offset => NanahiraDrawLayerOffsets.BodyOffset;
		protected override NanahiraPart Part => NanahiraPart.Body;
	}

	public sealed class NanahiraHeadLayer : NanahiraSkinDrawLayer
	{
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		protected override string TexturePath => "NanahiraSkin/Assets/Animation/HeadAndBackHair";
		protected override Vector2 Offset => NanahiraDrawLayerOffsets.HeadOffset;
		protected override NanahiraPart Part => NanahiraPart.Head;
	}

	public sealed class NanahiraMapHeadIconLayer : NanahiraSkinDrawLayer
	{
		public override bool IsHeadLayer => true;
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		protected override string TexturePath => "NanahiraSkin/Assets/MapHead";
		protected override Vector2 Offset => NanahiraDrawLayerOffsets.MapHeadOffset;
		protected override NanahiraPart Part => NanahiraPart.MapHead;
	}

	/// <summary>
	/// Pixel adjustments from the vanilla 40x56 frame anchor.
	/// Positive X moves right and positive Y moves down when facing right.
	/// The offsets mirror automatically when facing left or upside down.
	/// </summary>
	public static class NanahiraDrawLayerOffsets
	{
		public static Vector2 BackHairOffset = Vector2.Zero;
		public static Vector2 LegsOffset = Vector2.Zero;
		public static Vector2 BodyOffset = Vector2.Zero;
		public static Vector2 HeadOffset = Vector2.Zero;
		public static Vector2 MapHeadOffset = Vector2.Zero;
		public static Vector2 RibbonPivot = new Vector2(21f, 7f);
		public static float RibbonSwayPixels = 0.65f;
		public static float RibbonSwayRotation = 0.025f;
		public static float RibbonSwaySpeed = 2.2f;
	}
}
