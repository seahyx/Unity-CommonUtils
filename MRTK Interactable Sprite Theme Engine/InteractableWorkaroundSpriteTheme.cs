// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using CommonUtils;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
	/// <summary>
	/// <para>
	/// Theme Engine to change the <see cref="int"/> value on a <see cref="SpriteIndexer"/> component, which will change the sprite on the object.
	/// This is a workaround for the inane fact that there is no <see cref="ThemeStateProperty"/> for <see cref="Sprite"/>s.
	/// </para>
	/// As this is a workaround, there may be visual bugs.
	/// </summary>
	public class InteractableWorkaroundSpriteTheme : InteractableThemeBase
	{
		/// <inheritdoc />
		public override bool IsEasingSupported => false;

		private SpriteIndexer spriteIndexer;

		public InteractableWorkaroundSpriteTheme()
		{
			Types = new Type[] { typeof(SpriteIndexer) };
			Name = "Workaround Sprite Theme";
		}

		/// <inheritdoc />
		public override ThemeDefinition GetDefaultThemeDefinition()
		{
			return new ThemeDefinition()
			{
				ThemeType = GetType(),
				StateProperties = new List<ThemeStateProperty>()
				{
					new ThemeStateProperty()
					{
						Name = "Sprite Index",
						Type = ThemePropertyTypes.Int,
						Values = new List<ThemePropertyValue>(),
						Default = new ThemePropertyValue() { Int = 0 }
					},
				},
				CustomProperties = new List<ThemeProperty>(),
			};
		}

		/// <inheritdoc />
		public override void Init(GameObject host, ThemeDefinition settings)
		{
			spriteIndexer = host.GetComponent<SpriteIndexer>();

			base.Init(host, settings);
		}

		/// <inheritdoc />
		public override ThemePropertyValue GetProperty(ThemeStateProperty property)
		{
			ThemePropertyValue start = new ThemePropertyValue();
			start.Int = 0;

			if (spriteIndexer != null)
			{
				start.Int = spriteIndexer.GetIndex();
			}

			return start;
		}

		/// <inheritdoc />
		public override void SetValue(ThemeStateProperty property, int index, float percentage)
		{
			SetValue(property, property.Values[index]);
		}

		/// <inheritdoc />
		protected override void SetValue(ThemeStateProperty property, ThemePropertyValue value)
		{
			if (spriteIndexer != null)
			{
				spriteIndexer.SetIndex(value.Int);
			}
		}
	}
}
