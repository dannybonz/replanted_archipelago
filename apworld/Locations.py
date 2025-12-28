from typing import Dict, NamedTuple, Optional
from BaseClasses import Location
from .Data import LEVELS

class PVZRLocation(Location):
    game: str = "Plants vs. Zombies: Replanted"

LOCATION_ID_FROM_NAME: Dict[str, int] = {}
for level in LEVELS:
    LOCATION_ID_FROM_NAME[f"{LEVELS[level]["name"]} (Clear)"] = LEVELS[level]["clear_location_id"]
    for i in range (1, LEVELS[level]["flags"]):
        LOCATION_ID_FROM_NAME[f"{LEVELS[level]["name"]} (Flag #{str(i)})"] = LEVELS[level]["flag_location_ids"][i - 1]

for x in range(0, 200):
    LOCATION_ID_FROM_NAME[f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}"] = 5000 + x

LOCATION_NAME_FROM_ID = {v: k for k, v in LOCATION_ID_FROM_NAME.items()}