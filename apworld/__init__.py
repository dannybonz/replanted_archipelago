GEN_VERSION = "1.7" #Used to match with client
from BaseClasses import Item, ItemClassification
from worlds.AutoWorld import WebWorld, World
from .Items import PVZRItem, item_ids
from .Locations import LOCATION_ID_FROM_NAME
from .Options import PVZROptions, OPTION_GROUPS
from .Regions import create_regions
from Options import OptionError
from .Plants import create_plants, create_projectiles, randomise_plant_stats, get_all_potential_progression_plants
from .Levels import create_levels, randomise_zombie_lists, randomise_conveyors
from .Zombies import create_zombies
import copy, math

class PVZRWebWorld(WebWorld):
    theme = "jungle"
    
    '''
    setup = Tutorial(
        tutorial_name = "Setup Guide",
        description = "A guide to setting up the Plants vs. Zombies: Replanted Archipelago Multiworld",
        language = "English",
        file_name = "setup.md",
        link = "setup/en",
        authors = ["Bonzorio"]
    )
    tutorials = [setup]
    '''

    option_groups = OPTION_GROUPS

class PVZRWorld(World):
    """
    Plants vs. Zombies
    """
    game = "Plants vs. Zombies"
    web = PVZRWebWorld()
    options_dataclass = PVZROptions
    options: PVZROptions

    topology_present = False

    item_name_to_id = item_ids
    location_name_to_id = LOCATION_ID_FROM_NAME

    ut_can_gen_without_yaml = True

    def get_shop_unlock_item_name(self) -> str:
        if self.options.shop_behaviour.value == 1:
            return "Crazy Dave's Car Keys"
        else:
            return "Progressive Twiddydinkies"

    def pick_progression_items(self) -> list[str]:
        progression_items = ["Roof Cleaners", "Shovel"]
        if not self.options.minigame_levels.value in [0, 4]:
            progression_items.append("Mini-games")
        if not self.options.puzzle_levels.value in [0, 4]:
            progression_items.append("Puzzle Mode")
        if not self.options.survival_levels.value in [0, 4]:
            progression_items.append("Survival Mode")
        if not self.options.cloudy_day_levels.value in [0, 4]:
            progression_items.append("Cloudy Day")
        if self.options.bonus_levels.value == 1:
            progression_items.append("Bonus Levels")
        if self.options.china_level.value == 2:
            progression_items.append("China Access")

        if self.options.shop_behaviour.value > 0:
            progression_items.append(self.get_shop_unlock_item_name())
            if self.options.shop_items.value > 8:
                number_of_pages = (self.options.shop_items.value + 7) // 8
                restocks = max(0, number_of_pages - 1)
                if self.options.shop_behaviour.value == 1:
                    progression_items += ["Twiddydinkies Restock"] * restocks
                else:
                    progression_items += ["Progressive Twiddydinkies"] * restocks

        individual_item_level_types = {"Adventure": self.options.adventure_mode_progression.value == 3, "Mini-games": self.options.minigame_levels.value == 4, "Puzzle": self.options.puzzle_levels.value == 4, "Survival": self.options.survival_levels.value == 4, "Cloudy Day": self.options.cloudy_day_levels.value == 4, "Bonus Levels": self.options.bonus_levels.value == 2}
        for level in self.included_levels:
            level_data = self.included_levels[level]
            if level_data.type in individual_item_level_types and individual_item_level_types[level_data.type] and not level in self.starting_levels + ["5-10"]:
                progression_items.append(level_data.unlock_item_name)

        if self.options.adventure_mode_progression.value in [1, 2]:
            progression_items += ["Night Access", "Pool Access", "Fog Access", "Roof Access"]

        progression_items += [progression_plant for progression_plant in self.progression_plants if progression_plant not in self.starting_plants]
        progression_items += ["Extra Seed Slot"] * (10 - (self.preplaced_progression.count("Extra Seed Slot")))

        if self.options.progressive_sun_capacity_items.value:
            progression_items += ["Progressive Sun Capacity"] * 6

        if self.options.taco_hunt_items.value > 0:
            progression_items += ["Taco"] * self.options.taco_hunt_items.value

        if self.options.individual_tile_unlock_items.value:
            for row_index in range(1, 7):
                for column_index in range(1, 10):
                    tile_unlock_name = f"Tile Unlock: Row #{row_index}, Column #{column_index}"
                    if not tile_unlock_name in self.starting_items:
                        progression_items.append(tile_unlock_name)

        return sorted(progression_items)

    def pick_useful_items(self) -> list[str]:
        useful_items = ["Suburban Almanac", "Pool Cleaners", "Wall-nut First Aid", "Imitater"]

        if self.options.zen_garden_items.value:
            useful_items += ["Zen Garden", "Phonograph", "Gardening Glove", "Wheelbarrow", "Stinky", "Gold Watering Can"]
        if self.options.mower_reward_upgrades.value > 0:
            useful_items += ["Mower Reward Upgrade"] * self.options.mower_reward_upgrades.value

        self.sun_per_upgrade = [0, 5, 25, 50][self.options.starting_sun_upgrades.value]
        if self.sun_per_upgrade > 0:
            sun_upgrades_to_generate = int(self.options.maximum_sun_upgrades.value/self.sun_per_upgrade)
            useful_items += ["Additional Starting Sun"] * sun_upgrades_to_generate

        remaining_plants = [plant_name for plant_name in sorted(self.all_plants.keys()) if not plant_name in self.starting_plants + self.progression_item_names]
        self.random.shuffle(remaining_plants)
        for seed_packet in remaining_plants:
            useful_items.append(seed_packet)

        return sorted(useful_items)

    def pick_trap_items(self, number_of_traps) -> list[str]:
        trap_items = []

        trap_weights = {"Zombie Ambush Trap": self.options.zombie_ambush_trap_weight.value, "Mower Deploy Trap": self.options.mower_deploy_trap_weight.value, "Seed Packet Cooldown Trap": self.options.seed_packet_cooldown_trap_weight.value, "Zombie Shuffle Trap": self.options.zombie_shuffle_trap_weight.value}
        if sum(list(trap_weights.values())) != 0:
            for i in range(0, number_of_traps):
                trap_items.append(self.random.choices(list(trap_weights.keys()), weights=list(trap_weights.values()), k=1)[0])

        return sorted(trap_items)

    def pick_filler_items(self, remaining_locations) -> list[str]:
        filler_items = []

        filler_unlocks = ["Mustache Mode", "Future Zombies Mode", "Tricked Out Mode", "Daisies Mode", "Pinata Mode", "Alternate Brains Sound"] #"Dancing Zombies Mode"
        if self.random.random() <= 0.1:
            filler_unlocks.append("Bacon") #It won't help against the zombies, but everyone loves bacon!

        self.random.shuffle(filler_unlocks)
        
        while len(filler_unlocks) > 0 and len(filler_items) < remaining_locations:
            filler_items.append(filler_unlocks.pop())

        while len(filler_items) < remaining_locations:
            filler_items.append(self.get_filler_item_name())

        return sorted(filler_items)

    def select_wavesanity_locations(self, number_of_locations, huge_waves) -> list[int]:
        included_wave_locations = []
        potential_wave_locations = []
        for level in self.included_levels:
            level_data = self.included_levels[level]
            waves_per_flag = level_data.waves
            if level_data.flags > 1:
                waves_per_flag = level_data.waves/level_data.flags
            for wave_number in range(1, level_data.waves):
                if wave_number % waves_per_flag == 0:
                    if huge_waves:
                        included_wave_locations.append({"level_id": level_data.level_id, "wave_number": wave_number})
                    else:
                        potential_wave_locations.append({"level_id": level_data.level_id, "wave_number": wave_number})
                else:
                    potential_wave_locations.append({"level_id": level_data.level_id, "wave_number": wave_number})
        self.random.shuffle(potential_wave_locations)

        if number_of_locations == -1:
            number_of_locations = len(potential_wave_locations)

        included_wave_locations += potential_wave_locations[:min(len(potential_wave_locations), number_of_locations)]

        self.wavesanity_map = {}
        for location in included_wave_locations:
            if not location["level_id"] in self.wavesanity_map:
                self.wavesanity_map[location["level_id"]] = []
            self.wavesanity_map[location["level_id"]].append(location["wave_number"])

    def create_items(self) -> None:
        if not hasattr(self.multiworld, "re_gen_passthrough"):
            if self.options.early_sunflower.value:
                self.multiworld.early_items[self.player]["Sunflower"] = 1
                self.multiworld.early_items[self.player]["Coffee Bean"] = 1
                self.multiworld.early_items[self.player]["Sun-shroom"] = 1
            if self.options.early_shovel.value:
                self.multiworld.early_items[self.player]["Shovel"] = 1
            if self.options.early_zen_garden.value and self.options.zen_garden_items.value:
                self.multiworld.early_items[self.player]["Zen Garden"] = 1
            if self.options.shop_items.value > 16 and self.options.shop_behaviour.value > 0:
                self.multiworld.early_items[self.player][self.get_shop_unlock_item_name()] = 1
            if self.options.starting_seed_slots.value == 1:
                self.multiworld.early_items[self.player]["Extra Seed Slot"] = 1

            item_pool: list[PVZRItem] = []
            self.filler_item_names: list[str] = []
            self.trap_item_names: list[str] = []

            #Restrictive start prevention if playing a solo seed
            if self.multiworld.players == 1 and self.options.shop_behaviour.value > 0 and (self.options.zombie_randomisation.value or self.options.shop_items.value < 8 or self.options.plant_stat_randomisation.value or self.options.individual_tile_unlock_items.value or self.options.progressive_sun_capacity_items.value):
                #Puts shop access in sphere 1
                self.preplaced_progression.append(self.get_shop_unlock_item_name())
                self.progression_item_names.remove(self.get_shop_unlock_item_name())

                if len(self.starting_levels) >= 1:
                    self.multiworld.get_location(f"{self.included_levels[self.random.choice(self.starting_levels)].name} (Clear)", self.player).place_locked_item(self.create_item(self.get_shop_unlock_item_name()))
                else:
                    self.multiworld.get_location(f"Day: Level 1-1 (Clear)", self.player).place_locked_item(self.create_item(self.get_shop_unlock_item_name()))

                if self.options.starting_seed_slots.value == 1: #Adds an Extra Seed Slot to page 1 of the shop
                    self.preplaced_progression.append("Extra Seed Slot")
                    shop_item_number = 1
                    if self.options.shop_items.value > 1:
                        shop_item_number = self.random.randint(1, min(8, self.options.shop_items.value))
                    self.multiworld.get_location(f"Crazy Dave's Twiddydinkies: Item #{shop_item_number}", self.player).place_locked_item(self.create_item("Extra Seed Slot"))

            #Locked items
            self.multiworld.get_location("Roof: Dr. Zomboss (Clear)", self.player).place_locked_item(self.create_item("Music Video")) 

            #Starting inventory
            for starting_item in self.starting_items:
                self.multiworld.push_precollected(self.create_item(starting_item))

            total_locations = len(self.multiworld.get_unfilled_locations(self.player))
            remaining_locations = total_locations - len(self.progression_item_names + self.useful_item_names)

            number_of_traps = int(remaining_locations * (self.options.trap_percentage.value/100)) #Determines the number of traps based on the number of filler items left and the desired trap percentage, rounding down
            if number_of_traps > 0:
                self.trap_item_names = self.pick_trap_items(number_of_traps)
            else:
                self.trap_item_names = []

            self.filler_item_names = self.pick_filler_items(total_locations - len(self.progression_item_names + self.useful_item_names + self.trap_item_names))

            for item in self.progression_item_names + self.useful_item_names + self.filler_item_names + self.trap_item_names:
                item_pool.append(self.create_item(item))

            self.multiworld.itempool += item_pool

    def generate_early(self) -> None: 
        if hasattr(self.multiworld, "re_gen_passthrough"): #If generated through Universal Tracker passthrough
            slot_data: dict = self.multiworld.re_gen_passthrough[self.game]
            self.match_world_to_slot_data(slot_data)
        else:
            #GOTY compatability mode
            if self.options.goty_compatability_mode.value:
                self.options.cloudy_day_levels.value = 0
                self.options.bonus_levels.value = 0
                self.options.china_level.value = 0
                self.options.randomised_zombies.value["TrashCan"] = 0
                self.options.zombie_randomised_modes.value["Survival"] = 0

            #Setup level unlock order randomisation
            self.minigame_unlocks = { 51: 0, 52: 0, 53: 0, 54: 1, 55: 2, 56: 3, 57: 4, 58: 5, 59: 6, 60: 7, 61: 8, 62: 9, 63: 10, 64: 11, 65: 12, 66: 13, 67: 14, 68: 15, 69: 16, 70: 17}
            self.survival_unlocks = { 89: 0, 90: 0, 91: 0, 92: 1, 93: 2, 94: 3, 95: 4, 96: 5, 97: 6, 98: 7 } 
            self.vasebreaker_unlocks =  { 71: 0, 72: 1, 73: 2, 74: 3, 75: 4, 76: 5, 77: 6, 78: 7, 79: 8 }
            self.izombie_unlocks =  { 80: 0, 81: 1, 82: 2, 83: 3, 84: 4, 85: 5, 86: 6, 87: 7, 88: 8 }
            self.cloudy_day_unlocks =  { 109: 0, 110: 1, 111: 2, 112: 3, 113: 4, 114: 5, 115: 6, 116: 7, 117: 8, 118: 9, 119: 10, 120: 11 }

            if (self.options.minigame_levels.value == 2):            
                minigame_unlock_values = list(self.minigame_unlocks.values())
                self.random.shuffle(minigame_unlock_values)
                self.minigame_unlocks = dict(zip(self.minigame_unlocks, minigame_unlock_values))
            elif (self.options.minigame_levels.value == 3):
                for key in self.minigame_unlocks:
                    self.minigame_unlocks[key] = 0

            if (self.options.puzzle_levels.value == 2):
                vasebreaker_unlock_values = list(self.vasebreaker_unlocks.values())
                self.random.shuffle(vasebreaker_unlock_values)
                self.vasebreaker_unlocks = dict(zip(self.vasebreaker_unlocks, vasebreaker_unlock_values))
                izombie_unlock_values = list(self.izombie_unlocks.values())
                self.random.shuffle(izombie_unlock_values)
                self.izombie_unlocks = dict(zip(self.izombie_unlocks, izombie_unlock_values))
            elif (self.options.puzzle_levels.value == 3):
                for key in self.vasebreaker_unlocks:
                    self.vasebreaker_unlocks[key] = 0
                for key in self.izombie_unlocks:
                    self.izombie_unlocks[key] = 0
            
            if (self.options.survival_levels.value == 2):
                survival_unlock_values = list(self.survival_unlocks.values())
                self.random.shuffle(survival_unlock_values)
                self.survival_unlocks = dict(zip(self.survival_unlocks, survival_unlock_values))
            elif (self.options.survival_levels.value == 3):
                for key in self.survival_unlocks:
                    self.survival_unlocks[key] = 0

            if (self.options.cloudy_day_levels.value == 2):
                cloudy_day_unlock_values = list(self.cloudy_day_unlocks.values())
                self.random.shuffle(cloudy_day_unlock_values)
                self.cloudy_day_unlocks = dict(zip(self.cloudy_day_unlocks, cloudy_day_unlock_values))
            elif (self.options.cloudy_day_levels.value == 3):
                for key in self.cloudy_day_unlocks:
                    self.cloudy_day_unlocks[key] = 0                

            #Set up goal
            self.adventure_levels_goal = self.options.adventure_levels_goal.value
            self.adventure_areas_goal = self.options.adventure_areas_goal.value
            if self.options.adventure_mode_progression.value == 3:
                self.fast_goal = True
            else:
                self.fast_goal = self.options.fast_goal.value

            self.minigame_levels_goal = 0
            self.puzzle_levels_goal = 0
            self.survival_levels_goal = 0
            self.cloudy_day_levels_goal = 0
            self.bonus_levels_goal = 0
            self.overall_levels_goal = 0
            self.taco_goal = 0
            if self.options.taco_hunt_items.value > 0:
                self.taco_goal = self.options.taco_hunt_items.value * (self.options.taco_hunt_percentage / 100)
            if (self.options.minigame_levels_goal.value > 0 and self.options.minigame_levels.value != 0):
                self.minigame_levels_goal = self.options.minigame_levels_goal.value
            if (self.options.puzzle_levels_goal.value > 0 and self.options.puzzle_levels.value != 0):
                self.puzzle_levels_goal = self.options.puzzle_levels_goal.value
            if (self.options.survival_levels_goal.value > 0 and self.options.survival_levels.value != 0):
                self.survival_levels_goal = self.options.survival_levels_goal.value
            if (self.options.cloudy_day_levels_goal.value > 0 and self.options.cloudy_day_levels.value != 0):
                self.cloudy_day_levels_goal = self.options.cloudy_day_levels_goal.value
            if (self.options.bonus_levels_goal.value > 0 and self.options.bonus_levels.value != 0):
                self.bonus_levels_goal = self.options.bonus_levels_goal.value

            if (self.options.total_levels_goal.value > 0):
                total_levels = 49
                if (self.options.minigame_levels.value != 0):
                    total_levels += 20
                if (self.options.puzzle_levels.value != 0):
                    total_levels += 18
                if (self.options.survival_levels.value != 0):
                    total_levels += 10
                if (self.options.cloudy_day_levels.value != 0):
                    total_levels += 12
                if (self.options.bonus_levels.value != 0):
                    total_levels += 10
                if (self.options.china_level.value != 0):
                    total_levels += 1
                
                if (total_levels < self.options.total_levels_goal.value):
                    self.overall_levels_goal = total_levels
                else:
                    self.overall_levels_goal = self.options.total_levels_goal.value

            #Set up defaults
            self.usable_plants = []
            self.plantable_plants = []
            self.all_plants = create_plants()
            self.all_projectiles = create_projectiles()
            self.included_levels = create_levels(self)
            self.all_zombies = create_zombies()
            
            #Enable easy upgrades
            if self.options.easy_upgrade_plants.value:
                for plant_name in self.all_plants:
                    if self.all_plants[plant_name].easy_upgrade_cost != -1:
                        self.all_plants[plant_name].cost = self.all_plants[plant_name].easy_upgrade_cost
                        self.all_plants[plant_name].upgrades_from = "None"
                        self.all_plants[plant_name].unmodified.cost = self.all_plants[plant_name].easy_upgrade_cost
                        self.all_plants[plant_name].unmodified.upgrades_from = "None"

            #Set plant defaults
            if self.options.progressive_sun_capacity_items.value: #Cap to 135 sun max
                self.starting_plants = [self.random.choice(["Peashooter", "Split Pea", "Cactus", "Cabbage-pult", "Kernel-pult", "Starfruit"])]
            else:
                self.starting_plants = [self.random.choice(["Peashooter", "Chomper", "Snow Pea", "Repeater", "Split Pea", "Cactus", "Cabbage-pult", "Kernel-pult", "Starfruit"])]
            if self.options.starting_plants.value > 1:
                remaining_plants = [plant_name for plant_name in sorted(self.all_plants.keys()) if not plant_name in self.starting_plants]
                self.random.shuffle(remaining_plants)
                self.starting_plants += remaining_plants[:self.options.starting_plants.value - 1]
            self.conveyor_attackers = ["Peashooter", "Snow Pea", "Repeater", "Split Pea", "Cactus", "Cabbage-pult", "Kernel-pult", "Starfruit", "Melon-pult", "Threepeater", "Fume-shroom", "Scaredy-shroom", "Puff-shroom"]
            if self.options.easy_upgrade_plants.value:
                self.conveyor_attackers += ["Winter Melon", "Gatling Pea"]
            self.wall_plants = [{"Wall-nut"}, {"Tall-nut"}, {"Pumpkin"}]

            #Randomise zombie lists
            if self.options.zombie_randomisation.value:
                randomise_zombie_lists(self)

            #Randomise plant stats
            self.progression_plants = get_all_potential_progression_plants(self)
            if self.options.plant_stat_randomisation.value:
                self.usable_plants, self.plantable_plants, self.wall_plants = randomise_plant_stats(self)
                self.progression_plants = {progression_plant_name for progression_plant_name in self.progression_plants if progression_plant_name in self.usable_plants and progression_plant_name in self.plantable_plants}
                self.conveyor_attackers = [conveyor_attacker_name for conveyor_attacker_name in self.conveyor_attackers if conveyor_attacker_name in self.usable_plants]
                for plant_set in self.wall_plants:
                    self.progression_plants.update(plant_set)

            #Randomise conveyors
            if self.options.conveyor_randomisation.value:
                randomise_conveyors(self)

            #Starting items
            self.starting_slots = ["Extra Seed Slot"] * (self.options.starting_seed_slots.value - 1)
            self.starting_items = self.starting_plants + self.starting_slots + ["Lawn Mowers"]
            self.starting_levels = []

            #Starting levels
            if self.options.adventure_mode_progression.value in [1, 2]:
                self.starting_items.append("Day Access")
            elif self.options.adventure_mode_progression.value == 3:
                self.starting_levels = ["1-1", "1-2", "1-3", "1-4", "1-5"]
                for level in self.starting_levels:
                    self.starting_items.append(self.included_levels[level].unlock_item_name)
            if self.options.china_level.value == 1:
                self.starting_items.append("China Access")

            #Starting tiles
            if self.options.individual_tile_unlock_items.value:
                back_columns = [1, 2, 3, 4]
                front_columns = [5, 6, 7, 8, 9]
                starting_tiles = {}
                for row_index in range(1, 7):
                    if row_index in [2, 3, 4]:
                        starting_tiles_in_row = {self.random.randint(1, 4), self.random.randint(5, 9)}
                    else:
                        starting_tiles_in_row = {self.random.randint(1, 4), self.random.randint(1, 9)}
                    for column_index in starting_tiles_in_row:
                        self.starting_items.append(f"Tile Unlock: Row #{row_index}, Column #{column_index}")

            #Pick progression and useful items
            self.preplaced_progression = ["Music Video"] + self.starting_items
            self.progression_item_names = self.pick_progression_items()
            self.useful_item_names = self.pick_useful_items()

            #Check for location overflow
            enabled_locations = -12 #Buffer for *some* filler item space
            for level in self.included_levels:
                enabled_locations += 1
                if self.options.huge_wave_locations.value and self.included_levels[level].flags > 1:
                    enabled_locations += self.included_levels[level].flags - 1
            if self.options.shop_behaviour.value > 0:
                enabled_locations += self.options.shop_items.value

            #Account for overflow by adding extra locations
            number_of_wavesanity_locations = 0
            if len(self.progression_item_names + self.useful_item_names) > enabled_locations:
                number_of_wavesanity_locations += len(self.progression_item_names + self.useful_item_names) - enabled_locations
            self.select_wavesanity_locations(number_of_wavesanity_locations, self.options.huge_wave_locations.value)

    def match_world_to_slot_data(self, slot_data) -> None: #Used for Universal Tracker support
        #UT Match player options
        self.options.adventure_mode_progression.value = slot_data["adventure_mode_progression"]
        self.options.minigame_levels.value = slot_data["minigame_levels"]
        self.options.puzzle_levels.value = slot_data["puzzle_levels"]
        self.options.survival_levels.value = slot_data["survival_levels"]
        self.options.cloudy_day_levels.value = slot_data["cloudy_day_levels"]
        self.options.bonus_levels.value = slot_data["bonus_levels"]
        self.options.china_level.value = slot_data["china_level"]
        self.options.easy_upgrade_plants.value = slot_data["easy_upgrade_plants"]
        self.options.shop_items.value = 96
        self.options.progressive_sun_capacity_items.value = slot_data["progressive_sun_capacity_items"]
        self.options.individual_tile_unlock_items.value = slot_data["individual_tile_unlock_items"]

        #UT Defaults
        self.included_levels = create_levels(self)
        self.all_plants = create_plants()
        self.all_projectiles = create_projectiles()
        self.all_zombies = create_zombies()
        self.usable_plants = []
        self.plantable_plants = []        
        self.wall_plants = [{"Wall-nut"}, {"Tall-nut"}, {"Pumpkin"}]
        self.starting_plants = []
        self.select_wavesanity_locations(-1, True)
        self.starting_items = []
        self.progression_item_names = []
        self.useful_item_names = []

        #UT Level unlock orders
        self.minigame_unlocks = {int(k): v for k, v in slot_data["minigame_unlocks"].items()}
        self.survival_unlocks = {int(k): v for k, v in slot_data["survival_unlocks"].items()}       
        self.vasebreaker_unlocks = {int(k): v for k, v in slot_data["vasebreaker_unlocks"].items()}       
        self.izombie_unlocks = {int(k): v for k, v in slot_data["izombie_unlocks"].items()}
        self.cloudy_day_unlocks = {int(k): v for k, v in slot_data["cloudy_day_unlocks"].items()}

        #UT Goals
        self.adventure_levels_goal = slot_data["adventure_levels_goal"]
        self.adventure_areas_goal = slot_data["adventure_areas_goal"]
        self.minigame_levels_goal = slot_data["minigame_levels_goal"]
        self.puzzle_levels_goal = slot_data["puzzle_levels_goal"]
        self.survival_levels_goal = slot_data["survival_levels_goal"]
        self.cloudy_day_levels_goal = slot_data["cloudy_day_levels_goal"]
        self.bonus_levels_goal = slot_data["bonus_levels_goal"]
        self.adventure_areas_goal = slot_data["adventure_areas_goal"]
        self.overall_levels_goal = slot_data["overall_levels_goal"]
        self.taco_goal = slot_data["taco_goal"]
        self.fast_goal = slot_data["fast_goal"]

        #UT Zombie randomisation
        if slot_data["zombie_map"] != {}:
            zombie_id_to_zombie_name = {}
            for zombie in self.all_zombies:
                zombie_id_to_zombie_name[self.all_zombies[zombie].zombie_id] = self.all_zombies[zombie].name

            for level in self.included_levels:
                level_data = self.included_levels[level]
                if level_data.level_id in slot_data["zombie_map"]:
                    zombies_indexes_for_level = slot_data["zombie_map"][level_data.level_id]
                    self.included_levels[level].zombies = [zombie_id_to_zombie_name[zombie_index] for zombie_index in zombies_indexes_for_level]

        #UT Enable easy upgrades
        if self.options.easy_upgrade_plants.value:
            for plant_name in self.all_plants:
                if self.all_plants[plant_name].easy_upgrade_cost != -1:
                    self.all_plants[plant_name].cost = self.all_plants[plant_name].easy_upgrade_cost
                    self.all_plants[plant_name].upgrades_from = "None"
                    self.all_plants[plant_name].unmodified.cost = self.all_plants[plant_name].easy_upgrade_cost
                    self.all_plants[plant_name].unmodified.upgrades_from = "None"

        #UT Plant stat randomisation
        self.progression_plants = get_all_potential_progression_plants(self)
        if slot_data["firing_rates"] != {} or slot_data["sun_prices"] != {} or slot_data["plant_healths"] != {} or slot_data["recharge_times"] != {}:
            for plant in self.all_plants:
                plant_data = self.all_plants[plant]
                if plant_data.plant_id in slot_data["sun_prices"]:
                    self.all_plants[plant].cost = slot_data["sun_prices"][plant_data.plant_id]
                if plant_data.plant_id in slot_data["recharge_times"]:
                    self.all_plants[plant].packet_cooldown = slot_data["recharge_times"][plant_data.plant_id]
                if plant_data.plant_id in slot_data["firing_rates"]:
                    self.all_plants[plant].firing_cooldown = slot_data["firing_rates"][plant_data.plant_id]
                if plant_data.plant_id in slot_data["plant_healths"]:
                    self.all_plants[plant].health = slot_data["plant_healths"][plant_data.plant_id]

            #UT Projectile randomisation
            for projectile in self.all_projectiles:
                projectile_data = self.all_projectiles[projectile]
                if projectile_data.projectile_id in slot_data["projectile_damages"]:
                    self.all_projectiles[projectile].damage = slot_data["projectile_damages"][projectile_data.projectile_id]

            self.usable_plants = [plant_name for plant_name in self.all_plants if self.all_plants[plant_name].is_usable(self)]
            self.plantable_plants = [plant_name for plant_name in self.all_plants if self.all_plants[plant_name].is_plantable()]
            self.progression_plants = {progression_plant_name for progression_plant_name in self.progression_plants if progression_plant_name in self.usable_plants and progression_plant_name in self.plantable_plants}
            self.wall_plants = [{plant_name} for plant_name in self.all_plants if self.all_plants[plant_name].is_wallable()]
            for plant in self.wall_plants:
                self.progression_plants.update(plant)

    def generate_basic(self) -> None:
        #Music randomisation
        self.music_map = []
        if self.options.music_shuffle.value == 2:
            for i in range(0, 125):
                self.music_map.append(self.random.randint(0, 8))
        elif self.options.music_shuffle.value == 1:            
            self.music_map = [0, 1, 2, 3, 4, 5, 6, 7, 8]
            self.random.shuffle(self.music_map)

        #Shop prices
        self.shop_prices = []
        if self.options.shop_behaviour.value > 0:
            for x in range(0, self.options.shop_items.value):
                self.shop_prices.append(self.random.randint(0, 40))

    def fill_slot_data(self) -> dict[str, object]:
        #Level modifications
        self.zombie_map = {}
        self.conveyor_map = {}
        self.zombie_weight_map = {}
        for level in self.included_levels:
            level_data = self.included_levels[level]
            if level_data.zombies != [] and self.options.zombie_randomisation.value and self.options.zombie_randomised_modes.value[level_data.type]: #Zombie Rando
                self.zombie_map[level_data.level_id] = [self.all_zombies[zombie_name].zombie_id for zombie_name in level_data.zombies]
            if self.options.conveyor_randomisation.value and level_data.conveyor: #Conveyor Rando
                level_conveyor_map = {"weights": {}, "default": []} 
                for plant_name in level_data.conveyor:
                    plant_data = self.all_plants[plant_name]
                    level_conveyor_map["weights"][plant_data.plant_id] = level_data.conveyor[plant_name]
                default_seeds = []
                if level_data.conveyor_default > 0:
                    level_conveyor_map["default"] = list(level_conveyor_map["weights"].keys())[:level_data.conveyor_default]
                self.conveyor_map[level_data.level_id] = level_conveyor_map
            if level_data.zombies != [] and self.options.zombie_weight_randomisation.value == 2:
                zombie_weight_map = {}
                for zombie in level_data.zombies:
                    if self.all_zombies[zombie].weight > 0:
                        zombie_weight_map[self.all_zombies[zombie].zombie_id] = self.random.randint(1, 10000)
                self.zombie_weight_map[level_data.level_id] = zombie_weight_map
        if self.options.zombie_weight_randomisation.value == 1:
            for zombie in self.all_zombies:
                if self.all_zombies[zombie].weight > 0:
                    self.zombie_weight_map[self.all_zombies[zombie].zombie_id] = self.random.randint(1, 10000)

        #Plant stat rando
        self.sun_prices = {}
        self.recharge_times = {}
        self.firing_rates = {}
        self.projectile_damages = {}
        self.plant_healths = {}
        if self.options.plant_stat_randomisation.value:
            for plant_name in self.all_plants:
                plant_data = self.all_plants[plant_name]
                self.sun_prices[plant_data.plant_id] = plant_data.cost
                self.recharge_times[plant_data.plant_id] = plant_data.packet_cooldown
                if plant_data.firing_cooldown != -1:
                    self.firing_rates[plant_data.plant_id] = plant_data.firing_cooldown
                if not plant_data.invincible:
                    self.plant_healths[plant_data.plant_id] = plant_data.health
            if not self.options.maintain_vanilla_projectile_strength.value:
                for projectile in self.all_projectiles:
                    projectile_data = self.all_projectiles[projectile]
                    self.projectile_damages[projectile_data.projectile_id] = projectile_data.damage
        elif self.options.easy_upgrade_plants.value:
            for plant_name in ["Twin Sunflower", "Gatling Pea", "Gloom-shroom", "Cattail", "Winter Melon", "Gold Magnet", "Spikerock", "Cob Cannon"]:
                plant_data = self.all_plants[plant_name]
                self.sun_prices[plant_data.plant_id] = plant_data.cost

        return {"music_map": self.music_map, "starting_inv_count": len(self.starting_items), "adventure_mode_progression": self.options.adventure_mode_progression.value, "shop_prices": self.shop_prices, "minigame_unlocks": self.minigame_unlocks, "survival_unlocks": self.survival_unlocks, "izombie_unlocks": self.izombie_unlocks, "vasebreaker_unlocks": self.vasebreaker_unlocks, "gen_version": GEN_VERSION, "imitater_open": self.options.imitater_behaviour.value == 1, "disable_storm_flashes": self.options.disable_storm_flashes.value, "adventure_areas_goal": self.adventure_areas_goal, "minigame_levels_goal": self.minigame_levels_goal, "puzzle_levels_goal": self.puzzle_levels_goal, "survival_levels_goal": self.survival_levels_goal, "deathlink_enabled": self.options.death_link.value, "fast_goal": self.fast_goal, "adventure_levels_goal": self.adventure_levels_goal, "easy_upgrade_plants": self.options.easy_upgrade_plants.value, "cloudy_day_levels_goal": self.cloudy_day_levels_goal, "bonus_levels_goal": self.bonus_levels_goal, "overall_levels_goal": self.overall_levels_goal, "cloudy_day_unlocks": self.cloudy_day_unlocks, "zombie_map": self.zombie_map, "minigame_levels": self.options.minigame_levels.value, "puzzle_levels": self.options.puzzle_levels.value, "survival_levels": self.options.survival_levels.value, "bonus_levels": self.options.bonus_levels.value, "cloudy_day_levels": self.options.cloudy_day_levels.value, "sun_prices": self.sun_prices, "recharge_times": self.recharge_times, "firing_rates": self.firing_rates, "projectile_damages": self.projectile_damages, "plant_healths": self.plant_healths, "conveyor_map": self.conveyor_map, "sun_per_upgrade": self.sun_per_upgrade, "energylink_enabled": self.options.energy_link.value, "taco_goal": self.taco_goal, "china_level": self.options.china_level.value, "zombie_weight_map": self.zombie_weight_map, "zombie_weight_randomisation": self.options.zombie_weight_randomisation.value, "ringlink_enabled": self.options.ring_link.value, "progressive_sun_capacity_items": self.options.progressive_sun_capacity_items.value, "individual_tile_unlock_items": self.options.individual_tile_unlock_items.value, "wavesanity_map": self.wavesanity_map}

    @staticmethod
    def interpret_slot_data(slot_data: dict[str, object]) -> dict[str, object]:
        return slot_data

    def get_filler_item_name(self) -> str:
        random = self.random.random()

        items = ["Silver Coin", "Gold Coin", "Diamond"]
        weights = [3, 2, 1]

        if random >= 0.8:
            if self.options.random_seed_filler.value:
                return "Random Seed Packet"
        elif random >= 0.7:
            if self.options.zombie_freeze_filler.value:
                return "Mass Zombie Freeze"
        elif random >= 0.4:
            if self.options.zen_garden_items.value:
                items = ["Fertilizer", "Tree Food", "Bug Spray", "Chocolate", "Zen Garden Sprout"]
                weights = [3, 2, 1, 1, 3]

        return self.random.choices(items, weights=weights, k=1)[0]
    
    def create_item(self, name: str) -> PVZRItem:
        try:
            if name in self.progression_item_names + self.preplaced_progression:
                item_classification = ItemClassification.progression
            elif name in self.useful_item_names:
                item_classification = ItemClassification.useful
            elif name in self.trap_item_names:
                item_classification = ItemClassification.trap
            else:
                item_classification = ItemClassification.filler
        except:
            item_classification = ItemClassification.filler

        return PVZRItem(name, item_classification, item_ids[name], self.player)

    def set_rules(self) -> None:
        self.multiworld.completion_condition[self.player] = lambda state: state.has("Music Video", self.player)

        if self.options.shop_behaviour.value > 0:
            for i in range(8, self.options.shop_items.value):
                amount_of_restocks_required = int(i / 8)
                self.multiworld.get_location(f"Crazy Dave's Twiddydinkies: Item #{str(i + 1)}", self.player).access_rule = lambda state, req=amount_of_restocks_required: (state.has("Twiddydinkies Restock", self.player, req) or state.has("Progressive Twiddydinkies", self.player, req + 1))

    def create_regions(self) -> None:
        create_regions(self)        

    def write_spoiler(self, spoiler_handle: object) -> None:
        spoiler_string = ""
        
        if self.options.zombie_randomisation.value:
            spoiler_string += "\nRandomised Zombies:\n"
            for level in self.included_levels:
                level_data = self.included_levels[level]
                if level_data.zombies != level_data.unmodified.zombies:
                    zombie_string = ""
                    for zombie in [zombie for zombie in level_data.zombies if not zombie in ["Flag", "Bobsled"]]:
                        zombie_string += f"{zombie}, "
                    spoiler_string += f"\n{level_data.name}: {zombie_string.strip(", ")}"
            spoiler_string += "\n"
        
        if self.options.conveyor_randomisation.value:
            spoiler_string += "\nRandomised Conveyors:\n"
            for level in self.included_levels:
                level_data = self.included_levels[level]
                if level_data.conveyor != level_data.unmodified.conveyor:
                    conveyor_string = ""
                    for plant_name in level_data.conveyor:
                        conveyor_string += f"{plant_name} ({level_data.conveyor[plant_name]}), "
                    spoiler_string += f"\n{level_data.name}: {conveyor_string.strip(", ")}"
            spoiler_string += "\n"

        if self.options.plant_stat_randomisation.value:
            spoiler_string += "\nPlant Stat Modifications:\n"
            for plant_name in self.all_plants:
                plant = self.all_plants[plant_name]

                stat_strings = []
                sun_cost_change = plant.cost - plant.unmodified.cost
                if sun_cost_change > 0:
                    stat_strings.append(f"+{sun_cost_change} Cost ({plant.cost} Total)")
                elif sun_cost_change < 0:
                    stat_strings.append(f"{sun_cost_change} Cost ({plant.cost} Total)")

                packet_cooldown_change = (plant.packet_cooldown - plant.unmodified.packet_cooldown) * 0.01
                if packet_cooldown_change > 0:
                    stat_strings.append(f"+{packet_cooldown_change:.2f}s Refresh ({plant.packet_cooldown * 0.01:.2f}s Total)")
                elif packet_cooldown_change < 0:
                    stat_strings.append(f"{packet_cooldown_change:.2f}s Refresh ({plant.packet_cooldown * 0.01:.2f}s Total)")

                toughness_change = plant.health - plant.unmodified.health
                if toughness_change > 0:
                    stat_strings.append(f"+{toughness_change} HP ({plant.health} Total)")
                elif toughness_change < 0:
                    stat_strings.append(f"{toughness_change} HP ({plant.health} Total)")

                if plant.unmodified.firing_cooldown > 0:
                    firing_rate_multiplier = plant.firing_cooldown/plant.unmodified.firing_cooldown
                    if firing_rate_multiplier != 1:
                        stat_strings.append(f"x{1/firing_rate_multiplier:.2f} Rate")

                for projectile_name in plant.projectiles:
                    projectile_damage_mult = self.all_projectiles[projectile_name].damage/self.all_projectiles[projectile_name].unmodified.damage
                    if projectile_damage_mult != 1:
                        stat_strings.append(f"x{projectile_damage_mult:.2f} {projectile_name} Damage")

                if len(stat_strings) > 0:
                    spoiler_string += f"\n{plant_name}: {" / ".join(stat_strings)}"
            spoiler_string += "\n"

        spoiler_string += "\nExpected Level Loadouts:\n"
        for level in self.included_levels:
            level_data = self.included_levels[level]
            if level_data.expected_loadout != None:              
                spoiler_string += f"\n{level_data.name}: {(", ").join(level_data.expected_loadout).strip(", ")}"
        spoiler_string += "\n"

        spoiler_handle.write(spoiler_string[:-1])