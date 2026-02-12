from typing import Dict, List, Any, TextIO

from BaseClasses import Item, ItemClassification
from worlds.AutoWorld import WebWorld, World
from worlds.LauncherComponents import Component, components, launch_subprocess, Type
from .Items import PVZRItem, item_ids
from .Locations import LOCATION_ID_FROM_NAME
from .Options import PVZROptions, OPTION_GROUPS
from .Rules import set_rules, expected_level_loadouts
from .Regions import create_regions
from Options import OptionError
from .Data import ALL_PLANTS, ATTACKING_PLANTS, PROGRESSION_PLANTS, LEVELS, GEN_VERSION, ZOMBIE_TYPES, NO_RANDO_ZOMBIES, POOL_ONLY_ZOMBIES, PLANT_STATS, ALL_PROJECTILES, PROJECTILE_STATS
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
    Plants vs. Zombies: Replanted
    """
    game = "Plants vs. Zombies: Replanted"
    web = PVZRWebWorld()
    options_dataclass = PVZROptions
    options: PVZROptions

    junk_pool: Dict[str, int]
    topology_present = False

    item_name_to_id = item_ids
    location_name_to_id = LOCATION_ID_FROM_NAME

    ut_can_gen_without_yaml = False

    def pick_progression_items(self):
        progression_items = ["Roof Cleaners"]
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

        if self.options.shop_items.value > 8:
            number_of_pages = (self.options.shop_items.value + 7) // 8
            restocks = max(0, number_of_pages - 1)
            progression_items += ["Twiddydinkies Restock"] * restocks

        individual_item_level_types = {"adventure": self.options.adventure_mode_progression.value == 3, "minigame": self.options.minigame_levels.value == 4, "puzzle": self.options.puzzle_levels.value == 4, "survival": self.options.survival_levels.value == 4, "cloudy": self.options.cloudy_day_levels.value == 4, "bonus": self.options.bonus_levels.value == 2}
        for level in LEVELS:
            if LEVELS[level]["type"] in individual_item_level_types and individual_item_level_types[LEVELS[level]["type"]] and not level in self.starting_levels + ["5-10"]:
                progression_items.append(LEVELS[level]["unlock_item_name"])

        if self.options.shop_items.value > 0:
            progression_items.append("Crazy Dave's Car Keys")

        if self.options.adventure_mode_progression.value in [1, 2]:
            progression_items += ["Night Access", "Pool Access", "Fog Access", "Roof Access"]

        for seed_packet in ALL_PLANTS:
            if seed_packet in PROGRESSION_PLANTS and not seed_packet in self.starting_plants:
                progression_items.append(seed_packet)
        
        progression_items += ["Extra Seed Slot"] * (10 - self.options.starting_seed_slots.value)
        return progression_items

    def pick_useful_items(self, remaining_locations):
        useful_items = ["Shovel", "Suburban Almanac", "Pool Cleaners", "Wall-nut First Aid"]

        remaining_plants = copy.copy(ALL_PLANTS)
        self.random.shuffle(remaining_plants)
        for seed_packet in remaining_plants:
            if len(useful_items) < remaining_locations:
                if not seed_packet in self.starting_plants + self.progression_item_names:
                    useful_items.append(seed_packet)

        return useful_items

    def pick_trap_items(self, number_of_traps):
        trap_items = []

        trap_weights = {"Zombie Ambush Trap": self.options.zombie_ambush_trap_weight.value, "Mower Deploy Trap": self.options.mower_deploy_trap_weight.value, "Seed Packet Cooldown Trap": self.options.seed_packet_cooldown_trap_weight.value}
        if sum(list(trap_weights.values())) != 0:
            for i in range(0, number_of_traps):
                trap_items.append(self.random.choices(list(trap_weights.keys()), weights=list(trap_weights.values()), k=1)[0])

        return trap_items

    def pick_filler_items(self, remaining_locations):
        filler_items = []

        filler_unlocks = ["Mustache Mode", "Future Zombies Mode", "Tricked Out Mode", "Daisies Mode", "Pinata Mode", "Alternate Brains Sound"] #"Dancing Zombies Mode"
        self.random.shuffle(filler_unlocks)
        
        while len(filler_unlocks) > 0 and len(filler_items) < remaining_locations:
            filler_items.append(filler_unlocks.pop())

        while len(filler_items) < remaining_locations:
            filler_items.append(self.get_filler_item_name())

        return filler_items

    def create_items(self) -> None:
        starting_plant = self.random.choice(ATTACKING_PLANTS)
        remaining_plants = [p for p in ALL_PLANTS if p != starting_plant]
        self.random.shuffle(remaining_plants)
        self.starting_plants = [starting_plant] + remaining_plants[:self.options.starting_plants.value - 1]

        self.starting_slots = ["Extra Seed Slot"] * (self.options.starting_seed_slots.value - 1)

        self.starting_items = self.starting_plants + self.starting_slots + ["Lawn Mowers"]
        
        self.starting_levels = []

        if self.options.adventure_mode_progression.value in [1, 2]:
            self.starting_items.append("Day Access")
        elif self.options.adventure_mode_progression.value == 3:
            self.starting_levels = ["1-1", "1-2", "1-3", "1-4", "1-5"]
            for level in self.starting_levels:
                self.starting_items.append(LEVELS[level]["unlock_item_name"])

        if self.options.early_sunflower.value:
            self.multiworld.early_items[self.player]["Sunflower"] = 1
            self.multiworld.early_items[self.player]["Coffee Bean"] = 1
            self.multiworld.early_items[self.player]["Sun-shroom"] = 1
        if self.options.early_shovel.value:
            self.multiworld.early_items[self.player]["Shovel"] = 1

        if self.options.shop_items.value > 16:
            self.multiworld.early_items[self.player]["Crazy Dave's Car Keys"] = 1

        item_pool: List[PVZRItem] = []
        self.progression_item_names: List[str] = []
        self.useful_item_names: List[str] = []
        self.filler_item_names: List[str] = []
        self.trap_item_names: List[str] = []
        self.preplaced_progression = ["Music Video"] + self.starting_items

        #Locked items
        self.multiworld.get_location("Roof: Dr. Zomboss (Clear)", self.player).place_locked_item(self.create_item("Music Video")) 

        #Starting inventory
        for starting_item in self.starting_items:
            self.multiworld.push_precollected(self.create_item(starting_item))

        total_locations = len(self.multiworld.get_unfilled_locations(self.player))

        self.progression_item_names = self.pick_progression_items()
        self.useful_item_names = self.pick_useful_items(total_locations - len(self.progression_item_names))

        if len(self.progression_item_names + self.useful_item_names) > total_locations:
            overflowing_item_count = len(self.progression_item_names + self.useful_item_names) - total_locations
            raise OptionError(f"Not enough locations are available. Adjust your options to include {overflowing_item_count} more locations/fewer items, then generate again.")
        
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
        #GOTY compatability mode
        if self.options.goty_compatability_mode.value:
            self.options.cloudy_day_levels.value = 0
            self.options.bonus_levels.value = 0
            NO_RANDO_ZOMBIES.append("TrashCan")

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

        #Setup goal
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
            
            if (total_levels < self.options.total_levels_goal.value):
                self.overall_levels_goal = total_levels
            else:
                self.overall_levels_goal = self.options.total_levels_goal.value
        
        self.zombie_map = {}
        if hasattr(self.multiworld, "re_gen_passthrough"): #If generated through Universal Tracker passthrough
            slot_data: dict = self.multiworld.re_gen_passthrough[self.game]
            self.minigame_unlocks = {int(k): v for k, v in slot_data["minigame_unlocks"].items()}
            self.survival_unlocks = {int(k): v for k, v in slot_data["survival_unlocks"].items()}       
            self.vasebreaker_unlocks = {int(k): v for k, v in slot_data["vasebreaker_unlocks"].items()}       
            self.izombie_unlocks = {int(k): v for k, v in slot_data["izombie_unlocks"].items()}
            self.cloudy_day_unlocks = {int(k): v for k, v in slot_data["cloudy_day_unlocks"].items()}
            self.zombie_map = {int(k): v for k, v in slot_data["zombie_map"].items()}

            self.options.adventure_mode_progression.value = slot_data["adventure_mode_progression"]
            self.options.minigame_levels.value = slot_data["minigame_levels"]
            self.options.puzzle_levels.value = slot_data["puzzle_levels"]
            self.options.survival_levels.value = slot_data["survival_levels"]
            self.options.cloudy_day_levels.value = slot_data["cloudy_day_levels"]
            self.options.bonus_levels.value = slot_data["bonus_levels"]
            self.options.easy_upgrade_plants.value = slot_data["easy_upgrade_plants"]

            self.adventure_levels_goal = slot_data["adventure_levels_goal"]
            self.adventure_areas_goal = slot_data["adventure_areas_goal"]
            self.minigame_levels_goal = slot_data["minigame_levels_goal"]
            self.puzzle_levels_goal = slot_data["puzzle_levels_goal"]
            self.survival_levels_goal = slot_data["survival_levels_goal"]
            self.cloudy_day_levels_goal = slot_data["cloudy_day_levels_goal"]
            self.bonus_levels_goal = slot_data["bonus_levels_goal"]
            self.adventure_areas_goal = slot_data["adventure_areas_goal"]
            self.overall_levels_goal = slot_data["overall_levels_goal"]

        #Setup zombie rando
        self.modified_levels = copy.deepcopy(LEVELS)
        if (self.options.zombie_randomisation):
            self.randomise_zombies()

    def generate_basic(self) -> None:
        #Music randomisation
        self.music_map = []
        if self.options.music_shuffle.value == 2:
            for level in LEVELS:
                self.music_map.append(self.random.randint(0, 8))
        elif self.options.music_shuffle.value == 1:            
            self.music_map = [0, 1, 2, 3, 4, 5, 6, 7, 8]
            self.random.shuffle(self.music_map)

        #Shop prices
        self.shop_prices = []
        for x in range(0, self.options.shop_items.value):
            self.shop_prices.append(self.random.randint(0, 40))

    def fill_slot_data(self) -> dict[str, Any]:
        #Plant stat rando
        self.sun_prices = {}
        self.recharge_times = {}
        self.firing_rates = {}
        self.projectile_damages = {}
        self.plant_healths = {}
        self.important_plants = list({plant for loadout in expected_level_loadouts.values() for plant in loadout})
        if self.options.plant_stat_randomisation.value:
            self.randomise_plant_stats()
        elif self.options.easy_upgrade_plants.value:
            for plant in ALL_PLANTS:
                if plant in PLANT_STATS and "easy_upgrade_cost" in PLANT_STATS[plant]:
                    self.sun_prices[ALL_PLANTS.index(plant)] = PLANT_STATS[plant]["easy_upgrade_cost"]

        return {"music_map": self.music_map, "starting_inv_count": len(self.starting_items), "adventure_mode_progression": self.options.adventure_mode_progression.value, "shop_prices": self.shop_prices, "minigame_unlocks": self.minigame_unlocks, "survival_unlocks": self.survival_unlocks, "izombie_unlocks": self.izombie_unlocks, "vasebreaker_unlocks": self.vasebreaker_unlocks, "gen_version": GEN_VERSION, "imitater_open": self.options.imitater_behaviour.value == 1, "disable_storm_flashes": self.options.disable_storm_flashes.value, "adventure_areas_goal": self.adventure_areas_goal, "minigame_levels_goal": self.minigame_levels_goal, "puzzle_levels_goal": self.puzzle_levels_goal, "survival_levels_goal": self.survival_levels_goal, "deathlink_enabled": self.options.death_link.value, "fast_goal": self.fast_goal, "adventure_levels_goal": self.adventure_levels_goal, "easy_upgrade_plants": self.options.easy_upgrade_plants.value, "cloudy_day_levels_goal": self.cloudy_day_levels_goal, "bonus_levels_goal": self.bonus_levels_goal, "overall_levels_goal": self.overall_levels_goal, "cloudy_day_unlocks": self.cloudy_day_unlocks, "zombie_map": self.zombie_map, "minigame_levels": self.options.minigame_levels.value, "puzzle_levels": self.options.puzzle_levels.value, "survival_levels": self.options.survival_levels.value, "bonus_levels": self.options.bonus_levels.value, "cloudy_day_levels": self.options.cloudy_day_levels.value, "sun_prices": self.sun_prices, "recharge_times": self.recharge_times, "firing_rates": self.firing_rates, "projectile_damages": self.projectile_damages, "plant_healths": self.plant_healths}

    @staticmethod
    def interpret_slot_data(slot_data: dict[str:Any]) -> dict[str:Any]:
        return slot_data

    def get_filler_item_name(self) -> str:
        items = ["Silver Coin", "Gold Coin", "Diamond"]
        weights = [3, 2, 1]
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
        set_rules(self)

    def create_regions(self) -> None:
        create_regions(self)        

    def write_spoiler(self, spoiler_handle: TextIO) -> None:
        spoiler_string = ""
        if self.zombie_map != {}:
            spoiler_string += "\nRandomised Zombies:\n"
            for level in [level for level in self.modified_levels if self.modified_levels[level]["type"] == "adventure" and self.modified_levels[level]["choose"]]:
                zombie_string = ""
                for zombie in [zombie for zombie in self.modified_levels[level]["zombies"] if not zombie in ["Flag", "Bobsled"]]:
                    zombie_string += f"{zombie}, "
                spoiler_string += f"\n{self.modified_levels[level]["name"]}: {zombie_string.strip(", ")}"
            spoiler_string += "\n"
        
        if self.recharge_times != {}:
            spoiler_string += "\nRandomised Plant Stats:\n"
            for plant_index in self.sun_prices:
                plant = ALL_PLANTS[plant_index]
                if PLANT_STATS[plant]["cost"] == 0:
                    sun_mult = 1
                elif "easy_upgrade_cost" in PLANT_STATS[plant] and self.options.easy_upgrade_plants.value:
                    sun_mult = self.sun_prices[plant_index]/PLANT_STATS[plant]["easy_upgrade_cost"]
                else:
                    sun_mult = self.sun_prices[plant_index]/PLANT_STATS[plant]["cost"]

                spoiler_string += f"\n{plant}: x{sun_mult:.2f} Cost / x{self.recharge_times[plant_index]/PLANT_STATS[plant]["refresh"]:.2f} Refresh"
                if plant_index in self.plant_healths:
                    spoiler_string += f" / x{self.plant_healths[plant_index]/PLANT_STATS[plant]["health"]:.2f} Toughness"
                if plant_index in self.firing_rates:
                    spoiler_string += f" / x{1/(self.firing_rates[plant_index]/PLANT_STATS[plant]["rate"]):.2f} Rate"
                if "projectiles" in PLANT_STATS[plant]:
                    for projectile in PLANT_STATS[plant]["projectiles"]:
                        spoiler_string += f" / x{self.projectile_damages[ALL_PROJECTILES.index(projectile)]/PROJECTILE_STATS[projectile]["damage"]:.2f} {projectile} Damage"
            spoiler_string += "\n"

        if expected_level_loadouts != {}:
            spoiler_string += "\nExpected Level Loadouts:\n"
            for level in LEVELS:
                if LEVELS[level]["name"] in expected_level_loadouts:              
                    spoiler_string += f"\n{LEVELS[level]["name"]}: {(", ").join(expected_level_loadouts[LEVELS[level]["name"]]).strip(", ")}"
            spoiler_string += "\n"

        spoiler_handle.write(spoiler_string[:-1])

    def randomise_zombies(self) -> None:
        zombie_blacklist = NO_RANDO_ZOMBIES
        for zombie in self.options.randomised_zombies.value:
            if self.options.randomised_zombies[zombie] == 0:
                zombie_blacklist.append(zombie)

        for level in self.modified_levels:
            if self.modified_levels[level]["type"] == "adventure" and self.modified_levels[level]["choose"]:
                if hasattr(self.multiworld, "re_gen_passthrough"): #Universal Tracker
                    self.modified_levels[level]["zombies"] = [ZOMBIE_TYPES[z] for z in self.zombie_map[self.modified_levels[level]["id"]]]
                else:                    
                    old_zombies = self.modified_levels[level]["zombies"]

                    possible_zombies = [z for z in ZOMBIE_TYPES if z not in zombie_blacklist and (self.modified_levels[level]["location"] in ["Pool", "Fog"] or z not in POOL_ONLY_ZOMBIES)]
                    self.random.shuffle(possible_zombies)
                    
                    new_zombies = [z for z in old_zombies if z in zombie_blacklist]
                    new_zombies += possible_zombies[:len(old_zombies) - len(new_zombies)]

                    if "Zomboni" in new_zombies:
                        new_zombies.append("Bobsled")
                    self.modified_levels[level]["zombies"] = new_zombies

                    self.zombie_map[self.modified_levels[level]["id"]] = [ZOMBIE_TYPES.index(z) for z in new_zombies]

    def apply_randomness_intensity(self, mult, randomness_intensity):
        return 1.0 + (mult - 1.0) * randomness_intensity

    def randomise_plant_stats(self) -> None: #This is built on a wing and a prayer
        projectile_mults = {}

        #First, randomise projectile damage
        for projectile in ALL_PROJECTILES:
            if self.options.maintain_vanilla_projectile_strength.value:
                mult = 1
            else:
                if "projectiles" in PLANT_STATS[self.starting_plants[0]] and projectile in PLANT_STATS[self.starting_plants[0]]["projectiles"]: #Reduced randomness on starting plant
                    mult = self.apply_randomness_intensity(self.random.lognormvariate(0.0, 1), 0.3)
                    if mult > 1.5:
                        mult = 1.5
                else:
                    mult = self.random.lognormvariate(0.0, 1)

            projectile_mults[projectile] = mult
            self.projectile_damages[ALL_PROJECTILES.index(projectile)] = round(PROJECTILE_STATS[projectile]["damage"] * mult)

        #Then randomise plant stats keeping the randomised projectiles in mind
        for plant in PLANT_STATS:
            plant_index = ALL_PLANTS.index(plant)
            stats = PLANT_STATS[plant]
            
            randomness_intensity = 1
            if plant in self.important_plants: #Reduced randomness on "important plants" (plants that AP expects you to use in your run)
                randomness_intensity = 0.6

            #If applicable, calculate average projectile damage
            if "projectiles" in stats:
                avg_projectile_mult = sum(projectile_mults[p] for p in stats["projectiles"]) / len(stats["projectiles"])
            else:
                avg_projectile_mult = 1.0

            #Firing Rate (lower number = faster firing)
            if "rate" in stats:
                if plant in self.important_plants:
                    rate_mult = max(0.3, avg_projectile_mult + self.random.uniform(-0.15, 0.15))
                else:
                    rate_mult = self.apply_randomness_intensity(max(0.3, self.random.lognormvariate(0.0, 1)), randomness_intensity)

                self.firing_rates[plant_index] = round(stats["rate"] * rate_mult)
            else:
                rate_mult = 1

            #Health
            if "health" in stats:
                health_mult = self.apply_randomness_intensity(self.random.lognormvariate(0.0, 1), randomness_intensity * 0.2)
                self.plant_healths[plant_index] = round(stats["health"] * health_mult)
            else:
                health_mult = 1

            if health_mult == 1 and rate_mult == 1 and not "projectiles" in stats:
                sun_cost_mult = self.random.lognormvariate(0.0, randomness_intensity) #Random multiplier for sun 
                packet_cd_mult = 1/sun_cost_mult #Do the inverse on recharge time
            else:
                #Calculate a "power value" so we can work out an appropriate cost/recharge
                dps_value = avg_projectile_mult / rate_mult
                
                if "health" in stats: 
                    health_value = health_mult ** 1.3
                    health_value = max(0.5, min(health_value, 2.0))
                else:
                    health_value = 1.0

                power_value = math.pow(dps_value**0.75 * health_value**0.4, 1.0)

                #Total penalty to be compensated by sun and recharge
                if power_value > 1.0: #Plant buffed
                    total_penalty = power_value ** (1.4 + 0.3 * (power_value - 1)) #Penalty ramps higher and higher if plant is OP
                elif power_value < 1: #Plant nerfed
                    total_penalty = power_value ** 1.2

                split = self.random.betavariate(5, 2) #Randomly split the penalty between the two, with a priority on Sun over Refresh

                sun_cost_mult = total_penalty ** split
                packet_cd_mult = (total_penalty ** (1 - split))
                if packet_cd_mult > 1:
                    packet_cd_mult = min(30, packet_cd_mult ** 3) #Packet cooldown has to do more work than sun in order to nerf a plant

            base_cost = stats["cost"]
            if "easy_upgrade_cost" in stats and self.options.easy_upgrade_plants.value:
                base_cost = stats["easy_upgrade_cost"]

            #Prevent Sun cost from exceeding a maximum; if it does, then increase cooldown accordingly instead
            max_price = self.random.choice(range(1000, 970, -5))
            if plant == "Sun-shroom":
                max_price = 50 #Cap Sun-shroom price at 50 sun
            raw_sun_price = base_cost * sun_cost_mult
            if raw_sun_price > max_price: #Too expensive
                excess_mult = raw_sun_price / max_price
                sun_cost_mult = max_price / base_cost
                packet_cd_mult *= excess_mult

            self.sun_prices[plant_index] = 5 * round((base_cost * sun_cost_mult) / 5)
            base_refresh = stats["refresh"]
            self.recharge_times[plant_index] = 5 * round((base_refresh * packet_cd_mult) / 5)