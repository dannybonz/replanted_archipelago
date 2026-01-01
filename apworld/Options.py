from dataclasses import dataclass
from Options import Choice, Range, Toggle, PerGameCommonOptions, DeathLink

class HugeWaveLocations(Toggle):
    """
    Include locations for defeating Huge Waves prior to each level's Final Wave.
    """
    display_name = "Huge Wave Locations"
    default = True

class IncludeMinigames(Toggle):
    """
    Include levels found on the Mini-games page.
    """
    display_name = "Include Mini-games"
    default = True

class IncludePuzzleLevels(Toggle):
    """
    Include levels found on the Puzzles page.
    """
    display_name = "Include Puzzle Levels"
    default = True   
     
class IncludeSurvivalLevels(Toggle):
    """
    Include levels found on the Survival page.
    """
    display_name = "Include Survival Levels"
    default = True   

class IncludeCloudyDayLevels(Toggle):
    """
    Include levels found on the Cloudy Day page. 
    These can be particularly challenging compared to the rest of the game.
    """
    display_name = "Include Cloudy Day Levels"
    default = False   

class IncludeBonusLevels(Toggle):
    """
    Include levels found on the Bonus Levels page.
    """
    display_name = "Include Bonus Levels"
    default = False

class MinigamePuzzleSurvivalOrder(Choice):
    """
    Determines the order in which you can play each individual Mini-game, Puzzle or Survival level.
    
    - vanilla: After receiving a mode's unlock item, the first three levels within it are unlocked (for I, Zombie and Vasebreaker, only the first level of each is unlocked). Completing any level within the mode will unlock its next one, in order.
    - randomised: After receiving a a mode's unlock item, three random levels within it are unlocked (for I, Zombie and Vasebreaker, one random level of each is unlocked). Completing any level within the mode will unlock another one at random.
    - open: After receiving a mode's unlock item, you have instant access to all levels within it.
    - items: Each Mini-game, Puzzle or Survival level is individually unlocked as a separate item.
    """
    display_name = "Mini-game, Puzzle and Survival Order"
    option_vanilla = 0
    option_randomised = 1
    option_open = 2
    option_items = 3
    default = 1

class AdventureModeProgression(Choice):
    """
    Determines the way in which you will progress through Adventure Mode.
    This setting only affects levels within Adventure Mode and has no impact on the other modes.

    - linear: Adventure Mode levels will be unlocked by clearing the previous level, just like in the vanilla game. Clearing an area will unlock the next one.
    - area_unlock_items: Locks each unique area (Night, Pool, Fog and Roof) of Adventure Mode behind a different item. You can swap between unlocked areas whenever you like, but the levels within each area must be cleared in order. (e.g. You must clear 5-1 before you can play 5-2)
    - open_area_unlock_items: After receiving an area unlock item, all levels within that area are instantly unlocked. (e.g. After unlocking Fog Access, you can instantly play 4-2 without playing 4-1)
    """
    display_name = "Adventure Mode Progression"
    option_linear = 0
    option_area_unlock_items = 1
    option_open_area_unlock_items = 2
    default = 1

class Goal(Choice):
    """
    Determines the unlock condition for the final battle with Dr. Zomboss in 5-10.
    Clearing this level completes the game.

    - adventure: The final battle will unlock after all 49 levels of Adventure Mode have been cleared.
    - roof_only: The final battle will unlock after 5-9 has been cleared.
    """
    display_name = "Goal"
    option_adventure = 0
    option_roof_only = 1
    default = 0

class StartingSeedSlots(Range):
    """
    How many seed slots to begin the game with.
    """
    display_name = "Starting Seed Slots"
    range_start = 6
    range_end = 10
    default = 6

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

class MusicShuffle(Choice):
    """
    Randomises the music played during levels.
    """
    display_name = "Music Shuffle"
    option_disabled = 0
    option_by_type = 1
    option_by_level = 2
    default = 0

class EarlySunflower(Toggle):
    """
    Attempts to place Sunflower, Sun-shroom and Coffee Bean in the first sphere, making for a more beginner-friendly game.
    """
    display_name = "Early Sunflower"
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

@dataclass
class PVZROptions(PerGameCommonOptions):
    goal: Goal
    adventure_mode_progression: AdventureModeProgression
    huge_wave_locations: HugeWaveLocations
    shop_items: ShopItems
    include_minigames: IncludeMinigames
    include_puzzle_levels: IncludePuzzleLevels
    include_survival_levels: IncludeSurvivalLevels
    minigame_puzzle_survival_order: MinigamePuzzleSurvivalOrder
    include_cloudy_day_levels: IncludeCloudyDayLevels
    include_bonus_levels: IncludeBonusLevels
    music_shuffle: MusicShuffle
    early_sunflower: EarlySunflower
    starting_seed_slots: StartingSeedSlots
    starting_plants: StartingPlants
    imitater_behaviour: ImitaterBehaviour
    trap_percentage: TrapPercentage
    mower_deploy_trap_weight: MowerDeployTrapWeight
    seed_packet_cooldown_trap_weight: SeedPacketCooldownTrapWeight
    zombie_ambush_trap_weight: ZombieAmbushTrapWeight
