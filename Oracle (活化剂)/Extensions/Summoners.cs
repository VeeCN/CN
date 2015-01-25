﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using OC = Oracle.Program;

namespace Oracle.Extensions
{
    internal static class Summoners
    {
        private static bool _isJungling;
        private static string _smiteSlot;
        private static Menu _mainMenu, _menuConfig;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public static readonly string[] SmallMinions =
        {
            "SRU_Murkwolf",
            "SRU_Razorbeak",
            "SRU_Krug",
            "Sru_Crab",
            "SRU_Gromp"
        };

        public static readonly string[] EpicMinions =
        {
            "TT_Spiderboss",
            "SRU_Baron",
            "SRU_Dragon"
        };

        public static readonly string[] LargeMinions =
        {
            "SRU_Blue",
            "SRU_Red",
            "TT_NWraith",
            "TT_NGolem",
            "TT_NWolf"
        };

        public static readonly int[] SmiteAll =
        {
            3713, 3726, 3725, 3724, 3723,
            3711, 3722, 3721, 3720, 3719,
            3715, 3718, 3717, 3716, 3714,
            3706, 3710, 3709, 3708, 3707,
        };

        private static bool _hh, _cc, _bb, _ee, _ii, _ss;
        private static readonly int[] _smitePurple = { 3713, 3726, 3725, 3724, 3723 };
        private static readonly int[] _smiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] _smiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] _smiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        public static void Initialize(Menu root)
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

            _mainMenu = new Menu("召唤师技能", "summoners");
            _menuConfig = new Menu("召唤师技能配置", "sconfig");
            _isJungling = SmiteAll.Any(x => Items.HasItem(x));

            foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                _menuConfig.AddItem(new MenuItem("suseOn" + x.SkinName, "Use for " + x.SkinName)).SetValue(true);
            _mainMenu.AddSubMenu(_menuConfig);

            var smite = Me.GetSpellSlot("summonersmite");
            if (smite != SpellSlot.Unknown || _isJungling)
            {
                _ss = true;
                var Smite = new Menu("惩戒", "msmite");
                Smite.AddItem(new MenuItem("useSmite", "启用")).SetValue(new KeyBind(77, KeyBindType.Toggle, true));
                Smite.AddItem(new MenuItem("smiteSpell", "惩戒+天赋")).SetValue(true);
                Smite.AddItem(new MenuItem("drawSmite", "显示惩戒范围")).SetValue(true);
                Smite.AddItem(new MenuItem("smiteSmall", "惩戒小野怪")).SetValue(false);
                Smite.AddItem(new MenuItem("smiteLarge", "惩戒大野怪")).SetValue(true);
                Smite.AddItem(new MenuItem("smiteEpic", "惩戒史诗级野怪")).SetValue(true);
                Smite.AddItem(new MenuItem("smitemode", "惩戒敌人: "))
                    .SetValue(new StringList(new[] { "可击杀", "连招", "不使用" }));
                Smite.AddItem(
                    new MenuItem("saveSmite", "节省惩戒充能").SetValue(true));
                _mainMenu.AddSubMenu(Smite);
            }

            var ignite = Me.GetSpellSlot("summonerdot");
            if (ignite != SpellSlot.Unknown)
            {
                _ii = true;
                var Ignite = new Menu("引燃", "mignite");
                Ignite.AddItem(new MenuItem("useIgnite", "启用")).SetValue(true);
                Ignite.AddItem(new MenuItem("dotMode", "模式: ")).SetValue(new StringList(new[] {"击杀", "连招"}));
                _mainMenu.AddSubMenu(Ignite);
            }

            var heal = Me.GetSpellSlot("summonerheal");
            if (heal != SpellSlot.Unknown)
            {
                _hh = true;
                var Heal = new Menu("治疗", "mheal");
                Heal.AddItem(new MenuItem("useHeal", "启用")).SetValue(true);
                Heal.AddItem(new MenuItem("useHealPct", "血量低于（%）使用 ")).SetValue(new Slider(25, 1));
                Heal.AddItem(new MenuItem("useHealDmg", "伤害高于（%）使用 ")).SetValue(new Slider(40, 1));
                _mainMenu.AddSubMenu(Heal);
            }

            var clarity = Me.GetSpellSlot("summonermana");
            if (clarity != SpellSlot.Unknown)
            {
                _cc = true;
                var Clarity = new Menu("清晰术", "mclarity");
                Clarity.AddItem(new MenuItem("useClarity", "启用")).SetValue(true);
                Clarity.AddItem(new MenuItem("useClarityPct", "蓝量低于（%）使用 ")).SetValue(new Slider(40, 1));
                _mainMenu.AddSubMenu(Clarity);
            }

            var barrier = Me.GetSpellSlot("summonerbarrier");
            if (barrier != SpellSlot.Unknown)
            {
                _bb = true;
                var Barrier = new Menu("屏障", "mbarrier");
                Barrier.AddItem(new MenuItem("useBarrier", "启用")).SetValue(true);
                Barrier.AddItem(new MenuItem("useBarrierPct", "血量低于（%）使用 ")).SetValue(new Slider(25, 1));
                Barrier.AddItem(new MenuItem("useBarrierDmg", "伤害高于（%）使用")).SetValue(new Slider(40, 1));
                _mainMenu.AddSubMenu(Barrier);
            }

            var exhaust = Me.GetSpellSlot("summonerexhaust");
            if (exhaust != SpellSlot.Unknown)
            {
                _ee = true;
                var Exhaust = new Menu("虚弱", "mexhaust");
                Exhaust.AddItem(new MenuItem("useExhaust", "启用")).SetValue(true);
                Exhaust.AddItem(new MenuItem("aExhaustPct", "友方血量低于（%）使用")).SetValue(new Slider(35));
                Exhaust.AddItem(new MenuItem("eExhaustPct", "敌方血量低于（%）使用")).SetValue(new Slider(35));
                Exhaust.AddItem(new MenuItem("exhDanger", "对危险敌人使用")).SetValue(true);
                Exhaust.AddItem(new MenuItem("exhaustMode", "模式: "))
                    .SetValue(new StringList(new[] {"总是开启", "连招"}));

                _mainMenu.AddSubMenu(Exhaust);
            }

            root.AddSubMenu(_mainMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Me.IsValidTarget(300, false))
            {
                return;
            }

            FindSmite();
            CheckExhaust();
            CheckIgnite();
            CheckSmite();
            CheckClarity();
            CheckHeal(OC.IncomeDamage);
            CheckBarrier(OC.IncomeDamage);
        }

        #region Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Me.GetSpellSlot("summonersmite") == SpellSlot.Unknown && !_isJungling)
                return;

            if (!_mainMenu.Item("drawSmite").GetValue<bool>() || Me.IsDead)
                return;

            if (_mainMenu.Item("useSmite").GetValue<KeyBind>().Active && !Me.IsDead)
                Render.Circle.DrawCircle(Me.Position, 760, Color.White, 2);

            // Credits: Crisdmc
            foreach (var m in MinionManager.GetMinions(Me.Position, 760f, MinionTypes.All, MinionTeam.Neutral))
            {
                bool valid;
                if (Utility.Map.GetMap().Type.Equals(Utility.Map.MapType.TwistedTreeline))
                {
                    valid = m.IsHPBarRendered && !m.IsDead &&
                            (LargeMinions.Any(n => m.Name.Substring(0, m.Name.Length - 5).Equals(n) ||
                                                   EpicMinions.Any(
                                                       nx => m.Name.Substring(0, m.Name.Length - 5).Equals(nx))));
                }
                else
                {
                    valid = m.IsHPBarRendered && !m.IsDead &&
                            (!m.Name.Contains("Mini") &&
                             (SmallMinions.Any(z => m.Name.StartsWith(z)) || LargeMinions.Any(n => m.Name.StartsWith(n)) ||
                              EpicMinions.Any(nx => m.Name.StartsWith(nx))));
                }

                if (!valid)
                {
                    continue;
                }

                var hpBarPos = m.HPBarPosition;
                hpBarPos.X += 35;
                hpBarPos.Y += 18;
                var smiteDmg = (int) Me.GetSummonerSpellDamage(m, Damage.SummonerSpell.Smite);
                var damagePercent = smiteDmg/m.MaxHealth;
                var hpXPos = hpBarPos.X + (63*damagePercent);

                Drawing.DrawLine(hpXPos, hpBarPos.Y, hpXPos, hpBarPos.Y + 5, 2,
                    smiteDmg > m.Health ? Color.Lime : Color.White);
            }
        }

        #endregion

        #region Ignite

        private static void CheckIgnite()
        {
            if (!_ii)
                return;

            var ignite = Me.GetSpellSlot("summonerdot");
            if (ignite == SpellSlot.Unknown)
                return;


            if (ignite != SpellSlot.Unknown && !_mainMenu.Item("useIgnite").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
                return;

            foreach (
                var target in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(target => target.IsValidTarget(600)) 
                        .Where(target => !target.HasBuff("summonerdot", true)))
            {
                
                if (_mainMenu.Item("dotMode").GetValue<StringList>().SelectedIndex == 0)
                {
                    if (target.Health <= Me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite))
                    {
                        Me.Spellbook.CastSpell(ignite, target);
                    }
                }
            }

            if (_mainMenu.Item("dotMode").GetValue<StringList>().SelectedIndex == 1)
            {

                if (OC.CurrentTarget.IsValidTarget(600) && 
                    OC.CurrentTarget.Health <= OC.GetComboDamage(Me, OC.CurrentTarget))
                {
                    if (OC.Origin.Item("ComboKey").GetValue<KeyBind>().Active)
                        Me.Spellbook.CastSpell(ignite, OC.CurrentTarget);
                }
            }
        }

        #endregion

        #region Barrier

        private static void CheckBarrier(float incdmg = 0)
        {
            if (!_bb)
                return;

            var barrier = Me.GetSpellSlot("summonerbarrier");
            if (barrier == SpellSlot.Unknown)
                return;

            if (barrier != SpellSlot.Unknown && !_mainMenu.Item("useBarrier").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(barrier) != SpellState.Ready)
                return;

            var iDamagePercent = (int) ((incdmg/Me.MaxHealth)*100);
            var mHealthPercent = (int) ((Me.Health/Me.MaxHealth)*100);

            if (mHealthPercent <= _mainMenu.Item("useBarrierPct").GetValue<Slider>().Value &&
                _menuConfig.Item("suseOn" + Me.SkinName).GetValue<bool>())
            {
                if ((iDamagePercent >= 1 || incdmg >= Me.Health))
                {
                    if (OC.AggroTarget.NetworkId == Me.NetworkId)
                        Me.Spellbook.CastSpell(barrier, Me);
                }
            }

            else if (iDamagePercent >= _mainMenu.Item("useBarrierDmg").GetValue<Slider>().Value)
                if (OC.AggroTarget.NetworkId == Me.NetworkId)
                    Me.Spellbook.CastSpell(barrier, Me);

            else if (Me.HasBuff("summonerdot", true) && _mainMenu.Item("barrierDot").GetValue<bool>())
                if (OC.AggroTarget.NetworkId == Me.NetworkId)
                    Me.Spellbook.CastSpell(barrier, Me);
        }

        #endregion

        #region Heal

        private static void CheckHeal(float incdmg = 0)
        {
            if (!_hh)
                return;

            var heal = Me.GetSpellSlot("summonerheal");
            if (heal == SpellSlot.Unknown)
                return;

            if (heal != SpellSlot.Unknown && !_mainMenu.Item("useHeal").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(heal) != SpellState.Ready)
                return;

            var target = OC.Friendly();
            var iDamagePercent = (int) ((incdmg/Me.MaxHealth)*100);

            if (target.Distance(Me.ServerPosition) <= 700f)
            {
                var aHealthPercent = (int) ((target.Health/target.MaxHealth)*100);
                if (aHealthPercent <= _mainMenu.Item("useHealPct").GetValue<Slider>().Value &&
                    _menuConfig.Item("suseOn" + target.SkinName).GetValue<bool>())
                {
                    if ((iDamagePercent >= 1 || incdmg >= target.Health))
                    {
                        if (OC.AggroTarget.NetworkId == target.NetworkId)
                            Me.Spellbook.CastSpell(heal, target);
                    }
                }

                else if (iDamagePercent >= _mainMenu.Item("useHealDmg").GetValue<Slider>().Value &&
                         _menuConfig.Item("suseOn" + target.SkinName).GetValue<bool>())
                {
                    if (OC.AggroTarget.NetworkId == target.NetworkId)
                        Me.Spellbook.CastSpell(heal, target);
                }
            }
        }

        #endregion

        #region Clarity

        private static void CheckClarity()
        {
            if (!_cc)
                return;

            var clarity = Me.GetSpellSlot("summonermana");
            if (clarity == SpellSlot.Unknown)
                return;

            if (clarity != SpellSlot.Unknown && !_mainMenu.Item("useClarity").GetValue<bool>())
                return;

            if (Me.Spellbook.CanUseSpell(clarity) != SpellState.Ready)
                return;

            var target = OC.Friendly();
            if (!(target.Distance(Me.Position) <= 600f))
                return;

            var aManaPercent = (int) ((target.Mana/target.MaxMana)*100);
            if (aManaPercent > _mainMenu.Item("useClarityPct").GetValue<Slider>().Value)
                return;

            if (!_menuConfig.Item("suseOn" + target.SkinName).GetValue<bool>())
                return;

            if (!Me.InFountain() && !Me.IsRecalling())
                Me.Spellbook.CastSpell(clarity, target);
        }

        #endregion

        #region Smite

        private static void FindSmite()
        {
            if (_smiteBlue.Any(x => Items.HasItem(x)))
                _smiteSlot = "s5_summonersmiteplayerganker";
            else if (_smiteRed.Any(x => Items.HasItem(x)))
                _smiteSlot = "s5_summonersmiteduel";
            else if (_smiteGrey.Any(x => Items.HasItem(x)))
                _smiteSlot = "s5_summonersmitequick";
            else if (_smitePurple.Any(x => Items.HasItem(x)))
                _smiteSlot = "itemsmiteaoe";
            else
            {
                _smiteSlot = "summonersmite";
            }

            _isJungling = SmiteAll.Any(x => Items.HasItem(x));
        }

        private static void CheckSmite()
        {
            if (!_ss)
                return;

            var smite = Me.GetSpellSlot(_smiteSlot);
            if (smite == SpellSlot.Unknown)
            {
                return;
            }

            if (smite != SpellSlot.Unknown && !_mainMenu.Item("useSmite").GetValue<KeyBind>().Active)
            {
                return;
            }

            CheckChampSmite("Vi", "self", 125f, SpellSlot.E);
            CheckChampSmite("JarvanIV", "vector", 770f, SpellSlot.Q);
            CheckChampSmite("Poppy", "target" ,125f, SpellSlot.Q);
            CheckChampSmite("Riven", "self", 125f, SpellSlot.W);
            CheckChampSmite("Malphite", "self", 200f, SpellSlot.E);
            CheckChampSmite("LeeSin", "self", 1100f, SpellSlot.Q, 1);
            CheckChampSmite("Nunu", "target", 125f, SpellSlot.Q);
            CheckChampSmite("Olaf", "target", 325f, SpellSlot.E);
            CheckChampSmite("Elise", "target", 425f, SpellSlot.Q);
            CheckChampSmite("Warwick", "target", 400f, SpellSlot.Q);
            CheckChampSmite("MasterYi", "target", 600f, SpellSlot.Q);
            CheckChampSmite("Kayle", "target", 650, SpellSlot.Q);
            CheckChampSmite("Khazix", "target", 325f, SpellSlot.Q);
            CheckChampSmite("MonkeyKing", "target", 300f, SpellSlot.Q);

            if (Me.Spellbook.CanUseSpell(smite) != SpellState.Ready)
                return;

            var save = _mainMenu.Item("saveSmite").GetValue<bool>();
            if (_mainMenu.Item("smitemode").GetValue<StringList>().SelectedIndex == 0 &&
                Me.GetSpell(smite).Name == "s5_summonersmiteplayerganker")
            {
                if (Me.Spellbook.CanUseSpell(smite) == SpellState.Ready)
                {
                    foreach (var ou in ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(760)))
                    {
                        if (save && Me.Spellbook.GetSpell(smite).Ammo <= 1)
                        {
                            return;
                        }

                        if (ou.Health <= 20 + 8 * Me.Level)
                        {
                            Me.Spellbook.CastSpell(smite, ou);
                        }
                    }
                }
            }

            if (_mainMenu.Item("smitemode").GetValue<StringList>().SelectedIndex == 1 &&
                _smiteSlot == "s5_summonersmiteplayerganker")
            {
                if (OC.Origin.Item("ComboKey").GetValue<KeyBind>().Active &&
                    Me.Spellbook.CanUseSpell(smite) == SpellState.Ready)
                {
                    if (!save && Me.Spellbook.GetSpell(smite).Ammo > 1)
                    {
                        Me.Spellbook.CastSpell(smite, OC.CurrentTarget);
                    }
                }
            }

            var minionList = MinionManager.GetMinions(Me.Position, 760f, MinionTypes.All, MinionTeam.Neutral);
            foreach (var minion in minionList.Where(m => m.IsValidTarget(760f)))
            {
                var damage = (float) Me.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite);

                if (LargeMinions.Any(name => minion.Name.StartsWith(name) && !minion.Name.Contains("Mini")))
                {
                    if (minion.Health <= damage && _mainMenu.Item("smiteLarge").GetValue<bool>())
                        Me.Spellbook.CastSpell(smite, minion);
                }

                else if (SmallMinions.Any(name => minion.Name.StartsWith(name) && !minion.Name.Contains("Mini")))
                {
                    if (minion.Health <= damage && _mainMenu.Item("smiteSmall").GetValue<bool>())
                        Me.Spellbook.CastSpell(smite, minion);
                }

                else if (EpicMinions.Any(name => minion.Name.StartsWith(name)))
                {
                    if (minion.Health <= damage && _mainMenu.Item("smiteEpic").GetValue<bool>())
                        Me.Spellbook.CastSpell(smite, minion);      
                }       
            }
        }

        private static void CheckChampSmite(string name, string type, float range, SpellSlot slot, int stage = 0)
        {
            if (OC.ChampionName != name)
                return;

            if (!_mainMenu.Item("smiteSpell").GetValue<bool>())
                return;

            if (OC.ChampionName == name && Me.Spellbook.CanUseSpell(slot) == SpellState.Unknown)
                return;

            var spell = new Spell(slot, range);
            if (!spell.IsReady())
            {
                return;
            }

            var inst = Me.Spellbook.GetSpell(slot);
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(range)))
            {              
                var smitedamage = (float) Me.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite);           
                var champdamage = (float) Me.GetSpellDamage(minion, slot, stage);


                if (Me.Distance(minion.Position) > range)
                    return;

                if (EpicMinions.Any(xe => minion.Name.StartsWith(xe) && !minion.Name.Contains("Mini")))
                {
                    if (_mainMenu.Item("smiteEpic").GetValue<bool>() && minion.Health <= smitedamage + champdamage)
                    {
                        if (name == "LeeSin" && inst.Name == "blindmonkqtwo" && !minion.HasBuff("BlindMonkSonicWave"))
                            return;

                        switch (type)
                        {
                            case "self":
                                spell.Cast();
                                break;
                            case "vector":
                                spell.Cast(minion.ServerPosition);
                                break;
                            case "target":
                                spell.CastOnUnit(minion);
                                break;
                        }
                    }
                }

                else if (LargeMinions.Any(xe => minion.Name.StartsWith(xe) && !minion.Name.Contains("Mini")))
                {
                    if (_mainMenu.Item("smiteLarge").GetValue<bool>() && minion.Health <= smitedamage + champdamage)
                    {
                        if (name == "LeeSin" && inst.Name == "blindmonkqtwo" && !minion.HasBuff("BlindMonkSonicWave"))
                            return;

                        switch (type)
                        {
                            case "self":
                                spell.Cast();
                                break;
                            case "vector":
                                spell.Cast(minion.ServerPosition);
                                break;
                            case "target":
                                spell.CastOnUnit(minion);
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Exhaust

        private static void CheckExhaust()
        {
            if (!_ee)
                return;

            var exhaust = Me.GetSpellSlot("summonerexhaust");
            if (exhaust == SpellSlot.Unknown)
                return;

            if (exhaust != SpellSlot.Unknown && !_mainMenu.Item("useExhaust").GetValue<bool>())
                return;

            if (!OC.Origin.Item("ComboKey").GetValue<KeyBind>().Active &&
                _mainMenu.Item("exhaustMode").GetValue<StringList>().SelectedIndex == 1)
                return;

            var target = OC.Friendly();
            if (Me.Spellbook.CanUseSpell(exhaust) == SpellState.Ready)
            {
                if (OC.DangerUlt && _mainMenu.Item("exhDanger").GetValue<bool>())
                {
                    if (OC.Attacker.Distance(Me.ServerPosition, true) <= 650*650)
                    {
                        Me.Spellbook.CastSpell(exhaust, OC.Attacker);
                    }
                }

                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => x.IsValidTarget(900))
                            .OrderByDescending(xe => xe.BaseAttackDamage + xe.FlatPhysicalDamageMod))
                {
                    if (enemy.Distance(Me.ServerPosition, true) <= 650*650)
                    {
                        var aHealthPercent = target.Health/target.MaxHealth*100;
                        var eHealthPercent = enemy.Health/enemy.MaxHealth*100;

                        if (eHealthPercent <= _mainMenu.Item("eExhaustPct").GetValue<Slider>().Value)
                        {
                            if (!enemy.IsFacing(target))
                                Me.Spellbook.CastSpell(exhaust, enemy);
                        }

                        else if (aHealthPercent <= _mainMenu.Item("aExhaustPct").GetValue<Slider>().Value)
                        {
                            if (enemy.IsFacing(target))
                                Me.Spellbook.CastSpell(exhaust, enemy);
                        }
                    }
                }
            }
        }
        #endregion
    }
}