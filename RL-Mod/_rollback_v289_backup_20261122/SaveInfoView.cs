using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.2：联机中心「存档」分页。只读查看 5 个存档槽的进度，不触发任何游戏读档/切换流程。
    /// 用 SaveManager.LoadGameData(slot, SaveDataType.Player, out object) 加载 PlayerSaveData 副本，
    /// 全程反射访问游戏类型（零编译依赖），任何异常都被吞掉并显示友好提示，绝不影响游戏。
    /// </summary>
    internal static class SaveInfoView
    {
        private static Type s_saveMgrType;
        private static Type s_playerDataType;
        private static MethodInfo s_loadGameData;
        private static bool s_initTried;

        private static int s_slot = -1;           // -1 = 跟随当前存档

        // 装备槽 -> EquipmentCategoryType(1=Weapon,2=Head,3=Chest,4=Cape,5=Trinket)
        private static readonly string[] EquipSlotNames = { "武器", "头盔", "胸甲", "披风", "饰品" };
        private static readonly string[] EquipFields =
        {
            "EdgeEquipmentType", "HeadEquipmentType", "ChestEquipmentType", "CapeEquipmentType", "TrinketEquipmentType"
        };
        private static readonly int[] EquipCategories = { 1, 2, 3, 4, 5 };

        // Boss flag 枚举值（来自 PlayerSaveFlag）+ 显示名
        private static readonly int[] BossFlagValues = { 500, 510, 520, 530, 540, 550, 570, 560 };
        private static readonly string[] BossNames =
        {
            "城堡 Boss", "森林 Boss", "石桥 Boss", "书房 Boss", "高塔 Boss", "洞穴 Boss", "花园 Boss", "最终 Boss"
        };

        private static readonly string[] ClassNames =
        {
            "剑", "斧", "魔杖", "双刀", "回流", "手杖", "长矛", "苦无", "弓", "回旋镖",
            "变形", "双节棍", "军刀", "汤勺", "拳套", "骑枪", "枪械", "武士刀", "加农炮", "鲁特琴", "星象"
        };
        private static readonly int[] ClassValues =
        {
            10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210
        };

        private static void EnsureInit()
        {
            if (s_initTried) return;
            s_initTried = true;
            try
            {
                s_saveMgrType = AccessTools.TypeByName("SaveManager");
                s_playerDataType = AccessTools.TypeByName("PlayerSaveData");
                if (s_saveMgrType != null)
                {
                    // LoadGameData(int, SaveDataType, out object) —— 三个参数
                    foreach (var m in s_saveMgrType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (m.Name == "LoadGameData" && m.GetParameters().Length == 3
                            && m.GetParameters()[2].IsOut)
                        {
                            s_loadGameData = m;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SAVEVIEW] 初始化失败: " + ex.Message);
            }
        }

        internal static void Draw(GUIStyle hStyle, GUIStyle bodyStyle, GUIStyle smallStyle)
        {
            EnsureInit();

            GUILayout.Label("── 存档查看（只读，不影响当前游戏）──", hStyle);
            GUILayout.Label("切换槽位查看各存档的进度。加载失败/空槽会显示提示。", smallStyle);
            GUILayout.Space(8);

            // 槽位按钮 + 跟随当前
            GUILayout.BeginHorizontal();
            GUILayout.Label("存档槽:", GUILayout.Width(60));
            for (int i = 0; i < 5; i++)
            {
                string label = "P" + (i + 1);
                if (GUILayout.Button(label, GUILayout.Width(42)))
                {
                    s_slot = i;
                }
            }
            if (GUILayout.Button("当前", GUILayout.Width(52)))
            {
                s_slot = -1;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            int slot = s_slot;
            if (slot < 0)
            {
                slot = GetCurrentProfile();
                GUILayout.Label("当前存档槽: P" + (slot + 1), smallStyle);
            }

            object data = LoadPlayer(slot, out string err);
            if (data == null)
            {
                GUIStyle red = new GUIStyle(bodyStyle);
                red.normal.textColor = new Color(1f, 0.55f, 0.5f);
                GUILayout.Label("无法读取 P" + (slot + 1) + "：" + err, red);
                return;
            }

            DrawOverview(data, hStyle, bodyStyle);
            GUILayout.Space(8);
            DrawCharacter(data, hStyle, bodyStyle, smallStyle);
            GUILayout.Space(8);
            DrawBosses(data, hStyle, bodyStyle);
        }

        private static int GetCurrentProfile()
        {
            try
            {
                object v = Traverse.Create(s_saveMgrType).Property("CurrentProfile").GetValue();
                return v == null ? 0 : Convert.ToInt32(v);
            }
            catch { return 0; }
        }

        private static object LoadPlayer(int slot, out string err)
        {
            err = "";
            try
            {
                if (s_loadGameData == null) { err = "存档接口不可用"; return null; }
                object[] args = new object[] { slot, 0, null }; // SaveDataType.Player = 0
                object result = s_loadGameData.Invoke(null, args);
                int code = result == null ? -1 : Convert.ToInt32(result);
                if (code != 0) { err = "空槽或读取失败 (code=" + code + ")"; return null; }
                object data = args[2];
                if (data == null) { err = "无数据"; return null; }
                return data;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return null;
            }
        }

        private static object GetField(object data, string name)
        {
            try { return Traverse.Create(data).Field(name).GetValue(); }
            catch { return null; }
        }

        private static string FieldStr(object data, string name, string def = "?")
        {
            object v = GetField(data, name);
            return v == null ? def : v.ToString();
        }

        private static bool ToBool(object v)
        {
            if (v == null) return false;
            if (v is bool) return (bool)v;
            try { return Convert.ToInt32(v) != 0; }
            catch { return false; }
        }

        private static int FieldInt(object data, string name)
        {
            object v = GetField(data, name);
            return v == null ? 0 : Convert.ToInt32(v);
        }

        private static void DrawOverview(object data, GUIStyle h, GUIStyle body)
        {
            GUILayout.Label("── 进度总览 ──", h);
            int ancestors = FieldInt(data, "NumAncestors");
            int ngPlus = FieldInt(data, "NewGamePlusLevel");
            int enemies = FieldInt(data, "EnemiesKilled");
            int deaths = FieldInt(data, "TimesDied");
            uint seconds = 0;
            try { object sv = GetField(data, "SecondsPlayed"); if (sv != null) seconds = Convert.ToUInt32(sv); }
            catch { }
            string mode = FieldStr(data, "GameModeType", "?");
            string inCastle = "否";
            try { bool b = ToBool(GetField(data, "InCastle")); inCastle = b ? "是" : "否"; }
            catch { }
            string dead = "否";
            try { bool b = ToBool(GetField(data, "IsDead")); dead = b ? "是" : "否"; }
            catch { }

            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            GUILayout.Label("游戏模式: " + mode + "    周目(NG+): " + ngPlus, body);
            GUILayout.Label("祖先代数: " + ancestors + "    处于城堡: " + inCastle + "    角色死亡: " + dead, body);
            GUILayout.Label("击杀敌人: " + enemies + "    死亡次数: " + deaths, body);
            GUILayout.Label("游戏时长: " + (int)ts.TotalHours + " 小时 " + ts.Minutes + " 分", body);
        }

        private static void DrawCharacter(object data, GUIStyle h, GUIStyle body, GUIStyle small)
        {
            GUILayout.Label("── 当前角色 ──", h);
            object ch = GetField(data, "CurrentCharacter");
            if (ch == null) { GUILayout.Label("无角色数据", small); return; }

            // 职业
            int classVal = 0;
            try { object cv = GetField(ch, "ClassType"); if (cv != null) classVal = Convert.ToInt32(cv); }
            catch { }
            string cls = ClassName(classVal);
            string trait1 = TraitName(GetField(ch, "TraitOne"));
            string trait2 = TraitName(GetField(ch, "TraitTwo"));
            GUILayout.Label("职业: " + cls + "    特质: " + trait1 + " / " + trait2, body);

            // 精通等级（反射调用 GetClassMasteryRank）
            string rank = TryMasteryRank(data, classVal);
            if (rank != null) GUILayout.Label("职业精通等级: " + rank, body);

            // 装备
            GUILayout.Label("装备:", h);
            for (int i = 0; i < EquipFields.Length; i++)
            {
                object ev = GetField(ch, EquipFields[i]);
                int equipType = ev == null ? 0 : Convert.ToInt32(ev);
                string name = EquipName(equipType, EquipCategories[i]);
                GUILayout.Label("  " + EquipSlotNames[i] + ": " + name, body);
            }
        }

        private static string TryMasteryRank(object data, int classVal)
        {
            try
            {
                var m = s_playerDataType.GetMethod("GetClassMasteryRank", new[] { typeof(int) });
                if (m == null)
                {
                    foreach (var mm in s_playerDataType.GetMethods())
                    {
                        if (mm.Name == "GetClassMasteryRank" && mm.GetParameters().Length == 1)
                        { m = mm; break; }
                    }
                }
                if (m == null) return null;
                // 参数是 ClassType 枚举，用反射转
                var pType = m.GetParameters()[0].ParameterType;
                object arg = pType.IsEnum ? Enum.ToObject(pType, classVal) : (object)classVal;
                object r = m.Invoke(data, new[] { arg });
                return r == null ? null : r.ToString();
            }
            catch { return null; }
        }

        private static string EquipName(int equipType, int category)
        {
            if (equipType == 0) return "无";
            try
            {
                Type lib = AccessTools.TypeByName("EquipmentLibrary");
                if (lib == null) return "装备#" + equipType;
                var gm = lib.GetMethod("GetEquipmentData");
                if (gm == null) return "装备#" + equipType;
                var ps = gm.GetParameters();
                object catArg = ps[0].ParameterType.IsEnum ? Enum.ToObject(ps[0].ParameterType, category) : (object)category;
                object eqArg = ps[1].ParameterType.IsEnum ? Enum.ToObject(ps[1].ParameterType, equipType) : (object)equipType;
                object ed = gm.Invoke(null, new[] { catArg, eqArg });
                if (ed == null) return "装备#" + equipType;
                object name = Traverse.Create(ed).Field("Name").GetValue();
                if (name == null) name = Traverse.Create(ed).Field("Title").GetValue();
                string s = name == null ? null : name.ToString();
                return string.IsNullOrEmpty(s) ? "装备#" + equipType : s;
            }
            catch { return "装备#" + equipType; }
        }

        private static string ClassName(int v)
        {
            for (int i = 0; i < ClassValues.Length; i++)
                if (ClassValues[i] == v) return ClassNames[i] + "系 (" + v + ")";
            return "职业#" + v;
        }

        private static string TraitName(object traitObj)
        {
            string s = traitObj == null ? "0" : traitObj.ToString();
            // 尝试把枚举名转成可读文本，未知就直接显示
            if (s == "0" || s.Equals("None", StringComparison.OrdinalIgnoreCase)) return "无";
            return s;
        }

        private static void DrawBosses(object data, GUIStyle h, GUIStyle body)
        {
            GUILayout.Label("── Boss 击杀进度（当前周目）──", h);
            object flagTable = GetField(data, "FlagTable");
            if (flagTable == null) { GUILayout.Label("无标记数据", body); return; }

            int beaten = 0;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < BossFlagValues.Length; i++)
            {
                // v1.9.4：只用 *_Defeated（当前周目击败状态）。
                // 之前 done=first||cur 会把 *_Defeated_FirstTime（历史首次）也算进去，
                // NG+ 重置后 Defeated=false 但 FirstTime 仍是 true -> 误判为"已击败"。
                bool cur = FlagValue(flagTable, BossFlagValues[i]);
                if (cur) beaten++;
                GUILayout.Label("  " + BossNames[i] + ": " + (cur ? "已击败" : "未击败"), body);
                sb.Append(BossNames[i]).Append('=').Append(cur ? "1" : "0").Append(' ');
            }
            GUILayout.Label("已击败 Boss: " + beaten + " / " + BossFlagValues.Length, h);
            // 诊断（节流：只在切槽后打一次）
            if (s_bossLogSlot != s_slot)
            {
                s_bossLogSlot = s_slot;
                Plugin.Log.LogInfo("[SAVEVIEW] Boss flags slot=" + s_slot + ": " + sb.ToString());
            }
        }

        private static int s_bossLogSlot = int.MinValue;

        private static bool FlagValue(object flagTable, int enumValue)
        {
            try
            {
                var dict = flagTable as IDictionary;
                if (dict == null) return false;
                foreach (DictionaryEntry kv in dict)
                {
                    int keyVal;
                    object k = kv.Key;
                    if (k is PlayerSaveFlag) keyVal = (int)(PlayerSaveFlag)k;
                    else if (k is Enum) keyVal = Convert.ToInt32(k);
                    else if (k is string) keyVal = ParseFlagName((string)k);
                    else keyVal = Convert.ToInt32(k);
                    if (keyVal == enumValue) return ToBool(kv.Value);
                }
            }
            catch { }
            return false;
        }

        /// <summary>字符串 flag 名（如 "CastleBoss_Defeated"）转枚举值，兼容非枚举 key 的存档。</summary>
        private static int ParseFlagName(string name)
        {
            try
            {
                object v = Enum.Parse(typeof(PlayerSaveFlag), name);
                return v == null ? -1 : Convert.ToInt32(v);
            }
            catch { return -1; }
        }
    }
}
