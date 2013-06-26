using System.Reflection;
using Mono.Cecil;
using ScrollsModLoader.Interfaces;

namespace StatsOn.mod
{
    public class StatsOn : BaseMod
    {
        private BattleMode _battleMode;
        private bool _done;
        private FieldInfo _showStatsInfo;


        public static string GetName()
        {
            return "StatsOn";
        }

        public static int GetVersion()
        {
            return 2;
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
            if (_done) return;

            if (info.targetMethod.Equals("Update"))
            {
                if (_battleMode == null)
                {
                    _battleMode = (BattleMode) info.target;
                } 

                FieldInfo _turnInfo = typeof(BattleMode).GetField("currentTurn", BindingFlags.NonPublic | BindingFlags.Instance);
                var turn = (int)_turnInfo.GetValue(_battleMode);

                if (turn < 1) return;

                _showStatsInfo = typeof (BattleMode).GetField("showUnitStats", BindingFlags.NonPublic | BindingFlags.Instance);

                if (_showStatsInfo != null && !(bool) _showStatsInfo.GetValue(_battleMode))
                {
                    typeof (BattleMode).GetMethod("toggleUnitStats", BindingFlags.NonPublic | BindingFlags.Instance)
                                       .Invoke(_battleMode, null);
                }

                _done = true;
            }
        }
    }
}