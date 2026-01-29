from typing import Dict, List, NamedTuple, Optional
from BaseClasses import Region
from worlds.AutoWorld import World
from .Locations import PVZRLocation, LOCATION_ID_FROM_NAME, LOCATION_NAME_FROM_ID
from .Data import LEVEL_LOCATIONS, LEVEL_TYPE_NAMES
from .Rules import can_clear_level
from .Items import PVZRItem
from BaseClasses import ItemClassification

def make_region_rule(world, player, level_data):
    location_data = LEVEL_LOCATIONS[level_data["location"]]
    at_night = location_data["at_night"]
    has_pool = location_data["has_pool"]
    on_roof = location_data["on_roof"]

    access_item = None
    if level_data["type"] == "adventure" and world.options.adventure_mode_progression.value in [1, 2] and not level_data["name"] == "Roof: Dr. Zomboss":
        if on_roof:
            access_item = "Roof Access"
        elif has_pool and at_night:
            access_item = "Fog Access"
        elif has_pool:
            access_item = "Pool Access"
        elif at_night:
            access_item = "Night Access"
        else:
            access_item = "Day Access"
    elif level_data["type"] == "adventure" and world.options.adventure_mode_progression.value == 3 and not level_data["name"] == "Roof: Dr. Zomboss":
        access_item = level_data["unlock_item_name"]
    elif (level_data["type"] == "minigame" and world.options.minigame_levels.value == 4) or (level_data["type"] == "puzzle" and world.options.puzzle_levels.value == 4) or (level_data["type"] == "survival" and world.options.survival_levels.value == 4) or (level_data["type"] == "cloudy" and world.options.cloudy_day_levels.value == 4) or (level_data["type"] == "bonus" and world.options.bonus_levels.value == 2):
        access_item = level_data["unlock_item_name"]
    else:
        access_item = {"adventure": None, "minigame": "Mini-games", "cloudy": "Cloudy Day", "puzzle": "Puzzle Mode", "bonus": "Bonus Levels", "survival": "Survival Mode"}[level_data["type"]]

    if access_item == None:
        return lambda state: can_clear_level(state, world, player, level_data, at_night, has_pool, on_roof)
    else: 
        return lambda state: can_clear_level(state, world, player, level_data, at_night, has_pool, on_roof) and state.has(access_item, player)

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