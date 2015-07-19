﻿#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 Flash.cs is part of SFXUtility.

 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SFXLibrary.Logger;
using SFXUtility.Classes;

#endregion

namespace SFXUtility.Features.Others
{
    internal class Flash : Child<Others>
    {
        public Flash(SFXUtility sfx) : base(sfx) {}

        public override string Name
        {
            get { return Global.Lang.Get("F_Flash"); }
        }

        protected override void OnEnable()
        {
            Spellbook.OnCastSpell += OnSpellbookCastSpell;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Spellbook.OnCastSpell -= OnSpellbookCastSpell;
            base.OnDisable();
        }

        protected override void OnLoad()
        {
            try
            {
                Menu = new Menu(Name, Name);

                Menu.AddItem(new MenuItem(Name + "IgniteCheck", Global.Lang.Get("Flash_IgniteCheck")).SetValue(false));
                Menu.AddItem(new MenuItem(Name + "Extend", Global.Lang.Get("Flash_Extend")).SetValue(false));

                Menu.AddItem(new MenuItem(Name + "Enabled", Global.Lang.Get("G_Enabled")).SetValue(false));

                Parent.Menu.AddSubMenu(Menu);
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnSpellbookCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            try
            {
                if (sender == null || !sender.Owner.IsMe || args.Slot == SpellSlot.Unknown ||
                    !ObjectManager.Player.Spellbook.GetSpell(args.Slot)
                        .Name.Equals("SummonerFlash", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                if (Menu.Item(Name + "IgniteCheck").GetValue<bool>())
                {
                    var buff =
                        ObjectManager.Player.Buffs.Where(
                            b => b.Name.Equals("SummonerDot", StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(b => b.EndTime)
                            .FirstOrDefault();
                    if (buff != null && buff.EndTime - Game.Time > 0f)
                    {
                        var hero = buff.Caster as Obj_AI_Hero;
                        if (hero != null)
                        {
                            var ticks = (int) (buff.EndTime - Game.Time);
                            var damage =
                                hero.GetSummonerSpellDamage(ObjectManager.Player, Damage.SummonerSpell.Ignite) / 5f *
                                (ticks > 0 ? ticks : 1);
                            if (damage >= ObjectManager.Player.Health)
                            {
                                args.Process = false;
                                return;
                            }
                        }
                    }
                }
                if (Menu.Item(Name + "Extend").GetValue<bool>())
                {
                    if (ObjectManager.Player.ServerPosition.To2D().Distance(args.StartPosition) < 390f)
                    {
                        args.Process = false;
                        ObjectManager.Player.Spellbook.CastSpell(
                            args.Slot, ObjectManager.Player.ServerPosition.Extend(args.StartPosition, 400f));
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }
    }
}