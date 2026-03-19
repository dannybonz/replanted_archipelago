import copy
from dataclasses import dataclass, field
from .Items import plant_names
from itertools import combinations

@dataclass
class Level:
    name: str
    clear_location_id: int
    level_id: int
    unlock_item_name: str
    choose: bool = True
    location: str = "Day"
    zombies: list[str] = field(default_factory = list)
    flags: int = 0
    type: str = "Adventure"
    flag_location_ids: list[int] = field(default_factory = list)
    conveyor_default: int = 0
    forced_plants: set[str] = field(default_factory = set)

    special: str | None = None
    conveyor: dict | None = None

    at_night: bool = False
    has_pool: bool = False
    on_roof: bool = False
    on_ceramic: bool = False

    expected_loadout: list[str] = None
    plant_combinations = None

    def __post_init__(self):
        self.has_pool = self.location == "Pool" or self.location == "Fog"
        self.on_roof = self.location == "Roof" or self.location == "Night Roof"
        self.at_night = self.location == "Night" or self.location == "Fog"
        self.on_ceramic = self.location == "Roof" or self.location == "Night Roof" or self.location == "China"
        self.unmodified = copy.deepcopy(self)

    def randomise_zombies(self, world, zombie_blacklist):
        unmodified_zombies = copy.deepcopy(self.unmodified.zombies)
        possible_zombies = [zombie_type for zombie_type in sorted(world.all_zombies.keys()) if zombie_type not in zombie_blacklist and (self.has_pool or not world.all_zombies[zombie_type].aquatic)]

        if self.conveyor != None and world.options.conveyor_randomisation.value == 0: #If this is a Conveyor level, and Conveyor rando is NOT enabled - consider which plants are available in the level
            eligible_zombies = self.eligible_zombies_for_loadout(world, self.conveyor) + unmodified_zombies
            possible_zombies = [zombie for zombie in possible_zombies if zombie in eligible_zombies]
        elif self.level_id <= 4: #First four levels forced to have basic logic
            eligible_zombies = self.eligible_zombies_for_loadout(world, world.starting_plants)
            possible_zombies = [zombie for zombie in possible_zombies if zombie in eligible_zombies]

        level_banned_zombies = []
        if self.special == "bowling":
            level_banned_zombies += ["Bungee", "Balloon"]
        elif self.name == "Mini-games: Column Like You See 'Em":
            level_banned_zombies += ["Balloon"]
        elif self.name == "Roof: Level 5-5":
            level_banned_zombies += ["Digger", "Balloon", "Pogo", "Zomboni"]
        elif self.name in ["Mini-games: Bobsled Bonanza", "Mini-games: Pogo Party", "Bonus Levels: Air Raid"]:
            level_banned_zombies += ["GigaGargantuar"]
        elif self.name == "Mini-games: It's Raining Seeds":
            level_banned_zombies += ["Balloon"]

        for zombie in level_banned_zombies:
            if zombie in possible_zombies:
                possible_zombies.remove(zombie)

        world.random.shuffle(possible_zombies)
        
        new_zombies = [zombie_type for zombie_type in unmodified_zombies if zombie_type in zombie_blacklist]
        new_zombies += possible_zombies[:len(unmodified_zombies) - len(new_zombies)]

        if self.name in ["Mini-games: ZomBotany", "Mini-games: ZomBotany 2"] and not "PeaHead" in new_zombies: #ZomBotany levels require PeaHead
            new_zombies[0] = "PeaHead"

        if "Zomboni" in new_zombies:
            new_zombies.append("Bobsled")

        self.zombies = new_zombies

    def randomise_conveyor(self, world):
        #Look at zombies in the level and use plant loadouts that counter them
        possible_threat_counters = self.create_plant_combinations(world)
        plants_to_use = []
        for threat_name in possible_threat_counters:
            possible_plants = possible_threat_counters[threat_name]
            plants_to_use += world.random.choice(possible_threat_counters[threat_name])
        plants_to_use = sorted(set(plants_to_use))

        #Ban plants from being selected
        banned_plants = ["Sunflower", "Sun-shroom", "Coffee Bean", "Imitater", "Plantern", "Lily Pad", "Flower Pot", "Twin Sunflower"]
        if self.on_ceramic:
            banned_plants += ["Spikeweed", "Spikerock"]
        if self.name == "Mini-games: Portal Combat":
            banned_plants += ["Starfruit", "Split Pea"]
        plants_to_use = [plant for plant in plants_to_use if plant not in banned_plants]

        included_attackers = [plant_name for plant_name in plants_to_use if plant_name in world.conveyor_attackers]

        if len(included_attackers) == 0: #If you somehow got this far without an appropriate attacking plant, add one
            available_attackers = [plant for plant in world.conveyor_attackers]
            if not self.at_night:
                available_attackers = [plant for plant in available_attackers if not "-shroom" in plant]
            if self.on_roof:
                available_attackers = [plant for plant in available_attackers if "-pult" in plant]
            included_attackers.append(world.random.choice(available_attackers))

        conveyor_weights = {}
        conveyor_weights[included_attackers[0]] = 15 #Set your primary attacker weight to 15
        for attacker in included_attackers[1:]:
            conveyor_weights[attacker] = world.random.randint(5,17) #Set any remaining attackers to a random weight

        for plant in plants_to_use:
            if not plant in conveyor_weights: #Plants that haven't already had their weights decided
                conveyor_weights[plant] = world.random.randint(5, 10) #Set any remaining plants to a random weight, capped lower than your main attacker

        if self.has_pool:
            conveyor_weights["Lily Pad"] = world.random.randint(28, 32)
        if self.on_ceramic:
            conveyor_weights["Flower Pot"] = world.random.randint(48, 52)
        if self.at_night and not (self.has_pool or self.on_roof):
            conveyor_weights["Grave Buster"] = world.random.randint(15, 20)

        if self.special == "boss":
            conveyor_weights["Flower Pot"] = world.random.randint(54, 56)
            conveyor_weights["Ice-shroom"] = world.random.randint(7, 9)
            conveyor_weights["Jalapeno"] = world.random.randint(11, 13)
        elif self.special == "column":
            conveyor_weights["Flower Pot"] = 155
            conveyor_weights["Jalapeno"] = world.random.randint(12, 17)

        if len(self.conveyor) - len(conveyor_weights) >= 1: #If there are more slots to add plants, add an instant/wall
            handy_conveyor_plants = ["Squash", "Cherry Bomb", "Jalapeno", "Wall-nut", "Pumpkin", "Tall-nut"]
            if self.at_night:
                handy_conveyor_plants += ["Ice-shroom", "Doom-shroom", "Hypno-shroom"]
            if self.special == "boss":
                handy_conveyor_plants = ["Melon-pult", "Kernel-pult", "Cabbage-pult"]
                if world.options.easy_upgrade_plants.value:
                    handy_conveyor_plants.append("Winter Melon")

            handy_conveyor_plants = [plant for plant in handy_conveyor_plants if not plant in conveyor_weights]
            if len(handy_conveyor_plants) > 0:
                conveyor_weights[world.random.choice(handy_conveyor_plants)] = world.random.randint(5,10)

            if len(self.conveyor) - len(conveyor_weights) >= 1: #Until the conveyor is full, add random plants (with some restrictions)
                possible_plants = []
                for plant_name in sorted(world.all_plants.keys()):
                    plant_stats = world.all_plants[plant_name]
                    if not ((plant_name in banned_plants + list(conveyor_weights.keys())) or
                        (plant_stats.upgrades_from != "None" and not world.options.easy_upgrade_plants.value) or
                        (plant_stats.nocturnal and not self.at_night) or
                        (plant_stats.aquatic and not self.has_pool) or
                        (plant_name == "Umbrella Leaf" and not ("Bungee" in self.zombies or "Catapult" in self.zombies)) or
                        (plant_name == "Blover" and "Balloon" not in self.zombies) or
                        (plant_name == "Grave Buster" and not (self.at_night and not (self.has_pool or self.on_roof))) or
                        (plant_name == "Torchwood" and not ("Peashooter" in conveyor_weights or "Repeater" in conveyor_weights or "Threepeater" in conveyor_weights))):
                        possible_plants.append(plant_name)
                world.random.shuffle(possible_plants)
                    
                while (len(self.conveyor) - len(conveyor_weights) >= 1) and len(possible_plants) > 1:
                    conveyor_weights[possible_plants.pop()] = world.random.randint(1,5) #Low weight on these random plants

        self.conveyor = conveyor_weights

    def can_clear(self, state, world, player):
        #Non-negotiables
        required_items = []
        if self.on_roof and (self.name in ["Mini-games: Pogo Party", "Mini-games: Column Like You See 'Em"] or "GigaGargantuar" in self.zombies):
            required_items.append("Roof Cleaners")
        if self.name in ["Mini-games: Pogo Party", "Mini-games: Bobsled Bonanza", "Bonus Levels: Air Raid"] and "Gargantuar" in self.zombies:
            required_items.append("Shovel")

        if not all(state.has(item, player) for item in required_items):
            return False

        if self.choose:
            #Get relevant unlocked plants
            if self.plant_combinations == None:
                self.plant_combinations = self.create_plant_combinations(world)
            unlocked_plants = {plant for plant in world.progression_plants if (state.has(plant, player) or plant in self.forced_plants)}

            #Get unlocked combinations to counter threat
            unlocked_combinations = {}
            for threat in sorted(self.plant_combinations):
                unlocked_combinations_for_threat = [combination for combination in self.plant_combinations[threat] if combination.issubset(unlocked_plants)]
                if not unlocked_combinations_for_threat: #Cannot counter this threat
                    return False
                unlocked_combinations[threat] = unlocked_combinations_for_threat

            #Count seed slots
            number_of_seed_slots = state.count("Extra Seed Slot", player) + 1
            if self.type == "Survival":
                number_of_seed_slots *= 2

            #Create a loadout from unlocked combinations while considering seed slot count
            selected_plants = set()
            for req in sorted(unlocked_combinations, key=lambda r: len(unlocked_combinations[r])):
                combos = sorted(unlocked_combinations[req], key=lambda combo: (len([p for p in combo if p not in selected_plants]), sorted(combo))) #Try to re-use plants where possible
                for combo in combos:
                    new_plants = [p for p in combo if p not in selected_plants]
                    if len(selected_plants) + len(new_plants) <= number_of_seed_slots:
                        selected_plants.update(combo)
                        break
                else: #Out of seed slots
                    return False
            self.expected_loadout = selected_plants
        return True

    def create_plant_combinations(self, world):
        possible_combinations = {}

        #Forced
        if self.forced_plants:
            possible_combinations["forced"] = [self.forced_plants]

        #Attackers
        if not self.on_roof:
            possible_combinations["attacker"] = [{"Peashooter"}, {"Chomper"}, {"Snow Pea"}, {"Repeater"}, {"Split Pea"}, {"Cactus"}, {"Cabbage-pult"}, {"Kernel-pult"}, {"Starfruit"}]
            if self.at_night:
                possible_combinations["attacker"].append({"Fume-shroom"})
        else:
            possible_combinations["attacker"] = [{"Cabbage-pult"}, {"Kernel-pult"}, {"Melon-pult"}]
            if world.options.easy_upgrade_plants.value:
                possible_combinations["attacker"].append({"Winter Melon"})

        #Lily Pad
        if self.has_pool:
            possible_combinations["pool"] = [{"Lily Pad"}]
            #Hard Difficulty - allow cattail or seashroom?

        #Flower Pot
        if self.on_ceramic:
            possible_combinations["flowerpot"] = [{"Flower Pot"}]

        #Cloudy Day attackers
        if self.type == "Cloudy Day":
            possible_combinations["Cloudy Day"] = [{"Peashooter"}, {"Snow Pea"}, {"Repeater"}, {"Cactus"}, {"Cabbage-pult"}, {"Kernel-pult"}]
        
        #Sun producers
        if self.choose and (self.type != "Adventure" or self.flags > 1 or self.at_night) and not self.name in ["Mini-games: Last Stand"]:
            if self.at_night:
                possible_combinations["sun"] = [{"Sun-shroom"}]
            else:
                possible_combinations["sun"] = [{"Sunflower"}]

        #Wall plants
        if self.type == "Survival" or self.name in ["Roof: Level 5-5", "Mini-games: Column Like You See 'Em"] or any(zombie in self.zombies for zombie in ["PeaHead", "GatlingHead", "TallnutHead"]):
            possible_combinations["wall"] = [] + world.wall_plants

        #AOE plants
        if self.type == "Survival" or self.name in ["Mini-games: Last Stand", "Mini-games: Column Like You See 'Em"]  or "GigaGargantuar" in self.zombies:
            possible_combinations["aoe"] = [{"Melon-pult"}]
            if not (self.on_roof and self.conveyor == None):
                possible_combinations["aoe"] += [{"Repeater", "Torchwood"}, {"Threepeater", "Torchwood"}]
            if world.options.easy_upgrade_plants.value:
                possible_combinations["aoe"].append({"Winter Melon"})
            if self.at_night:
                possible_combinations["aoe"].append({"Fume-shroom"})
            elif self.conveyor == None:
                possible_combinations["aoe"].append({"Fume-shroom", "Coffee Bean"})

        #Night plants
        if self.at_night and self.choose:
            possible_combinations["night"] = [{"Puff-shroom", "Fume-shroom"}, {"Scaredy-shroom", "Fume-shroom"}, {"Puff-shroom", "Scaredy-shroom"}]

        #Multi-lane
        if self.name == "Bonus Levels: Unsodded":
            possible_combinations["lanes"] = [{"Threepeater"}, {"Starfruit"}]
        
        #Bomb
        if self.name == "Bonus Levels: Unsodded" and self.conveyor == None:
            possible_combinations["bomb"] = [{"Cherry Bomb"}, {"Doom-shroom", "Coffee Bean"}, {"Ice-shroom", "Coffee Bean"}]

        #Balloon
        if "Balloon" in self.zombies:
            possible_combinations["balloon"] = [{"Cactus"}, {"Blover"}]
            if self.has_pool:
                possible_combinations["balloon"].append({"Cattail"})

        #Shields
        if any(zombie in self.zombies for zombie in ["ScreenDoor", "Ladder"]):
            possible_combinations["shield"] = [{"Cabbage-pult"}, {"Kernel-pult"}]
            if self.at_night:
                possible_combinations["shield"] += [{"Fume-shroom"}, {"Magnet-shroom"}]
            elif self.conveyor == None:
                possible_combinations["shield"] += [{"Fume-shroom", "Coffee Bean"}, {"Magnet-shroom", "Coffee Bean"}]

        #Digger
        if "Digger" in self.zombies:
            possible_combinations["digger"] = [{"Starfruit"}, {"Split Pea"}]
            if self.has_pool:
                possible_combinations["digger"].append({"Cattail"})
            if self.at_night:
                possible_combinations["digger"].append({"Magnet-shroom"})
            elif self.conveyor == None:
                possible_combinations["digger"].append({"Magnet-shroom", "Coffee Bean"})

        #Snorkel
        if "Snorkel" in self.zombies:
            possible_combinations["snorkel"] = [{"Cabbage-pult"}, {"Kernel-pult"}, {"Melon-pult"}] + world.wall_plants
            if world.options.easy_upgrade_plants.value:
                possible_combinations["snorkel"].append({"Winter Melon"})

        #Pogo
        if "Pogo" in self.zombies:
            possible_combinations["pogo"] = [{"Split Pea"}, {"Starfruit"}, {"Tall-nut"}]
            if self.at_night:
                possible_combinations["pogo"].append({"Magnet-shroom"})
            elif self.conveyor == None:
                possible_combinations["pogo"].append({"Magnet-shroom", "Coffee Bean"})
            if self.has_pool:
                possible_combinations["pogo"].append({"Cattail"})

        #Football
        if any(zombie in self.zombies for zombie in ["Football"]):
            possible_combinations["football"] = [] + world.wall_plants
            if self.at_night:
                possible_combinations["football"] += [{"Magnet-shroom"}, {"Hypno-shroom"}]
            elif self.conveyor == None:
                possible_combinations["football"] += [{"Magnet-shroom", "Coffee Bean"}, {"Hypno-shroom", "Coffee Bean"}]
            if not self.name in ["Mini-games: Last Stand"]:
                possible_combinations["football"] += [{"Cherry Bomb"}, {"Squash"}, {"Jalapeno"}]

        #Magnet
        if self.name in ["Bonus Levels: Unsodded"] and any(zombie in self.zombies for zombie in ["Football", "Buckethead", "Pogo", "ScreenDoor"]) and self.conveyor == None:        
            possible_combinations["magnet"] = [{"Magnet-shroom", "Coffee Bean"}]

        #Zomboni
        if any(zombie in self.zombies for zombie in ["Zomboni"]):
            possible_combinations["zomboni"] = [{"Cherry Bomb"}, {"Squash"}, {"Jalapeno"}]
            if not self.on_roof:
                possible_combinations["zomboni"].append({"Spikeweed"})
                if world.options.easy_upgrade_plants.value:
                    possible_combinations["zomboni"].append({"Spikerock"})

        #Gargantuar
        if any(zombie in self.zombies for zombie in ["Gargantuar", "GigaGargantuar"]):
            if not self.name in ["Mini-games: Last Stand"]:       
                possible_combinations["garg"] = [{"Cherry Bomb", "Squash"}, {"Squash", "Jalapeno"}, {"Jalapeno", "Cherry Bomb"}]

            #Garg bonanza - overwrites the standard garg requirement
            if self.name in ["Mini-games: Pogo Party", "Mini-games: Bobsled Bonanza", "Bonus Levels: Air Raid"]:
                if self.at_night:
                    possible_combinations["garg"] = [{"Ice-shroom", "Sun-shroom", "Doom-shroom", "Kernel-pult", "Potato Mine"}]
                else:
                    possible_combinations["garg"] = [{"Ice-shroom", "Doom-shroom", "Kernel-pult", "Potato Mine"}]

        #Cherry Bomb/Jalapeno guarantee
        if (self.special in ["little"]) or self.name in ["Mini-games: Column Like You See 'Em"]:
            possible_combinations["little"] = [{"Cherry Bomb"}, {"Jalapeno"}]

        #Insta-kill for Roof: Level 5-5
        if self.name == "Roof: Level 5-5":
            possible_combinations["bungeeblitz"] = [{"Cherry Bomb"}, {"Jalapeno"}, {"Squash"}, {"Chomper"}]

        #Bungee
        if self.name in ["Mini-games: Bobsled Bonanza", "Mini-games: Pogo Party", "Bonus Levels: Air Raid"] and "Bungee" in self.zombies:
            possible_combinations["bungee_bonanza"] = [{"Umbrella Leaf"}]
        #Grave Buster
        if self.name == "Bonus Levels: Grave Danger" or (self.type == "Survival" and self.at_night and not self.has_pool):
            possible_combinations["grave"] = [{"Grave Buster"}]

        #Spikeweed
        if self.name in ["Mini-games: Bobsled Bonanza", "Mini-games: Pogo Party", "Bonus Levels: Air Raid"] and "Zomboni" in self.zombies:
            possible_combinations["spikeweed"] = [{"Spikeweed"}]
            if world.options.easy_upgrade_plants.value:
                possible_combinations["spikeweed"] += [{"Spikerock"}]

        if world.usable_plants != []:
            if self.conveyor == None: #Ensure only progression items are used for rule building (generally only needed for seed stat rando as plants can lose their progression status)
                for threat in possible_combinations:
                    possible_combinations[threat] = [combo for combo in possible_combinations[threat] if all(plant in world.progression_plants for plant in combo)]
            elif world.options.plant_stat_randomisation.value != 0: #Tries to only use actually good plants when building conveyor loadouts
                for threat in possible_combinations:
                    possible_combinations[threat] = [combo for combo in possible_combinations[threat] if all(plant in world.usable_plants for plant in combo)]

        return possible_combinations

    def eligible_zombies_for_loadout(self, world, plant_loadout): #Used for Zombie rando when Conveyor Rando is disabled. Given a list of plants, it spits out what Zombies you can theoretically win against
        copied_level_data = copy.deepcopy(self) #Make a copy of this level so that we can mess about with it
        copied_level_data.zombies = sorted(world.all_zombies.keys()) #Change the copy to have all zombies in the game - this list will then be wittled down
        potential_threats = copied_level_data.create_plant_combinations(world) 

        threat_to_zombie_name = {"balloon": ["Balloon"], "football": ["Football"], "zomboni": ["Zomboni"], "digger": ["Digger"], "pogo": ["Pogo"], "garg": ["Gargantuar"], "wall": ["PeaHead", "GatlingHead", "TallnutHead"]} #These Zombies become in-logic so long as this one specific threat is covered
        eligible_zombies = ["Normal", "Flag", "Conehead", "Polevaulter", "Buckethead", "Newspaper", "Dancer", "DolphinRider", "JackInTheBox", "Digger", "Bungee", "Catapult", "JalapenoHead", "SquashHead", "TrashCan"] #These zombies have 0 logic attached to them
        covered_threats = []

        for threat in potential_threats:
            for counter_combo in potential_threats[threat]:
                if all(plant in plant_loadout for plant in counter_combo):
                    covered_threats.append(threat)
                    if threat in threat_to_zombie_name:
                        eligible_zombies += threat_to_zombie_name[threat]
                    break

        if "aoe" in covered_threats and "garg" in covered_threats:
            eligible_zombies.append("GigaGargantuar")

        if "shield" in covered_threats:
            eligible_zombies += ["Ladder", "ScreenDoor"]

        return eligible_zombies

def create_levels(world = None):
    levels = {
        "1-1": Level(
            name = "Day: Level 1-1",
            zombies = ['Normal', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1000,
            unlock_item_name = "Day Unlock: Level 1-1",
            level_id = 1
        ),

        "1-2": Level(
            name = "Day: Level 1-2",
            flags = 1,
            zombies = ['Normal', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1001,
            unlock_item_name = "Day Unlock: Level 1-2",
            level_id = 2
        ),

        "1-3": Level(
            name = "Day: Level 1-3",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1002,
            unlock_item_name = "Day Unlock: Level 1-3",
            level_id = 3
        ),

        "1-4": Level(
            name = "Day: Level 1-4",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1003,
            unlock_item_name = "Day Unlock: Level 1-4",
            level_id = 4
        ),

        "1-5": Level(
            name = "Day: Level 1-5",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Flag'],
            choose = False,
            flag_location_ids = [],
            clear_location_id = 1004,
            special = "bowling",
            unlock_item_name = "Day Unlock: Level 1-5",
            level_id = 5
        ),

        "1-6": Level(
            name = "Day: Level 1-6",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1005,
            unlock_item_name = "Day Unlock: Level 1-6",
            level_id = 6
        ),

        "1-7": Level(
            name = "Day: Level 1-7",
            flags = 2,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Flag'],
            flag_location_ids = [2000],
            clear_location_id = 1006,
            unlock_item_name = "Day Unlock: Level 1-7",
            level_id = 7
        ),

        "1-8": Level(
            name = "Day: Level 1-8",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1007,
            unlock_item_name = "Day Unlock: Level 1-8",
            level_id = 8
        ),

        "1-9": Level(
            name = "Day: Level 1-9",
            flags = 2,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Flag'],
            flag_location_ids = [2001],
            clear_location_id = 1008,
            unlock_item_name = "Day Unlock: Level 1-9",
            level_id = 9
        ),

        "1-10": Level(
            name = "Day: Level 1-10",
            flags = 2,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Flag'],
            choose = False,
            flag_location_ids = [2002],
            clear_location_id = 1009,
            conveyor = {'Peashooter': 20, 'Cherry Bomb': 20, 'Wall-nut': 15, 'Repeater': 20, 'Snow Pea': 10, 'Chomper': 5, 'Potato Mine': 10},
            unlock_item_name = "Day Unlock: Level 1-10",
            level_id = 10
        ),

        "2-1": Level(
            name = "Night: Level 2-1",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Newspaper', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1010,
            unlock_item_name = "Night Unlock: Level 2-1",
            level_id = 11
        ),

        "2-2": Level(
            name = "Night: Level 2-2",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Newspaper', 'Flag'],
            flag_location_ids = [2003],
            clear_location_id = 1011,
            unlock_item_name = "Night Unlock: Level 2-2",
            level_id = 12
        ),

        "2-3": Level(
            name = "Night: Level 2-3",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1012,
            unlock_item_name = "Night Unlock: Level 2-3",
            level_id = 13
        ),

        "2-4": Level(
            name = "Night: Level 2-4",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'ScreenDoor', 'Flag'],
            flag_location_ids = [2004],
            clear_location_id = 1013,
            unlock_item_name = "Night Unlock: Level 2-4",
            level_id = 14
        ),

        "2-5": Level(
            name = "Night: Level 2-5",
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'Flag'],
            choose = False,
            flag_location_ids = [],
            clear_location_id = 1014,
            special = "whack",
            unlock_item_name = "Night Unlock: Level 2-5",
            level_id = 15
        ),

        "2-6": Level(
            name = "Night: Level 2-6",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1015,
            unlock_item_name = "Night Unlock: Level 2-6",
            level_id = 16
        ),

        "2-7": Level(
            name = "Night: Level 2-7",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag'],
            flag_location_ids = [2005],
            clear_location_id = 1016,
            unlock_item_name = "Night Unlock: Level 2-7",
            level_id = 17
        ),

        "2-8": Level(
            name = "Night: Level 2-8",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1017,
            unlock_item_name = "Night Unlock: Level 2-8",
            level_id = 18
        ),

        "2-9": Level(
            name = "Night: Level 2-9",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag'],
            flag_location_ids = [2006],
            clear_location_id = 1018,
            unlock_item_name = "Night Unlock: Level 2-9",
            level_id = 19
        ),

        "2-10": Level(
            name = "Night: Level 2-10",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag', 'Football', 'Dancer'],
            choose = False,
            flag_location_ids = [2007],
            clear_location_id = 1019,
            conveyor = {'Grave Buster': 20, 'Ice-shroom': 15, 'Doom-shroom': 15, 'Hypno-shroom': 10, 'Scaredy-shroom': 15, 'Fume-shroom': 15, 'Puff-shroom': 10},
            unlock_item_name = "Night Unlock: Level 2-10",
            level_id = 20
        ),

        "3-1": Level(
            name = "Pool: Level 3-1",
            flags = 1,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1020,
            unlock_item_name = "Pool Unlock: Level 3-1",
            level_id = 21
        ),

        "3-2": Level(
            name = "Pool: Level 3-2",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [2008],
            clear_location_id = 1021,
            unlock_item_name = "Pool Unlock: Level 3-2",
            level_id = 22
        ),

        "3-3": Level(
            name = "Pool: Level 3-3",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'Snorkel', 'Flag'],
            flag_location_ids = [2009],
            clear_location_id = 1022,
            unlock_item_name = "Pool Unlock: Level 3-3",
            level_id = 23
        ),

        "3-4": Level(
            name = "Pool: Level 3-4",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Snorkel', 'Flag'],
            flag_location_ids = [2010, 2011],
            clear_location_id = 1023,
            unlock_item_name = "Pool Unlock: Level 3-4",
            level_id = 24
        ),

        "3-5": Level(
            name = "Pool: Level 3-5",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Snorkel', 'Flag', 'Football'],
            choose = False,
            flag_location_ids = [2012],
            clear_location_id = 1024,
            conveyor = {'Lily Pad': 25, 'Wall-nut': 15, 'Peashooter': 25, 'Cherry Bomb': 35},
            special = "little",
            unlock_item_name = "Pool Unlock: Level 3-5",
            level_id = 25
        ),

        "3-6": Level(
            name = "Pool: Level 3-6",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Football', 'Zomboni', 'Flag'],
            flag_location_ids = [2013],
            clear_location_id = 1025,
            unlock_item_name = "Pool Unlock: Level 3-6",
            level_id = 26
        ),

        "3-7": Level(
            name = "Pool: Level 3-7",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Football', 'Snorkel', 'Zomboni', 'DolphinRider', 'Flag'],
            flag_location_ids = [2014, 2015],
            clear_location_id = 1026,
            unlock_item_name = "Pool Unlock: Level 3-7",
            level_id = 27
        ),

        "3-8": Level(
            name = "Pool: Level 3-8",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Dancer', 'DolphinRider', 'Flag'],
            flag_location_ids = [2016],
            clear_location_id = 1027,
            unlock_item_name = "Pool Unlock: Level 3-8",
            level_id = 28
        ),

        "3-9": Level(
            name = "Pool: Level 3-9",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Dancer', 'Zomboni', 'DolphinRider', 'Flag'],
            flag_location_ids = [2017, 2018],
            clear_location_id = 1028,
            unlock_item_name = "Pool Unlock: Level 3-9",
            level_id = 29
        ),

        "3-10": Level(
            name = "Pool: Level 3-10",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Football', 'Dancer', 'Snorkel', 'Zomboni', 'Flag'],
            choose = False,
            flag_location_ids = [2019, 2020],
            clear_location_id = 1029,
            conveyor = {'Lily Pad': 25, 'Squash': 5, 'Threepeater': 25, 'Tangle Kelp': 5, 'Jalapeno': 10, 'Spikeweed': 10, 'Torchwood': 10, 'Tall-nut': 10},
            unlock_item_name = "Pool Unlock: Level 3-10",
            level_id = 30
        ),

        "4-1": Level(
            name = "Fog: Level 4-1",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Football', 'JackInTheBox', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1030,
            unlock_item_name = "Fog Unlock: Level 4-1",
            level_id = 31
        ),

        "4-2": Level(
            name = "Fog: Level 4-2",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Football', 'JackInTheBox', 'Flag'],
            flag_location_ids = [2021],
            clear_location_id = 1031,
            unlock_item_name = "Fog Unlock: Level 4-2",
            level_id = 32
        ),

        "4-3": Level(
            name = "Fog: Level 4-3",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Football', 'Balloon', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1032,
            unlock_item_name = "Fog Unlock: Level 4-3",
            level_id = 33
        ),

        "4-4": Level(
            name = "Fog: Level 4-4",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'DolphinRider', 'Balloon', 'Flag'],
            flag_location_ids = [2022],
            clear_location_id = 1033,
            unlock_item_name = "Fog Unlock: Level 4-4",
            level_id = 34
        ),

        "4-5": Level(
            name = "Fog: Level 4-5",
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Flag'],
            choose = False,
            flag_location_ids = [],
            clear_location_id = 1034,
            special = "vasebreaker",
            unlock_item_name = "Fog Unlock: Level 4-5",
            level_id = 35
        ),

        "4-6": Level(
            name = "Fog: Level 4-6",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Digger', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1035,
            unlock_item_name = "Fog Unlock: Level 4-6",
            level_id = 36
        ),

        "4-7": Level(
            name = "Fog: Level 4-7",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'JackInTheBox', 'Digger', 'Flag'],
            flag_location_ids = [2023],
            clear_location_id = 1036,
            unlock_item_name = "Fog Unlock: Level 4-7",
            level_id = 37
        ),

        "4-8": Level(
            name = "Fog: Level 4-8",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Pogo', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1037,
            unlock_item_name = "Fog Unlock: Level 4-8",
            level_id = 38
        ),

        "4-9": Level(
            name = "Fog: Level 4-9",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Balloon', 'Pogo', 'Flag'],
            flag_location_ids = [2024],
            clear_location_id = 1038,
            unlock_item_name = "Fog Unlock: Level 4-9",
            level_id = 39
        ),

        "4-10": Level(
            name = "Fog: Level 4-10",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Flag'],
            choose = False,
            flag_location_ids = [2025],
            clear_location_id = 1039,
            conveyor = {'Lily Pad': 25, 'Sea-shroom': 10, 'Magnet-shroom': 5, 'Blover': 5, 'Cactus': 15, 'Starfruit': 25, 'Split Pea': 5, 'Pumpkin': 10},
            unlock_item_name = "Fog Unlock: Level 4-10",
            level_id = 40
        ),

        "5-1": Level(
            name = "Roof: Level 5-1",
            flags = 1,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Football', 'Bungee', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1040,
            unlock_item_name = "Roof Unlock: Level 5-1",
            level_id = 41
        ),

        "5-2": Level(
            name = "Roof: Level 5-2",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Bungee', 'Flag'],
            flag_location_ids = [2026],
            clear_location_id = 1041,
            unlock_item_name = "Roof Unlock: Level 5-2",
            level_id = 42
        ),

        "5-3": Level(
            name = "Roof: Level 5-3",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Ladder', 'Flag'],
            flag_location_ids = [2027],
            clear_location_id = 1042,
            unlock_item_name = "Roof Unlock: Level 5-3",
            level_id = 43
        ),

        "5-4": Level(
            name = "Roof: Level 5-4",
            flags = 3,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Pogo', 'Ladder', 'Flag'],
            flag_location_ids = [2028, 2029],
            clear_location_id = 1043,
            unlock_item_name = "Roof Unlock: Level 5-4",
            level_id = 44
        ),

        "5-5": Level(
            name = "Roof: Level 5-5",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Ladder', 'Flag'],
            choose = False,
            flag_location_ids = [2030],
            clear_location_id = 1044,
            conveyor = {'Flower Pot': 50, 'Chomper': 25, 'Pumpkin': 15, 'Cherry Bomb': 10},
            unlock_item_name = "Roof Unlock: Level 5-5",
            level_id = 45
        ),

        "5-6": Level(
            name = "Roof: Level 5-6",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Catapult', 'Flag'],
            flag_location_ids = [2031],
            clear_location_id = 1045,
            unlock_item_name = "Roof Unlock: Level 5-6",
            level_id = 46
        ),

        "5-7": Level(
            name = "Roof: Level 5-7",
            flags = 3,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Bungee', 'Ladder', 'Catapult', 'Flag'],
            flag_location_ids = [2032, 2033],
            clear_location_id = 1046,
            unlock_item_name = "Roof Unlock: Level 5-7",
            level_id = 47
        ),

        "5-8": Level(
            name = "Roof: Level 5-8",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Gargantuar', 'Flag'],
            flag_location_ids = [2034],
            clear_location_id = 1047,
            unlock_item_name = "Roof Unlock: Level 5-8",
            level_id = 48
        ),

        "5-9": Level(
            name = "Roof: Level 5-9",
            flags = 3,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'JackInTheBox', 'Bungee', 'Ladder', 'Catapult', 'Gargantuar', 'Flag'],
            flag_location_ids = [2035, 2036],
            clear_location_id = 1048,
            unlock_item_name = "Roof Unlock: Level 5-9",
            level_id = 49
        ),

        "5-10": Level(
            name = "Roof: Dr. Zomboss",
            location = "Night Roof",
            choose = False,
            flag_location_ids = [],
            zombies = [],
            clear_location_id = 1049,
            conveyor = {'Flower Pot': 55, 'Melon-pult': 10, 'Jalapeno': 12, 'Cabbage-pult': 10, 'Kernel-pult': 5, 'Ice-shroom': 8},
            special = "boss",
            conveyor_default = 4,
            unlock_item_name = "Night Roof Unlock: Dr. Zomboss",
            level_id = 50
        )
    }

    if world == None or world.options.minigame_levels.value != 0:
        levels = levels | {
            "ChallengeWarAndPeas": Level(
                zombies = ['PeaHead', 'WallnutHead'],
                name = "Mini-games: ZomBotany",
                flags = 2,
                type = "Mini-games",
                flag_location_ids = [2037],
                clear_location_id = 1050,
                unlock_item_name = "Mini-game Unlock: ZomBotany",
                level_id = 51
            ),

            "ChallengeWallnutBowling": Level(
                zombies = ['Newspaper', 'Normal', 'Buckethead', 'Polevaulter', 'Conehead'],
                name = "Mini-games: Wall-nut Bowling",
                choose = False,
                flags = 2,
                type = "Mini-games",
                flag_location_ids = [2038],
                clear_location_id = 1051,
                special = "bowling",
                unlock_item_name = "Mini-game Unlock: Wall-nut Bowling",
                level_id = 52
            ),

            "ChallengeSlotMachine": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: Slot Machine",
                choose = False,
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1052,
                special = "slot",
                unlock_item_name = "Mini-game Unlock: Slot Machine",
                level_id = 53
            ),

            "ChallengeRainingSeeds": Level(
                zombies = ['Bungee', 'ScreenDoor', 'Football', 'JackInTheBox', 'Newspaper', 'Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: It's Raining Seeds",
                location = "Fog",
                choose = False,
                flags = 4,
                type = "Mini-games",
                flag_location_ids = [2039, 2040, 2041],
                clear_location_id = 1053,
                special = "raining",
                unlock_item_name = "Mini-game Unlock: It's Raining Seeds",
                level_id = 54
            ),

            "ChallengeBeghouled": Level(
                zombies = ['ScreenDoor', 'Football', 'Newspaper', 'Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: Beghouled",
                location = "Night",
                choose = False,
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1054,
                special = "beghouled",
                unlock_item_name = "Mini-game Unlock: Beghouled",
                level_id = 55
            ),

            "ChallengeInvisighoul": Level(
                zombies = ['DolphinRider', 'JackInTheBox', 'Normal', 'Buckethead', 'Conehead', 'Zomboni'],
                name = "Mini-games: Invisi-ghoul",
                location = "Fog",
                choose = False,
                flags = 2,
                type = "Mini-games",
                flag_location_ids = [2042],
                clear_location_id = 1055,
                conveyor = {'Peashooter': 25, 'Wall-nut': 15, 'Kernel-pult': 5, 'Squash': 15, 'Lily Pad': 30, 'Ice-shroom': 10},
                conveyor_default = 2,
                unlock_item_name = "Mini-game Unlock: Invisi-ghoul",
                level_id = 56
            ),

            "ChallengeSeeingStars": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: Seeing Stars",
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1056,
                special = "art",
                forced_plants = {'Starfruit'},
                unlock_item_name = "Mini-game Unlock: Seeing Stars",
                level_id = 57
            ),

            "ChallengeZombiquarium": Level(
                zombies = [],
                name = "Mini-games: Zombiquarium",
                choose = False,
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1057,
                special = "zombiquarium",
                unlock_item_name = "Mini-game Unlock: Zombiquarium",
                level_id = 58
            ),

            "ChallengeBeghouledTwist": Level(
                zombies = ['ScreenDoor', 'Football', 'Newspaper', 'Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: Beghouled Twist",
                location = "Night",
                choose = False,
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1058,
                special = "beghouled",
                unlock_item_name = "Mini-game Unlock: Beghouled Twist",
                level_id = 59
            ),

            "ChallengeLittleTrouble": Level(
                zombies = ['Football', 'Normal', 'Snorkel', 'Conehead'],
                name = "Mini-games: Big Trouble Little Zombie",
                location = "Pool",
                choose = False,
                flags = 3,
                type = "Mini-games",
                flag_location_ids = [2043, 2044],
                clear_location_id = 1059,
                conveyor = {'Lily Pad': 25, 'Wall-nut': 15, 'Peashooter': 25, 'Cherry Bomb': 35},
                special = "little",
                unlock_item_name = "Mini-game Unlock: Big Trouble Little Zombie",
                level_id = 60
            ),

            "ChallengePortalCombat": Level(
                zombies = ['Balloon', 'Football', 'Normal', 'Buckethead'],
                name = "Mini-games: Portal Combat",
                location = "Night",
                choose = False,
                flags = 2,
                type = "Mini-games",
                flag_location_ids = [2045],
                clear_location_id = 1060,
                conveyor = {'Peashooter': 25, 'Repeater': 20, 'Torchwood': 10, 'Cactus': 15, 'Wall-nut': 15, 'Cherry Bomb': 15},
                unlock_item_name = "Mini-game Unlock: Portal Combat",
                level_id = 61
            ),

            "ChallengeColumn": Level(
                zombies = ['Football', 'Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: Column Like You See 'Em",
                location = "Roof",
                choose = False,
                flags = 3,
                type = "Mini-games",
                flag_location_ids = [2046, 2047],
                clear_location_id = 1061,
                conveyor = {'Flower Pot': 155, 'Melon-Pult': 5, 'Chomper': 5, 'Pumpkin': 15, 'Jalapeno': 10, 'Squash': 10},
                special = "column",
                conveyor_default = 6,
                unlock_item_name = "Mini-game Unlock: Column Like You See 'Em",
                level_id = 62
            ),

            "ChallengeBobsledBonanza": Level(
                zombies = ['Zomboni'],
                name = "Mini-games: Bobsled Bonanza",
                location = "Pool",
                flags = 4,
                type = "Mini-games",
                flag_location_ids = [2048, 2049, 2050],
                clear_location_id = 1062,
                unlock_item_name = "Mini-game Unlock: Bobsled Bonanza",
                level_id = 63
            ),

            "ChallengeSpeed": Level(
                zombies = ['DolphinRider', 'Normal', 'Polevaulter', 'Conehead'],
                name = "Mini-games: Zombie Nimble Zombie Quick",
                location = "Pool",
                flags = 4,
                type = "Mini-games",
                flag_location_ids = [2051, 2052, 2053],
                clear_location_id = 1063,
                unlock_item_name = "Mini-game Unlock: Zombie Nimble Zombie Quick",
                level_id = 64
            ),

            "ChallengeWhackAZombie": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Mini-games: Whack a Zombie",
                location = "Night",
                choose = False,
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1064,
                special = "whack",
                unlock_item_name = "Mini-game Unlock: Whack a Zombie",
                level_id = 65
            ),

            "ChallengeLastStand": Level(
                zombies = ['DolphinRider', 'Football', 'JackInTheBox', 'Ladder', 'Newspaper', 'Normal', 'Buckethead', 'Polevaulter', 'Conehead'],
                name = "Mini-games: Last Stand",
                location = "Pool",
                flags = 5,
                type = "Mini-games",
                flag_location_ids = [2054, 2055, 2056, 2057],
                clear_location_id = 1065,
                unlock_item_name = "Mini-game Unlock: Last Stand",
                level_id = 66
            ),

            "ChallengeWarAndPeas2": Level(
                zombies = ['GatlingHead', 'JalapenoHead', 'PeaHead', 'SquashHead', 'TallnutHead', 'WallnutHead'],
                name = "Mini-games: ZomBotany 2",
                location = "Pool",
                flags = 3,
                type = "Mini-games",
                flag_location_ids = [2058, 2059],
                clear_location_id = 1066,
                unlock_item_name = "Mini-game Unlock: ZomBotany 2",
                level_id = 67
            ),

            "ChallengeWallnutBowling2": Level(
                zombies = ['Dancer', 'ScreenDoor', 'Newspaper', 'Normal', 'Buckethead', 'Polevaulter', 'Conehead'],
                name = "Mini-games: Wall-nut Bowling 2",
                choose = False,
                flags = 3,
                type = "Mini-games",
                flag_location_ids = [2060, 2061],
                clear_location_id = 1067,
                special = "bowling",
                unlock_item_name = "Mini-game Unlock: Wall-nut Bowling 2",
                level_id = 68
            ),

            "ChallengePogoParty": Level(
                zombies = ['Pogo'],
                name = "Mini-games: Pogo Party",
                location = "Roof",
                flags = 3,
                type = "Mini-games",
                flag_location_ids = [2062, 2063],
                clear_location_id = 1068,
                unlock_item_name = "Mini-game Unlock: Pogo Party",
                level_id = 69
            ),

            "ChallengeFinalBoss": Level(
                zombies = [],
                name = "Mini-games: Dr. Zomboss's Revenge",
                location = "Night Roof",
                choose = False,
                type = "Mini-games",
                flag_location_ids = [],
                clear_location_id = 1069,
                conveyor = {'Flower Pot': 55, 'Melon-pult': 10, 'Jalapeno': 12, 'Cabbage-pult': 10, 'Kernel-pult': 5, 'Ice-shroom': 8},
                special = "boss",
                conveyor_default = 4,
                unlock_item_name = "Mini-game Unlock: Dr. Zomboss's Revenge",
                level_id = 70
            )
        }

    if world == None or world.options.puzzle_levels.value != 0:
        levels = levels | {
            "ScaryPotter1": Level(
                zombies = [],
                name = "Puzzle: Vasebreaker",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1070,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Vasebreaker",
                level_id = 71
            ),

            "ScaryPotter2": Level(
                zombies = [],
                name = "Puzzle: To The Left",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1071,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: To The Left",
                level_id = 72
            ),

            "ScaryPotter3": Level(
                zombies = [],
                name = "Puzzle: Third Vase",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1072,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Third Vase",
                level_id = 73
            ),

            "ScaryPotter4": Level(
                zombies = [],
                name = "Puzzle: Chain Reaction",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1073,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Chain Reaction",
                level_id = 74
            ),

            "ScaryPotter5": Level(
                zombies = [],
                name = "Puzzle: M is for Metal",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1074,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: M is for Metal",
                level_id = 75
            ),

            "ScaryPotter6": Level(
                zombies = [],
                name = "Puzzle: Scary Potter",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1075,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Scary Potter",
                level_id = 76
            ),

            "ScaryPotter7": Level(
                zombies = [],
                name = "Puzzle: Hokey Pokey",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1076,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Hokey Pokey",
                level_id = 77
            ),

            "ScaryPotter8": Level(
                zombies = [],
                name = "Puzzle: Another Chain Reaction",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1077,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Another Chain Reaction",
                level_id = 78
            ),

            "ScaryPotter9": Level(
                zombies = [],
                name = "Puzzle: Ace of Vase",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1078,
                special = "vasebreaker",
                unlock_item_name = "Puzzle Unlock: Ace of Vase",
                level_id = 79
            ),

            "PuzzleIZombie1": Level(
                zombies = [],
                name = "Puzzle: I, Zombie",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1079,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: I, Zombie",
                level_id = 80
            ),

            "PuzzleIZombie2": Level(
                zombies = [],
                name = "Puzzle: I, Zombie Too",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1080,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: I, Zombie Too",
                level_id = 81
            ),

            "PuzzleIZombie3": Level(
                zombies = [],
                name = "Puzzle: Can You Dig It?",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1081,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: Can You Dig It?",
                level_id = 82
            ),

            "PuzzleIZombie4": Level(
                zombies = [],
                name = "Puzzle: Totally Nuts",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1082,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: Totally Nuts",
                level_id = 83
            ),

            "PuzzleIZombie5": Level(
                zombies = [],
                name = "Puzzle: Dead Zeppelin",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1083,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: Dead Zeppelin",
                level_id = 84
            ),

            "PuzzleIZombie6": Level(
                zombies = [],
                name = "Puzzle: Me Smash!",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1084,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: Me Smash!",
                level_id = 85
            ),

            "PuzzleIZombie7": Level(
                zombies = [],
                name = "Puzzle: ZomBoogie",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1085,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: ZomBoogie",
                level_id = 86
            ),

            "PuzzleIZombie8": Level(
                zombies = [],
                name = "Puzzle: Three Hit Wonder",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1086,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: Three Hit Wonder",
                level_id = 87
            ),

            "PuzzleIZombie9": Level(
                zombies = [],
                name = "Puzzle: All your brainz r belong to us",
                location = "Night",
                choose = False,
                type = "Puzzle",
                flag_location_ids = [],
                clear_location_id = 1087,
                special = "izombie",
                unlock_item_name = "Puzzle Unlock: All your brainz r belong to us",
                level_id = 88
            )
            }
    if world == None or world.options.survival_levels.value != 0:
        levels = levels | {
            "SurvivalNormalStage1": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer'],
                name = "Survival: Day",
                flags = 5,
                type = "Survival",
                flag_location_ids = [2064, 2065, 2066, 2067],
                clear_location_id = 1088,
                unlock_item_name = "Survival Unlock: Day",
                level_id = 89
            ),

            "SurvivalNormalStage2": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer'],
                name = "Survival: Night",
                location = "Night",
                flags = 5,
                type = "Survival",
                flag_location_ids = [2068, 2069, 2070, 2071],
                clear_location_id = 1089,
                unlock_item_name = "Survival Unlock: Night",
                level_id = 90
            ),

            "SurvivalNormalStage3": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'Snorkel'],
                name = "Survival: Pool",
                location = "Pool",
                flags = 5,
                type = "Survival",
                flag_location_ids = [2072, 2073, 2074, 2075],
                clear_location_id = 1090,
                unlock_item_name = "Survival Unlock: Pool",
                level_id = 91
            ),

            "SurvivalNormalStage4": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'Snorkel', 'Zomboni', 'DolphinRider', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Ladder', 'Catapult'],
                name = "Survival: Fog",
                location = "Fog",
                flags = 5,
                type = "Survival",
                flag_location_ids = [2076, 2077, 2078, 2079],
                clear_location_id = 1091,
                unlock_item_name = "Survival Unlock: Fog",
                level_id = 92
            ),

            "SurvivalNormalStage5": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Zomboni', 'JackInTheBox', 'Balloon', 'Pogo', 'Bungee', 'Ladder', 'Catapult'],
                name = "Survival: Roof",
                location = "Roof",
                flags = 5,
                type = "Survival",
                flag_location_ids = [2080, 2081, 2082, 2083],
                clear_location_id = 1092,
                unlock_item_name = "Survival Unlock: Roof",
                level_id = 93
            ),

            "SurvivalHardStage1": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'Zomboni', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Ladder', 'Catapult', 'Gargantuar'],
                name = "Survival: Day (Hard)",
                flags = 10,
                type = "Survival",
                flag_location_ids = [2084, 2085, 2086, 2087, 2088, 2089, 2090, 2091, 2092],
                clear_location_id = 1093,
                unlock_item_name = "Survival Unlock: Day (Hard)",
                level_id = 94
            ),

            "SurvivalHardStage2": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Ladder', 'Catapult', 'Gargantuar'],
                name = "Survival: Night (Hard)",
                location = "Night",
                flags = 10,
                type = "Survival",
                flag_location_ids = [2093, 2094, 2095, 2096, 2097, 2098, 2099, 2100, 2101],
                clear_location_id = 1094,
                unlock_item_name = "Survival Unlock: Night (Hard)",
                level_id = 95
            ),

            "SurvivalHardStage3": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'Snorkel', 'Zomboni', 'DolphinRider', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Ladder', 'Catapult', 'Gargantuar'],
                name = "Survival: Pool (Hard)",
                location = "Pool",
                flags = 10,
                type = "Survival",
                flag_location_ids = [2102, 2103, 2104, 2105, 2106, 2107, 2108, 2109, 2110],
                clear_location_id = 1095,
                unlock_item_name = "Survival Unlock: Pool (Hard)",
                level_id = 96
            ),

            "SurvivalHardStage4": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'Snorkel', 'Zomboni', 'DolphinRider', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Ladder', 'Catapult', 'Gargantuar'],
                name = "Survival: Fog (Hard)",
                location = "Fog",
                flags = 10,
                type = "Survival",
                flag_location_ids = [2111, 2112, 2113, 2114, 2115, 2116, 2117, 2118, 2119],
                clear_location_id = 1096,
                unlock_item_name = "Survival Unlock: Fog (Hard)",
                level_id = 97
            ),

            "SurvivalHardStage5": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Zomboni', 'JackInTheBox', 'Balloon', 'Pogo', 'Bungee', 'Ladder', 'Catapult', 'Gargantuar'],
                name = "Survival: Roof (Hard)",
                location = "Roof",
                flags = 10,
                type = "Survival",
                flag_location_ids = [2120, 2121, 2122, 2123, 2124, 2125, 2126, 2127, 2128],
                clear_location_id = 1097,
                unlock_item_name = "Survival Unlock: Roof (Hard)",
                level_id = 98
            )
        }

    if world == None or world.options.bonus_levels.value != 0:
        levels = levels | {
            "ChallengeArtChallenge1": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Art Challenge Wall-nut",
                type = "Bonus Levels",
                flag_location_ids = [],
                clear_location_id = 1098,
                special = "art",
                forced_plants = {'Wall-nut'},
                unlock_item_name = "Bonus Levels Unlock: Art Challenge Wall-nut",
                level_id = 99
            ),

            "ChallengeSunnyDay": Level(
                zombies = ['Football', 'JackInTheBox', 'Normal', 'Buckethead', 'Polevaulter', 'Conehead'],
                name = "Bonus Levels: Sunny Day",
                flags = 4,
                type = "Bonus Levels",
                flag_location_ids = [2129, 2130, 2131],
                clear_location_id = 1099,
                unlock_item_name = "Bonus Levels Unlock: Sunny Day",
                level_id = 100
            ),

            "ChallengeResodded": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Unsodded",
                flags = 4,
                type = "Bonus Levels",
                flag_location_ids = [2132, 2133, 2134],
                clear_location_id = 1100,
                unlock_item_name = "Bonus Levels Unlock: Unsodded",
                level_id = 101
            ),

            "ChallengeBigTime": Level(
                zombies = ['ScreenDoor', 'Football', 'JackInTheBox', 'Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Big Time",
                flags = 4,
                type = "Bonus Levels",
                flag_location_ids = [2135, 2136, 2137],
                clear_location_id = 1101,
                unlock_item_name = "Bonus Levels Unlock: Big Time",
                level_id = 102
            ),

            "ChallengeArtChallenge2": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Art Challenge Sunflower",
                type = "Bonus Levels",
                flag_location_ids = [],
                clear_location_id = 1102,
                special = "art",
                forced_plants = {'Starfruit', 'Umbrella Leaf', 'Wall-nut'},
                unlock_item_name = "Bonus Levels Unlock: Art Challenge Sunflower",
                level_id = 103
            ),

            "ChallengeAirRaid": Level(
                zombies = ['Balloon'],
                name = "Bonus Levels: Air Raid",
                location = "Fog",
                flags = 2,
                type = "Bonus Levels",
                flag_location_ids = [2138],
                clear_location_id = 1103,
                unlock_item_name = "Bonus Levels Unlock: Air Raid",
                level_id = 104
            ),

            "ChallengeHighGravity": Level(
                zombies = ['Balloon', 'ScreenDoor', 'Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: High Gravity",
                location = "Roof",
                flags = 2,
                type = "Bonus Levels",
                flag_location_ids = [2139],
                clear_location_id = 1104,
                unlock_item_name = "Bonus Levels Unlock: High Gravity",
                level_id = 105
            ),

            "ChallengeGraveDanger": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Grave Danger",
                location = "Night",
                flags = 2,
                type = "Bonus Levels",
                flag_location_ids = [2140],
                clear_location_id = 1105,
                unlock_item_name = "Bonus Levels Unlock: Grave Danger",
                level_id = 106
            ),

            "ChallengeShovel": Level(
                zombies = ['Normal', 'Conehead'],
                name = "Bonus Levels: Can You Dig It?",
                choose = False,
                flags = 3,
                type = "Bonus Levels",
                flag_location_ids = [2141, 2142],
                clear_location_id = 1106,
                conveyor = {'Peashooter': 100},
                unlock_item_name = "Bonus Levels Unlock: Can You Dig It?",
                level_id = 107
            ),

            "ChallengeStormyNight": Level(
                zombies = ['Balloon', 'DolphinRider', 'Normal', 'Conehead'],
                name = "Bonus Levels: Dark Stormy Night",
                location = "Fog",
                choose = False,
                flags = 3,
                type = "Bonus Levels",
                flag_location_ids = [2143, 2144],
                clear_location_id = 1107,
                conveyor = {'Lily Pad': 30, 'Cactus': 10, 'Peashooter': 20, 'Puff-shroom': 15, 'Cherry Bomb': 25},
                unlock_item_name = "Bonus Levels Unlock: Dark Stormy Night",
                level_id = 108
            )
        }

    if world == None or world.options.cloudy_day_levels.value != 0:
        levels = levels | {
            "CloudyDay1": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter'],
                name = "Cloudy Day: Level 1",
                flags = 1,
                type = "Cloudy Day",
                flag_location_ids = [],
                clear_location_id = 1108,
                unlock_item_name = "Cloudy Day Unlock: Level 1",
                level_id = 109
            ),

            "CloudyDay2": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter'],
                name = "Cloudy Day: Level 2",
                flags = 2,
                type = "Cloudy Day",
                flag_location_ids = [2145],
                clear_location_id = 1109,
                unlock_item_name = "Cloudy Day Unlock: Level 2",
                level_id = 110
            ),

            "CloudyDay3": Level(
                zombies = ['Normal', 'Conehead', 'Buckethead'],
                name = "Cloudy Day: Level 3",
                flags = 1,
                type = "Cloudy Day",
                flag_location_ids = [],
                clear_location_id = 1110,
                unlock_item_name = "Cloudy Day Unlock: Level 3",
                level_id = 111
            ),

            "CloudyDay4": Level(
                zombies = ['Normal', 'Conehead', 'Buckethead', 'Polevaulter'],
                name = "Cloudy Day: Level 4",
                flags = 3,
                type = "Cloudy Day",
                flag_location_ids = [2146, 2147],
                clear_location_id = 1111,
                unlock_item_name = "Cloudy Day Unlock: Level 4",
                level_id = 112
            ),

            "CloudyDay5": Level(
                zombies = ['Normal', 'Conehead', 'Zomboni'],
                name = "Cloudy Day: Level 5",
                location = "Pool",
                flags = 2,
                type = "Cloudy Day",
                flag_location_ids = [2148],
                clear_location_id = 1112,
                unlock_item_name = "Cloudy Day Unlock: Level 5",
                level_id = 113
            ),

            "CloudyDay6": Level(
                zombies = ['Normal', 'Conehead', 'Zomboni', 'Buckethead', 'Snorkel'],
                name = "Cloudy Day: Level 6",
                location = "Pool",
                flags = 3,
                type = "Cloudy Day",
                flag_location_ids = [2149, 2150],
                clear_location_id = 1113,
                unlock_item_name = "Cloudy Day Unlock: Level 6",
                level_id = 114
            ),

            "CloudyDay7": Level(
                zombies = ['Normal', 'Conehead', 'DolphinRider'],
                name = "Cloudy Day: Level 7",
                location = "Pool",
                flags = 2,
                type = "Cloudy Day",
                flag_location_ids = [2151],
                clear_location_id = 1114,
                unlock_item_name = "Cloudy Day Unlock: Level 7",
                level_id = 115
            ),

            "CloudyDay8": Level(
                zombies = ['Normal', 'Conehead', 'DolphinRider', 'Zomboni', 'Polevaulter', 'Buckethead'],
                name = "Cloudy Day: Level 8",
                location = "Pool",
                flags = 3,
                type = "Cloudy Day",
                flag_location_ids = [2152, 2153],
                clear_location_id = 1115,
                unlock_item_name = "Cloudy Day Unlock: Level 8",
                level_id = 116
            ),

            "CloudyDay9": Level(
                zombies = ['Normal', 'Conehead', 'Catapult'],
                name = "Cloudy Day: Level 9",
                location = "Roof",
                flags = 2,
                type = "Cloudy Day",
                flag_location_ids = [2154],
                clear_location_id = 1116,
                unlock_item_name = "Cloudy Day Unlock: Level 9",
                level_id = 117
            ),

            "CloudyDay10": Level(
                zombies = ['Normal', 'Conehead', 'Catapult', 'Bungee', 'Ladder'],
                name = "Cloudy Day: Level 10",
                location = "Roof",
                flags = 3,
                type = "Cloudy Day",
                flag_location_ids = [2155, 2156],
                clear_location_id = 1117,
                unlock_item_name = "Cloudy Day Unlock: Level 10",
                level_id = 118
            ),

            "CloudyDay11": Level(
                zombies = ['Normal', 'Conehead', 'Gargantuar'],
                name = "Cloudy Day: Level 11",
                location = "Roof",
                flags = 2,
                type = "Cloudy Day",
                flag_location_ids = [2157],
                clear_location_id = 1118,
                unlock_item_name = "Cloudy Day Unlock: Level 11",
                level_id = 119
            ),

            "CloudyDay12": Level(
                zombies = ['Normal', 'Conehead', 'Gargantuar', 'Bungee', 'Catapult', 'Ladder', 'JackInTheBox', 'Buckethead'],
                name = "Cloudy Day: Level 12",
                location = "Roof",
                flags = 3,
                type = "Cloudy Day",
                flag_location_ids = [2158, 2159],
                clear_location_id = 1119,
                unlock_item_name = "Cloudy Day Unlock: Level 12",
                level_id = 120
            )
        }

    if world == None or world.options.china_level.value != 0:
        levels = levels | {
            "China": Level(
                zombies = ['Normal', 'Conehead', 'Buckethead', 'Polevaulter', 'Bungee', 'Football'],
                name = "China: The Great Wall",
                location = "China",
                flags = 2,
                type = "China",
                flag_location_ids = [2160],
                clear_location_id = 1120,
                unlock_item_name = "China Unlock: The Great Wall",
                level_id = 121
            )
        }

    return levels

def randomise_zombie_lists(world):
    zombie_blacklist = ["Normal", "Flag", "DuckyTube", "Yeti", "Target", "Zombatar", "Imp", "Boss", "Bobsled", "BackupDancer"]
    for zombie in world.options.randomised_zombies.value:
        if world.options.randomised_zombies[zombie] == 0:
            zombie_blacklist.append(zombie)

    permitted_zombie_rando_modes = []
    for mode in world.options.zombie_randomised_modes.value:
        if world.options.zombie_randomised_modes.value[mode] == 1:
            permitted_zombie_rando_modes.append(mode)

    for level in world.included_levels:
        level_data = world.included_levels[level]
        if level_data.type in permitted_zombie_rando_modes and not (level_data.special in ["beghouled", "slot", "zombiquarium", "whack", "boss", "vasebreaker", "izombie"]):
            world.included_levels[level].randomise_zombies(world, zombie_blacklist)

def randomise_conveyors(world):
    for level in world.included_levels:
        level_data = world.included_levels[level]
        if level_data.conveyor != None and not (level_data.special in ["bowling"]): #Don't randomise Wall-nut Bowling levels
            world.included_levels[level].randomise_conveyor(world)
            