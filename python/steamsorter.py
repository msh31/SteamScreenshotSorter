import os
import json

config_file = 'config.json'
image_extensions = {'.jpg', '.jpeg', '.png', '.bmp', '.gif', '.tiff'}
default_config_data = { 'ApiKey': '', 'Directory': '' }

if not os.path.exists(config_file):
    with open(config_file, 'w') as file:
        json.dump(default_config_data, file, indent=2)

    api_key = input('Enter your Steam API key: ')
    sort_directory = input('Enter the directory of the external screenshots folder: ')

    default_config_data['ApiKey'] = api_key
    default_config_data['Directory'] = sort_directory

    with open(config_file, 'w') as file:
        json.dump(default_config_data, file, indent=2)
else:
    with open(config_file, 'r') as file:
        config = json.load(file)
    print('Config loaded successfully')

