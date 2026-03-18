from BaseClasses import Location
from .Levels import create_levels, Level

class PVZRLocation(Location):
    game: str = "Plants vs. Zombies"

all_levels: list[Level] = create_levels()
LOCATION_ID_FROM_NAME: dict[str, int] = {}
for level in all_levels:
    level_data: Level = all_levels[level]
    LOCATION_ID_FROM_NAME[f"{level_data.name} (Clear)"] = level_data.clear_location_id
    for i in range (1, level_data.flags):
        LOCATION_ID_FROM_NAME[f"{level_data.name} (Flag #{str(i)})"] = level_data.flag_location_ids[i - 1]

for x in range(0, 200):
    LOCATION_ID_FROM_NAME[f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}"] = 5000 + x

LOCATION_NAME_FROM_ID: dict[int, str] = {v: k for k, v in LOCATION_ID_FROM_NAME.items()}