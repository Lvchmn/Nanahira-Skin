using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NanahiraSkin.Common
{
	public enum NanahiraPart
	{
		BackHair,
		Legs,
		Body,
		Head,
		MapHead
	}

	/// <summary>
	/// Supplied Nanahira.zip: 20 vertical 40x56 frames in Terraria's order.
	/// Keep sheet geometry and crop rules here when replacing the artwork.
	/// </summary>
	public static class NanahiraAnimation
	{
		public const int FrameWidth = 40;
		public const int FrameHeight = 56;
		public const int FrameCount = 20;
		public const int HeadBottom = 32; // exclusive, within one frame
		public const int BackHairBottom = 44; // exclusive

		public static int GetFrameIndex(Rectangle vanillaFrame)
		{
			if (vanillaFrame.Height <= 0 || vanillaFrame.Y < 0)
				return 0;
			int frame = vanillaFrame.Y / vanillaFrame.Height;
			// Other mods may provide extra frames. Fall back to standing safely.
			return frame < FrameCount ? frame : 0;
		}

		public static Rectangle GetSourceRectangle(NanahiraPart part, int frame)
		{
			if (frame < 0 || frame >= FrameCount)
				frame = 0;
			int top = 0;
			int bottom = FrameHeight;
			if (part == NanahiraPart.Head)
			{
				// In l3, the raised ribbon of rows 7-9 and 14-16 occupies the final
				// two pixels of the preceding row. Read it above the correct head.
				top = frame is 7 or 8 or 9 or 14 or 15 or 16 ? -2 : 0;
				bottom = HeadBottom;
			}
			else if (part == NanahiraPart.BackHair)
			{
				// Remaining hair is y=32..43. Never include the next ribbon at 54..55.
				top = HeadBottom;
				bottom = BackHairBottom;
			}
			return new Rectangle(0, frame * FrameHeight + top, FrameWidth, bottom - top);
		}

		public static Vector2 GetSliceOrigin(Vector2 fullFrameOrigin, Rectangle source,
			int frame, SpriteEffects effects)
		{
			int top = source.Y - frame * FrameHeight;
			// SpriteBatch flips within the cropped rectangle. Preserve the pivot
			// of the full frame even when the player is upside down.
			int removedFromTop = (effects & SpriteEffects.FlipVertically) != 0
				? FrameHeight - top - source.Height : top;
			return fullFrameOrigin - new Vector2(0f, removedFromTop);
		}
	}
}
