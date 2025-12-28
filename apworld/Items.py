from typing import Dict, NamedTuple, Optional
from BaseClasses import Item, ItemClassification
from .Data import SEED_PACKETS

class PVZRItem(Item):
	game: str = "Plants vs. Zombies: Replanted"

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
	"Day Access": 20,
	"Night Access": 21,
	"Pool Access": 22,
	"Fog Access": 23,
	"Roof Access": 24,
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
	"Nothing": 63,
	"Mower Deploy Trap": 70,
	"Seed Packet Cooldown Trap": 71,
	"Zombie Ambush Trap": 72
}

for seed_packet in SEED_PACKETS:
	item_ids[seed_packet] = 100 + SEED_PACKETS.index(seed_packet)