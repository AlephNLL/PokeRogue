namespace GameData
{
    public enum Stance { AGRESSIVE, DEFENSIVE, AGILE, CAUTIOUS, TRICKY }
    public enum AbilityEffect { NONE, HEAL, UPATK, UPDEF, UPSPEED, DOWNATK, DOWNDEF, DOWNSPEED, STANCECHANGE, APPLYSTATUS, CURESTATUS }
    public enum AbilityType { ACTIVE, PASSIVE }

    public enum AbilityTarget { SELF, ONEENEMY, ONEALLY, ALLENEMIES, ALLALLIES, ALL }

    public enum Status { NONE, BURNED, POISONED, PARALYZED, FROZEN, ASLEEP }

    public enum Stats { HP, ATK, DEF, SPEED, LUCK }

    public enum PassiveEffects { UPATK, UPATKONSTATUS, UPDEF, UPSPEED, DOWNATK, DOWNDEF, DOWNSPEED, ADDTURN, SKIPTURN, APPLYSTATUS }

    public enum ItemEffects { UPATK, UPDEF, UPSPEED, ADDTURN, APPLYSTATUS, HEAL, LEVELUP }

    public enum ExecutionTime { BATTLESTART, TURNSTART, TURNEND, ONHIT, ONHURT, OUTSIDEBATTLE, ONDEATH, ONKILL, ONSTANCECHANGE, ONSTATUSCHANGE }

    public enum VFX { BUFF, NERF, HIT, HEAL, FREEZE, BURN, POISON, PARALYZE, SLEEP  }

    public enum Difficulty { EASY, NORMAL, HARD}

    public enum RoomType {NOT_ASSIGNED, Enemy, Shop, HardEnemy, Treasure, Boss, Heal, Spawn}

    public enum NodeEvents { NONE, GOLD, HEAL, TRANSITION, SPECIAL}

    public enum Events { NONE, GAINGOLD, GAINITEM, LOSEGOLD, LOSEITEM, HEAL, LEVELUP, DAMAGE, APPLYSTATUS}
}
