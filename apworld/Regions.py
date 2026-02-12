from typing import Dict, List, NamedTuple, Optional
from BaseClasses import Region
from worlds.AutoWorld import World
from .Locations import PVZRLocation, LOCATION_ID_FROM_NAME, LOCATION_NAME_FROM_ID
from .Data import LEVEL_LOCATIONS, LEVEL_TYPE_NAMES
from .Rules import can_clear_level
from .Items import PVZRItem
from BaseClasses import ItemClassification

def get_cleared_adventure_areas(state, world, player):
    return state.has("Adventure Level Cleared (Area: Day)", player, 10) + state.has("Adventure Level Cleared (Area: Night)", player, 10) + state.has("Adventure Level Cleared (Area: Pool)", player, 10) + state.has("Adventure Level Cleared (Area: Fog)",  player, 10) + state.has("Adventure Level Cleared (Area: Roof)",  player, 9)

def get_cleared_adventure_levels(state, world, player):
    return state.count("Adventure Level Cleared (Area: Day)", player) + state.count("Adventure Level Cleared (Area: Night)", player) + state.count("Adventure Level Cleared (Area: Pool)", player) + state.count("Adventure Level Cleared (Area: Fog)",  player) + state.count("Adventure Level Cleared (Area: Roof)",  player)

def get_total_cleared_levels(state, world, player):
    return get_cleared_adventure_levels(state, world, player) + state.count("Mini-game Level Cleared", player) + state.count("I, Zombie Level Cleared", player) + state.count("Vasebreaker Level Cleared", player) + state.count("Survival Level Cleared",  player) + state.count("Cloudy Day Level Cleared",  player) + state.count("Bonus Levels Level Cleared",  player)
    
def can_access_level(state, world, player, level_data):
    #Access items
    if level_data["type"] == "adventure" and world.options.adventure_mode_progression.value in [1, 2]:
        access_item = f"{level_data["location"]} Access"
    elif level_data["type"] == "adventure" and world.options.adventure_mode_progression.value == 3 and not level_data["name"] == "Roof: Dr. Zomboss":
        access_item = level_data["unlock_item_name"]
    elif (level_data["type"] == "minigame" and world.options.minigame_levels.value == 4) or (level_data["type"] == "puzzle" and world.options.puzzle_levels.value == 4) or (level_data["type"] == "survival" and world.options.survival_levels.value == 4) or (level_data["type"] == "cloudy" and world.options.cloudy_day_levels.value == 4) or (level_data["type"] == "bonus" and world.options.bonus_levels.value == 2):
        access_item = level_data["unlock_item_name"]
    else:
        access_item = {"adventure": None, "minigame": "Mini-games", "cloudy": "Cloudy Day", "puzzle": "Puzzle Mode", "bonus": "Bonus Levels", "survival": "Survival Mode"}[level_data["type"]]
    if access_item != None and not state.has(access_item, player):
        return False

    #Clears
    if level_data["type"] == "minigame" and world.options.minigame_levels.value in [1, 2] and world.minigame_unlocks[level_data["id"]] > 0:
        if not state.has("Mini-game Level Cleared", player, world.minigame_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "survival" and world.options.survival_levels.value in [1, 2] and world.survival_unlocks[level_data["id"]] > 0:
        if not state.has("Survival Level Cleared", player, world.survival_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "puzzle" and world.options.puzzle_levels.value in [1, 2] and level_data["id"] < 80:
        if not state.has("Vasebreaker Level Cleared", player, world.vasebreaker_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "puzzle" and world.options.puzzle_levels.value in [1, 2]:
        if not state.has("I, Zombie Level Cleared", player, world.izombie_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "cloudy" and world.options.cloudy_day_levels.value in [1, 2]:
        if not state.has("Cloudy Day Level Cleared", player, world.cloudy_day_unlocks[level_data["id"]]):
            return False
    elif level_data["name"] == "Roof: Dr. Zomboss":
        if world.fast_goal == False and not state.can_reach_location("Roof: Level 5-9 (Clear)", player):
            return False
        if world.adventure_levels_goal > 0 and get_cleared_adventure_levels(state, world, player) < world.adventure_levels_goal:
            return False
        if world.adventure_areas_goal > 0 and get_cleared_adventure_areas(state, world, player) < world.adventure_areas_goal:
            return False
        if world.survival_levels_goal > 0 and state.has("Survival Level Cleared", player, world.survival_levels_goal) == False:
            return False
        if world.minigame_levels_goal > 0 and state.has("Mini-game Level Cleared", player, world.minigame_levels_goal) == False:
            return False
        if world.puzzle_levels_goal > 0 and state.count("Vasebreaker Level Cleared", player) + state.count("I, Zombie Level Cleared", player) < world.puzzle_levels_goal:
            return False
        if world.cloudy_day_levels_goal > 0 and state.has("Cloudy Day Level Cleared", player, world.cloudy_day_levels_goal) == False:
            return False
        if world.bonus_levels_goal > 0 and state.has("Bonus Levels Level Cleared", player, world.bonus_levels_goal) == False:
            return False
        if world.overall_levels_goal > 0 and get_total_cleared_levels(state, world, player) < world.overall_levels_goal:
            return False

    return True

def make_region_rule(world, player, level_data):
    return lambda state: can_access_level(state, world, player, level_data) and can_clear_level(state, world, player, level_data)

def create_regions(world: World) -> None:
    player = world.player
    multiworld = world.multiworld
    
    menu_region = Region("Menu", player, multiworld)
    multiworld.regions.append(menu_region)
    
    previous_region = menu_region

    allowed_types = ["adventure"]
    if world.options.minigame_levels.value != 0:
        allowed_types.append("minigame")
    if world.options.puzzle_levels.value != 0:
        allowed_types.append("puzzle")
    if world.options.survival_levels.value != 0:
        allowed_types.append("survival")
    if world.options.cloudy_day_levels.value != 0:
        allowed_types.append("cloudy")
    if world.options.bonus_levels.value != 0:
        allowed_types.append("bonus")

    adventure_level_index = 0
    for level in world.modified_levels:

        level_data = world.modified_levels[level]

        if level_data["type"] in allowed_types:

            region_locations = [level_data["clear_location_id"]]

            if (world.options.huge_wave_locations.value):
                for flag_location in level_data["flag_location_ids"]:
                    region_locations.append(flag_location)

            level_region = Region(level_data["name"], player, multiworld)
            level_region.locations += [PVZRLocation(player, LOCATION_NAME_FROM_ID[location], location, level_region) for location in region_locations]

            if level_data["type"] in ["minigame", "bonus", "puzzle", "survival", "cloudy"] or (level_data["type"] == "adventure" and (level_data["name"] == "Roof: Dr. Zomboss" or (world.options.adventure_mode_progression.value == 1 and adventure_level_index % 10 == 0) or (world.options.adventure_mode_progression.value in [2, 3]))):
                menu_region.connect(connecting_region = level_region,  rule = make_region_rule(world, player, level_data))
            elif level_data["type"] == "adventure":
                previous_region.connect(connecting_region = level_region,  rule = make_region_rule(world, player, level_data))

            level_clear_event_location = PVZRLocation(player, f"{level_data["name"]} (Level Clear)", None, level_region)
            if (level_data["type"] == "adventure"):
                level_clear_event_item_name = f"Adventure Level Cleared (Area: {level_data["location"]})"
            elif (level_data["type"] == "puzzle"):
                if level_data["id"] < 80:
                    level_clear_event_item_name = "Vasebreaker Level Cleared"
                else:
                    level_clear_event_item_name = "I, Zombie Level Cleared"
            else:
                level_clear_event_item_name = f"{LEVEL_TYPE_NAMES[level_data["type"]]} Level Cleared"
            level_clear_event_location.place_locked_item(PVZRItem(level_clear_event_item_name, ItemClassification.progression, None, player))
            level_region.locations.append(level_clear_event_location)

            previous_region = level_region

            if level_data["type"] == "adventure":
                adventure_level_index += 1

    shop_region = Region("Crazy Dave's Twiddydinkies", player, multiworld)
    for x in range(0, world.options.shop_items.value):
        shop_region.locations += [PVZRLocation(player, f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}", 5000 + x, shop_region)]
    menu_region.connect(connecting_region = shop_region, rule = lambda state: state.has("Crazy Dave's Car Keys", player))