from worlds.AutoWorld import World

def has_wall(state, player):
    return any(state.has(item, player) for item in ["Wall-nut", "Tall-nut", "Pumpkin"])

def has_pult(state, player):
    return any(state.has(item, player) for item in ["Cabbage-pult", "Kernel-pult", "Melon-pult"])

def has_instant(state, player, amount, at_night):
    instants = ["Chomper", "Cherry Bomb", "Jalapeno", "Squash", "Potato Mine"]
    count = sum(state.has(plant, player) for plant in instants)
    if state.has("Doom-shroom", player) and (at_night or state.has("Coffee Bean", player)):
        count += 1
    return count >= amount

def has_magnet(state, player, at_night):
    return state.has("Magnet-shroom", player) and (at_night or state.has("Coffee Bean", player))

def can_counter_screen_door(state, player, at_night, has_pool, on_roof):
    return state.has("Fume-shroom", player) or has_magnet(state, player, at_night) or has_pult(state, player) or (has_wall(state, player) and (state.has("Spikeweed", player) or has_instant(state, player, 1, at_night)))

def can_counter_ladder(state, player, at_night, has_pool, on_roof):
    return state.has("Fume-shroom", player) or has_magnet(state, player, at_night) or has_pult(state, player) or has_instant(state, player, 1, at_night)

def can_counter_football(state, player, at_night, has_pool, on_roof):
    return has_instant(state, player, 1, at_night) or has_wall(state, player) or has_magnet(state, player, at_night)

def can_counter_garg(state, player, at_night, has_pool, on_roof):
    return has_instant(state, player, 2, at_night)

def can_counter_snorkel(state, player, at_night, has_pool, on_roof):
    return has_wall(state, player) or has_pult(state, player)

def can_counter_zomboni(state, player, at_night, has_pool, on_roof):
    return has_instant(state, player, 1, at_night) or (state.has("Spikeweed", player) and not on_roof)

def can_counter_balloon(state, player, at_night, has_pool, on_roof):
    return state.has("Cactus", player) or state.has("Blover", player) or (has_pool and state.has("Lily Pad", player) and state.has("Cattail", player))

def can_counter_digger(state, player, at_night, has_pool, on_roof):
    return has_magnet(state, player, at_night) or state.has("Split Pea", player) or (state.has("Starfruit", player) and not on_roof) or (has_pool and state.has("Lily Pad", player) and state.has("Cattail", player)) or (state.has("Spikeweed", player) and has_wall(state, player) and not on_roof)

def can_counter_pogo(state, player, at_night, has_pool, on_roof):
    return has_magnet(state, player, at_night) or state.has("Split Pea", player) or (state.has("Starfruit", player) and not on_roof) or (has_pool and state.has("Lily Pad", player) and state.has("Cattail", player)) or state.has("Tall-nut", player)

def can_counter_peashooter(state, player, at_night, has_pool, on_roof):
    return has_wall(state, player)

def can_counter_wallnut(state, player, at_night, has_pool, on_roof):
    return has_wall(state, player) and has_instant(state, player, 1, at_night)

def has_sun_producer(state, player, at_night, has_pool, on_roof):
    return state.has("Sunflower", player) or (state.has("Sun-shroom", player) and at_night)

def get_cleared_adventure_areas(state, player):
    return state.has("Adventure Level Cleared (Area: Day)", player, 10) + state.has("Adventure Level Cleared (Area: Night)", player, 10) + state.has("Adventure Level Cleared (Area: Pool)", player, 10) + state.has("Adventure Level Cleared (Area: Fog)",  player, 10) + state.has("Adventure Level Cleared (Area: Roof)",  player, 9)

def get_cleared_adventure_levels(state, player):
    return state.count("Adventure Level Cleared (Area: Day)", player) + state.count("Adventure Level Cleared (Area: Night)", player) + state.count("Adventure Level Cleared (Area: Pool)", player) + state.count("Adventure Level Cleared (Area: Fog)",  player) + state.count("Adventure Level Cleared (Area: Roof)",  player)

def get_total_cleared_levels(state, player):
    return get_cleared_adventure_levels(state, player) + state.count("Mini-game Level Cleared", player) + state.count("I, Zombie Level Cleared", player) + state.count("Vasebreaker Level Cleared", player) + state.count("Survival Level Cleared",  player) + state.count("Cloudy Day Level Cleared",  player) + state.count("Bonus Levels Level Cleared",  player)
    
ZOMBIE_COUNTERS = {
    "Balloon": can_counter_balloon,
    "Door": can_counter_screen_door,
    "Football": can_counter_football,
    "Gargantuar": can_counter_garg,
    "Snorkel": can_counter_snorkel,
    "Zamboni": can_counter_zomboni,
    "Ladder": can_counter_ladder,
    "Digger": can_counter_digger,
    "Pogo": can_counter_pogo,
    "PeaHead": can_counter_peashooter,
    "WallnutHead": can_counter_wallnut,
    "GatlingHead": can_counter_peashooter,
    "TallnutHead": can_counter_wallnut,
    "TrashCan": can_counter_screen_door
}

def can_clear_level(state, world, player, level_data, at_night, has_pool, on_roof):

    if level_data["type"] == "minigame" and world.minigame_unlocks[level_data["id"]] > 0:
        if not state.has("Mini-game Level Cleared", player, world.minigame_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "survival" and world.survival_unlocks[level_data["id"]] > 0:
        if not state.has("Survival Level Cleared", player, world.survival_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "puzzle" and level_data["id"] < 80:
        if not state.has("Vasebreaker Level Cleared", player, world.vasebreaker_unlocks[level_data["id"]]):
            return False
    elif level_data["type"] == "puzzle":
        if not state.has("I, Zombie Level Cleared", player, world.izombie_unlocks[level_data["id"]]):
            return False
    elif level_data["name"] == "Roof: Dr. Zomboss":
        if world.fast_goal == False and not state.can_reach_location("Roof: Level 5-9 (Clear)", player):
            return False
        if world.adventure_levels_goal > 0 and get_cleared_adventure_levels(state, player) < world.adventure_levels_goal:
            return False
        if world.adventure_areas_goal > 0 and get_cleared_adventure_areas(state, player) < world.adventure_areas_goal:
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
        if world.overall_levels_goal > 0 and get_total_cleared_levels(state, player) < world.overall_levels_goal:
            return False

    if level_data["choose"]:
        if level_data["type"] == "survival" and not has_wall(state, player):
            return False
        if at_night:
            if level_data["type"] == "survival":
                if not (state.has("Fume-shroom", player)):
                    return False
                if has_pool == False and not (state.has("Grave Buster", player)):
                    return False

            night_required_plants = [state.has("Puff-shroom", player), state.has("Scaredy-shroom", player), state.has("Sun-shroom", player), state.has("Fume-shroom", player)]
            if sum(night_required_plants) < 3:
                return False

        if has_pool:
            if not state.has("Lily Pad", player):
                return False
        if on_roof:
            if not (state.has("Flower Pot", player) and has_pult(state, player)):
                return False
        if (level_data["flags"] > 1 or level_data["location"] != "Day" or level_data["type"] in ["minigame", "survival", "bonus"]) and not has_sun_producer(state, player, at_night, has_pool, on_roof):
            return False
        
        if level_data["name"] == "Bonus Levels: Unsodded" and not (state.has("Threepeater", player) or state.has("Starfruit", player)):
            return False
        elif level_data["name"] == "Bonus Levels: Grave Danger" and not state.has("Grave Buster", player):
            return False
        elif level_data["name"] == "Mini-games: Pogo Party" and not state.has("Roof Cleaners", player):
            return False
        elif level_data["name"] in ["Minigames: Last Stand", "Survival: Day (Hard)", "Survival: Night (Hard)", "Survival: Pool (Hard)", "Survival: Fog (Hard)", "Survival: Roof (Hard)"] and not ((state.has("Fume-shroom", player) and (at_night or state.has("Coffee Bean", player))) or (state.has("Torchwood", player) and (state.has("Threepeater", player) or state.has("Repeater", player))) or (state.has("Melon-pult", player))):
            return False
        elif level_data["name"] == "Mini-games: Bobsled Bonanza" and not state.has("Spikeweed", player):
            return False
            
        for zombie in level_data["zombies"]:
            if zombie in ZOMBIE_COUNTERS:
                if not ZOMBIE_COUNTERS[zombie](state, player, at_night, has_pool, on_roof):
                    return False

    return True


def set_rules(world: World) -> None:

    player = world.player
    multiworld = world.multiworld

    multiworld.completion_condition[player] = lambda state: state.has("Music Video", player)

    for x in range(8, world.options.shop_items.value):
        amount_of_restocks_required = int(x / 8)
        multiworld.get_location(f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}", player).access_rule = lambda state, req=amount_of_restocks_required: (state.has("Twiddydinkies Restock", player, req))