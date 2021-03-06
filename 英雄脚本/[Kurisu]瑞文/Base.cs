﻿using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace KurisuRiven
{
    // KurisuRiven Base Class
    public static class Base
    {       
        // spell tickcounts
        public static int LastQ;
        public static int LastW;
        public static int LastE;
        public static int LastR;
        public static int LastAA;
        public static int LastWS;

        // can casts checks
        public static bool CanQ;
        public static bool CanW;
        public static bool CanE;
        public static bool CanMV;
        public static bool CanAA;
        public static bool CanWS;
        public static bool CanHD;
        public static bool HasHD;

        // did casts checks
        public static bool DidQ;
        public static bool DidW;
        public static bool DidE;
        public static bool DidWS;
        public static bool DidAA;

        // main vars
        public static Menu Settings;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;
        public static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public static bool UltOn;
        public static bool CanBurst;
        public static int CleaveCount;
        public static int PassiveCount;
        public static float TrueRange;
        public static float ComboDamage;

        // shorten commonly used
        internal static bool GetBool(string item)
        {
            return Settings.Item(item).GetValue<bool>();
        }

        internal static int GetSlider(string item)
        {
            return Settings.Item(item).GetValue<Slider>().Value;
        }

        internal static int GetList(string item)
        {
            return Settings.Item(item).GetValue<StringList>().SelectedIndex;
        }

        internal static void Initialize(EventArgs args)
        {
            if (Me.ChampionName != "Riven")
                return;

            // build 
            OnMenuUpdate();
            OnSpellUpdate();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.OnDraw;

            // load spells
            W = new Spell(SpellSlot.W, 250f);
            E = new Spell(SpellSlot.E, 270f);
            Q = new Spell(SpellSlot.Q, 260f);
            R = new Spell(SpellSlot.R, 1100f);

            // set prediction
            Q.SetSkillshot(0.25f, 100f, 1400f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 300, 2200f, false, SkillshotType.SkillshotCone);

            Game.PrintChat("<b><font color=\"#FF9900\">KurisuRiven:</font></b> 鍔犺浇鎴愬姛  Vee姹夊寲");
        }

        // riven spell queue
        internal static void OnGameUpdate(EventArgs args)
        {
            // get true range
            TrueRange = Me.AttackRange + Me.Distance(Me.BBox.Minimum) + 1;

            HasHD = Items.HasItem(3077) || Items.HasItem(3074);
            CanHD = !DidAA && (Items.CanUseItem(3077) || Items.CanUseItem(3074));

            // check ult
            UltOn = Me.GetSpell(SpellSlot.R).Name != "RivenFengShuiEngine";

            if (Combo.Target != null && R.IsReady())
            {
                var dmg = Helpers.GetDmg("P", true) * 2 + Helpers.GetDmg("Q", true) + Helpers.GetDmg("W", true) +
                          Helpers.GetDmg("I") + Helpers.GetDmg("ITEMS", true);

                CanBurst = dmg >= Combo.Target.Health;
            }

            else
            {
                CanBurst = false;
            }

            ComboDamage = Helpers.GetDmg("P", true)*3 + Helpers.GetDmg("Q", true)*3 + Helpers.GetDmg("W", true) +
                          Helpers.GetDmg("I") + Helpers.GetDmg("R", true) + Helpers.GetDmg("ITEMS");

            Combo.OnGameUpdate();
            Combo.LaneFarm();
            Combo.Flee();

            Helpers.OnBuffUpdate();
            Helpers.Windslash();

            Orbwalker.SetAttack(CanMV);
            Orbwalker.SetMovement(CanMV);

            // riven spell queue
            if (DidAA && Environment.TickCount - LastAA >= (int)(Me.AttackDelay * 100) + Game.Ping/2 + GetSlider("delay"))
            {
                DidAA = false;
                CanMV = true;
                CanQ = true;
                CanE = true;
                CanW = true;
                CanWS = true;
            }

            if (DidQ && Environment.TickCount - LastQ >= (int)(Me.AttackCastDelay * 1000) + Game.Ping/2 + 57)
            {
                DidQ = false;
                CanMV = true;
                CanAA = true;
            }

            if (DidW && Environment.TickCount - LastW >= 233)
            {
                DidW = false;
                CanMV = true;
                CanAA = true;
            }

            if (DidE && Environment.TickCount - LastE >= 350)
            {
                DidE = false;
                CanMV = true;
            }

            if (!CanW && !(DidAA || DidQ || DidE) && W.IsReady())
            {
                CanW = true;
            }

            if (!CanE && !(DidAA || DidQ || DidW) && E.IsReady())
            {
                CanE = true;
            }

            if (!CanWS && !DidAA && UltOn && R.IsReady())
            {
                CanWS = true;
            }

            if (!CanAA && !(DidQ || DidW || DidE || DidWS) &&
                Environment.TickCount - LastAA >= 1000)
            {
                CanAA = true;
            }

            if (!CanMV && !(DidQ || DidW || DidE || DidWS) &&
                Environment.TickCount - LastAA >= 1000)
            {
                CanMV = true;
            }
        }

        internal static void OnSpellUpdate()
        {
            // on animation
            Obj_AI_Base.OnPlayAnimation += (sender, args) =>
            {
                if (!(DidQ || DidW || DidWS || DidE) && 
                    sender.IsMe && args.Animation.Contains("Idle"))
                {
                    Orbwalking.LastAATick = Environment.TickCount + Game.Ping/2;
                    CanAA = true;
                }

            };

            // on cast
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (!sender.IsMe) 
                    return;

                switch (args.SData.Name)
                {
                    case "RivenTriCleave":
                        DidQ = true;
                        //CanMV = false;
                        LastQ = Environment.TickCount;
                        Utility.DelayAction.Add(100,
                            () => Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                        if (GetList("engage") == 1 && HasHD)
                            Helpers.CheckR(Combo.Target);
                        CanQ = false;
                        break;
                    case "RivenMartyr":
                        DidW = true;
                        //CanMV = false;
                        LastW = Environment.TickCount;
                        CanW = false;
                        break;
                    case "RivenFeint":
                        DidE = true;
                        //CanMV = false;
                        LastE = Environment.TickCount;
                        CanE = false;
                        break;
                    case "RivenFengShuiEngine":
                        LastR = Environment.TickCount;
                        if (GetBool("multir3") && CanBurst)
                        {
                            var flashslot = Me.GetSpellSlot("summonerflash");
                            if (Me.Spellbook.CanUseSpell(flashslot) == SpellState.Ready &&
                                Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            {
                                if (Combo.Target.Distance(Me.ServerPosition) > E.Range + TrueRange + 50 &&
                                    Combo.Target.Distance(Me.ServerPosition) <= E.Range + TrueRange + 500)
                                {
                                    Me.Spellbook.CastSpell(flashslot, Combo.Target.ServerPosition);
                                }
                            }
                        }
                        break;
                    case "rivenizunablade":
                        LastWS = Environment.TickCount;
                        CanWS = false;
                        if (Q.IsReady())
                            Q.Cast(Combo.Target.ServerPosition);
                        break;
                    case "ItemTiamatCleave":
                        CanWS = true;
                        if (GetList("wsmode") == 1 && UltOn && CanWS &&
                            Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                        {
                            if (CanBurst)
                                Utility.DelayAction.Add(150, () => R.Cast(Combo.Target.ServerPosition));
                        }

                        if (GetList("engage") == 1 && !CanBurst)
                            if (Q.IsReady())
                                Q.Cast(Combo.Target.ServerPosition);
                        break;

                    case "summonerflash":
                        if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            if (W.IsReady() && GetBool("multir3"))
                                W.Cast();
                        break;
                }

                if (args.SData.Name.Contains("BasicAttack"))
                {
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        if (CanBurst || !GetBool("usecombow") || !GetBool("usecomboe"))
                        {
                            // delay till after aa
                            Utility.DelayAction.Add(50 + (int)(Me.AttackDelay * 100) + Game.Ping/2 + GetSlider("delay"), delegate
                            {
                                if (Items.CanUseItem(3077))
                                    Items.UseItem(3077);
                                if (Items.CanUseItem(3074))
                                    Items.UseItem(3074);
                            });
                        }
                    }

                    else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (!GetBool("usejunglew") || !GetBool("usejunglee"))
                        {
                            // delay till after aa
                            Utility.DelayAction.Add(50 + (int)(Me.AttackDelay * 100) + Game.Ping/2 + GetSlider("delay"), delegate
                            {
                                if (Items.CanUseItem(3077))
                                    Items.UseItem(3077);
                                if (Items.CanUseItem(3074))
                                    Items.UseItem(3074);
                            });

                        }
                    }
                }

                if (!DidQ && args.SData.Name.Contains("BasicAttack"))
                {
                    DidAA = true;
                    CanAA = false;
                    CanQ = false;
                    CanW = false;
                    CanE = false;
                    CanWS = false;
                    LastAA = Environment.TickCount;
                }
            };
        }

        internal static void OrbTo(Obj_AI_Base target)
        {
            if (CanAA && CanMV)
            {
                if (target.IsValidTarget(TrueRange + 10))
                {
                    if (!(DidQ || DidW || DidE || DidAA))
                    {
                        CanQ = false;
                        Me.IssueOrder(GameObjectOrder.AttackUnit, target);                          
                    }
                }
            }
        }

        internal static void OnMenuUpdate()
        {
            Settings = new Menu("[Kurisu]放逐之刃", "kurisuriven", true);

            var tsMenu = new Menu("目标选择", "selector");
            TargetSelector.AddToMenu(tsMenu);
            Settings.AddSubMenu(tsMenu);

            var kMenu = new Menu("走砍", "rorb");
            Orbwalker = new Orbwalking.Orbwalker(kMenu);
            kMenu.AddItem(new MenuItem("fleemode", "逃跑")).SetValue(new KeyBind(65, KeyBindType.Press));
            Settings.AddSubMenu(kMenu);

            var drMenu = new Menu("显示设置", "drawings");
            drMenu.AddItem(new MenuItem("drawengage", "显示战斗范围")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawkill", "显示击杀提示")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawtarg", "显示目标")).SetValue(true);
            Settings.AddSubMenu(drMenu);

            var mMenu = new Menu("连招", "combostuff");
            mMenu.AddItem(new MenuItem("useignote", "使用智能引燃")).SetValue(true);
            mMenu.AddItem(new MenuItem("ultwhen", "可击杀时使用R")).SetValue(new StringList(new[] { "爆发", "极限" }));
            mMenu.AddItem(new MenuItem("wsmode", "智能R模式"))
                .SetValue(new StringList(new[] { "可击杀", "爆发" }, 1));
            mMenu.AddItem(new MenuItem("multir3", "可击杀时 闪现 + 爆发?")).SetValue(false);
            mMenu.AddItem(new MenuItem("engage", "战斗模式"))
                .SetValue(new StringList(new[] { "正常", "先用提亚马特" }));
            Settings.AddSubMenu(mMenu);

            var sMenu = new Menu("技能设置", "Spells");

            var menuQ = new Menu("Q设置", "cleave");
            menuQ.AddItem(new MenuItem("usecomboq", "连招使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("usejungleq", "打野使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("uselaneq", "清线使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("qint", "打断技能")).SetValue(true);
            menuQ.AddItem(new MenuItem("qgap", "拉近距离")).SetValue(true);
            sMenu.AddSubMenu(menuQ);

            var menuW = new Menu("W设置", "kiburst");
            menuW.AddItem(new MenuItem("usecombow", "连招使用")).SetValue(true);
            menuW.AddItem(new MenuItem("usejunglew", "打野使用")).SetValue(true);
            menuW.AddItem(new MenuItem("uselanew", "清线使用")).SetValue(true);
            menuW.AddItem(new MenuItem("antigap", "防突进")).SetValue(true);
            menuW.AddItem(new MenuItem("wint", "打断技能")).SetValue(true);
            sMenu.AddSubMenu(menuW);

            var menuE = new Menu("E设置", "valor");
            menuE.AddItem(new MenuItem("usecomboe", "连招使用")).SetValue(true);
            menuE.AddItem(new MenuItem("usejunglee", "打野使用")).SetValue(true);
            menuE.AddItem(new MenuItem("uselanee", "清线使用")).SetValue(true);
            menuE.AddItem(new MenuItem("vhealth", "生命值低于%使用 ")).SetValue(new Slider(40, 1));
            sMenu.AddSubMenu(menuE);

            var menuR = new Menu("R设置", "blade");
            menuR.AddItem(new MenuItem("user", "连招使用")).SetValue(true);
            menuR.AddItem(new MenuItem("usews", "光速QA")).SetValue(true);
            sMenu.AddSubMenu(menuR);

            Settings.AddSubMenu(sMenu);

            var oMenu = new Menu("其他", "otherstuff");
            oMenu.AddItem(new MenuItem("forceaa", "清线使用QA")).SetValue(false);
            oMenu.AddItem(new MenuItem("useitems", "使用破败/幽梦")).SetValue(true);
            oMenu.AddItem(new MenuItem("keepq", "自动Q")).SetValue(true);
            oMenu.AddItem(new MenuItem("delay", "平A->Q 延迟")).SetValue(new Slider(0, 0, 200));
            oMenu.AddItem(new MenuItem("autow", "自动W")).SetValue(true);
            oMenu.AddItem(new MenuItem("wmin", "自动W最少人数")).SetValue(new Slider(2, 1, 5));
            oMenu.AddItem(new MenuItem("debugtrue", "调试炮塔范围")).SetValue(false);
            oMenu.AddItem(new MenuItem("debugdmg", "调试连招伤害")).SetValue(false);
            Settings.AddSubMenu(oMenu);

            Settings.AddToMainMenu();
        }
    }
}
