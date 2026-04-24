from BaseClasses import Location
from .Levels import create_levels, Level

class PVZRLocation(Location):
    game: str = "Plants vs. Zombies"

all_levels: list[Level] = create_levels()
LOCATION_ID_FROM_NAME: dict[str, int] = {}
for level in all_levels:
    level_data: Level = all_levels[level]
    LOCATION_ID_FROM_NAME[f"{level_data.name} (Clear)"] = level_data.clear_location_id
    waves_per_flag = level_data.waves
    if level_data.flags > 1:
        waves_per_flag = level_data.waves/level_data.flags
    for i in range(1, level_data.waves):
        wave_location_id = (level_data.level_id * 10000) + i
        if i % waves_per_flag == 0:
            LOCATION_ID_FROM_NAME[f"{level_data.name} (Flag #{int(i/waves_per_flag)})"] = wave_location_id
        else:
            LOCATION_ID_FROM_NAME[f"{level_data.name} (Wave #{str(i)})"] = wave_location_id

for x in range(0, 200):
    LOCATION_ID_FROM_NAME[f"Crazy Dave's Twiddydinkies: Item #{str(x + 1)}"] = 5000 + x

LOCATION_NAME_FROM_ID: dict[int, str] = {v: k for k, v in LOCATION_ID_FROM_NAME.items()}