#include <iostream>
#include <string>
#include <vector>
#include <filesystem>
#include <algorithm>

#include "sorter.hpp"

void Sorter::appIdToName(std::vector<int> ids)
{
    for(const auto& entry : ids)
    {
        
    }
}

int main() 
{
    std::vector<std::string> fileExtensions = { ".jpg", ".jpeg", ".png", ".avif" };
    std::vector<int> appIds;
    std::string folderLocation, apiKey, appId;

    std::cout << "Enter folder location (or 'q' to exit):";
    std::getline(std::cin, folderLocation);

    if(folderLocation == "q") {
        return 0;
    }

    for(const auto& entry : std::filesystem::directory_iterator(folderLocation)) 
    {
        if (entry.is_regular_file()) {
            std::string filename = entry.path().filename().string();
            std::string extension = filename.substr(filename.find_last_of("."));
            size_t underscorePos = filename.find("_");

            if (std::find(fileExtensions.begin(), fileExtensions.end(), extension) == fileExtensions.end()) {
                continue;
            }

            if (underscorePos == std::string::npos) {
                continue;
            }

            std::string appIdStr = filename.substr(0, underscorePos);

            if (!std::all_of(appIdStr.begin(), appIdStr.end(), ::isdigit)) {
                continue;
            }

            int appId = std::stoi(appIdStr);
            appIds.push_back(appId);
        }
    }

    std::cout << "\nTotal images processed: " << appIds.size() << "\n";

    return 0;
}