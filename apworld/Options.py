from dataclasses import dataclass
from Options import Choice, Range, Toggle, PerGameCommonOptions, DeathLink, OptionCounter, OptionGroup

class AdventureModeProgression(Choice):
    """
    Determines the way in which you will progress through Adventure Mode.
    This setting only affects levels within Adventure Mode and has no impact on the other modes.

    - linear: Adventure Mode levels will be unlocked by clearing the previous level, just like in the vanilla game. Clearing an area will unlock the next one.
    - area_unlock_items: Locks each unique area (Night, Pool, Fog and Roof) of Adventure Mode behind a different item. You can swap between unlocked areas whenever you like, but the levels within each area must be cleared in order. (e.g. You must clear 5-1 before you can play 5-2)
    - open_area_unlock_items: After receiving an area unlock item, all levels within that area are instantly unlocked. (e.g. After unlocking Fog Access, you can instantly play 4-2 without playing 4-1)
    - level_items: You will start with the first five levels unlocked. Other levels are individually unlocked as separate items. If this option is enabled without enabling enough additional locations to place these items in, you will not be able to generate a world.
    """
    display_name = "Adventure Mode Progression"
    option_linear = 0
    option_area_unlock_items = 1
    option_open_area_unlock_items = 2
    option_level_items = 3
    default = 1    

class HugeWaveLocations(Toggle):
    """
    Include locations for defeating Huge Waves prior to each level's Final Wave.
    """
    display_name = "Huge Wave Locations"
    default = True

class ZombieRandomisation(Toggle):
    """
    Randomises the Zombie types included in each level of Adventure Mode. 
    For example, you may see Dancing Zombies in 1-2 or Gargantuars in 1-3.

    This currently doesn't affect X-5 or X-10 levels.
    """
    display_name = "Zombie Randomisation"
    default = False

class RandomisedZombies(OptionCounter):
    """
    Determines which Zombies will be included in randomisation.
    This setting only matters if zombie_randomisation is set to true.
    If zombie_randomisation is set to false, this option will be ignored and no Zombies will be randomised.

    Any Zombie type set to 1 will be included in randomisation and can appear in randomly selected levels.
    Any Zombie type set to 0 will stay in its usual level(s) and will not be randomised to any others.

    *Trash Can Zombie is forced to 0 if goty_compatability_mode is enabled.
    """
    display_name = "Randomised Zombies"
    min = 0
    max = 1
    valid_keys = ["Conehead", "Polevaulter", "Buckethead", "Newspaper", "ScreenDoor", "Football", "Dancer", "Snorkel", "Zomboni", "DolphinRider", "JackInTheBox", "Balloon", "Digger", "Pogo", "Bungee", "Ladder", "Catapult", "Gargantuar", "PeaHead", "WallnutHead", "JalapenoHead", "GatlingHead", "SquashHead", "TallnutHead", "GigaGargantuar", "TrashCan"]
    default = {
        "Conehead": 1,
        "Polevaulter": 1,
        "Buckethead": 1,
        "Newspaper": 1,
        "ScreenDoor": 1,
        "Football": 1,
        "Dancer": 1,
        "Snorkel": 1,
        "Zomboni": 1,
        "DolphinRider": 1,
        "JackInTheBox": 1,
        "Balloon": 1,
        "Digger": 1,
        "Pogo": 1,
        "Bungee": 1,
        "Ladder": 1,
        "Catapult": 1,
        "Gargantuar": 1,
        "PeaHead": 0,
        "WallnutHead": 0,
        "JalapenoHead": 0,
        "GatlingHead": 0,
        "SquashHead": 0,
        "TallnutHead": 0,
        "GigaGargantuar": 0,
        "TrashCan": 0
    }

class PlantStatRandomisation(Toggle):
    """
    Randomises the stats of each Plant in the game. 
    This affects Sun Cost, Packet Refresh Time, Toughness, Firing Rate and Projectile Damage.
    Plants should remain relatively balanced while still providing variety.

    This setting applies across modes, however it does not currently affect levels without a "Choose Your Seeds" screen.

    This is a brand new option that has not yet been heavily tested. 
    It may be unwise to bring it to group multiworlds in its current state.
    Enable at your own risk, but if you're up to the challenge, let me know how you get on!
    """
    display_name = "Plant Stat Randomisation"
    default = False  

class MaintainVanillaProjectileStrength(Toggle):
    """
    Prevents Plant Stat Randomisation from affecting the damage of projectiles.
    If this option is enabled, then projectiles will maintain their original damage values.
    This will result in plants that play much more similarly to their standard versions.

    This option only matters if plant_stat_randomisation is set to true.
    If plant_stat_randomisation is set to false then projectiles will provide their usual damage and this option is irrelevant.
    """
    display_name = "Maintain Vanilla Projectile Strength"
    default = False  

class MinigameLevels(Choice):
    """
    Determines how Mini-game levels will be incorporated into the game.

    - off: Mini-game levels will not be included.
    - vanilla: After obtaining the "Mini-games" item, the first three Mini-game levels will be available to play. Clearing any Mini-game level will unlock a new one, in order.
    - randomised_order: After obtaining the "Mini-games" item, three random Mini-game levels will be available to play. Clearing any Mini-game level will unlock a new one, in a randomised order.
    - open: After obtaining the "Mini-games" item, you have instant access to all Mini-game levels.
    - level_items: Every individual Mini-game level is a separate item.
    """
    display_name = "Mini-game Levels"
    option_off = 0
    option_vanilla = 1
    option_randomised_order = 2
    option_open = 3
    option_level_items = 4
    default = 2

class PuzzleLevels(Choice):
    """
    Determines how Puzzle levels will be incorporated into the game.

    - off: Puzzle levels will not be included.
    - vanilla: After obtaining the "Puzzle Mode" item, the first Vasebreaker level and the first I, Zombie level will be available to play. Clearing a Puzzle level will unlock the next one in its category, in order.
    - randomised_order: After obtaining the "Puzzle Mode" item, one random Vasebreaker level and one random I, Zombie level will be available to play. Clearing any Puzzle level will unlock the next one in its category, in a randomised order.
    - open: After obtaining the "Puzzle Mode" item, you have instant access to all Puzzle levels.
    - level_items: Every individual Puzzle level is a separate item.
    """
    display_name = "Puzzle Levels"
    option_off = 0
    option_vanilla = 1
    option_randomised_order = 2
    option_open = 3
    option_level_items = 4
    default = 2

class SurvivalLevels(Choice):
    """
    Determines how Survival levels will be incorporated into the game.

    - off: Survival levels will not be included.
    - vanilla: After obtaining the "Survival Mode" item, the first three Survival levels will be available to play. Clearing any Survival level will unlock a new one, in order.
    - randomised_order: After obtaining the "Survival Mode" item, three random Survival levels will be available to play. Clearing any Survival level will unlock a new one, in a randomised order.
    - open: After obtaining the "Survival Mode" item, you have instant access to all Survival levels.
    - level_items: Every individual Survival level is a separate item.
    """
    display_name = "Survival Levels"
    option_off = 0
    option_vanilla = 1
    option_randomised_order = 2
    option_open = 3
    option_level_items = 4
    default = 2

class CloudyDayLevels(Choice):
    """
    Determines how Cloudy Day levels will be incorporated into the game.
    These can be particularly challenging compared to the rest of the game.

    - off: Cloudy Day levels will not be included.
    - vanilla: After obtaining the "Cloudy Day" item, the first Cloudy Day level will be available to play. Clearing a Cloudy Day level will unlock the next one, in order.
    - randomised_order: After obtaining the "Cloudy Day" item, one random Cloudy Day level will be available to play. Clearing a Cloudy Day level will unlock a new one, in a randomised order.
    - open: After obtaining the "Cloudy Day" item, you have instant access to all Cloudy Day levels.
    - level_items: Every individual Cloudy Day level is a separate item.

    *This option is forced to "off" if goty_compatability_mode is enabled.
    """
    display_name = "Cloudy Day Levels"
    option_off = 0
    option_vanilla = 1
    option_randomised_order = 2
    option_open = 3
    option_level_items = 4
    default = 0

class BonusLevels(Choice):
    """
    Determines how Bonus Levels (also known as Limbo Levels) will be incorporated into the game.

    - off: Bonus Levels will not be included.
    - vanilla: After obtaining the "Bonus Levels" item, you have instant access to all Bonus Levels.
    - level_items: Every individual Bonus Level is a separate item.

    *This option is forced to "off" if goty_compatability_mode is enabled.
    """
    display_name = "Bonus Levels"
    option_off = 0
    option_vanilla = 1
    option_level_items = 2
    default = 0

class AdventureLevelsGoal(Range):
    """
    Determines how many unique levels of Adventure Mode (if any) must be cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    """
    display_name = "Adventure Levels Goal"
    range_start = 0
    range_end = 49
    default = 49

class AdventureAreasGoal(Range):
    """
    Determines how many unique areas of Adventure Mode (if any) must be fully cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    """
    display_name = "Adventure Areas Goal"
    range_start = 0
    range_end = 5
    default = 0

class MinigameLevelsGoal(Range):
    """
    Determines how many unique Mini-game levels (if any) must be cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    Mini-games must be enabled in order for this setting to have any effect.
    """
    display_name = "Mini-game Levels Goal"
    range_start = 0
    range_end = 20
    default = 0

class PuzzleLevelsGoal(Range):
    """
    Determines how many unique Puzzle levels (if any) must be cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    Puzzle Levels must be enabled in order for this setting to have any effect.
    """
    display_name = "Puzzle Levels Goal"
    range_start = 0
    range_end = 18
    default = 0

class SurvivalLevelsGoal(Range):
    """
    Determines how many unique Survival levels (if any) must be cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    Survival Levels must be enabled in order for this setting to have any effect.
    """
    display_name = "Survival Levels Goal"
    range_start = 0
    range_end = 10
    default = 0

class CloudyDayLevelsGoal(Range):
    """
    Determines how many unique Cloudy Day levels (if any) must be cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    Cloudy Day Levels must be enabled in order for this setting to have any effect.

    *This option is forced to 0 if goty_compatability_mode is enabled.
    """
    display_name = "Cloudy Day Levels Goal"
    range_start = 0
    range_end = 12
    default = 0

class BonusLevelsGoal(Range):
    """
    Determines how many unique Bonus Levels (if any) must be cleared before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    Bonus Levels must be enabled in order for this setting to have any effect.

    *This option is forced to 0 if goty_compatability_mode is enabled.
    """
    display_name = "Bonus Levels Goal"
    range_start = 0
    range_end = 10
    default = 0

class TotalLevelsGoal(Range):
    """
    Determines how many unique levels (if any) must be cleared across all modes combined before you can play the final battle with Dr. Zomboss in Roof: Level 5-10.
    If this option is assigned a number higher than the number of available levels then it will be automatically reduced.
    """
    display_name = "Total Levels Goal"
    range_start = 0
    range_end = 119
    default = 0

class FastGoal(Toggle):
    """
    If this option is enabled, you can instantly fight Dr. Zomboss in Roof: Level 5-10 as soon as your custom goal requirements are met.
    If this option is disabled, your custom goal requirements will be needed in addition to how your chosen adventure_mode_progression setting would typically lead to 5-10. (i.e. "linear" requires 5-9 to be completed; "area_unlock_items" requires Roof Access and 5-9 to be completed; "open_area_unlock_items" requires Roof Access)

    This option is forced on if adventure_mode_progression is set to "level_items".
    If you are playing with adventure_mode_progression set to "level_items", make sure you set at least one goal.
    If you have adventure_mode_progression set to "level_items" with all goals set to 0, you will be able to complete the game immediately!
    """
    display_name = "Fast Goal"
    default = False

class ShopItems(Range):
    """
    How many items to include in Crazy Dave's shop.
    You cannot access the shop until you obtain Crazy Dave's Car Keys. 

    Each page contains eight items. 
    Each page after the first must be unlocked by obtaining a "Twiddydinkies Restock" item.

    A value of 0 disables the shop entirely.
    A value of 64 has eight pages.
    """
    display_name = "Shop Items"
    range_start = 0
    range_end = 64
    default = 16

class StartingPlants(Range):
    """
    How many unique plants to begin the game with.
    """
    display_name = "Starting Plants"
    range_start = 1
    range_end = 49
    default = 1    

class StartingSeedSlots(Range):
    """
    How many seed slots to begin the game with.
    """
    display_name = "Starting Seed Slots"
    range_start = 1
    range_end = 10
    default = 6

class EarlySunflower(Toggle):
    """
    Attempts to place Sunflower, Sun-shroom and Coffee Bean in the first sphere, making for a more beginner-friendly game.
    """
    display_name = "Early Sunflower"
    default = False

class EarlyShovel(Toggle):
    """
    Attempts to place the Shovel in the first sphere.
    """
    display_name = "Early Shovel"
    default = False    

class EasyUpgradePlants(Toggle):
    """
    Allows you to place upgrade plants without requiring their base form.
    For example, Gatling Pea could be placed straight onto the lawn without requiring a Repeater first.
    Enabling this option increases the Sun prices of Upgrade Plants accordingly.
    """
    display_name = "Easy Upgrade Plants"
    default = False

class ImitaterBehaviour(Choice):
    """
    Determines which seeds can be copied by Imitater.

    - normal: Imitater can copy the seeds of plants you have already unlocked.
    - open: Imitater can copy any seed in the game - even the seeds of plants you haven't unlocked yet.
    """
    display_name = "Imitater Behaviour"
    option_normal = 0
    option_open = 1
    default = 0

class MusicShuffle(Choice):
    """
    Randomises the music played during levels.
    """
    display_name = "Music Shuffle"
    option_disabled = 0
    option_by_type = 1
    option_by_level = 2
    default = 0

class DisableStormFlashes(Toggle):
    """
    Disables the storm flashes in "Fog: Level 4-10" and "Dark Stormy Night".
    """
    display_name = "Disable Storm Flashes"
    default = False

class TrapPercentage(Range):
    """
    Determines the percentage of filler items that will be replaced with trap items.
    """
    display_name = "Trap Percentage"
    range_start = 0
    range_end = 100
    default = 0   

class MowerDeployTrapWeight(Range):
    """
    This trap instantly activates all Lawn Mowers on the lawn.
    A higher number means that you are more likely to see the given trap. A value of 0 means the trap will not appear.
    """
    display_name = "Mower Deploy Trap Weight"
    range_start = 0
    range_end = 100
    default = 50

class SeedPacketCooldownTrapWeight(Range):
    """
    This trap instantly puts all Seed Packets on cooldown.
    A higher number means that you are more likely to see the given trap. A value of 0 means the trap will not appear.
    """
    display_name = "Seed Packet Cooldown Trap Weight"
    range_start = 0
    range_end = 100
    default = 50

class ZombieAmbushTrapWeight(Range):
    """
    This trap instantly causes an ambush of Zombies.
    A higher number means that you are more likely to see the given trap. A value of 0 means the trap will not appear.
    """
    display_name = "Zombie Ambush Trap Weight"
    range_start = 0
    range_end = 100
    default = 50

class GotyCompatabilityMode(Toggle):
    """
    Forces your options to comply with the restrictions of Victor Tran's GOTY client.
    Setting this to true will automatically adjust your options to ensure they remain compatible.
    """
    display_name = "GOTY Compatability Mode"
    default = False

@dataclass
class PVZROptions(PerGameCommonOptions):
    adventure_mode_progression: AdventureModeProgression
    huge_wave_locations: HugeWaveLocations
    zombie_randomisation: ZombieRandomisation
    randomised_zombies: RandomisedZombies
    plant_stat_randomisation: PlantStatRandomisation
    maintain_vanilla_projectile_strength: MaintainVanillaProjectileStrength
    minigame_levels: MinigameLevels
    puzzle_levels: PuzzleLevels
    survival_levels: SurvivalLevels
    cloudy_day_levels: CloudyDayLevels
    bonus_levels: BonusLevels
    adventure_levels_goal: AdventureLevelsGoal
    adventure_areas_goal: AdventureAreasGoal
    minigame_levels_goal: MinigameLevelsGoal
    puzzle_levels_goal: PuzzleLevelsGoal
    survival_levels_goal: SurvivalLevelsGoal
    cloudy_day_levels_goal: CloudyDayLevelsGoal
    bonus_levels_goal: BonusLevelsGoal
    total_levels_goal: TotalLevelsGoal
    fast_goal: FastGoal
    shop_items: ShopItems
    starting_plants: StartingPlants
    starting_seed_slots: StartingSeedSlots
    early_sunflower: EarlySunflower
    early_shovel: EarlyShovel
    easy_upgrade_plants: EasyUpgradePlants
    imitater_behaviour: ImitaterBehaviour
    music_shuffle: MusicShuffle
    disable_storm_flashes: DisableStormFlashes
    goty_compatability_mode: GotyCompatabilityMode 
    death_link: DeathLink
    trap_percentage: TrapPercentage
    mower_deploy_trap_weight: MowerDeployTrapWeight
    seed_packet_cooldown_trap_weight: SeedPacketCooldownTrapWeight
    zombie_ambush_trap_weight: ZombieAmbushTrapWeight

OPTION_GROUPS = [
    OptionGroup("AP Settings", [GotyCompatabilityMode, DeathLink]),
    OptionGroup("Level Access", [AdventureModeProgression, MinigameLevels, PuzzleLevels, SurvivalLevels, CloudyDayLevels, BonusLevels]),    
    OptionGroup("Extra Locations", [HugeWaveLocations, ShopItems]),
    OptionGroup("Goal", [AdventureLevelsGoal, AdventureAreasGoal, MinigameLevelsGoal, PuzzleLevelsGoal, SurvivalLevelsGoal, CloudyDayLevelsGoal, BonusLevelsGoal, TotalLevelsGoal, FastGoal]),
    OptionGroup("Zombie & Plant Randomisation", [ZombieRandomisation, RandomisedZombies, PlantStatRandomisation, MaintainVanillaProjectileStrength]),
    OptionGroup("Other Tweaks", [EasyUpgradePlants, ImitaterBehaviour, DisableStormFlashes, MusicShuffle]),
    OptionGroup("Early Items", [StartingPlants, StartingSeedSlots, EarlySunflower, EarlyShovel]),
    OptionGroup("Traps", [TrapPercentage, MowerDeployTrapWeight, SeedPacketCooldownTrapWeight, ZombieAmbushTrapWeight])
]