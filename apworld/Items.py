from BaseClasses import Item

class PVZRItem(Item):
	game: str = "Plants vs. Zombies"

item_ids: dict[str, int] = {
	"Music Video": 1,
	"Crazy Dave's Car Keys": 2,
	"Extra Seed Slot": 3,
	"Shovel": 4,
	"Suburban Almanac": 5,
	"Zen Garden": 6,
	"Mini-games": 7,
	"Puzzle Mode": 8,
	"Survival Mode": 9,
	"Cloudy Day": 10,
	"Bonus Levels": 11,
	"Roof Cleaners": 12,
	"Pool Cleaners": 13,
	"Lawn Mowers": 14,
	"Twiddydinkies Restock": 15,
	"Wall-nut First Aid": 16,
	"Rake": 17,
	"Additional Starting Sun": 18,
	"Mower Reward Upgrade": 19,
	"Day Access": 20,
	"Night Access": 21,
	"Pool Access": 22,
	"Fog Access": 23,
	"Roof Access": 24,
	"China Access": 25,
	"Progressive Shovel": 26,
	"Taco": 27,
	"Progressive Twiddydinkies": 28,
	"Progressive Loot Rate": 29,
	"Gardening Glove": 30,
	"Gold Watering Can": 31,
	"Phonograph": 32,
	"Stinky": 33,
	"Wheelbarrow": 34,
	"Progressive Sun Capacity": 35,
	"Mustache Mode": 50,
	"Future Zombies Mode": 51,
	"Tricked Out Mode": 52,
	"Daisies Mode": 53,
	"Pinata Mode": 54,
	"Alternate Brains Sound": 55,
	"Dancing Zombies Mode": 56,
	"Silver Coin": 60,
	"Gold Coin": 61,
	"Diamond": 62,
	"Bacon": 63,
	"Random Seed Packet": 64,
	"Tree Food": 65,
	"Fertilizer": 66,
	"Bug Spray": 67,
	"Chocolate": 68,
	"Mass Zombie Freeze": 69,
	"Mower Deploy Trap": 70,
	"Seed Packet Cooldown Trap": 71,
	"Zombie Ambush Trap": 72,
	"Zombie Shuffle Trap": 73,
	"Zen Garden Sprout": 74
}

#100-199 is reserved for plants
plant_names: list[str] = ['Peashooter', 'Sunflower', 'Cherry Bomb', 'Wall-nut', 'Potato Mine', 'Snow Pea', 'Chomper', 'Repeater', 'Puff-shroom', 'Sun-shroom', 'Fume-shroom', 'Grave Buster', 'Hypno-shroom', 'Scaredy-shroom', 'Ice-shroom', 'Doom-shroom', 'Lily Pad', 'Squash', 'Threepeater', 'Tangle Kelp', 'Jalapeno', 'Spikeweed', 'Torchwood', 'Tall-nut', 'Sea-shroom', 'Plantern', 'Cactus', 'Blover', 'Split Pea', 'Starfruit', 'Pumpkin', 'Magnet-shroom', 'Cabbage-pult', 'Flower Pot', 'Kernel-pult', 'Coffee Bean', 'Garlic', 'Umbrella Leaf', 'Marigold', 'Melon-pult', 'Gatling Pea', 'Twin Sunflower', 'Gloom-shroom', 'Cattail', 'Winter Melon', 'Gold Magnet', 'Spikerock', 'Cob Cannon', 'Imitater']
for plant_name in plant_names:
	item_ids[plant_name] = 100 + plant_names.index(plant_name)

#200-500 is reserved for level unlocks
level_unlock_names: list[str] = ['Day Unlock: Level 1-1', 'Day Unlock: Level 1-2', 'Day Unlock: Level 1-3', 'Day Unlock: Level 1-4', 'Day Unlock: Level 1-5', 'Day Unlock: Level 1-6', 'Day Unlock: Level 1-7', 'Day Unlock: Level 1-8', 'Day Unlock: Level 1-9', 'Day Unlock: Level 1-10', 'Night Unlock: Level 2-1', 'Night Unlock: Level 2-2', 'Night Unlock: Level 2-3', 'Night Unlock: Level 2-4', 'Night Unlock: Level 2-5', 'Night Unlock: Level 2-6', 'Night Unlock: Level 2-7', 'Night Unlock: Level 2-8', 'Night Unlock: Level 2-9', 'Night Unlock: Level 2-10', 'Pool Unlock: Level 3-1', 'Pool Unlock: Level 3-2', 'Pool Unlock: Level 3-3', 'Pool Unlock: Level 3-4', 'Pool Unlock: Level 3-5', 'Pool Unlock: Level 3-6', 'Pool Unlock: Level 3-7', 'Pool Unlock: Level 3-8', 'Pool Unlock: Level 3-9', 'Pool Unlock: Level 3-10', 'Fog Unlock: Level 4-1', 'Fog Unlock: Level 4-2', 'Fog Unlock: Level 4-3', 'Fog Unlock: Level 4-4', 'Fog Unlock: Level 4-5', 'Fog Unlock: Level 4-6', 'Fog Unlock: Level 4-7', 'Fog Unlock: Level 4-8', 'Fog Unlock: Level 4-9', 'Fog Unlock: Level 4-10', 'Roof Unlock: Level 5-1', 'Roof Unlock: Level 5-2', 'Roof Unlock: Level 5-3', 'Roof Unlock: Level 5-4', 'Roof Unlock: Level 5-5', 'Roof Unlock: Level 5-6', 'Roof Unlock: Level 5-7', 'Roof Unlock: Level 5-8', 'Roof Unlock: Level 5-9', 'Night Roof Unlock: Dr. Zomboss', 'Mini-game Unlock: ZomBotany', 'Mini-game Unlock: Wall-nut Bowling', 'Mini-game Unlock: Slot Machine', "Mini-game Unlock: It's Raining Seeds", 'Mini-game Unlock: Beghouled', 'Mini-game Unlock: Invisi-ghoul', 'Mini-game Unlock: Seeing Stars', 'Mini-game Unlock: Zombiquarium', 'Mini-game Unlock: Beghouled Twist', 'Mini-game Unlock: Big Trouble Little Zombie', 'Mini-game Unlock: Portal Combat', "Mini-game Unlock: Column Like You See 'Em", 'Mini-game Unlock: Bobsled Bonanza', 'Mini-game Unlock: Zombie Nimble Zombie Quick', 'Mini-game Unlock: Whack a Zombie', 'Mini-game Unlock: Last Stand', 'Mini-game Unlock: ZomBotany 2', 'Mini-game Unlock: Wall-nut Bowling 2', 'Mini-game Unlock: Pogo Party', "Mini-game Unlock: Dr. Zomboss's Revenge", 'Puzzle Unlock: Vasebreaker', 'Puzzle Unlock: To The Left', 'Puzzle Unlock: Third Vase', 'Puzzle Unlock: Chain Reaction', 'Puzzle Unlock: M is for Metal', 'Puzzle Unlock: Scary Potter', 'Puzzle Unlock: Hokey Pokey', 'Puzzle Unlock: Another Chain Reaction', 'Puzzle Unlock: Ace of Vase', 'Puzzle Unlock: I, Zombie', 'Puzzle Unlock: I, Zombie Too', 'Puzzle Unlock: Can You Dig It?', 'Puzzle Unlock: Totally Nuts', 'Puzzle Unlock: Dead Zeppelin', 'Puzzle Unlock: Me Smash!', 'Puzzle Unlock: ZomBoogie', 'Puzzle Unlock: Three Hit Wonder', 'Puzzle Unlock: All your brainz r belong to us', 'Survival Unlock: Day', 'Survival Unlock: Night', 'Survival Unlock: Pool', 'Survival Unlock: Fog', 'Survival Unlock: Roof', 'Survival Unlock: Day (Hard)', 'Survival Unlock: Night (Hard)', 'Survival Unlock: Pool (Hard)', 'Survival Unlock: Fog (Hard)', 'Survival Unlock: Roof (Hard)', 'Bonus Levels Unlock: Art Challenge Wall-nut', 'Bonus Levels Unlock: Sunny Day', 'Bonus Levels Unlock: Unsodded', 'Bonus Levels Unlock: Big Time', 'Bonus Levels Unlock: Art Challenge Sunflower', 'Bonus Levels Unlock: Air Raid', 'Bonus Levels Unlock: High Gravity', 'Bonus Levels Unlock: Grave Danger', 'Bonus Levels Unlock: Can You Dig It?', 'Bonus Levels Unlock: Dark Stormy Night', 'Cloudy Day Unlock: Level 1', 'Cloudy Day Unlock: Level 2', 'Cloudy Day Unlock: Level 3', 'Cloudy Day Unlock: Level 4', 'Cloudy Day Unlock: Level 5', 'Cloudy Day Unlock: Level 6', 'Cloudy Day Unlock: Level 7', 'Cloudy Day Unlock: Level 8', 'Cloudy Day Unlock: Level 9', 'Cloudy Day Unlock: Level 10', 'Cloudy Day Unlock: Level 11', 'Cloudy Day Unlock: Level 12', 'China Unlock: The Great Wall']
for i in range(0, len(level_unlock_names)):
	item_ids[level_unlock_names[i]] = 201 + i

#Lawn tiles ID format = 10AB where A is row and B is column (1000-2000 reserved)
for row_index in range(0, 6): #6 rows
	for column_index in range(0, 9): #9 Columns
		item_ids[f"Tile Unlock: Row #{row_index + 1}, Column #{column_index + 1}"] = 1000 + (row_index * 10) + column_index