using ADVance.Base;

namespace ADVance.Editor
{
    public class TempADVanceManager : ADVanceManagerBase
    {
        protected override void OnInitialize()
        {
            // エディター専用の一時的なマネージャーなので、特別な初期化処理は不要
        }
    }
}