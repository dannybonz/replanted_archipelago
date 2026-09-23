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
    waves: int = 0
    type: str = "Adventure"
    flag_location_ids: list[int] = field(default_factory = list)
    conveyor_default: int = 0
    forced_plants: set[str] = field(default_factory = set)
    vasebreaker_plants: list[dict] = field(default_factory = list)
    vasebreaker_zombies: list[dict] = field(default_factory = list)
    izombie_zombies: set[str] = field(default_factory = set)
    core_conveyor_plants: set[str] = field(default_factory = set)

    special: str | None = None
    conveyor: dict | None = None

    at_night: bool = False
    has_pool: bool = False
    on_roof: bool = False
    on_ceramic: bool = False

    expected_loadout: list[str] = None
    plant_combinations = None
    lawn_rows: int = 5
    ignore_locked_tiles: bool = False

    plant_banlist: list[str] = field(default_factory = list)

    def __post_init__(self):
        self.has_pool = (self.location == "Pool" or self.location == "Fog") and not self.special == "vasebreaker"
        self.on_roof = self.location == "Roof" or self.location == "Night Roof"
        self.at_night = self.location == "Night" or self.location == "Fog"
        self.on_ceramic = self.location == "Roof" or self.location == "Night Roof" or self.location == "China"
        if self.has_pool:
            self.lawn_rows = 6

        self.unmodified = copy.deepcopy(self)

        if len(self.core_conveyor_plants) == 0 and self.conveyor != None:
            self.core_conveyor_plants = set(self.conveyor.keys())

    def list_zombies(self, wave_index = -1) -> list[str]:
        zombie_list = self.zombies
        if wave_index == -1:
            for wave_of_zombies in self.vasebreaker_zombies:
                zombie_list += list(wave_of_zombies.keys())
        else:
            zombie_list += list(self.vasebreaker_zombies[wave_index].keys())
        return list(set(zombie_list))

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
            level_banned_zombies += ["Balloon", "Bungee", "GigaGargantuar", "GatlingHead", "JalapenoHead", "SquashHead"]
        elif self.name == "Roof: Level 5-5":
            level_banned_zombies += ["Digger", "Balloon", "Pogo", "Zomboni"]
        elif self.name in ["Mini-games: Bobsled Bonanza", "Mini-games: Pogo Party", "Bonus Levels: Air Raid"]:
            level_banned_zombies += ["GigaGargantuar"]
        elif self.name == "Mini-games: It's Raining Seeds":
            level_banned_zombies += ["Balloon"]
        elif self.special == "art":
            level_banned_zombies += ["Gargantuar", "GigaGargantuar", "TallnutHead", "JalapenoHead", "Zomboni", "Bungee"]
        elif self.name == "Mini-games: Last Stand":
            level_banned_zombies += ["GigaGargantuar"]

        if self.on_ceramic and self.type == "Cloudy Day":
            level_banned_zombies += [world.random.choice(["GatlingHead", "JalapenoHead"])] #Having two uber-destructive zombies on the roof in Cloudy Day is way too hard

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
            if plant in ["Melon-pult", "Winter Melon", "Jalapeno"] and self.name == "Mini-games: Column Like You See 'Em":
                conveyor_weights[plant] = world.random.randint(17, 25)

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

        self.core_conveyor_plants = list(conveyor_weights.keys())

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

    def randomise_zombie_vases(self, world, zombie_blacklist):
        for wave_index in range (0, len(self.unmodified.vasebreaker_zombies)):
            unmodified_wave_of_zombies = self.unmodified.vasebreaker_zombies[wave_index]

            zombie_blacklist += ["Bungee", "Balloon", "Snorkel", "DolphinRider", "Digger"]
            possible_zombies = [zombie_type for zombie_type in sorted(world.all_zombies.keys()) if zombie_type not in zombie_blacklist]

            if self.vasebreaker_plants != None and world.options.vasebreaker_randomisation.value == 0:
                eligible_zombies = self.eligible_zombies_for_loadout(world, list(self.vasebreaker_plants[wave_index].keys())) + list(unmodified_wave_of_zombies.keys())
                possible_zombies = [zombie for zombie in possible_zombies if zombie in eligible_zombies]

            target_zombie_total = sum(unmodified_wave_of_zombies.values())
            zombie_types = {}
            for zombie in unmodified_wave_of_zombies:
                if not zombie in possible_zombies:
                    zombie_types[zombie] = unmodified_wave_of_zombies[zombie]

            vases_to_fill = target_zombie_total - sum(zombie_types.values())
            if vases_to_fill > 0:
                additional_zombie_types = world.random.sample(possible_zombies, world.random.randint(1, vases_to_fill))
                for zombie in additional_zombie_types:
                    zombie_types[zombie] = 1   
        
                zombie_maxes = {"Gargantuar": 5, "GigaGargantuar": 3}

                weakest_to_strongest = sorted(additional_zombie_types, key=lambda zombie: world.all_zombies[zombie].value)
                zombie_index = 0
                loops_completed = 0
                while target_zombie_total - sum(zombie_types.values()) > 0:
                    zombie_type = weakest_to_strongest[zombie_index]

                    amount_to_add = min(1, target_zombie_total - sum(zombie_types.values()) * 0.5)
                    if zombie_type in zombie_maxes and zombie_types[zombie_type] + amount_to_add > zombie_maxes[zombie_type]:
                        amount_to_add = zombie_maxes[zombie_type] - zombie_types[zombie_type]
                    zombie_types[zombie_type] += amount_to_add

                    zombie_index += 1
                    if zombie_index >= len(weakest_to_strongest):
                        zombie_index = 0
                        loops_completed += 1

                    #Prevent a rare infinite loop
                    if loops_completed > 20:
                        if "Normal" in zombie_types:
                            zombie_types["Normal"] += target_zombie_total - sum(zombie_types.values())
                        else:
                            zombie_types["Normal"] = target_zombie_total - sum(zombie_types.values())
            
            self.vasebreaker_zombies[wave_index] = zombie_types

    def randomise_plant_vases(self, world):
        for wave_index in range(0, len(self.unmodified.vasebreaker_plants)):
            unmodified_wave_of_vases = self.unmodified.vasebreaker_plants[wave_index]
            target_plant_total = sum(unmodified_wave_of_vases.values())

            possible_threat_counters = self.create_plant_combinations(world, wave_index)
            plants_to_use = []
            for threat_name in possible_threat_counters:
                possible_plants = possible_threat_counters[threat_name]
                plants_to_use += world.random.choice(possible_threat_counters[threat_name])
            plants_to_use = sorted(set(plants_to_use))

            plant_types = {}
            target_plant_amount = min(7, int((target_plant_total * 0.75) / len(plants_to_use)))
            for plant in plants_to_use:
                if plant == "Magnet-shroom":
                    plant_types[plant] = min(target_plant_amount, 3)                
                else:
                    plant_types[plant] = target_plant_amount

            vases_to_fill = target_plant_total - sum(plant_types.values())

            #Utility
            for plant in ["Plantern", "Cherry Bomb", "Squash"]:
                amount_to_add = world.random.randint(0, min(2, vases_to_fill))
                if amount_to_add > 0:
                    if plant in plant_types:
                        plant_types[plant] += world.random.randint(0, 2)
                    else:
                        plant_types[plant] = amount_to_add
                vases_to_fill -= amount_to_add

            #Walls
            for x in range(0, world.random.randint(0, min(2, vases_to_fill))):
                wall_plant = list(world.random.choice(world.wall_plants))[0]
                if wall_plant in plant_types:
                    plant_types[wall_plant] += 1
                else:
                    plant_types[wall_plant] = 1
                vases_to_fill -= 1

            #Random plants
            ineligible_plants = ["Sea-shroom", "Lily Pad", "Cattail", "Sunflower", "Twin Sunflower", "Sun-shroom", "Grave Buster", "Tangle Kelp", "Blover", "Flower Pot", "Coffee Bean", "Umbrella Leaf", "Marigold", "Cob Cannon", "Gold Magnet"]
            if not world.options.easy_upgrade_plants.value:
                ineligible_plants += ["Gatling Pea", "Gloom-shroom", "Winter Melon", "Spikerock"]
            if not any(zombie in self.vasebreaker_zombies[wave_index] for zombie in ["Football", "Buckethead", "Pogo", "ScreenDoor"]):
                ineligible_plants += ["Magnet-shroom"]
            if not ("Peashooter" in plant_types or "Repeater" in plant_types or "Threepeater" in plant_types):
                ineligible_plants += ["Torchwood"]

            possible_plants = [plant for plant in world.all_plants if not plant in ineligible_plants]
            if plant == "Backwards Repeater (Vasebreaker)" and "Repeater" in world.usable_plants:
                possible_plants.append("Backwards Repeater (Vasebreaker)")

            while vases_to_fill > 0:
                plant_to_add = world.random.choice(possible_plants)
                amount_to_add = world.random.randint(1, min(3, vases_to_fill))
                if plant_to_add in plant_types:
                    plant_types[plant_to_add] += amount_to_add
                else:
                    plant_types[plant_to_add] = amount_to_add
                vases_to_fill -= amount_to_add
            
            self.vasebreaker_plants[wave_index] = plant_types

    def requires_a_wall_plant(self):
        return self.type == "Survival" or self.name in ["Roof: Level 5-5", "Mini-games: Column Like You See 'Em", "Bonus Levels: High Gravity"] or any(zombie in self.zombies for zombie in ["PeaHead", "GatlingHead", "TallnutHead"])

    def can_clear(self, state, world, player):
        #Non-negotiables
        required_items = []
        if self.on_roof and (self.name in ["Mini-games: Pogo Party", "Mini-games: Column Like You See 'Em"] or "GigaGargantuar" in self.zombies or self.type == "Cloudy Day" or self.flags >= 3):
            required_items.append("Roof Cleaners")
        if (self.name in ["Mini-games: Pogo Party", "Mini-games: Bobsled Bonanza", "Bonus Levels: Air Raid"] and "Gargantuar" in self.zombies) or (world.options.individual_tile_unlock_items.value and self.flags >= 2 and not self.ignore_locked_tiles):
            required_items.append("Shovel")

        if not all(state.has(item, player) for item in required_items):
            return False

        #Tile unlocks
        if world.options.individual_tile_unlock_items.value and not self.ignore_locked_tiles:
            tile_numbers = {}
            available_tiles = {}
            total_tiles_unlocked = 0
            for row_index in range(1, self.lawn_rows + 1):
                available_tiles[row_index] = []
                for column_index in range(1, 10):
                    if state.has(f"Tile Unlock: Row #{row_index}, Column #{column_index}", player):
                        available_tiles[row_index].append(column_index)
                        total_tiles_unlocked += 1
                tile_numbers[row_index] = {"total": len(available_tiles[row_index]), "rear": sum(1 for column_index in available_tiles[row_index] if column_index <= 4), "front": sum(1 for column_index in available_tiles[row_index] if column_index >= 7)}

            if self.name == "Day: Level 1-1":
                del available_tiles[1]
                del available_tiles[2]
                del available_tiles[4]
                del available_tiles[5]
            elif self.name in ["Day: Level 1-2", "Day: Level 1-3"]:
                del available_tiles[1]
                del available_tiles[5]

            required_rear_columns_per_row = 1
            required_total_columns_per_row = 1 + self.flags
            required_front_columns_per_row = 0

            if self.choose and not self.name in ["Day: Level 1-1", "Day: Level 1-2", "Day: Level 1-3", "Day: Level 1-4"]:
                required_total_tiles = int(self.lawn_rows * 1.5)
                if (self.at_night):
                    required_total_tiles += self.lawn_rows
                if total_tiles_unlocked < required_total_tiles:
                    return False

            if self.requires_a_wall_plant() or any(zombie in self.zombies for zombie in ["Snorkel", "DolphinRider", "Polevaulter", "Football"]):
                required_front_columns_per_row += 1
            
            if self.name in ["Mini-games: Pogo Party", "Mini-games: Bobsled Bonanza", "Bonus Levels: Air Raid"]:
                required_total_columns_per_row += 4
                required_rear_columns_per_row += 1
                if "Gargantuar" in self.zombies:
                    required_total_columns_per_row = 7
            elif self.name in ["Mini-games: Column Like You See 'Em"] or self.type == "Survival" or self.special == "boss":
                required_total_columns_per_row = 7
            elif self.type == "Cloudy Day":
                required_total_columns_per_row += 2

            for row_index in range(1, self.lawn_rows + 1):
                if min(required_total_columns_per_row, 9) > tile_numbers[row_index]["total"] or min(required_rear_columns_per_row, 4) > tile_numbers[row_index]["rear"] or min(required_front_columns_per_row, 3) > tile_numbers[row_index]["front"]:
                    return False

        if self.choose:
            #Get relevant unlocked plants
            if self.plant_combinations == None:
                self.plant_combinations = self.create_plant_combinations(world)
            unlocked_plants = {plant for plant in world.progression_plants if ((state.has(plant, player) or plant in self.forced_plants) and not plant in self.plant_banlist + ["Backwards Repeater (Vasebreaker)"])}

            if (not self.ignore_locked_tiles) and world.options.progressive_sun_capacity_items.value: #Check plants are affordable with sun cap limits
                total_sun_capacity = 150 * (2 ** state.count("Progressive Sun Capacity", player))
                unlocked_plants = {plant for plant in unlocked_plants if world.all_plants[plant].cost * 1.1 <= total_sun_capacity}

                if self.type in ["Survival", "Cloudy Day"] and total_sun_capacity < 1000:
                    return False
                elif self.name == "Mini-games: Last Stand" and total_sun_capacity < 5000:
                    return False

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

        #Vasebreaker plant locks
        elif self.special == "vasebreaker" and world.options.lock_vasebreaker_plants.value:
            for wave_of_vases in self.vasebreaker_plants:
                total_vases = sum(wave_of_vases.values())
                empty_vases = sum([wave_of_vases[plant] for plant in wave_of_vases if not state.has(plant, player)])
                if (empty_vases/total_vases) > 0.05: #Level is OOL if more than 5% of vases are empty in a wave
                    return False 

        #I, Zombie locks
        elif self.special == "izombie" and world.options.lock_izombie_zombies.value:
            locked_zombies = len([zombie for zombie in self.izombie_zombies if not state.has(zombie, player)])
            if locked_zombies > 0:
                return False

        #Conveyor locks
        elif self.conveyor != None and world.options.lock_conveyor_plants.value:
            locked_plants = [plant for plant in self.core_conveyor_plants if not state.has(plant, player)]
            if len(locked_plants) > 0:
                return False

        #Bowling locks
        elif self.special == "bowling" and world.options.lock_conveyor_plants.value:
            if not (state.has("Wall-nut", player) and state.has("Explode-o-nut (Wall-nut Bowling)", player)):
                return False
            if self.name == "Mini-games: Wall-nut Bowling 2" and not state.has("Giant Wall-nut (Wall-nut Bowling)", player):
                return False
        
        return True

    def create_plant_combinations(self, world, wave_index = -1):
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
        if self.special == "vasebreaker":
            possible_combinations["attacker"].append({"Backwards Repeater (Vasebreaker)"})

        if self.conveyor == None and self.special != "vasebreaker":
            attacker_max_price = 200
            if self.type == "Cloudy Day":
                attacker_max_price = 150
            
            #Check to see if a plant even exists at such a low price
            actual_lowest_price = 999
            for combination in possible_combinations["attacker"]:
                combination_cost = 0
                for plant in combination:
                    combination_cost += world.all_plants[plant].cost
                if combination_cost < actual_lowest_price:
                    actual_lowest_price = combination_cost

            #No such plant exists, so move the goal posts to be a little lenient (sorry, player)
            if actual_lowest_price > attacker_max_price:
                attacker_max_price = actual_lowest_price

            #Remove combinations that don't meed the requirement
            for combination in possible_combinations["attacker"]:
                combination_cost = 0
                for plant in combination:
                    combination_cost += world.all_plants[plant].cost
                if combination_cost > attacker_max_price: #Nothing too expensive for your main attacker
                    possible_combinations["attacker"].remove(combination)

        #Lily Pad
        if self.has_pool:
            possible_combinations["pool"] = [{"Lily Pad"}]

        #Flower Pot
        if self.on_ceramic:
            possible_combinations["flowerpot"] = [{"Flower Pot"}]

        #Sun producers
        if self.choose and (self.type != "Adventure" or self.flags > 1 or self.at_night) and not self.name in ["Mini-games: Last Stand"]:
            if self.at_night:
                possible_combinations["sun"] = [{"Sun-shroom"}]
            else:
                possible_combinations["sun"] = [{"Sunflower"}]

        #Wall plants
        if self.requires_a_wall_plant():
            possible_combinations["wall"] = [] + world.wall_plants

        #AOE plants
        if self.type == "Survival" or self.name in ["Mini-games: Last Stand", "Mini-games: Column Like You See 'Em"]  or "GigaGargantuar" in self.list_zombies(wave_index):
            possible_combinations["aoe"] = [{"Melon-pult"}]
            if not (self.on_roof):
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
        if "Balloon" in self.list_zombies(wave_index):
            possible_combinations["balloon"] = [{"Cactus"}, {"Blover"}]
            if self.has_pool:
                possible_combinations["balloon"].append({"Cattail"})

        #Ladder (Vasebreaker)
        if any(zombie in self.list_zombies(wave_index) for zombie in ["Ladder"]) and self.special == "vasebreaker":
            possible_combinations["ladder"] = [{"Magnet-shroom"}, {"Wall-nut"}, {"Tall-nut"}, {"Pumpkin"}, {"Jalapeno"}]

        #Catapult (Vasebreaker)
        if any(zombie in self.list_zombies(wave_index) for zombie in ["Catapult"]) and self.special == "vasebreaker":
            possible_combinations["ladder"] = [{"Spikeweed"}, {"Spikerock"}, {"Jalapeno"}, {"Umbrella Leaf"}]

        #Shields
        if any(zombie in self.list_zombies(wave_index) for zombie in ["ScreenDoor", "Ladder"]):
            possible_combinations["shield"] = [{"Cabbage-pult"}, {"Kernel-pult"}]
            if self.at_night:
                possible_combinations["shield"] += [{"Fume-shroom"}, {"Magnet-shroom"}]
            elif self.conveyor == None:
                possible_combinations["shield"] += [{"Fume-shroom", "Coffee Bean"}, {"Magnet-shroom", "Coffee Bean"}]

        #Digger
        if "Digger" in self.list_zombies(wave_index):
            possible_combinations["digger"] = [{"Starfruit"}, {"Split Pea"}]
            if self.has_pool:
                possible_combinations["digger"].append({"Cattail"})
            if self.at_night:
                possible_combinations["digger"].append({"Magnet-shroom"})
            elif self.conveyor == None:
                possible_combinations["digger"].append({"Magnet-shroom", "Coffee Bean"})

        #Snorkel
        if "Snorkel" in self.list_zombies(wave_index):
            possible_combinations["snorkel"] = [{"Cabbage-pult"}, {"Kernel-pult"}, {"Melon-pult"}] + world.wall_plants
            if world.options.easy_upgrade_plants.value:
                possible_combinations["snorkel"].append({"Winter Melon"})

        #Pogo
        if "Pogo" in self.list_zombies(wave_index):
            possible_combinations["pogo"] = [{"Split Pea"}, {"Starfruit"}, {"Tall-nut"}]
            if self.at_night:
                possible_combinations["pogo"].append({"Magnet-shroom"})
            elif self.conveyor == None:
                possible_combinations["pogo"].append({"Magnet-shroom", "Coffee Bean"})
            if self.has_pool:
                possible_combinations["pogo"].append({"Cattail"})

        #Football
        if any(zombie in self.list_zombies(wave_index) for zombie in ["Football"]):
            possible_combinations["football"] = [] + world.wall_plants
            if self.at_night:
                possible_combinations["football"] += [{"Magnet-shroom"}, {"Hypno-shroom"}]
            elif self.conveyor == None:
                possible_combinations["football"] += [{"Magnet-shroom", "Coffee Bean"}, {"Hypno-shroom", "Coffee Bean"}]
            if not self.name in ["Mini-games: Last Stand"]:
                possible_combinations["football"] += [{"Cherry Bomb"}, {"Squash"}, {"Jalapeno"}]

        #Magnet
        if self.name in ["Bonus Levels: Unsodded"] and any(zombie in self.list_zombies(wave_index) for zombie in ["Football", "Buckethead", "Pogo", "ScreenDoor"]) and self.conveyor == None:        
            possible_combinations["magnet"] = [{"Magnet-shroom", "Coffee Bean"}]

        #Zomboni
        if any(zombie in self.list_zombies(wave_index) for zombie in ["Zomboni"]):
            possible_combinations["zomboni"] = [{"Cherry Bomb"}, {"Squash"}, {"Jalapeno"}]
            if not self.on_roof:
                possible_combinations["zomboni"].append({"Spikeweed"})
                if world.options.easy_upgrade_plants.value:
                    possible_combinations["zomboni"].append({"Spikerock"})

        #Gargantuar
        if any(zombie in self.list_zombies(wave_index) for zombie in ["Gargantuar", "GigaGargantuar"]):
            if not self.name in ["Mini-games: Last Stand"]:       
                possible_combinations["garg"] = [{"Cherry Bomb", "Squash"}, {"Squash", "Jalapeno"}, {"Jalapeno", "Cherry Bomb"}]

            #Garg bonanza - overwrites the standard garg requirement
            if self.name in ["Mini-games: Pogo Party", "Mini-games: Bobsled Bonanza", "Bonus Levels: Air Raid"]:
                if self.at_night:
                    possible_combinations["garg"] = [{"Ice-shroom", "Sun-shroom", "Doom-shroom", "Kernel-pult", "Potato Mine"}]
                else:
                    possible_combinations["garg"] = [{"Ice-shroom", "Doom-shroom", "Kernel-pult", "Potato Mine"}]

        #Cherry Bomb/Jalapeno guarantee
        if (self.special in ["little"]):
            possible_combinations["little"] = [{"Cherry Bomb"}, {"Jalapeno"}]

        #Insta-kill for Roof: Level 5-5
        if self.name == "Roof: Level 5-5":
            possible_combinations["bungeeblitz"] = [{"Cherry Bomb"}, {"Jalapeno"}, {"Squash"}, {"Chomper"}]

        #Bungee
        if self.name in ["Mini-games: Bobsled Bonanza", "Mini-games: Pogo Party", "Bonus Levels: Air Raid"] and "Bungee" in self.list_zombies(wave_index):
            possible_combinations["bungee_bonanza"] = [{"Umbrella Leaf"}]
            
        #Grave Buster
        if self.name == "Bonus Levels: Grave Danger" or (self.type == "Survival" and self.at_night and not self.has_pool):
            possible_combinations["grave"] = [{"Grave Buster"}]

        #Column Like You See 'Em hard requirements
        if self.name in ["Mini-games: Column Like You See 'Em"]:
            possible_combinations["column"] = [{"Jalapeno", "Pumpkin"}]

        #High Gravity
        if self.name in ["Bonus Levels: High Gravity"]:
            possible_combinations["gravity"] = [{"Fume-shroom", "Coffee Bean"}, {"Chomper"}]
            if world.options.easy_upgrade_plants.value:
                possible_combinations["gravity"].append({"Gloom-shroom", "Coffee Bean"})

        #Spikeweed
        if self.name in ["Mini-games: Bobsled Bonanza", "Mini-games: Pogo Party", "Bonus Levels: Air Raid"] and "Zomboni" in self.list_zombies(wave_index):
            possible_combinations["spikeweed"] = [{"Spikeweed"}]
            if world.options.easy_upgrade_plants.value:
                possible_combinations["spikeweed"] += [{"Spikerock"}]

        #Insta-kill
        if (self.flags >= 3):
            possible_combinations["insta"] = [{"Squash"}, {"Cherry Bomb"}, {"Jalapeno"}, {"Potato Mine"}]

        if world.usable_plants != []:
            if self.conveyor == None and self.vasebreaker_plants == []: #Ensure only progression items are used for rule building (generally only needed for seed stat rando as plants can lose their progression status)
                for threat in possible_combinations:
                    possible_combinations[threat] = [combo for combo in possible_combinations[threat] if all(plant in world.progression_plants for plant in combo)]
            elif world.options.plant_stat_randomisation.value != 0: #Tries to only use actually good plants when building conveyor/vasebreaker loadouts
                for threat in possible_combinations:
                    possible_combinations[threat] = [combo for combo in possible_combinations[threat] if all((plant in world.usable_plants or (plant == "Backwards Repeater (Vasebreaker)" and "Repeater" in world.usable_plants)) for plant in combo)]

        return possible_combinations

    def eligible_zombies_for_loadout(self, world, plant_loadout): #Used for Zombie rando when Conveyor Rando is disabled. Given a list of plants, it spits out what Zombies you can theoretically win against
        copied_level_data = copy.deepcopy(self) #Make a copy of this level so that we can mess about with it
        copied_level_data.zombies = sorted(world.all_zombies.keys()) #Change the copy to have all zombies in the game - this list will then be wittled down
        potential_threats = copied_level_data.create_plant_combinations(world) 

        threat_to_zombie_name = {"balloon": ["Balloon"], "football": ["Football"], "zomboni": ["Zomboni"], "digger": ["Digger"], "pogo": ["Pogo"], "garg": ["Gargantuar"], "wall": ["PeaHead", "GatlingHead", "TallnutHead"]} #These Zombies become in-logic so long as this one specific threat is covered
        eligible_zombies = ["Normal", "Flag", "Conehead", "Polevaulter", "Buckethead", "Newspaper", "Dancer", "DolphinRider", "JackInTheBox", "Digger", "Bungee", "Catapult", "JalapenoHead", "SquashHead", "TrashCan", "Imp"] #These zombies have 0 logic attached to them
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
            level_id = 1,
            waves = 4
        ),

        "1-2": Level(
            name = "Day: Level 1-2",
            flags = 1,
            zombies = ['Normal', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1001,
            unlock_item_name = "Day Unlock: Level 1-2",
            level_id = 2,
            waves = 6
        ),

        "1-3": Level(
            name = "Day: Level 1-3",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1002,
            unlock_item_name = "Day Unlock: Level 1-3",
            level_id = 3,
            waves = 8
        ),

        "1-4": Level(
            name = "Day: Level 1-4",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1003,
            unlock_item_name = "Day Unlock: Level 1-4",
            level_id = 4,
            waves = 10
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
            level_id = 5,
            ignore_locked_tiles = True,
            waves = 8,
        ),

        "1-6": Level(
            name = "Day: Level 1-6",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1005,
            unlock_item_name = "Day Unlock: Level 1-6",
            level_id = 6,
            waves = 10
        ),

        "1-7": Level(
            name = "Day: Level 1-7",
            flags = 2,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Flag'],
            flag_location_ids = [2000],
            clear_location_id = 1006,
            unlock_item_name = "Day Unlock: Level 1-7",
            level_id = 7,
            waves = 20
        ),

        "1-8": Level(
            name = "Day: Level 1-8",
            flags = 1,
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1007,
            unlock_item_name = "Day Unlock: Level 1-8",
            level_id = 8,
            waves = 10
        ),

        "1-9": Level(
            name = "Day: Level 1-9",
            flags = 2,
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Flag'],
            flag_location_ids = [2001],
            clear_location_id = 1008,
            unlock_item_name = "Day Unlock: Level 1-9",
            level_id = 9,
            waves = 20
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
            level_id = 10,
            waves = 20
        ),

        "2-1": Level(
            name = "Night: Level 2-1",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Newspaper', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1010,
            unlock_item_name = "Night Unlock: Level 2-1",
            level_id = 11,
            waves = 10
        ),

        "2-2": Level(
            name = "Night: Level 2-2",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Newspaper', 'Flag'],
            flag_location_ids = [2003],
            clear_location_id = 1011,
            unlock_item_name = "Night Unlock: Level 2-2",
            level_id = 12,
            waves = 20
        ),

        "2-3": Level(
            name = "Night: Level 2-3",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1012,
            unlock_item_name = "Night Unlock: Level 2-3",
            level_id = 13,
            waves = 10
        ),

        "2-4": Level(
            name = "Night: Level 2-4",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'ScreenDoor', 'Flag'],
            flag_location_ids = [2004],
            clear_location_id = 1013,
            unlock_item_name = "Night Unlock: Level 2-4",
            level_id = 14,
            waves = 20
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
            level_id = 15,
            ignore_locked_tiles = True,
            waves = 8
        ),

        "2-6": Level(
            name = "Night: Level 2-6",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1015,
            unlock_item_name = "Night Unlock: Level 2-6",
            level_id = 16,
            waves = 10
        ),

        "2-7": Level(
            name = "Night: Level 2-7",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag'],
            flag_location_ids = [2005],
            clear_location_id = 1016,
            unlock_item_name = "Night Unlock: Level 2-7",
            level_id = 17,
            waves = 20
        ),

        "2-8": Level(
            name = "Night: Level 2-8",
            flags = 1,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1017,
            unlock_item_name = "Night Unlock: Level 2-8",
            level_id = 18,
            waves = 10
        ),

        "2-9": Level(
            name = "Night: Level 2-9",
            flags = 2,
            location = "Night",
            zombies = ['Normal', 'Conehead', 'ScreenDoor', 'Flag'],
            flag_location_ids = [2006],
            clear_location_id = 1018,
            unlock_item_name = "Night Unlock: Level 2-9",
            level_id = 19,
            waves = 20
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
            level_id = 20,
            waves = 20
        ),

        "3-1": Level(
            name = "Pool: Level 3-1",
            flags = 1,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1020,
            unlock_item_name = "Pool Unlock: Level 3-1",
            level_id = 21,
            waves = 10
        ),

        "3-2": Level(
            name = "Pool: Level 3-2",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Flag'],
            flag_location_ids = [2008],
            clear_location_id = 1021,
            unlock_item_name = "Pool Unlock: Level 3-2",
            level_id = 22,
            waves = 20
        ),

        "3-3": Level(
            name = "Pool: Level 3-3",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'Snorkel', 'Flag'],
            flag_location_ids = [2009],
            clear_location_id = 1022,
            unlock_item_name = "Pool Unlock: Level 3-3",
            level_id = 23,
            waves = 20
        ),

        "3-4": Level(
            name = "Pool: Level 3-4",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Snorkel', 'Flag'],
            flag_location_ids = [2010, 2011],
            clear_location_id = 1023,
            unlock_item_name = "Pool Unlock: Level 3-4",
            level_id = 24,
            waves = 30
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
            level_id = 25,
            waves = 20
        ),

        "3-6": Level(
            name = "Pool: Level 3-6",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Football', 'Zomboni', 'Flag'],
            flag_location_ids = [2013],
            clear_location_id = 1025,
            unlock_item_name = "Pool Unlock: Level 3-6",
            level_id = 26,
            waves = 20
        ),

        "3-7": Level(
            name = "Pool: Level 3-7",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Football', 'Snorkel', 'Zomboni', 'DolphinRider', 'Flag'],
            flag_location_ids = [2014, 2015],
            clear_location_id = 1026,
            unlock_item_name = "Pool Unlock: Level 3-7",
            level_id = 27,
            waves = 30
        ),

        "3-8": Level(
            name = "Pool: Level 3-8",
            flags = 2,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Dancer', 'DolphinRider', 'Flag'],
            flag_location_ids = [2016],
            clear_location_id = 1027,
            unlock_item_name = "Pool Unlock: Level 3-8",
            level_id = 28,
            waves = 20
        ),

        "3-9": Level(
            name = "Pool: Level 3-9",
            flags = 3,
            location = "Pool",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Dancer', 'Zomboni', 'DolphinRider', 'Flag'],
            flag_location_ids = [2017, 2018],
            clear_location_id = 1028,
            unlock_item_name = "Pool Unlock: Level 3-9",
            level_id = 29,
            waves = 30
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
            level_id = 30,
            waves = 30
        ),

        "4-1": Level(
            name = "Fog: Level 4-1",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Football', 'JackInTheBox', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1030,
            unlock_item_name = "Fog Unlock: Level 4-1",
            level_id = 31,
            waves = 10
        ),

        "4-2": Level(
            name = "Fog: Level 4-2",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Football', 'JackInTheBox', 'Flag'],
            flag_location_ids = [2021],
            clear_location_id = 1031,
            unlock_item_name = "Fog Unlock: Level 4-2",
            level_id = 32,
            waves = 20
        ),

        "4-3": Level(
            name = "Fog: Level 4-3",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Football', 'Balloon', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1032,
            unlock_item_name = "Fog Unlock: Level 4-3",
            level_id = 33,
            waves = 10
        ),

        "4-4": Level(
            name = "Fog: Level 4-4",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'DolphinRider', 'Balloon', 'Flag'],
            flag_location_ids = [2022],
            clear_location_id = 1033,
            unlock_item_name = "Fog Unlock: Level 4-4",
            level_id = 34,
            waves = 20
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
            level_id = 35,
            ignore_locked_tiles = True,
            vasebreaker_plants = [{"Peashooter": 5, "Squash": 5}, {"Peashooter": 4, "Snow Pea": 5, "Squash": 4}, {"Peashooter": 5, "Snow Pea": 5, "Hypno-shroom": 5}],
            vasebreaker_zombies = [{"Buckethead": 1, "Normal": 4}, {"Normal": 5, "Buckethead": 1, "Football": 1}, {"Normal": 6, "Buckethead": 2, "Dancer": 1, "JackInTheBox": 1}]
        ),

        "4-6": Level(
            name = "Fog: Level 4-6",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Digger', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1035,
            unlock_item_name = "Fog Unlock: Level 4-6",
            level_id = 36,
            waves = 10
        ),

        "4-7": Level(
            name = "Fog: Level 4-7",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'JackInTheBox', 'Digger', 'Flag'],
            flag_location_ids = [2023],
            clear_location_id = 1036,
            unlock_item_name = "Fog Unlock: Level 4-7",
            level_id = 37,
            waves = 20
        ),

        "4-8": Level(
            name = "Fog: Level 4-8",
            flags = 1,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Pogo', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1037,
            unlock_item_name = "Fog Unlock: Level 4-8",
            level_id = 38,
            waves = 10
        ),

        "4-9": Level(
            name = "Fog: Level 4-9",
            flags = 2,
            location = "Fog",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'Balloon', 'Pogo', 'Flag'],
            flag_location_ids = [2024],
            clear_location_id = 1038,
            unlock_item_name = "Fog Unlock: Level 4-9",
            level_id = 39,
            waves = 20
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
            level_id = 40,
            waves = 20
        ),

        "5-1": Level(
            name = "Roof: Level 5-1",
            flags = 1,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Football', 'Bungee', 'Flag'],
            flag_location_ids = [],
            clear_location_id = 1040,
            unlock_item_name = "Roof Unlock: Level 5-1",
            level_id = 41,
            waves = 10
        ),

        "5-2": Level(
            name = "Roof: Level 5-2",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Bungee', 'Flag'],
            flag_location_ids = [2026],
            clear_location_id = 1041,
            unlock_item_name = "Roof Unlock: Level 5-2",
            level_id = 42,
            waves = 20
        ),

        "5-3": Level(
            name = "Roof: Level 5-3",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Ladder', 'Flag'],
            flag_location_ids = [2027],
            clear_location_id = 1042,
            unlock_item_name = "Roof Unlock: Level 5-3",
            level_id = 43,
            waves = 20
        ),

        "5-4": Level(
            name = "Roof: Level 5-4",
            flags = 3,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Pogo', 'Ladder', 'Flag'],
            flag_location_ids = [2028, 2029],
            clear_location_id = 1043,
            unlock_item_name = "Roof Unlock: Level 5-4",
            level_id = 44,
            waves = 30
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
            level_id = 45,
            waves = 20
        ),

        "5-6": Level(
            name = "Roof: Level 5-6",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Catapult', 'Flag'],
            flag_location_ids = [2031],
            clear_location_id = 1045,
            unlock_item_name = "Roof Unlock: Level 5-6",
            level_id = 46,
            waves = 20
        ),

        "5-7": Level(
            name = "Roof: Level 5-7",
            flags = 3,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Bungee', 'Ladder', 'Catapult', 'Flag'],
            flag_location_ids = [2032, 2033],
            clear_location_id = 1046,
            unlock_item_name = "Roof Unlock: Level 5-7",
            level_id = 47,
            waves = 30
        ),

        "5-8": Level(
            name = "Roof: Level 5-8",
            flags = 2,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Gargantuar', 'Flag'],
            flag_location_ids = [2034],
            clear_location_id = 1047,
            unlock_item_name = "Roof Unlock: Level 5-8",
            level_id = 48,
            waves = 20
        ),

        "5-9": Level(
            name = "Roof: Level 5-9",
            flags = 3,
            location = "Roof",
            zombies = ['Normal', 'Conehead', 'Buckethead', 'JackInTheBox', 'Bungee', 'Ladder', 'Catapult', 'Gargantuar', 'Flag'],
            flag_location_ids = [2035, 2036],
            clear_location_id = 1048,
            unlock_item_name = "Roof Unlock: Level 5-9",
            level_id = 49,
            waves = 30
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
                level_id = 51,
                waves = 20
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
                level_id = 52,
                ignore_locked_tiles = True,
                waves = 20
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
                level_id = 53,
                ignore_locked_tiles = True
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
                level_id = 54,
                waves = 40
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
                level_id = 55,
                ignore_locked_tiles = True
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
                level_id = 56,
                waves = 20
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
                level_id = 57,
                ignore_locked_tiles = True
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
                level_id = 58,
                ignore_locked_tiles = True
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
                level_id = 59,
                ignore_locked_tiles = True
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
                level_id = 60,
                waves = 30
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
                level_id = 61,
                waves = 20
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
                conveyor = {'Flower Pot': 155, 'Melon-pult': 5, 'Chomper': 5, 'Pumpkin': 15, 'Jalapeno': 10, 'Squash': 10},
                special = "column",
                conveyor_default = 6,
                unlock_item_name = "Mini-game Unlock: Column Like You See 'Em",
                level_id = 62,
                waves = 30
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
                level_id = 63,
                waves = 40
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
                level_id = 64,
                waves = 40
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
                level_id = 65,
                ignore_locked_tiles = True,
                waves = 12
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
                level_id = 66,
                waves = 50
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
                level_id = 67,
                waves = 30
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
                level_id = 68,
                ignore_locked_tiles = True,
                waves = 30
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
                level_id = 69,
                waves = 30
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
                level_id = 71,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Peashooter": 5, "Snow Pea": 5, "Squash": 5}],
                vasebreaker_zombies = [{"Normal": 6, "Buckethead": 3, "JackInTheBox": 1}]
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
                level_id = 72,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Backwards Repeater (Vasebreaker)": 7, "Wall-nut": 3, "Potato Mine": 2, "Snow Pea": 3}],
                vasebreaker_zombies = [{"Normal": 6, "Buckethead": 3, "JackInTheBox": 1}]
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
                level_id = 73,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Wall-nut": 3, "Snow Pea": 4, "Backwards Repeater (Vasebreaker)": 6, "Hypno-shroom": 3, "Squash": 2}],
                vasebreaker_zombies = [{"Normal": 8, "Buckethead": 2, "JackInTheBox": 1, "Dancer": 1}]
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
                level_id = 74,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Backwards Repeater (Vasebreaker)": 4, "Puff-shroom": 11, "Hypno-shroom": 4}],
                vasebreaker_zombies = [{"Normal": 7, "Football": 1, "JackInTheBox": 8}]
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
                level_id = 75,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Snow Pea": 2, "Backwards Repeater (Vasebreaker)": 6, "Hypno-shroom": 2, "Squash": 4, "Pumpkin": 3, "Magnet-shroom": 3}],
                vasebreaker_zombies = [{"Normal": 7, "Football": 3, "Buckethead": 4, "JackInTheBox": 1}]
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
                level_id = 76,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Backwards Repeater (Vasebreaker)": 7, "Squash": 2, "Threepeater": 2, "Torchwood": 4, "Tall-nut": 5}],
                vasebreaker_zombies = [{"Normal": 7, "Football": 2, "Polevaulter": 5, "JackInTheBox": 1}]
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
                level_id = 77,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Wall-nut": 3, "Squash": 3, "Spikeweed": 13}],
                vasebreaker_zombies = [{"Normal": 10, "Buckethead": 1}]
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
                level_id = 78,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Backwards Repeater (Vasebreaker)": 4, "Puff-shroom": 7, "Squash": 5, "Tall-nut": 3}],
                vasebreaker_zombies = [{"Normal": 4, "Pogo": 4, "JackInTheBox": 8}]
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
                level_id = 79,
                ignore_locked_tiles = True,
                vasebreaker_plants = [{"Peashooter": 2, "Wall-nut": 1, "Potato Mine": 1, "Snow Pea": 2, "Backwards Repeater (Vasebreaker)": 6, "Squash": 5, "Threepeater": 2, "Plantern": 1}],
                vasebreaker_zombies = [{"Normal": 8, "Buckethead": 5, "JackInTheBox": 1, "Gargantuar": 1}]
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
                level_id = 80,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Football Zombie (I, Zombie)"}
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
                level_id = 81,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Screen Door Zombie (I, Zombie)"}
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
                level_id = 82,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Digger Zombie (I, Zombie)"}
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
                level_id = 83,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Ladder Zombie (I, Zombie)"}
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
                level_id = 84,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Bungee Zombie (I, Zombie)", "Balloon Zombie (I, Zombie)"}
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
                level_id = 85,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Pole Vaulting Zombie (I, Zombie)", "Gargantuar (I, Zombie)"}
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
                level_id = 86,
                ignore_locked_tiles = True,
                izombie_zombies = {"Zombie (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Pole Vaulting Zombie (I, Zombie)", "Dancing Zombie (I, Zombie)"}
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
                level_id = 87,
                ignore_locked_tiles = True,
                izombie_zombies = {"Imp (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Conehead Zombie (I, Zombie)", "Bungee Zombie (I, Zombie)", "Digger Zombie (I, Zombie)", "Ladder Zombie (I, Zombie)"}
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
                level_id = 88,
                ignore_locked_tiles = True,
                izombie_zombies = {"Imp (I, Zombie)", "Buckethead Zombie (I, Zombie)", "Pole Vaulting Zombie (I, Zombie)", "Conehead Zombie (I, Zombie)", "Football Zombie (I, Zombie)", "Bungee Zombie (I, Zombie)", "Digger Zombie (I, Zombie)", "Ladder Zombie (I, Zombie)"}
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
                level_id = 89,
                waves = 50
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
                level_id = 90,
                waves = 50
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
                level_id = 91,
                waves = 50
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
                level_id = 92,
                waves = 50
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
                level_id = 93,
                waves = 50
            ),

            "SurvivalHardStage1": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter', 'Buckethead', 'Newspaper', 'ScreenDoor', 'Football', 'Dancer', 'Zomboni', 'JackInTheBox', 'Balloon', 'Digger', 'Pogo', 'Ladder', 'Catapult', 'Gargantuar'],
                name = "Survival: Day (Hard)",
                flags = 10,
                type = "Survival",
                flag_location_ids = [2084, 2085, 2086, 2087, 2088, 2089, 2090, 2091, 2092],
                clear_location_id = 1093,
                unlock_item_name = "Survival Unlock: Day (Hard)",
                level_id = 94,
                waves = 100
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
                level_id = 95,
                waves = 100
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
                level_id = 96,
                waves = 100
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
                level_id = 97,
                waves = 100
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
                level_id = 98,
                waves = 100
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
                level_id = 99,
                ignore_locked_tiles = True
            ),

            "ChallengeSunnyDay": Level(
                zombies = ['Football', 'JackInTheBox', 'Normal', 'Buckethead', 'Polevaulter', 'Conehead'],
                name = "Bonus Levels: Sunny Day",
                flags = 4,
                type = "Bonus Levels",
                flag_location_ids = [2129, 2130, 2131],
                clear_location_id = 1099,
                unlock_item_name = "Bonus Levels Unlock: Sunny Day",
                level_id = 100,
                waves = 40
            ),

            "ChallengeResodded": Level(
                zombies = ['Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Unsodded",
                flags = 4,
                type = "Bonus Levels",
                flag_location_ids = [2132, 2133, 2134],
                clear_location_id = 1100,
                unlock_item_name = "Bonus Levels Unlock: Unsodded",
                level_id = 101,
                waves = 40
            ),

            "ChallengeBigTime": Level(
                zombies = ['ScreenDoor', 'Football', 'JackInTheBox', 'Normal', 'Buckethead', 'Conehead'],
                name = "Bonus Levels: Big Time",
                flags = 4,
                type = "Bonus Levels",
                flag_location_ids = [2135, 2136, 2137],
                clear_location_id = 1101,
                unlock_item_name = "Bonus Levels Unlock: Big Time",
                level_id = 102,
                waves = 40
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
                level_id = 103,
                ignore_locked_tiles = True
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
                level_id = 104,
                waves = 20
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
                level_id = 105,
                waves = 20
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
                level_id = 106,
                waves = 20
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
                level_id = 107,
                waves = 30
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
                level_id = 108,
                waves = 30
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
                level_id = 109,
                waves = 10
            ),

            "CloudyDay2": Level(
                zombies = ['Normal', 'Conehead', 'Polevaulter'],
                name = "Cloudy Day: Level 2",
                flags = 2,
                type = "Cloudy Day",
                flag_location_ids = [2145],
                clear_location_id = 1109,
                unlock_item_name = "Cloudy Day Unlock: Level 2",
                level_id = 110,
                waves = 20
            ),

            "CloudyDay3": Level(
                zombies = ['Normal', 'Conehead', 'Buckethead'],
                name = "Cloudy Day: Level 3",
                flags = 1,
                type = "Cloudy Day",
                flag_location_ids = [],
                clear_location_id = 1110,
                unlock_item_name = "Cloudy Day Unlock: Level 3",
                level_id = 111,
                waves = 10
            ),

            "CloudyDay4": Level(
                zombies = ['Normal', 'Conehead', 'Buckethead', 'Polevaulter'],
                name = "Cloudy Day: Level 4",
                flags = 3,
                type = "Cloudy Day",
                flag_location_ids = [2146, 2147],
                clear_location_id = 1111,
                unlock_item_name = "Cloudy Day Unlock: Level 4",
                level_id = 112,
                waves = 30
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
                level_id = 113,
                waves = 20
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
                level_id = 114,
                waves = 30
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
                level_id = 115,
                waves = 20
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
                level_id = 116,
                waves = 30
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
                level_id = 117,
                waves = 20
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
                level_id = 118,
                waves = 30
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
                level_id = 119,
                waves = 20
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
                level_id = 120,
                waves = 30
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
                level_id = 121,
                waves = 20
            )
        }

    return levels

def randomise_zombie_lists(world):
    zombie_blacklist = ["Normal", "Flag", "DuckyTube", "Yeti", "Target", "Zombatar", "Boss", "Bobsled", "BackupDancer"]
    for zombie in ["Conehead", "Polevaulter", "Buckethead", "Newspaper", "ScreenDoor", "Football", "Dancer", "Snorkel", "Zomboni", "DolphinRider", "JackInTheBox", "Balloon", "Digger", "Pogo", "Bungee", "Ladder", "Catapult", "Gargantuar", "PeaHead", "WallnutHead", "JalapenoHead", "GatlingHead", "SquashHead", "TallnutHead", "GigaGargantuar", "TrashCan", "Imp"]:
        if (zombie not in world.options.randomised_zombies.value) or world.options.randomised_zombies.value[zombie] == 0:
            zombie_blacklist.append(zombie)

    permitted_zombie_rando_modes = []
    for mode in world.options.zombie_randomised_modes.value:
        if world.options.zombie_randomised_modes.value[mode] == 1:
            permitted_zombie_rando_modes.append(mode)

    for level in world.included_levels:
        level_data = world.included_levels[level]
        if "Vasebreaker" in permitted_zombie_rando_modes and len(level_data.vasebreaker_zombies) > 0:
            world.included_levels[level].randomise_zombie_vases(world, zombie_blacklist)
        elif level_data.type in permitted_zombie_rando_modes and not (level_data.special in ["beghouled", "slot", "zombiquarium", "whack", "boss", "vasebreaker", "izombie"]):
            world.included_levels[level].randomise_zombies(world, zombie_blacklist)

def randomise_conveyors(world):
    for level in world.included_levels:
        level_data = world.included_levels[level]
        if level_data.conveyor != None and not (level_data.special in ["bowling"]): #Don't randomise Wall-nut Bowling levels
            world.included_levels[level].randomise_conveyor(world)
        
def randomise_plant_vases(world):
    for level in world.included_levels:
        level_data = world.included_levels[level]
        if len(level_data.vasebreaker_plants) > 0 and len(level_data.vasebreaker_zombies) > 0:
            world.included_levels[level].randomise_plant_vases(world)