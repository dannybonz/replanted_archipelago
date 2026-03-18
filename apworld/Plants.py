import copy
from dataclasses import dataclass, field
from typing import List, Dict, Optional

def weighted_roll(world, max_value, power): #Rolls a number between 0 and a max value - higher power equals more likely to be low
    multiplier = world.random.random() ** power
    return round(multiplier * max_value)

def round_to_five(number):
    return round(number / 5) * 5

@dataclass
class Plant:
    name: str
    plant_id: int
    cost: int = 100
    packet_cooldown: int = 750
    health: int = 300

    damage: int = 0
    explosive: bool = False
    edible: bool = True
    can_wall: bool = True
    invincible: bool = False
    effect: str = "None"

    projectiles: list[str] = field(default_factory=list)
    projectile_weights: dict[str, float] = field(default_factory=dict)
    projectiles_per_shot: int = 1
    firing_cooldown: int = -1

    nocturnal: bool = False
    aquatic: bool = False
    bypass_roof_angle: bool = False

    easy_upgrade_cost: int = -1
    upgrades_from: str = "None"

    def __post_init__(self):
        if not self.edible:
            self.can_wall = False
        self.unmodified = copy.deepcopy(self)
        
    def get_average_projectile_damage(self, world, force_unmodified):
        total_weighted_damage = 0
        total_weight = 0

        for projectile_name in self.projectiles:
            weight = 1
            if projectile_name in self.projectile_weights:
                weight = self.projectile_weights[projectile_name]
            
            if force_unmodified:
                damage = world.all_projectiles[projectile_name].unmodified.damage
            else:
                damage = world.all_projectiles[projectile_name].damage

            total_weighted_damage += weight * damage
            total_weight += weight

        if total_weight == 0:
            return 0

        return total_weighted_damage / total_weight

    def get_average_projectile_damage_multiplier(self, world):
        old_damage_multiplier = self.unmodified.get_average_projectile_damage(world, True)
        if old_damage_multiplier == 0:
            return 1
        else:
            return self.get_average_projectile_damage(world, False) / self.unmodified.get_average_projectile_damage(world, True)             

    def get_dps(self, world, force_unmodified):
        return (self.get_average_projectile_damage(world, force_unmodified) * self.projectiles_per_shot) / self.firing_cooldown

    def get_dps_multiplier(self, world):
        dps = self.get_dps(world, False)
        if dps != 0:
            return self.get_dps(world, False) / self.unmodified.get_dps(world, True) 
        return 1/(self.firing_cooldown / self.unmodified.firing_cooldown)

    def is_plantable(self):
        if self.unmodified.cost == 0:
            sun_cost_mult = 1
        else:
            sun_cost_mult = self.cost / self.unmodified.cost

        return sun_cost_mult <= 1.4 and (self.packet_cooldown - self.unmodified.packet_cooldown <= 1000)

    def is_usable(self, world):
        dps_mult = self.get_dps_multiplier(world) 

        if self.firing_cooldown != 0:
            firing_cooldown_mult = self.firing_cooldown / self.unmodified.firing_cooldown
        else:
            firing_cooldown_mult = 1
        
        if self.name == "Garlic" and self.health < 300:
            return False

        return dps_mult >= 0.7 and firing_cooldown_mult <= 2 and not (self.unmodified.health >= 2500 and self.health <= 2500)

    def is_wallable(self):
        return self.health >= 2500 and (self.is_plantable() or (self.cost <= 300 and self.packet_cooldown <= 4000)) and self.edible and self.can_wall and not self.aquatic

    def randomise(self, world, ensure_usability, maintain_plantability): #Plant Stat Rando 2.0 - built on a wing and a prayer
        #Randomise Firing Cooldown
        if self.firing_cooldown != -1:
            maximum_firing_cooldown_mult_loss = 0.7
            maximum_firing_cooldown_mult_gain = 11
            if maintain_plantability:
                maximum_firing_cooldown_mult_loss = 0.2
            if ensure_usability:
                maximum_firing_cooldown_mult_gain = 0.2 

            base_number = 1
            if maintain_plantability:
                base_number = self.get_average_projectile_damage_multiplier(world)
            else:
                average_projectile_damage_multiplier = self.get_average_projectile_damage_multiplier(world)
                if average_projectile_damage_multiplier > 1:
                    base_number = world.random.uniform(1, average_projectile_damage_multiplier)
                else:
                    base_number = world.random.uniform(average_projectile_damage_multiplier, 1)

            if world.random.randint(0, 1): #Nerf
                firing_cooldown_multiplier = base_number + (weighted_roll(world, maximum_firing_cooldown_mult_gain * 100, 2) / 100)
            else: #Buff
                firing_cooldown_multiplier = base_number - (weighted_roll(world, maximum_firing_cooldown_mult_loss * 100, 2) / 100)

            firing_cooldown_multiplier = max(0.1, firing_cooldown_multiplier)
            if self.projectiles_per_shot > 2: #Can't shoot too fast
                firing_cooldown_multiplier = max(0.45, firing_cooldown_multiplier) #Won't drop below 0.45

            self.firing_cooldown = round(self.unmodified.firing_cooldown * firing_cooldown_multiplier)

            if world.options.easy_upgrade_plants.value == False and self.upgrades_from in world.all_plants and self.projectiles == world.all_plants[self.upgrades_from].projectiles: #Stop firing cooldown from downgrading
                self.firing_cooldown = min(round(world.all_plants[self.upgrades_from].firing_cooldown * 1.2), self.firing_cooldown)

        #Randomise Toughness
        if not self.invincible:
            if world.random.randint(0, 1): #Nerf
                if ensure_usability and self.unmodified.health >= 2500:
                    maximum_amount_to_remove = self.unmodified.health - 2500
                    health_addon = weighted_roll(world, int(self.unmodified.health * -0.1), 3)
                else:
                    health_addon = weighted_roll(world, int(self.unmodified.health * -0.9), 3)
            else: #Buff
                if not self.can_wall:
                    health_addon = weighted_roll(world, 600, 1)
                elif maintain_plantability:
                    health_addon = weighted_roll(world, 1000, 1)
                else:
                    health_addon = weighted_roll(world, 6000, 4)
            self.health = self.unmodified.health + health_addon
        else:
            health_addon = 0

        #Create price
        sun_multiplier = self.get_dps_multiplier(world) #Base sun initially off DPS multiplier
        if health_addon != 0: #Health has more of an impact on cooldown than sun
            if not self.edible:
                health_addon_cooldown_addon_multiplier = 0.001
                health_addon_sun_addon_multiplier = 0.001
            elif self.name == "Garlic":
                health_addon_cooldown_addon_multiplier = 1
                health_addon_sun_addon_multiplier = 2
            elif not self.can_wall: #If you can't use the plant as a wall, then it has a reduced impact
                health_addon_cooldown_addon_multiplier = 0.1
                health_addon_sun_addon_multiplier = 1
            else:
                health_addon_cooldown_addon_multiplier = 1
                health_addon_sun_addon_multiplier = 1

            if health_addon > 0: #Buffed
                packet_cooldown_addon = max(world.random.randint(1, 50), health_addon * (1.6 * health_addon_cooldown_addon_multiplier))
                sun_addon = health_addon * 0.03 * health_addon_sun_addon_multiplier
            else: #Nerfed
                packet_cooldown_addon = min(world.random.randint(-50, -1), health_addon * (0.05 * health_addon_cooldown_addon_multiplier))
                sun_addon = health_addon * 0.01 * health_addon_sun_addon_multiplier
        else: #Health unchanged
            packet_cooldown_addon = 0
            sun_addon = 0
        sun_addon = max(self.unmodified.cost * -0.75, sun_addon) #Place a limit on how much health can discount sun price

        if self.name in ["Sunflower", "Sun-shroom"] and sun_multiplier != 0:
            if sun_multiplier > 1:
                packet_cooldown_addon += (sun_multiplier * 200)
                sun_multiplier = sun_multiplier * 1.5
            else:
                packet_cooldown_addon += ((1 - sun_multiplier) * 70)

        if self.firing_cooldown != self.unmodified.firing_cooldown: #If firing rate has changed
            firing_cooldown_mult = self.firing_cooldown / self.unmodified.firing_cooldown
            if firing_cooldown_mult > 1: #Firing cooldown increased:
                sun_multiplier *= 1 + (1 - firing_cooldown_mult) * 0.05
            else: #Firing cooldown decreased
                sun_multiplier *= 1 + (1 - firing_cooldown_mult) * 0.10

        if self.unmodified.cost > 0:
            if world.random.randint(0,1): #Increase packet cooldown but decrease sun cost
                if maintain_plantability:
                    mix_and_match_cooldown_addon = weighted_roll(world, 1500, 1)
                else:
                    mix_and_match_cooldown_addon = weighted_roll(world, 3000, 1)
                if self.unmodified.packet_cooldown > 750:
                    if self.name in ["Coffee Bean", "Blover"]:
                        mix_and_match_sun_addon = round_to_five(mix_and_match_cooldown_addon * -0.0045)
                    else:
                        mix_and_match_sun_addon = round_to_five(mix_and_match_cooldown_addon * -0.006)
                else:
                    mix_and_match_sun_addon = round_to_five(mix_and_match_cooldown_addon * -0.003)
            else: #Decrease packet cooldown but increase sun cost
                if maintain_plantability:
                    mix_and_match_cooldown_addon = weighted_roll(world, self.unmodified.packet_cooldown * 0.25, 1) * -1
                else:
                    mix_and_match_cooldown_addon = weighted_roll(world, self.unmodified.packet_cooldown * 0.9, 1) * -1
                if self.unmodified.packet_cooldown > 750:
                    mix_and_match_sun_addon = round_to_five(mix_and_match_cooldown_addon * -0.04)
                else:
                    mix_and_match_sun_addon = round_to_five(mix_and_match_cooldown_addon * -0.01)

            if mix_and_match_sun_addon != 0:
                packet_cooldown_addon += mix_and_match_cooldown_addon
                sun_addon += mix_and_match_sun_addon

            base_cost = self.unmodified.cost
            if world.options.easy_upgrade_plants.value == False and self.upgrades_from != "None":
                base_form_base_cost = world.all_plants[self.upgrades_from].unmodified.cost
                base_form_randomised_cost = world.all_plants[self.upgrades_from].cost
                difference_in_cost = base_form_randomised_cost - base_form_base_cost
                base_cost -= difference_in_cost #I think this helps balance out sun costs for upgrade plants
                base_cost = max(world.random.choice(range(200, 20, -5)), base_cost) #Prevents negative plant costs (lol)

            self.cost = round_to_five(max(1, (base_cost * sun_multiplier) + sun_addon))
            if self.cost != self.unmodified.cost or self.firing_cooldown != self.unmodified.firing_cooldown or self.get_average_projectile_damage(world, False) != self.unmodified.get_average_projectile_damage(world, True) or self.health != self.unmodified.health:
                self.packet_cooldown = max(100, self.unmodified.packet_cooldown + packet_cooldown_addon)
        else:
            self.packet_cooldown = max(100, (self.unmodified.packet_cooldown * sun_multiplier) + packet_cooldown_addon)
    
        max_sun_cost = world.random.choice(range(1000, 970, -5))
        if self.unmodified.cost == 0:
            max_sun_cost = 0
        elif self.name == "Sun-shroom":
            max_sun_cost = 50
        exceeded_sun_cost = self.cost - max_sun_cost
        if exceeded_sun_cost > 0:
            self.cost = max_sun_cost
            self.packet_cooldown += exceeded_sun_cost * 10
        self.packet_cooldown = min(round(self.packet_cooldown), 50000)

        if (maintain_plantability and not self.is_plantable()) or (ensure_usability and not self.is_usable(world)):
            self.randomise(world, ensure_usability, maintain_plantability)

@dataclass
class Projectile:
    name: str
    projectile_id: int
    damage: int = 20
    aoe: bool = False
    effect: str = "None"

    def __post_init__(self):
        self.unmodified = copy.deepcopy(self)

    def randomise(self, world, ensure_usability, maintain_plantability):
        maximum_damage_mult_gain = 10
        maximum_damage_mult_loss = 0.9
        if ensure_usability:
            maximum_damage_mult_loss = 0.5
        if maintain_plantability:
            maximum_damage_mult_gain = 1

        if world.random.randint(0, 1): #Buff
            damage_mult = 1 + (weighted_roll(world, maximum_damage_mult_gain * 100, 2) / 100)
        else: #Nerf
            damage_mult = 1 - (weighted_roll(world, maximum_damage_mult_loss * 100, 2) / 100)

        self.damage = round(self.unmodified.damage * damage_mult)

def create_plants():
    return {
        "Peashooter": Plant(name = "Peashooter", projectiles = ["Pea"], firing_cooldown = 150, plant_id = 0),
        "Sunflower": Plant(name = "Sunflower", cost = 50, firing_cooldown = 2500, plant_id = 1),
        "Cherry Bomb": Plant(name = "Cherry Bomb", cost = 150, packet_cooldown = 5000, damage = 1800, edible = False, invincible = True, explosive = True, plant_id = 2),
        "Wall-nut": Plant(name = "Wall-nut", cost = 50, packet_cooldown = 3000, health = 4000, plant_id = 3),
        "Potato Mine": Plant(name = "Potato Mine", cost = 25, packet_cooldown = 3000, damage = 1800, can_wall = False, plant_id = 4),
        "Snow Pea": Plant(name = "Snow Pea", cost = 175, projectiles = ["Frozen Pea"], firing_cooldown = 150, plant_id = 5),
        "Chomper": Plant(name = "Chomper", cost = 150, damage = 1800, plant_id = 6),
        "Repeater": Plant(name = "Repeater", cost = 200, projectiles = ["Pea"], projectiles_per_shot = 2, firing_cooldown = 150, plant_id = 7),
        "Puff-shroom": Plant(name = "Puff-shroom", cost = 0, projectiles = ["Spore"], firing_cooldown = 150, nocturnal = True, plant_id = 8),
        "Sun-shroom": Plant(name = "Sun-shroom", cost = 25, firing_cooldown = 2500, nocturnal = True, plant_id = 9),
        "Fume-shroom": Plant(name = "Fume-shroom", cost = 75, firing_cooldown = 150, nocturnal = True, bypass_roof_angle = True, plant_id = 10),
        "Grave Buster": Plant(name = "Grave Buster", cost = 75, can_wall = False, plant_id = 11),
        "Hypno-shroom": Plant(name = "Hypno-shroom", cost = 75, packet_cooldown = 3000, nocturnal = True, effect = "Hypnotise", plant_id = 12),
        "Scaredy-shroom": Plant(name = "Scaredy-shroom", cost = 25, projectiles = ["Spore"], firing_cooldown = 150, nocturnal = True, plant_id = 13),
        "Ice-shroom": Plant(name = "Ice-shroom", cost = 75, packet_cooldown = 5000, nocturnal = True, can_wall = False, effect = "Freeze", plant_id = 14),
        "Doom-shroom": Plant(name = "Doom-shroom", cost = 125, packet_cooldown = 5000, nocturnal = True, damage = 1800, explosive = True, can_wall = False, plant_id = 15),
        "Lily Pad": Plant(name = "Lily Pad", cost = 25, aquatic = True, plant_id = 16),
        "Squash": Plant(name = "Squash", cost = 50, packet_cooldown = 3000, damage = 1800, edible = False, can_wall = False, plant_id = 17),
        "Threepeater": Plant(name = "Threepeater", cost = 325, projectiles = ["Pea"], firing_cooldown = 150, projectiles_per_shot = 3, plant_id = 18),
        "Tangle Kelp": Plant(name = "Tangle Kelp", cost = 25, packet_cooldown = 3000, damage = 1800, edible = False, aquatic = True, can_wall = False, plant_id = 19),
        "Jalapeno": Plant(name = "Jalapeno", cost = 125, packet_cooldown = 5000, damage = 1800, edible = False, invincible = True, explosive = True, plant_id = 20),
        "Spikeweed": Plant(name = "Spikeweed", edible = False, plant_id = 21),
        "Torchwood": Plant(name = "Torchwood", cost = 175, plant_id = 22),
        "Tall-nut": Plant(name = "Tall-nut", cost = 125, packet_cooldown = 3000, health = 8000, plant_id = 23),
        "Sea-shroom": Plant(name = "Sea-shroom", cost = 0, packet_cooldown = 3000, projectiles = ["Spore"], firing_cooldown = 150, nocturnal = True, aquatic = True, plant_id = 24),
        "Plantern": Plant(name = "Plantern", cost = 25, packet_cooldown = 3000, plant_id = 25),
        "Cactus": Plant(name = "Cactus", cost = 125, projectiles = ["Spike"], firing_cooldown = 150, plant_id = 26),
        "Blover": Plant(name = "Blover", invincible = True, edible = False, plant_id = 27),
        "Split Pea": Plant(name = "Split Pea", cost = 125, projectiles = ["Pea"], firing_cooldown = 150, plant_id = 28),
        "Starfruit": Plant(name = "Starfruit", cost = 125, projectiles = ["Star"], firing_cooldown = 150, plant_id = 29),
        "Pumpkin": Plant(name = "Pumpkin", cost = 125, packet_cooldown = 3000, health = 4000, plant_id = 30),
        "Magnet-shroom": Plant(name = "Magnet-shroom", nocturnal = True, plant_id = 31),
        "Cabbage-pult": Plant(name = "Cabbage-pult", projectiles = ["Cabbage"], firing_cooldown = 300, bypass_roof_angle = True, plant_id = 32),
        "Flower Pot": Plant(name = "Flower Pot", cost = 25, plant_id = 33),
        "Kernel-pult": Plant(name = "Kernel-pult", projectiles = ["Kernel", "Butter"], projectile_weights = {"Kernel": 0.75, "Butter": 0.25}, firing_cooldown = 300, bypass_roof_angle = True, plant_id = 34),
        "Coffee Bean": Plant(name = "Coffee Bean", cost = 75, edible = False, invincible = True, plant_id = 35),
        "Garlic": Plant(name = "Garlic", cost = 50, health = 400, can_wall = False, effect = "Yucky", plant_id = 36),
        "Umbrella Leaf": Plant(name = "Umbrella Leaf", plant_id = 37),
        "Marigold": Plant(name = "Marigold", cost = 50, packet_cooldown = 3000, firing_cooldown = 2500, plant_id = 38),
        "Melon-pult": Plant(name = "Melon-pult", cost = 300, projectiles = ["Melon"], firing_cooldown = 300, bypass_roof_angle = True, plant_id = 39),
        "Gatling Pea": Plant(name = "Gatling Pea", cost = 250, packet_cooldown = 5000, projectiles = ["Pea"], firing_cooldown = 150, projectiles_per_shot = 4, upgrades_from = "Repeater", easy_upgrade_cost = 450, plant_id = 40),
        "Twin Sunflower": Plant(name = "Twin Sunflower", cost = 150, packet_cooldown = 5000, firing_cooldown = 2500, projectiles_per_shot = 2, upgrades_from = "Sunflower", easy_upgrade_cost = 200, plant_id = 41),
        "Gloom-shroom": Plant(name = "Gloom-shroom", cost = 150, packet_cooldown = 5000, firing_cooldown = 200, upgrades_from = "Fume-shroom", nocturnal = True, projectiles_per_shot = 4, easy_upgrade_cost = 225, plant_id = 42),
        "Cattail": Plant(name = "Cattail", cost = 225, packet_cooldown = 5000, projectiles = ["Spike"], firing_cooldown = 150, projectiles_per_shot = 2, upgrades_from = "Lily Pad", aquatic = True, easy_upgrade_cost = 250, plant_id = 43),
        "Winter Melon": Plant(name = "Winter Melon", cost = 200, packet_cooldown = 5000, projectiles = ["Frozen Melon"], firing_cooldown = 300, upgrades_from = "Melon-pult", bypass_roof_angle = True, easy_upgrade_cost = 500, plant_id = 44),
        "Gold Magnet": Plant(name = "Gold Magnet", cost = 50, packet_cooldown = 5000, upgrades_from = "Magnet-shroom", easy_upgrade_cost = 150, plant_id = 45),
        "Spikerock": Plant(name = "Spikerock", cost = 125, packet_cooldown = 5000, health = 450, edible = False, upgrades_from = "Spikeweed", easy_upgrade_cost = 225, plant_id = 46),
        "Cob Cannon": Plant(name = "Cob Cannon", cost = 500, packet_cooldown = 5000, firing_cooldown = 600, upgrades_from = "Kernel-pult", easy_upgrade_cost = 700, plant_id = 47)
    }

def create_projectiles():
    return {
        "Pea": Projectile(name = "Pea", projectile_id = 0),
        "Frozen Pea": Projectile(name = "Frozen Pea", effect = "Chill", projectile_id = 1),
        "Cabbage": Projectile(name = "Cabbage", damage = 40, projectile_id = 2),
        "Melon": Projectile(name = "Melon", damage = 80, aoe = True, projectile_id = 3),
        "Spore": Projectile(name = "Spore", projectile_id = 4),
        "Frozen Melon": Projectile(name = "Melon", damage = 80, aoe = True, effect = "Chill", projectile_id = 5),
        "Star": Projectile(name = "Star", projectile_id = 6),
        "Spike": Projectile(name = "Spike", projectile_id = 7),
        "Kernel": Projectile(name = "Kernel", projectile_id = 8),
        "Butter": Projectile(name = "Butter", damage = 40, effect = "Stun", projectile_id = 9)
    }

def randomise_plant_stats(world):
    #Loop through all levels and create a list of plants that are deemed as progression - these will be randomised in such a way that they remain usable
    used_plants = {"standard": [], "conveyor": []}
    potential_progression_plants = []
    
    level_keys = sorted(world.included_levels.keys())
    world.random.shuffle(level_keys)

    for level in level_keys:
        level_data = world.included_levels[level]
        if level_data.conveyor != None:
            level_type_key = "conveyor"
        elif level_data.choose:
            level_type_key = "standard"
        else:
            continue #Not a conveyor level or a choose your seeds level, so it's irrelevant - plant stat rando won't be applied

        plant_combinations_for_level = level_data.create_plant_combinations(world)
        threats = sorted(plant_combinations_for_level.keys())
        world.random.shuffle(threats)

        for threat in threats:
            combinations_for_threat = plant_combinations_for_level[threat]

            threat_already_countered = False
            for combination in combinations_for_threat:
                if all(plant in used_plants[level_type_key] for plant in combination):
                    threat_already_countered = True
                potential_progression_plants += combination
            
            if not threat_already_countered:
                used_plants[level_type_key] += world.random.choice(combinations_for_threat)

    #Define plants to maintain usability
    #These will be a similar power level/buffed compared to their usual
    usable_plants = sorted(set(used_plants["standard"] + used_plants["conveyor"] + [world.starting_plants[0]]))

    #Decide plants to maintain plantability
    #These will be affordable/similar cooldown to their usual
    plantable_plants = sorted(set(used_plants["standard"] + [world.starting_plants[0]]))

    #Decide which projectiles can be messed with moreso than others
    usable_projectiles = []
    plantable_projectiles = []
    for plant in usable_plants:
        usable_projectiles += world.all_plants[plant].projectiles
    for plant in plantable_plants:
        plantable_projectiles += world.all_plants[plant].projectiles

    #Randomise projectiles
    if not world.options.maintain_vanilla_projectile_strength.value:
        for projectile in world.all_projectiles:
            world.all_projectiles[projectile].randomise(world, projectile in usable_projectiles, projectile in plantable_projectiles)

    if (not any(plant in usable_plants for plant in ["Wall-nut", "Tall-nut", "Pumpkin"])) or not any(plant in plantable_plants for plant in ["Wall-nut", "Tall-nut", "Pumpkin"]):
        guaranteed_wall = world.random.choice(["Wall-nut", "Tall-nut", "Pumpkin"])
        usable_plants.append(guaranteed_wall)
        plantable_plants.append(guaranteed_wall)

    #Randomise plants
    wall_plants = []
    for plant_name in world.all_plants:
        plant = world.all_plants[plant_name]
        plant.randomise(world, plant_name in usable_plants, plant_name in plantable_plants)

        if plant.is_usable(world):
            usable_plants.append(plant_name)

        if plant.is_plantable():
            plantable_plants.append(plant_name)

        if plant.is_wallable():
            wall_plants.append({plant_name})

    return usable_plants, plantable_plants, wall_plants

def get_all_potential_progression_plants(world):
    potential_progression_plants = []

    for level in world.included_levels:
        plant_combinations_for_level = world.included_levels[level].create_plant_combinations(world)
        threats = sorted(plant_combinations_for_level.keys())
        for threat in threats:
            combinations_for_threat = plant_combinations_for_level[threat]
            for combination in combinations_for_threat:
                potential_progression_plants += combination

    return sorted(set(potential_progression_plants))