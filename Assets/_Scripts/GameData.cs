namespace GameData
{
    public enum Stance { OFFENSIVE, DEFFENSIVE, AGILE, SERENE, IMPERATIVE }
    public enum AbilityEffect { NONE, HEAL, UPATK, UPDEF, UPSPEED, DOWNATK, DOWNDEF, DOWNSPEED, STANCECHANGE, APPLYBURN, APPLYPARA, APPLYPOISON, APPLYFRZ, APPLYSLP }
    public enum AbilityType { ACTIVE, PASSIVE }

    public enum AbilityTarget { SELF, ONEENEMY, ONEALLY, ALLENEMIES, ALLALLIES, ALL }

    public enum Status { NONE, BURNED, POISONED, PARALYZED, FROZEN, ASLEEP }

    public enum Stats { ATK, DEF, SPEED, LUCK }

    public enum PassiveEffects { UPATK, UPDEF, UPSPEED, DOWNATK, DOWNDEF, DOWNSPEED }

    public enum PassiveExecutionTime { TURNSTART, TURNEND, ONABILITYUSE, ONHURT, OUTSIDEBATTLE, ONDEATH, ONKILL }
}
