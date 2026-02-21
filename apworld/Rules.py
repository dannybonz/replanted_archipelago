from worlds.AutoWorld import World
from .Data import ALL_PLANTS, LEVEL_LOCATIONS, ZOMBIE_TYPES
import itertools

expected_level_loadouts = {}

def create_plant_combinations_for_level(world, level_data):
    location_data = LEVEL_LOCATIONS[level_data["location"]]
    at_night = location_data["at_night"]
    has_pool = location_data["has_pool"]
    on_roof = location_data["on_roof"]

    possible_combinations = {}

    #Attackers
    if not on_roof:
        possible_combinations["attacker"] = [["Peashooter"], ["Chomper"], ["Snow Pea"], ["Repeater"], ["Split Pea"], ["Cactus"], ["Cabbage-pult"], ["Kernel-pult"], ["Starfruit"]]
        if at_night:
            possible_combinations["attacker"].append(["Fume-shroom"])
    else:
        possible_combinations["attacker"] = [["Cabbage-pult"], ["Kernel-pult"], ["Melon-pult"]]
        if world.options.easy_upgrade_plants.value:
            possible_combinations["attacker"].append(["Winter Melon"])

    #Cloudy Day attackers
    if level_data["type"] == "cloudy":
        possible_combinations["cloudy"] = [["Peashooter"], ["Snow Pea"], ["Repeater"], ["Cactus"], ["Cabbage-pult"], ["Kernel-pult"]]
    
    #Sun producers
    if level_data["choose"] and (level_data["type"] != "adventure" or level_data["flags"] > 1):
        if at_night:
            possible_combinations["sun"] = [["Sun-shroom"]]
        else:
            possible_combinations["sun"] = [["Sunflower"]]

    #Wall plants
    if level_data["type"] == "survival" or level_data["name"] in ["Roof: Level 5-5", "Mini-games: Column Like You See 'Em"] or any(zombie in level_data["zombies"] for zombie in ["PeaHead", "GatlingHead", "TallnutHead"]):
        possible_combinations["wall"] = [["Wall-nut"], ["Tall-nut"], ["Pumpkin"]]

    #AOE plants
    if level_data["type"] == "survival" or level_data["name"] in ["Mini-games: Last Stand", "Mini-games: Column Like You See 'Em"]  or "GigaGargantuar" in level_data["zombies"]:
        possible_combinations["aoe"] = [["Repeater", "Torchwood"], ["Threepeater", "Torchwood"], ["Melon-pult"]]
        if world.options.easy_upgrade_plants.value:
            possible_combinations["aoe"].append(["Winter Melon"])
        if at_night:
            possible_combinations["aoe"].append(["Fume-shroom"])
        elif not "conveyor" in level_data:
            possible_combinations["aoe"].append(["Fume-shroom", "Coffee Bean"])

    #Night plants
    if at_night and level_data["choose"]:
        possible_combinations["night"] = [["Puff-shroom", "Fume-shroom"], ["Scaredy-shroom", "Fume-shroom"], ["Puff-shroom", "Scaredy-shroom"]]

    #Multi-lane
    if level_data["name"] == "Bonus Levels: Unsodded":
        possible_combinations["lanes"] = [["Threepeater"], ["Starfruit"]]

    #Balloon
    if "Balloon" in level_data["zombies"]:
        possible_combinations["balloon"] = [["Cactus"], ["Blover"]]
        if has_pool:
            possible_combinations["balloon"].append(["Cattail"])

    #Shields
    if any(zombie in level_data["zombies"] for zombie in ["ScreenDoor", "Ladder", "TrashCan"]):
        possible_combinations["shield"] = [["Cabbage-pult"], ["Kernel-pult"]]
        if at_night:
            possible_combinations["shield"] += [["Fume-shroom"], ["Magnet-shroom"]]
        elif not "conveyor" in level_data:
            possible_combinations["shield"] += [["Fume-shroom", "Coffee Bean"], ["Magnet-shroom", "Coffee Bean"]]

    #Digger
    if "Digger" in level_data["zombies"]:
        possible_combinations["digger"] = [["Starfruit"], ["Split Pea"]]
        if has_pool:
            possible_combinations["digger"].append(["Cattail"])
        if at_night:
            possible_combinations["digger"].append(["Magnet-shroom"])
        elif not "conveyor" in level_data:
            possible_combinations["digger"].append(["Magnet-shroom", "Coffee Bean"])

    #Snorkel
    if "Snorkel" in level_data["zombies"]:
        possible_combinations["snorkel"] = [["Cabbage-pult"], ["Kernel-pult"], ["Melon-pult"], ["Wall-nut"], ["Tall-nut"], ["Pumpkin"]]
        if world.options.easy_upgrade_plants.value:
            possible_combinations["snorkel"].append(["Winter Melon"])

    #Pogo
    if "Pogo" in level_data["zombies"]:
        possible_combinations["pogo"] = [["Split Pea"], ["Starfruit"], ["Tall-nut"]]
        if at_night:
            possible_combinations["pogo"].append(["Magnet-shroom"])
        elif not "conveyor" in level_data:
            possible_combinations["pogo"].append(["Magnet-shroom", "Coffee Bean"])
        if has_pool:
            possible_combinations["pogo"].append(["Cattail"])

    #Football
    if any(zombie in level_data["zombies"] for zombie in ["Football"]):
        possible_combinations["football"] = [["Cherry Bomb"], ["Squash"], ["Jalapeno"], ["Wall-nut"], ["Tall-nut"], ["Pumpkin"]]
        if at_night:
            possible_combinations["football"] += [["Magnet-shroom"], ["Hypno-shroom"]]
        elif not "conveyor" in level_data:
            possible_combinations["football"] += [["Magnet-shroom", "Coffee Bean"], ["Hypno-shroom", "Coffee Bean"]]

    #Zomboni
    if any(zombie in level_data["zombies"] for zombie in ["Zomboni"]):
        possible_combinations["zomboni"] = [["Cherry Bomb"], ["Squash"], ["Jalapeno"]]
        if not on_roof:
            possible_combinations["zomboni"].append(["Spikeweed"])

    #Gargantuar
    if any(zombie in level_data["zombies"] for zombie in ["Gargantuar", "GigaGargantuar"]):
        possible_combinations["garg"] = [["Cherry Bomb", "Squash"], ["Squash", "Jalapeno"], ["Jalapeno", "Cherry Bomb"]]

    #Little
    if "special" in level_data and level_data["special"] in ["little"]:
        possible_combinations["little"] = [["Cherry Bomb"], ["Jalapeno"]]

    return possible_combinations

def eligible_zombies_from_plants(world, level_data, given_plants): #Used for Zombie rando when Conveyor Rando is disabled. Given a list of plants, it spits out what Zombies you can theoretically win against
    level_data["zombies"] = ZOMBIE_TYPES #All Zombies
    potential_threats = create_plant_combinations_for_level(world, level_data)

    threat_to_zombie_name = {"balloon": ["Balloon"], "football": ["Football"], "zomboni": ["Zomboni"], "digger": ["Digger"], "pogo": ["Pogo"], "garg": ["Gargantuar"], "wall": ["PeaHead", "GatlingHead", "TallnutHead"]}
    eligible_zombies = ["Normal", "Flag", "Conehead", "Polevaulter", "Buckethead", "Newspaper", "Dancer", "DolphinRider", "JackInTheBox", "Digger", "Bungee", "Catapult", "JalapenoHead", "SquashHead"]
    covered_threats = []

    for threat in potential_threats:
        for counter_combo in potential_threats[threat]:
            if all(plant in given_plants for plant in counter_combo):
                covered_threats.append(threat)
                if threat in threat_to_zombie_name:
                    eligible_zombies += threat_to_zombie_name[threat]
                break

    if "aoe" in covered_threats and "garg" in covered_threats:
        eligible_zombies.append("GigaGargantuar")

    if "shield" in covered_threats:
        eligible_zombies += ["Ladder", "ScreenDoor", "TrashCan"]

    return eligible_zombies

def can_clear_level(state, world, player, level_data):
    if level_data["choose"]:
        location_data = LEVEL_LOCATIONS[level_data["location"]]
        at_night = location_data["at_night"]
        has_pool = location_data["has_pool"]
        on_roof = location_data["on_roof"]

        #Non-negotiables
        forced_items = []
        if has_pool:
            forced_items.append("Lily Pad")
        if on_roof or location_data["china"]:
            forced_items.append("Flower Pot")
        if on_roof and (level_data["name"] == "Mini-games: Pogo Party" or "GigaGargantuar" in level_data["zombies"]):
            forced_items.append("Roof Cleaners")
        if level_data["name"] == "Bonus Levels: Grave Danger" or (level_data["type"] == "survival" and at_night and not has_pool):
            forced_items.append("Grave Buster")
        if level_data["name"] == "Mini-games: Bobsled Bonanza" and "Zomboni" in level_data["zombies"]:
            forced_items.append("Spikeweed")

        if not all(state.has(item, player) for item in forced_items):
            return False

        forced_plants = [item for item in forced_items if item in ALL_PLANTS]

        if "forced" in level_data:
            forced_plants += level_data["forced"]

        #Remaining seed slots
        number_of_seed_slots = state.count("Extra Seed Slot", player) + 1
        if level_data["type"] == "survival":
            number_of_seed_slots *= 2
        usable_slots = number_of_seed_slots - len(forced_plants)
        if usable_slots < 0:
            return False

        #Build possible loadouts
        possible_combinations = create_plant_combinations_for_level(world, level_data)

        unlocked_plants = {plant for plant in ALL_PLANTS if state.has(plant, player)}
        choosable_combinations = {}
        for req, combos in possible_combinations.items():
            valid = [combo for combo in combos if all(p in unlocked_plants for p in combo)]
            if not valid:
                return False #Don't have the plants unlocked in order to meet this requirement
            choosable_combinations[req] = valid

        selected_plants = set(forced_plants)
        for req in sorted(choosable_combinations, key=lambda r: len(choosable_combinations[r])):
            combos = sorted(choosable_combinations[req], key=lambda combo: len([p for p in combo if p not in selected_plants])) #Try to re-use plants where possible
            for combo in combos:
                new_plants = [p for p in combo if p not in selected_plants]
                if len(selected_plants) + len(new_plants) <= number_of_seed_slots:
                    selected_plants.update(combo)
                    break
            else: #You cannot fit the requirement in
                return False

        expected_level_loadouts[level_data['name']] = selected_plants
    return True

def set_rules(world: World) -> None:

    player = world.player
    multiworld = world.multiworld

    multiworld.completion_condition[player] = lambda state: state.has("Music Video", player)

    for x in range(8, world.options.shop_items.value):
        amount_of_restocks_required = int(x / 8)
        multiworld.get_location(f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}", player).access_rule = lambda state, req=amount_of_restocks_required: (state.has("Twiddydinkies Restock", player, req))