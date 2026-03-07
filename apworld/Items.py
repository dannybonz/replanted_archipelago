from typing import Dict, NamedTuple, Optional
from BaseClasses import Item, ItemClassification
from .Data import ALL_PLANTS, LEVELS

class PVZRItem(Item):
	game: str = "Plants vs. Zombies"

item_ids: Dict[str, int] = {
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
	"Mower Deploy Trap": 70,
	"Seed Packet Cooldown Trap": 71,
	"Zombie Ambush Trap": 72
}

#100-199 is reserved for plants
for seed_packet in ALL_PLANTS:
	item_ids[seed_packet] = 100 + ALL_PLANTS.index(seed_packet)

#200-500 is reserved for level unlocks
for level in LEVELS:
	item_ids[LEVELS[level]["unlock_item_name"]] = 200 + LEVELS[level]["id"]