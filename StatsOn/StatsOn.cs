using System.Reflection;
using Mono.Cecil;
using ScrollsModLoader.Interfaces;

namespace StatsOn.mod
{
    public class StatsOn : BaseMod
    {
        private BattleMode _battleMode;
        private int _init;
        private int _lastTurn;
        private FieldInfo _showStatsInfo;


        public static string GetName()
        {
            return "StatsOn";
        }

        public static int GetVersion()
        {
            return 1;
        }

        public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
        {
            try
            {
                return new[]
                    {
                        scrollsTypes["BattleMode"].Methods.GetMethod("Update")[0]
                    };
            }
            catch
            {
                return new MethodDefinition[] {};
            }
        }


        public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            returnValue = null;
            return false;
        }

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if (info.targetMethod.Equals("Update"))
            {
                if (_battleMode == null)
                {
                    _battleMode = (BattleMode) info.target;
                }

                //the first update happens before the game is done initializing, causing it the stats to look weird until after the first refresh
                if (_init > 20)
                {
                    _showStatsInfo = typeof (BattleMode).GetField("showUnitStats",
                                                                  BindingFlags.NonPublic | BindingFlags.Instance);

                    if (_showStatsInfo != null && !(bool) _showStatsInfo.GetValue(_battleMode))
                    {
                        typeof (BattleMode).GetMethod("toggleUnitStats", BindingFlags.NonPublic | BindingFlags.Instance)
                                           .Invoke(_battleMode, null);
                    }


                    //units being attacked seems to cause the stats for that unit to turn off (but showUnitStats is still true)
                    //so setting showUnitStats to false at the beginning of each turn, causing an update for all of them, seems to be the easiest fix

                    FieldInfo _turnInfo = typeof (BattleMode).GetField("currentTurn",
                                                                       BindingFlags.NonPublic | BindingFlags.Instance);
                    var turn = (int) _turnInfo.GetValue(_battleMode);

                    if (turn > _lastTurn)
                    {
                        _showStatsInfo.SetValue(_battleMode, false);
                        _lastTurn = turn;
                    }
                }
                else
                    _init++;
            }
        }
    }
}