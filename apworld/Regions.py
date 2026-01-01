from typing import Dict, List, NamedTuple, Optional
from BaseClasses import Region
from worlds.AutoWorld import World
from .Locations import PVZRLocation, LOCATION_ID_FROM_NAME, LOCATION_NAME_FROM_ID
from .Data import LEVELS, LEVEL_LOCATIONS
from .Rules import can_clear_level
from .Items import PVZRItem
from BaseClasses import ItemClassification

def make_region_rule(world, player, level_data):
    location_data = LEVEL_LOCATIONS[level_data["location"]]
    at_night = location_data["at_night"]
    has_pool = location_data["has_pool"]
    on_roof = location_data["on_roof"]

    access_item = None
    if level_data["type"] == "adventure" and world.options.adventure_mode_progression.value in [1, 2]:
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
    elif world.options.minigame_puzzle_survival_order.value == 3 and level_data["type"] in ["minigame", "puzzle", "survival"]:
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
    if world.options.include_minigames.value:
        allowed_types.append("minigame")
    if world.options.include_bonus_levels.value:
        allowed_types.append("bonus")
    if world.options.include_puzzle_levels.value:
        allowed_types.append("puzzle")
    if world.options.include_survival_levels.value:
        allowed_types.append("survival")
    if world.options.include_cloudy_day_levels.value:
        allowed_types.append("cloudy")

    adventure_level_index = 0
    for level in LEVELS:

        level_data = LEVELS[level]

        if level_data["type"] in allowed_types:

            region_locations = [level_data["clear_location_id"]]

            if (world.options.huge_wave_locations.value):
                for flag_location in level_data["flag_location_ids"]:
                    region_locations.append(flag_location)

            level_region = Region(level_data["name"], player, multiworld)
            level_region.locations += [PVZRLocation(player, LOCATION_NAME_FROM_ID[location], location, level_region) for location in region_locations]

            if level_data["type"] in ["minigame", "bonus", "puzzle", "survival"] or (level_data["type"] == "adventure" and ((world.options.adventure_mode_progression.value == 1 and adventure_level_index % 10 == 0) or (world.options.adventure_mode_progression.value == 2))) or (level == "CloudyDay1"):

                if level_data["type"] in ["minigame", "puzzle", "survival"] and not world.options.minigame_puzzle_survival_order.value in [2, 3]:
                    clear_unlock_location = PVZRLocation(player, f"{level_data["name"]} (Clear Unlock)", None, level_region)
                    if level_data["type"] == "minigame":
                        clear_unlock_location.place_locked_item(PVZRItem("Progressive Minigame Unlock", ItemClassification.progression, None, player))
                    elif level_data["type"] == "survival":
                        clear_unlock_location.place_locked_item(PVZRItem("Progressive Survival Unlock", ItemClassification.progression, None, player))
                    elif level_data["type"] == "puzzle":
                        if level_data["id"] < 80:
                            clear_unlock_location.place_locked_item(PVZRItem("Progressive Vasebreaker Unlock", ItemClassification.progression, None, player))
                        else:
                            clear_unlock_location.place_locked_item(PVZRItem("Progressive I, Zombie Unlock", ItemClassification.progression, None, player))
                    level_region.locations.append(clear_unlock_location)

                menu_region.connect(connecting_region = level_region,  rule = make_region_rule(world, player, level_data))
            elif level_data["type"] in ["adventure", "cloudy"]:
                previous_region.connect(connecting_region = level_region,  rule = make_region_rule(world, player, level_data))

            if level_data["name"] in ["Day: Level 1-10", "Night: Level 2-10", "Pool: Level 3-10", "Fog: Level 4-10", "Roof: Level 5-9"]:
                area_clear_location = PVZRLocation(player, f"{level_data["name"]} (Area Clear)", None, level_region)
                area_clear_location.place_locked_item(PVZRItem("Area Clear", ItemClassification.progression, None, player))
                level_region.locations.append(area_clear_location)

            previous_region = level_region

            if level_data["type"] == "adventure":
                adventure_level_index += 1

    shop_region = Region("Crazy Dave's Twiddydinkies", player, multiworld)
    for x in range(0, world.options.shop_items.value):
        shop_region.locations += [PVZRLocation(player, f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}", 5000 + x, shop_region)]
    menu_region.connect(connecting_region = shop_region, rule = lambda state: state.has("Crazy Dave's Car Keys", player))