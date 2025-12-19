
/// <summary>
/// 物品类型
/// </summary>
public enum ItemType
{
    Seed,Commodity,Furniture,
    HoeTool,ChopTool,BreakTool,ReapTool,WaterTool,CollectTool,
    ReapableScenery
}
/// <summary>
/// 物品格子类型
/// </summary>
public enum SlotType
{
    Bag,Box,Shop
}
/// <summary>
/// 库存位置类型
/// </summary>
public enum InventoryLocation
{
    Player, Box
}
/// <summary>
/// 动作的类型（Carry-举起物品动作）(Hoe-锄地动作)（Water-浇水动作）
/// </summary>
public enum PartType
{
    None,Carry,Hoe,Break,Water,Collect,Chop,Reap
}
/// <summary>
/// 对应动作的身体部分
/// </summary>
public enum PartName
{
    Body,Hair,Arm,Tool
}
/// <summary>
/// 时间季节
/// </summary>
public enum Season
{
    春天,夏天,秋天,冬天
}
/// <summary>
/// 瓦片类型
/// </summary>
public enum GridType
{
    Diggable,DropItem,PlaceFuniture,NPCObstacle    
}

/// <summary>
/// 粒子特效类型
/// </summary>
public enum ParticleEffectType
{
    None, LeavesFalling01, LeavesFalling02, RockBreak, ReapableScenery
}

/// <summary>
/// 游戏运行状态
/// </summary>
public enum GameState
{
    Play,Pause
}
/// <summary>
/// 游戏灯光切换：早上或晚上
/// </summary>
public enum LightShift
{
    Morning,Night
}
/// <summary>
/// 音乐音效切片
/// </summary>
public enum SoundName
{
    None,
    FootStepSoft,FootStepHard,
    Axe,Pickaxe,Hoe,Reap,Water,Basket,Chop,
    Pickup,Plant,TreeFalling,Rustle,
    AmbientCountryside1,AmbientCountryside2,AmbientIndoor,
    MusicCalm1, MusicCalm2,MusicCalm3,MusicCalm4,MusicCalm5, MusicCalm6
}