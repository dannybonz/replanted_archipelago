from typing import Dict, List, Any

from BaseClasses import Item, ItemClassification
from worlds.AutoWorld import WebWorld, World
from worlds.LauncherComponents import Component, components, launch_subprocess, Type
from .Items import PVZRItem, item_ids
from .Locations import LOCATION_ID_FROM_NAME
from .Options import PVZROptions
from .Rules import set_rules
from .Regions import create_regions
from Options import OptionError
from .Data import SEED_PACKETS, ATTACKING_PLANTS, PROGRESSION_PLANTS, LEVELS, GEN_VERSION

class PVZRWebWorld(WebWorld):
    theme = "partyTime"
    
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

class PVZRWorld(World):
    """
    Plants vs. Zombies: Replanted
    """
    game = "Plants vs. Zombies: Replanted"
    web = PVZRWebWorld()
    options_dataclass = PVZROptions
    options: PVZROptions

    junk_pool: Dict[str, int]
    topology_present = True

    item_name_to_id = item_ids
    location_name_to_id = LOCATION_ID_FROM_NAME

    def pick_progression_items(self):
        progression_items = ["Roof Cleaners"]
        if self.options.include_minigames.value and not self.options.minigame_puzzle_survival_order.value == 3:
            progression_items.append("Mini-games")
        if self.options.include_puzzle_levels.value and not self.options.minigame_puzzle_survival_order.value == 3:
            progression_items.append("Puzzle Mode")
        if self.options.include_survival_levels.value and not self.options.minigame_puzzle_survival_order.value == 3:
            progression_items.append("Survival Mode")
        if self.options.include_cloudy_day_levels.value:
            progression_items.append("Cloudy Day")
        if self.options.include_bonus_levels.value:
            progression_items.append("Bonus Levels")
        if self.options.shop_items.value > 8:
            number_of_pages = (self.options.shop_items.value + 7) // 8
            restocks = max(0, number_of_pages - 1)
            progression_items += ["Twiddydinkies Restock"] * restocks

        if self.options.minigame_puzzle_survival_order.value == 3:
            permitted_types = {"minigame": self.options.include_minigames.value, "puzzle": self.options.include_puzzle_levels.value, "survival": self.options.include_survival_levels.value}
            for level in LEVELS:
                if LEVELS[level]["type"] in permitted_types and permitted_types[LEVELS[level]["type"]]:
                    progression_items.append(LEVELS[level]["unlock_item_name"])

        if self.options.shop_items.value > 0:
            progression_items.append("Crazy Dave's Car Keys")

        if self.options.adventure_mode_progression.value in [1, 2]:
            progression_items += ["Night Access", "Pool Access", "Fog Access", "Roof Access"]

        for seed_packet in SEED_PACKETS:
            if seed_packet in PROGRESSION_PLANTS and not seed_packet in self.starting_plants:
                progression_items.append(seed_packet)
        
        progression_items += ["Extra Seed Slot"] * (10 - self.options.starting_seed_slots.value)
        return progression_items

    def pick_useful_items(self, remaining_locations):
        useful_items = ["Shovel", "Suburban Almanac", "Pool Cleaners", "Wall-nut First Aid"]

        self.random.shuffle(SEED_PACKETS)
        for seed_packet in SEED_PACKETS:
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
        remaining_plants = [p for p in SEED_PACKETS if p != starting_plant]
        self.random.shuffle(remaining_plants)
        self.starting_plants = [starting_plant] + remaining_plants[:self.options.starting_plants.value - 1]

        self.starting_slots = ["Extra Seed Slot"] * (self.options.starting_seed_slots.value - 1)

        self.starting_items = self.starting_plants + self.starting_slots
        
        if self.options.adventure_mode_progression.value in [1, 2]:
            self.starting_items.append("Day Access")

        if self.options.early_sunflower.value:
            self.multiworld.early_items[self.player]["Sunflower"] = 1
            self.multiworld.early_items[self.player]["Coffee Bean"] = 1
            self.multiworld.early_items[self.player]["Sun-shroom"] = 1

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

        from Utils import visualize_regions

        visualize_regions(self.multiworld.get_region("Menu", self.player), "region_uml")

        self.progression_item_names = self.pick_progression_items()
        self.useful_item_names = self.pick_useful_items(total_locations - len(self.progression_item_names))

        if len(self.progression_item_names + self.useful_item_names) > total_locations:
            raise OptionError(f"Not enough locations are available. Adjust your options to include more locations, then generate again.")
        
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
        # Minigame unlock order randomisation
        self.minigame_unlocks = { 51: 0, 52: 0, 53: 0, 54: 1, 55: 2, 56: 3, 57: 4, 58: 5, 59: 6, 60: 7, 61: 8, 62: 9, 63: 10, 64: 11, 65: 12, 66: 13, 67: 14, 68: 15, 69: 16, 70: 17}
        self.survival_unlocks = { 89: 0, 90: 0, 91: 0, 92: 1, 93: 2, 94: 3, 95: 4, 96: 5, 97: 6, 98: 7 } 
        self.vasebreaker_unlocks =  { 71: 0, 72: 1, 73: 2, 74: 3, 75: 4, 76: 5, 77: 6, 78: 7, 79: 8 }
        self.izombie_unlocks =  { 80: 0, 81: 1, 82: 2, 83: 3, 84: 4, 85: 5, 86: 6, 87: 7, 88: 8 }

        if self.options.minigame_puzzle_survival_order.value == 1: #Randomised

            minigame_unlock_values = list(self.minigame_unlocks.values())
            self.random.shuffle(minigame_unlock_values)
            self.minigame_unlocks = dict(zip(self.minigame_unlocks, minigame_unlock_values))

            survival_unlock_values = list(self.survival_unlocks.values())
            self.random.shuffle(survival_unlock_values)
            self.survival_unlocks = dict(zip(self.survival_unlocks, survival_unlock_values))

            vasebreaker_unlock_values = list(self.vasebreaker_unlocks.values())
            self.random.shuffle(vasebreaker_unlock_values)
            self.vasebreaker_unlocks = dict(zip(self.vasebreaker_unlocks, vasebreaker_unlock_values))

            izombie_unlock_values = list(self.izombie_unlocks.values())
            self.random.shuffle(izombie_unlock_values)
            self.izombie_unlocks = dict(zip(self.izombie_unlocks, izombie_unlock_values))

        elif self.options.minigame_puzzle_survival_order.value in [2, 3]: #Open/item based
            for key in self.minigame_unlocks:
                self.minigame_unlocks[key] = 0
            for key in self.survival_unlocks:
                self.survival_unlocks[key] = 0
            for key in self.vasebreaker_unlocks:
                self.vasebreaker_unlocks[key] = 0
            for key in self.izombie_unlocks:
                self.izombie_unlocks[key] = 0
        
        if hasattr(self.multiworld, "re_gen_passthrough"): #If generated through Universal Tracker passthrough
            slot_data: dict = self.multiworld.re_gen_passthrough[self.game]
            self.minigame_unlocks = {int(k): v for k, v in slot_data["minigame_unlocks"].items()}
            self.survival_unlocks = {int(k): v for k, v in slot_data["survival_unlocks"].items()}       
            self.vasebreaker_unlocks = {int(k): v for k, v in slot_data["vasebreaker_unlocks"].items()}       
            self.izombie_unlocks = {int(k): v for k, v in slot_data["izombie_unlocks"].items()}
            
    def generate_basic(self) -> None:
        # Music randomisation
        self.music_map = []
        if self.options.music_shuffle.value == 2:            
            for level in LEVELS:
                self.music_map.append(self.random.randint(0, 8))
        elif self.options.music_shuffle.value == 1:            
            self.music_map = [0, 1, 2, 3, 4, 5, 6, 7, 8]
            self.random.shuffle(self.music_map)

        # Shop prices
        self.shop_prices = []
        for x in range(0, self.options.shop_items.value):
            self.shop_prices.append(self.random.randint(0, 40))

    def fill_slot_data(self) -> dict[str, Any]:
        if (self.options.goal == 0):
            require_all_levels = True
        else:
            require_all_levels = False

        imitater_open = False
        if (self.options.imitater_behaviour.value == 1):
            imitater_open = True

        individual_level_unlock_items = False
        if (self.options.minigame_puzzle_survival_order.value == 3):
            individual_level_unlock_items = True

        slot_data_dict = {"music_map": self.music_map, "starting_inv_count": len(self.starting_items), "adventure_mode_progression": self.options.adventure_mode_progression.value, "shop_prices": self.shop_prices, "minigame_unlocks": self.minigame_unlocks, "survival_unlocks": self.survival_unlocks, "izombie_unlocks": self.izombie_unlocks, "vasebreaker_unlocks": self.vasebreaker_unlocks, "gen_version": GEN_VERSION, "require_all_levels": require_all_levels, "imitater_open": imitater_open, "individual_level_unlock_items": individual_level_unlock_items}
        return slot_data_dict

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